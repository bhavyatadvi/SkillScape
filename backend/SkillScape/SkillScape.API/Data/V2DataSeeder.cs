using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.API.Data;

public static class V2DataSeeder
{
    private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    // Intermediate DTO that matches the JSON file format (with options as array)
    private class QuestionBankDto
    {
        public string Id { get; set; } = string.Empty;
        public string PhaseKey { get; set; } = string.Empty;
        public string SubPhaseKey { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string LanguageComplexity { get; set; } = "Simple";
        public string Dimension { get; set; } = string.Empty;
        public string CompetencyId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Intent { get; set; } = string.Empty;
        // Seed banks have used both a JSON string (targetDiscriminatesJson) and
        // a JSON array (targetDiscriminates). JsonElement accepts either shape.
        public JsonElement TargetDiscriminatesJson { get; set; }
        public JsonElement TargetDiscriminates { get; set; }
        public string GetTargetDiscriminatesJson()
        {
            var value = TargetDiscriminatesJson.ValueKind != JsonValueKind.Undefined
                ? TargetDiscriminatesJson
                : TargetDiscriminates;

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? "[]",
                JsonValueKind.Array => value.GetRawText(),
                _ => "[]"
            };
        }
        // Parsed as a list — will be serialized back to OptionsJson
        public List<JsonElement>? Options { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var seedsDir = FindSeedsDirectory();

        if (!Directory.Exists(seedsDir))
            return;

        // 1. Upsert Taxonomy
        var taxonomyFile = Path.Combine(seedsDir, "TaxonomySeed.json");
        if (File.Exists(taxonomyFile))
        {
            var taxonomyJson = await File.ReadAllTextAsync(taxonomyFile);
            // TaxonomySeed.json is authored with arrays/objects (phasesApplicable,
            // typicalRoles, salaryRange, etc.), while the entity stores those
            // values in *Json string columns. Deserializing directly silently
            // ignored these properties, leaving every node with phases "[]";
            // AfterPostGraduation then had no eligible taxonomy candidates.
            var taxonomyArray = JsonNode.Parse(taxonomyJson)?.AsArray();
            var jsonColumnMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["phasesApplicable"] = "PhasesApplicableJson",
                ["typicalRoles"] = "TypicalRolesJson",
                ["requiredDegrees"] = "RequiredDegreesJson",
                ["recommendedCourses"] = "RecommendedCoursesJson",
                ["institutionTypes"] = "InstitutionTypesJson",
                ["entranceExams"] = "EntranceExamsJson",
                ["salaryRange"] = "SalaryRangeJson",
                ["governmentExamsApplicable"] = "GovernmentExamsApplicableJson",
                ["eligibleEducationLevels"] = "EligibleEducationLevelsJson",
                ["traitWeights"] = "TraitWeightsJson",
                ["aptitudeDimensions"] = "AptitudeDimensionsJson",
                ["competencyDimensions"] = "CompetencyDimensionsJson",
                ["interestDimensions"] = "InterestDimensionsJson",
                ["workStyleDimensions"] = "WorkStyleDimensionsJson",
                ["workTaskDimensions"] = "WorkTaskDimensionsJson",
                ["knowledgeAreas"] = "KnowledgeAreasJson",
                ["questionTags"] = "QuestionTagsJson",
                ["subdomains"] = "SubdomainsJson",
                ["scoringWeights"] = "ScoringWeightsJson"
            };

            if (taxonomyArray != null)
            {
                foreach (var node in taxonomyArray.OfType<JsonObject>())
                {
                    foreach (var (sourceProperty, jsonColumn) in jsonColumnMappings)
                    {
                        if (node.TryGetPropertyValue(sourceProperty, out var value) && value != null)
                            node[jsonColumn] = JsonValue.Create(value.ToJsonString());
                    }
                }
            }

            var nodes = taxonomyArray?.Deserialize<List<CareerTaxonomyV2>>(_jsonOpts);
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    node.ParentId = node.ParentId ?? "";
                    
                    var existing = await context.CareerTaxonomyV2.FindAsync(node.Id);
                    if (existing == null)
                    {
                        context.CareerTaxonomyV2.Add(node);
                    }
                    else
                    {
                        context.Entry(existing).CurrentValues.SetValues(node);
                    }
                }
                await context.SaveChangesAsync();
            }
        }

        // 1.5 Upsert CareerSwitch Graph
        var graphFile = Path.Combine(seedsDir, "CareerSwitchGraphSeed.json");
        if (File.Exists(graphFile))
        {
            var graphJson = await File.ReadAllTextAsync(graphFile);
            
            // Need a quick local DTO since the JSON uses string NodeIds for transitions instead of ints
            var profilesDto = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(graphJson, _jsonOpts);
            if (profilesDto != null)
            {
                // First pass: upsert all CareerSwitchNodes
                foreach (var dto in profilesDto)
                {
                    var nodeId = dto["NodeId"].ToString();
                    var profile = await context.CareerSwitchNodes.FirstOrDefaultAsync(c => c.NodeId == nodeId);
                    if (profile == null)
                    {
                        profile = new CareerSwitchNode { NodeId = nodeId };
                        context.CareerSwitchNodes.Add(profile);
                    }
                    profile.Title = dto.ContainsKey("Title") ? dto["Title"].ToString() : "";
                    profile.Description = dto.ContainsKey("Description") ? dto["Description"].ToString() : "";
                    profile.Industry = dto.ContainsKey("Industry") ? dto["Industry"].ToString() : "";
                    profile.Domain = dto.ContainsKey("Domain") ? dto["Domain"].ToString() : "";
                    profile.MinimumEducation = dto.ContainsKey("MinimumEducation") ? dto["MinimumEducation"].ToString() : "";
                    profile.IsRegulated = dto.ContainsKey("IsRegulated") && bool.Parse(dto["IsRegulated"].ToString()!);
                }
                await context.SaveChangesAsync(); // Save to generate IDs

                // Second pass: Transitions
                foreach (var dto in profilesDto)
                {
                    if (dto.ContainsKey("OutgoingTransitions") && dto["OutgoingTransitions"] is JsonElement transitionsElem && transitionsElem.ValueKind == JsonValueKind.Array)
                    {
                        var fromNodeId = dto["NodeId"].ToString();
                        var fromProfile = await context.CareerSwitchNodes.FirstOrDefaultAsync(c => c.NodeId == fromNodeId);
                        
                        foreach (var tDto in transitionsElem.EnumerateArray())
                        {
                            var targetNodeId = tDto.GetProperty("ToCareerId").GetString();
                            var toProfile = await context.CareerSwitchNodes.FirstOrDefaultAsync(c => c.NodeId == targetNodeId);
                            
                            if (fromProfile != null && toProfile != null)
                            {
                                var transition = await context.CareerTransitions.FirstOrDefaultAsync(t => t.FromCareerId == fromProfile.Id && t.ToCareerId == toProfile.Id);
                                if (transition == null)
                                {
                                    transition = new CareerTransition { FromCareerId = fromProfile.Id, ToCareerId = toProfile.Id };
                                    context.CareerTransitions.Add(transition);
                                }
                                
                                transition.TransitionDifficulty = tDto.TryGetProperty("TransitionDifficulty", out var diff) ? diff.GetString() ?? "" : "";
                                transition.TransitionType = tDto.TryGetProperty("TransitionType", out var type) ? type.GetString() ?? "" : "";
                                transition.CompatibilityScore = tDto.TryGetProperty("CompatibilityScore", out var score) ? score.GetDouble() : 0;
                                transition.WhyItFits = tDto.TryGetProperty("WhyItFits", out var why) ? why.GetString() ?? "" : "";
                                transition.PotentialChallenges = tDto.TryGetProperty("PotentialChallenges", out var chal) ? chal.GetString() ?? "" : "";
                                transition.ExistingSkillsUsed = tDto.TryGetProperty("ExistingSkillsUsed", out var exist) ? exist.GetString() ?? "" : "";
                                transition.NewSkillsRequired = tDto.TryGetProperty("NewSkillsRequired", out var newS) ? newS.GetString() ?? "" : "";
                                transition.EntryRoles = tDto.TryGetProperty("EntryRoles", out var roles) ? roles.GetString() ?? "" : "";
                            }
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }

        // 2. Upsert question banks — handles both formats:
        //    a) Legacy format: fields map directly to QuizQuestionV2 (OptionsJson is a string)
        //    b) New format: "options" is a JSON array, must be serialized to OptionsJson
        var questionFiles = Directory.GetFiles(seedsDir, "QuestionBankSeed_*.json");
        foreach (var file in questionFiles)
        {
            var json = await File.ReadAllTextAsync(file);

            // First try the new DTO format (with options array)
            try
            {
                var dtos = JsonSerializer.Deserialize<List<QuestionBankDto>>(json, _jsonOpts);
                if (dtos != null && dtos.Count > 0 && dtos[0].Options != null)
                {
                    foreach (var dto in dtos)
                    {
                        var question = new QuizQuestionV2
                        {
                            Id = dto.Id,
                            PhaseKey = dto.PhaseKey,
                            SubPhaseKey = dto.SubPhaseKey,
                            Domain = dto.Domain,
                            Subdomain = dto.Subdomain,
                            QuestionType = dto.QuestionType,
                            LanguageComplexity = dto.LanguageComplexity,
                            Dimension = dto.Dimension,
                            CompetencyId = dto.CompetencyId,
                            Text = dto.Text,
                            Intent = dto.Intent,
                            TargetDiscriminatesJson = dto.GetTargetDiscriminatesJson(),
                            OptionsJson = dto.Options != null
                                ? JsonSerializer.Serialize(dto.Options, _jsonOpts)
                                : "[]",
                            DisplayOrder = dto.DisplayOrder,
                            IsActive = dto.IsActive,
                            EducationLevelsJson = "[]",
                            SubdomainIdsJson = "[]",
                            DimensionWeightsJson = "{}",
                            CareerWeightsJson = "{}",
                        };

                        var existing = await context.QuizQuestionsV2.FindAsync(question.Id);
                        if (existing == null)
                        {
                            context.QuizQuestionsV2.Add(question);
                        }
                        else
                        {
                            // Update key fields in case the bank was improved
                            existing.PhaseKey = question.PhaseKey;
                            existing.SubPhaseKey = question.SubPhaseKey;
                            existing.Text = question.Text;
                            existing.OptionsJson = question.OptionsJson;
                            existing.TargetDiscriminatesJson = question.TargetDiscriminatesJson;
                            existing.Domain = question.Domain;
                            existing.Subdomain = question.Subdomain;
                            existing.IsActive = question.IsActive;
                            existing.CompetencyId = question.CompetencyId;
                            existing.Intent = question.Intent;
                            context.Entry(existing).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                    continue; // next file
                }
            }
            catch { }

            // Fall back to legacy format (OptionsJson is already a string in the JSON)
            try
            {
                var questions = JsonSerializer.Deserialize<List<QuizQuestionV2>>(json, _jsonOpts);
                if (questions == null) continue;

                foreach (var question in questions)
                {
                    question.EducationLevelsJson ??= "[]";
                    question.SubdomainIdsJson ??= "[]";
                    question.DimensionWeightsJson ??= "{}";
                    question.CareerWeightsJson ??= "{}";
                    question.TargetDiscriminatesJson ??= "[]";
                    question.OptionsJson ??= "[]";
                    question.FieldId ??= "";
                    question.DomainId ??= "";
                    question.ConceptId ??= "";
                    question.Specialization ??= "";

                    var existing = await context.QuizQuestionsV2.FindAsync(question.Id);
                    if (existing == null)
                    {
                        context.QuizQuestionsV2.Add(question);
                    }
                    else
                    {
                        existing.PhaseKey = question.PhaseKey ?? existing.PhaseKey;
                        existing.SubPhaseKey = question.SubPhaseKey ?? existing.SubPhaseKey;
                        existing.Text = question.Text ?? existing.Text;
                        existing.OptionsJson = question.OptionsJson ?? existing.OptionsJson;
                        existing.TargetDiscriminatesJson = question.TargetDiscriminatesJson ?? existing.TargetDiscriminatesJson;
                        existing.Domain = question.Domain ?? existing.Domain;
                        existing.Subdomain = question.Subdomain ?? existing.Subdomain;
                        existing.IsActive = question.IsActive;
                        existing.CompetencyId = question.CompetencyId ?? existing.CompetencyId;
                        existing.Intent = question.Intent ?? existing.Intent;
                        existing.EducationLevelsJson = question.EducationLevelsJson ?? existing.EducationLevelsJson;
                        existing.SubdomainIdsJson = question.SubdomainIdsJson ?? existing.SubdomainIdsJson;
                        existing.DimensionWeightsJson = question.DimensionWeightsJson ?? existing.DimensionWeightsJson;
                        existing.CareerWeightsJson = question.CareerWeightsJson ?? existing.CareerWeightsJson;
                        context.Entry(existing).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                }
            }
            catch { }
        }

        await context.SaveChangesAsync();
        
        await SeedSalaryIntelligenceAsync(context);
    }

    private static async Task SeedSalaryIntelligenceAsync(ApplicationDbContext context)
    {
        var benchmarks = new List<SalaryBenchmark>();
        if (!await context.SalaryBenchmarks.AnyAsync())
        {
            benchmarks.AddRange(new[]
            {
                new SalaryBenchmark { Id = Guid.NewGuid().ToString(), RoleKey = "software_engineer", CanonicalRole = "Software Engineer", OccupationFamily = "IT", Domain = "Technology", Country = "India", Currency = "INR", ExperienceBand = "0-1", P25Salary = 300000, P50Salary = 600000, P75Salary = 1200000 },
                new SalaryBenchmark { Id = Guid.NewGuid().ToString(), RoleKey = "software_engineer", CanonicalRole = "Software Engineer", OccupationFamily = "IT", Domain = "Technology", Country = "India", Currency = "INR", ExperienceBand = "2-3", P25Salary = 600000, P50Salary = 1200000, P75Salary = 2000000 },
                new SalaryBenchmark { Id = Guid.NewGuid().ToString(), RoleKey = "software_engineer", CanonicalRole = "Software Engineer", OccupationFamily = "IT", Domain = "Technology", Country = "India", Currency = "INR", ExperienceBand = "4-5", P25Salary = 1000000, P50Salary = 1800000, P75Salary = 3000000 },
                new SalaryBenchmark { Id = Guid.NewGuid().ToString(), RoleKey = "data_scientist", CanonicalRole = "Data Scientist", OccupationFamily = "IT", Domain = "Technology", Country = "India", Currency = "INR", ExperienceBand = "0-1", P25Salary = 500000, P50Salary = 800000, P75Salary = 1500000 },
                new SalaryBenchmark { Id = Guid.NewGuid().ToString(), RoleKey = "data_scientist", CanonicalRole = "Data Scientist", OccupationFamily = "IT", Domain = "Technology", Country = "India", Currency = "INR", ExperienceBand = "2-3", P25Salary = 800000, P50Salary = 1500000, P75Salary = 2500000 },
                new SalaryBenchmark { Id = Guid.NewGuid().ToString(), RoleKey = "data_scientist", CanonicalRole = "Data Scientist", OccupationFamily = "IT", Domain = "Technology", Country = "India", Currency = "INR", ExperienceBand = "4-5", P25Salary = 1400000, P50Salary = 2200000, P75Salary = 4000000 },
            });
        }

        if (!await context.SalaryBenchmarks.AnyAsync(b => b.RoleKey == "ai_engineer" && b.Country == "India" && b.ExperienceBand == "0-3"))
        {
            benchmarks.Add(new SalaryBenchmark
            {
                Id = Guid.NewGuid().ToString(),
                RoleKey = "ai_engineer",
                CanonicalRole = "Artificial Intelligence / Machine Learning",
                OccupationFamily = "IT",
                Domain = "Technology",
                Country = "India",
                Currency = "INR",
                ExperienceBand = "0-3",
                // foundit reports a published 0-3 year average salary range of ₹7.92–13.76 LPA.
                P25Salary = 792000,
                P50Salary = 1084000,
                P75Salary = 1376000,
                DataSourceCount = 1,
                SourceDate = new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                LastUpdated = DateTime.UtcNow,
                DataQuality = "Medium",
                Notes = "Published industry average range for Artificial Intelligence, 0-3 years; not a role-specific guarantee."
            });
        }

        if (!await context.SalaryBenchmarks.AnyAsync(b => b.RoleKey == "pharmacy_research" && b.Country == "India" && b.ExperienceBand == "0-2"))
        {
            benchmarks.Add(new SalaryBenchmark
            {
                Id = Guid.NewGuid().ToString(),
                RoleKey = "pharmacy_research",
                CanonicalRole = "Pharmaceutical Research Associate",
                OccupationFamily = "Healthcare",
                Domain = "Pharmacy",
                Country = "India",
                Currency = "INR",
                ExperienceBand = "0-2",
                P25Salary = 180000,
                P50Salary = 240000,
                P75Salary = 300000,
                DataSourceCount = 1,
                SourceDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastUpdated = DateTime.UtcNow,
                DataQuality = "Low",
                Notes = "Fresher Research Associate range from a small self-reported sample; use as an indicative benchmark only."
            });
        }

        if (!await context.SalaryBenchmarks.AnyAsync(b => b.RoleKey == "pharmacy_clinical" && b.Country == "India" && b.ExperienceBand == "0-2"))
        {
            benchmarks.Add(new SalaryBenchmark
            {
                Id = Guid.NewGuid().ToString(),
                RoleKey = "pharmacy_clinical",
                CanonicalRole = "Clinical Pharmacist",
                OccupationFamily = "Healthcare",
                Domain = "Pharmacy",
                Country = "India",
                Currency = "INR",
                ExperienceBand = "0-2",
                P25Salary = 200000,
                P50Salary = 285000,
                P75Salary = 370000,
                DataSourceCount = 1,
                SourceDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastUpdated = DateTime.UtcNow,
                DataQuality = "Low",
                Notes = "Indicative fresher Clinical Pharmacist range from a self-reported salary dataset; actual pay varies by employer and location."
            });
        }

        if (!await context.SalaryBenchmarks.AnyAsync(b => b.RoleKey == "pharmacy_sales" && b.Country == "India" && b.ExperienceBand == "0-2"))
        {
            benchmarks.Add(new SalaryBenchmark
            {
                Id = Guid.NewGuid().ToString(),
                RoleKey = "pharmacy_sales",
                CanonicalRole = "Pharmaceutical Sales Representative",
                OccupationFamily = "Healthcare / Pharma Sales",
                Domain = "Pharmacy",
                Country = "India",
                Currency = "INR",
                ExperienceBand = "0-2",
                P25Salary = 120000,
                P50Salary = 260000,
                P75Salary = 400000,
                DataSourceCount = 1,
                SourceDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastUpdated = DateTime.UtcNow,
                DataQuality = "Low",
                Notes = "Indicative fresher Medical Representative range from salary submissions; pay varies by employer, location, and incentives."
            });
        }

        if (!await context.SalaryBenchmarks.AnyAsync(b => b.RoleKey == "architecture_urban_design" && b.Country == "India" && b.ExperienceBand == "0-2"))
        {
            benchmarks.Add(new SalaryBenchmark
            {
                Id = Guid.NewGuid().ToString(), RoleKey = "architecture_urban_design", CanonicalRole = "Urban Designer / Junior Architect",
                OccupationFamily = "Architecture & Planning", Domain = "Architecture", Country = "India", Currency = "INR", ExperienceBand = "0-2",
                P25Salary = 180000, P50Salary = 340000, P75Salary = 500000, DataSourceCount = 1,
                SourceDate = new DateTime(2025, 7, 9, 0, 0, 0, DateTimeKind.Utc), LastUpdated = DateTime.UtcNow, DataQuality = "Low",
                Notes = "Indicative fresher benchmark for junior architecture roles used as a proxy for urban design; pay varies by role, employer, and location."
            });
        }

        if (!await context.SalaryBenchmarks.AnyAsync(b => b.RoleKey == "architecture_general" && b.Country == "India" && b.ExperienceBand == "0-2"))
        {
            benchmarks.Add(new SalaryBenchmark
            {
                Id = Guid.NewGuid().ToString(), RoleKey = "architecture_general", CanonicalRole = "Junior Architect",
                OccupationFamily = "Architecture & Planning", Domain = "Architecture", Country = "India", Currency = "INR", ExperienceBand = "0-2",
                P25Salary = 180000, P50Salary = 340000, P75Salary = 500000, DataSourceCount = 1,
                SourceDate = new DateTime(2025, 7, 9, 0, 0, 0, DateTimeKind.Utc), LastUpdated = DateTime.UtcNow, DataQuality = "Low",
                Notes = "Fresher Junior Architect range from a small self-reported sample; actual pay varies by employer and location."
            });
        }

        if (!await context.SalaryBenchmarks.AnyAsync(b => b.RoleKey == "legal_professional" && b.Country == "India" && b.ExperienceBand == "0-2"))
        {
            benchmarks.Add(new SalaryBenchmark
            {
                Id = Guid.NewGuid().ToString(), RoleKey = "legal_professional", CanonicalRole = "Entry-Level Legal Professional",
                OccupationFamily = "Legal Services", Domain = "Law", Country = "India", Currency = "INR", ExperienceBand = "0-2",
                P25Salary = 240000, P50Salary = 420000, P75Salary = 720000, DataSourceCount = 1,
                SourceDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), LastUpdated = DateTime.UtcNow, DataQuality = "Low",
                Notes = "Indicative entry-level legal services range; advocate earnings vary substantially with city, court practice, employer, and client base."
            });
        }

        if (benchmarks.Count > 0)
        {
            context.SalaryBenchmarks.AddRange(benchmarks);
            await context.SaveChangesAsync();
        }

        var aiBenchmark = await context.SalaryBenchmarks
            .Include(b => b.Sources)
            .FirstOrDefaultAsync(b => b.RoleKey == "ai_engineer" && b.Country == "India" && b.ExperienceBand == "0-3");
        if (aiBenchmark != null && !aiBenchmark.Sources.Any())
        {
            aiBenchmark.Sources.Add(new SalaryBenchmarkSource
            {
                Id = Guid.NewGuid().ToString(),
                SourceName = "foundit Insights Tracker — October 2025",
                SourceUrl = "https://www.foundit.in/career-advice/wp-content/uploads/2025/11/foundit-Insights-Tracker-Oct25-Trends.pdf",
                PublishedAt = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                RetrievedAt = DateTime.UtcNow,
                SampleSize = 0,
                SourceType = "IndustryReport",
                Methodology = "Published industry average salary range",
                Notes = "Artificial Intelligence, 0-3 years: ₹7.92–13.76 LPA."
            });
            await context.SaveChangesAsync();
        }

        var pharmacyBenchmark = await context.SalaryBenchmarks
            .Include(b => b.Sources)
            .FirstOrDefaultAsync(b => b.RoleKey == "pharmacy_research" && b.Country == "India" && b.ExperienceBand == "0-2");
        if (pharmacyBenchmark != null && !pharmacyBenchmark.Sources.Any())
        {
            pharmacyBenchmark.Sources.Add(new SalaryBenchmarkSource
            {
                Id = Guid.NewGuid().ToString(),
                SourceName = "AmbitionBox — Pharmaceutical Job Fresher Salary",
                SourceUrl = "https://www.ambitionbox.com/salaries/pharmaceutical-job-salaries/fresher",
                PublishedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                RetrievedAt = DateTime.UtcNow,
                SampleSize = 3,
                SourceType = "SelfReportedSalarySurvey",
                Methodology = "Fresher Research Associate salary submissions",
                Notes = "The page reports ₹1.8–3.0 LPA from 3 Research Associate submissions."
            });
            await context.SaveChangesAsync();
        }

        var clinicalPharmacyBenchmark = await context.SalaryBenchmarks
            .Include(b => b.Sources)
            .FirstOrDefaultAsync(b => b.RoleKey == "pharmacy_clinical" && b.Country == "India" && b.ExperienceBand == "0-2");
        if (clinicalPharmacyBenchmark != null && !clinicalPharmacyBenchmark.Sources.Any())
        {
            clinicalPharmacyBenchmark.Sources.Add(new SalaryBenchmarkSource
            {
                Id = Guid.NewGuid().ToString(),
                SourceName = "AmbitionBox — Clinical Pharmacist Salary in India",
                SourceUrl = "https://www.ambitionbox.com/profile/clinical-pharmacist-salary",
                PublishedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                RetrievedAt = DateTime.UtcNow,
                SampleSize = 0,
                SourceType = "SelfReportedSalarySurvey",
                Methodology = "Self-reported salary estimates for entry-level Clinical Pharmacists",
                Notes = "Indicative fresher range; salary submissions vary by location and employer."
            });
            await context.SaveChangesAsync();
        }

        var pharmacySalesBenchmark = await context.SalaryBenchmarks
            .Include(b => b.Sources)
            .FirstOrDefaultAsync(b => b.RoleKey == "pharmacy_sales" && b.Country == "India" && b.ExperienceBand == "0-2");
        if (pharmacySalesBenchmark != null && !pharmacySalesBenchmark.Sources.Any())
        {
            pharmacySalesBenchmark.Sources.Add(new SalaryBenchmarkSource
            {
                Id = Guid.NewGuid().ToString(),
                SourceName = "AmbitionBox — Medical Representative Salary in India",
                SourceUrl = "https://www.ambitionbox.com/profile/medical-representative-salary?page=3",
                PublishedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                RetrievedAt = DateTime.UtcNow,
                SampleSize = 2300,
                SourceType = "SelfReportedSalarySurvey",
                Methodology = "Self-reported fresher Medical Representative salaries",
                Notes = "Published fresher range ₹1.2–4.0 LPA; actual pay varies by employer and location."
            });
            await context.SaveChangesAsync();
        }

        var architectureBenchmarks = await context.SalaryBenchmarks
            .Include(b => b.Sources)
            .Where(b => (b.RoleKey == "architecture_urban_design" || b.RoleKey == "architecture_general") && b.Country == "India" && b.ExperienceBand == "0-2")
            .ToListAsync();
        foreach (var benchmark in architectureBenchmarks.Where(b => !b.Sources.Any()))
        {
            benchmark.Sources.Add(new SalaryBenchmarkSource
            {
                Id = Guid.NewGuid().ToString(), SourceName = "AmbitionBox — Junior Architect and Interior Designer Salary in India",
                SourceUrl = "https://www.ambitionbox.com/profile/junior-architect-and-interior-designer-salary/architecture-and-planning-industry",
                PublishedAt = new DateTime(2025, 7, 9, 0, 0, 0, DateTimeKind.Utc), RetrievedAt = DateTime.UtcNow,
                SampleSize = 10, SourceType = "SelfReportedSalarySurvey", Methodology = "Fresher salary submissions for junior architect and interior designer roles",
                Notes = "Reported fresher range ₹1.8–5.0 LPA (10 submissions); used as an indicative architecture entry-level range."
            });
        }
        if (architectureBenchmarks.Any(b => !b.Sources.Any())) await context.SaveChangesAsync();
    }

    private static string FindSeedsDirectory()
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "..", "SkillScape.Infrastructure", "KnowledgeBase", "V2Seeds"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SkillScape.Infrastructure", "KnowledgeBase", "V2Seeds"),
            Path.Combine(AppContext.BaseDirectory, "KnowledgeBase", "V2Seeds")
        };

        return candidates.FirstOrDefault(Directory.Exists) ?? candidates[0];
    }
}
