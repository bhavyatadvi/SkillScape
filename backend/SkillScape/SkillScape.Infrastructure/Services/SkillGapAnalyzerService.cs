using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Core skill gap analyzer service.
/// Deterministically computes readiness score from career profile + career intelligence data.
/// AI is used only for narrative recommendations — never for the score.
/// </summary>
public class SkillGapAnalyzerService : ISkillGapAnalyzerService
{
    private readonly ICareerProfileService _profileService;
    private readonly CareerIntelligenceService _intelligence;
    private readonly GroqService _groqService;
    private readonly ILogger<SkillGapAnalyzerService> _logger;

    // Proficiency string → numeric level (0–100)
    private static readonly Dictionary<string, int> ProficiencyMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["beginner"]     = 30,
        ["basic"]        = 30,
        ["novice"]       = 25,
        ["intermediate"] = 55,
        ["proficient"]   = 65,
        ["advanced"]     = 75,
        ["expert"]       = 90,
        ["master"]       = 95,
    };

    // Importance weights for readiness score calculation
    private static readonly Dictionary<string, double> ImportanceWeights = new(StringComparer.OrdinalIgnoreCase)
    {
        ["core"]       = 3.0,
        ["supporting"] = 1.5,
        ["advanced"]   = 1.0,
    };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public SkillGapAnalyzerService(
        ICareerProfileService profileService,
        CareerIntelligenceService intelligence,
        GroqService groqService,
        ILogger<SkillGapAnalyzerService> logger)
    {
        _profileService = profileService;
        _intelligence = intelligence;
        _groqService = groqService;
        _logger = logger;
    }

    // ─── Profile Status ─────────────────────────────────────────────────────

    public async Task<ProfileStatusDto> GetProfileStatusAsync(string userId)
    {
        var profile = await _profileService.GetProfileAsync(userId);
        if (profile == null)
            return new ProfileStatusDto { HasProfile = false, Message = "No career profile found." };

        var hasSkills = profile.CurrentSkills.Count > 0;
        var hasEducation = !string.IsNullOrWhiteSpace(profile.EducationLevel) ||
                           !string.IsNullOrWhiteSpace(profile.Degree) ||
                           !string.IsNullOrWhiteSpace(profile.BroadField);
        var hasExperience = profile.ExperienceYears.HasValue || !string.IsNullOrWhiteSpace(profile.RelevantExperience);

        int pct = 0;
        if (hasEducation) pct += 35;
        if (hasSkills) pct += 35;
        if (hasExperience) pct += 30;

        return new ProfileStatusDto
        {
            HasProfile = true,
            ProfileCompletionPercent = pct,
            CompletenessScore = pct,
            ExtractedSkillsCount = profile.CurrentSkills.Count,
            YearsOfExperience = (int)(profile.ExperienceYears ?? 0),
            Degree = !string.IsNullOrWhiteSpace(profile.Degree) ? profile.Degree : (!string.IsNullOrWhiteSpace(profile.EducationLevel) ? profile.EducationLevel : "Undergrad"),
            BroadField = profile.BroadField ?? string.Empty,
            HasSkills = hasSkills,
            HasEducation = hasEducation,
            HasExperience = hasExperience,
            Message = "Profile found.",
        };
    }

    // ─── Main Analysis ──────────────────────────────────────────────────────

    public async Task<SkillAnalyzerResultDto> AnalyzeAsync(string userId, SkillAnalyzerRequest request)
    {
        // 1. Load career profile from DB (never trust frontend userId)
        var profile = await _profileService.GetProfileAsync(userId);
        if (profile == null)
        {
            return new SkillAnalyzerResultDto
            {
                IsLowDataProfile = true,
                LowDataMessage = "No career profile found. Please create your Career Profile first.",
                ReadinessLabel = "No Profile",
                OverallReadinessScore = 0,
            };
        }

        // 2. Resolve career from intelligence
        CareerDetailDto? career = null;
        if (!string.IsNullOrWhiteSpace(request.CareerId))
            career = await _intelligence.GetCareerByIdAsync(request.CareerId);

        if (career == null && !string.IsNullOrWhiteSpace(request.CustomCareerName))
            career = await _intelligence.FindCareerByNameAsync(request.CustomCareerName);

        if (career == null && !string.IsNullOrWhiteSpace(request.CareerId))
            career = await _intelligence.FindCareerByNameAsync(request.CareerId);

        if (career == null)
        {
            return new SkillAnalyzerResultDto
            {
                IsLowDataProfile = true,
                LowDataMessage = $"Career '{request.CustomCareerName ?? request.CareerId}' not found in career catalog. Please select a supported career.",
                ReadinessLabel = "Unknown Career",
                OverallReadinessScore = 0,
            };
        }

        // 3. Check for minimal profile data
        bool hasSkills = profile.CurrentSkills.Count > 0;
        bool hasEducation = !string.IsNullOrWhiteSpace(profile.EducationLevel) ||
                            !string.IsNullOrWhiteSpace(profile.Degree) ||
                            !string.IsNullOrWhiteSpace(profile.BroadField);

        if (!hasSkills && !hasEducation)
        {
            return new SkillAnalyzerResultDto
            {
                CareerId = career.Id,
                CareerName = career.Name,
                IsLowDataProfile = true,
                LowDataMessage = "Your Career Profile has very little information. Please add your education and skills to get an accurate analysis.",
                ReadinessLabel = "Limited Profile Data",
                OverallReadinessScore = 0,
            };
        }

        // 4. Get all skill requirements for the career
        var requirements = career.SkillRequirements;
        if (requirements.Count == 0)
        {
            return new SkillAnalyzerResultDto
            {
                CareerId = career.Id,
                CareerName = career.Name,
                IsLowDataProfile = true,
                LowDataMessage = "No skill requirements found for this career in the career catalog.",
                ReadinessLabel = "Insufficient Data",
                OverallReadinessScore = 0,
            };
        }

        // 5. Compute current skill levels from profile (evidence-based)
        var currentSkillLevels = ComputeCurrentSkillLevels(profile);

        // 6. Compute skill gaps
        var (strengths, gaps) = ComputeSkillGaps(requirements, currentSkillLevels, profile);

        // 7. Compute overall readiness score (deterministic, weighted)
        int readinessScore = ComputeReadinessScore(requirements, currentSkillLevels);
        string readinessLabel = GetReadinessLabel(readinessScore);

        // 8. Education fit
        var educationFit = ComputeEducationFit(profile, career);

        // 9. Experience fit
        var experienceFit = ComputeExperienceFit(profile, career, request.CurrentLevel);

        // 10. Skills fit label
        string skillsFit = GetSkillsFitLabel(gaps);

        // 11. Learning roadmap (deterministic)
        var roadmap = BuildLearningRoadmap(career, gaps, request.Timeline);

        // 12. AI recommendations (narrative only — does not affect score)
        var recommendations = await GetAIRecommendationsAsync(
            career, profile, gaps, strengths, educationFit, experienceFit, request, readinessScore);
        roadmap = await EnrichRoadmapWithGroqAsync(career, profile, request, roadmap);

        // These breakdown scores must be calculated from the profile and role;
        // do not translate qualitative labels into fixed 75/50 display values.
        int expScore = ComputeExperienceFitScore(profile, career, request.CurrentLevel);
        int eduScore = ComputeEducationFitScore(profile, career, educationFit.label);

        return new SkillAnalyzerResultDto
        {
            CareerId = career.Id,
            CareerName = career.Name,
            TargetCareerTitle = career.Name,
            DomainId = career.DomainId,
            DomainName = career.DomainName,
            SubDomainId = career.SubDomainId,
            SubDomainName = career.SubDomainName,
            Domain = career.Domain,
            TargetLevel = string.IsNullOrWhiteSpace(request.TargetLevel) ? "Mid-Level" : request.TargetLevel,
            TargetWorkType = string.IsNullOrWhiteSpace(request.TargetWorkType) ? "Full-Time" : request.TargetWorkType,
            PreparationType = request.PreparationType,
            CurrentLevel = request.CurrentLevel,
            Timeline = request.Timeline,
            OverallReadinessScore = readinessScore,
            ReadinessLabel = readinessLabel,
            SkillAlignmentScore = readinessScore,
            ExperienceFitScore = expScore,
            EducationFitScore = eduScore,
            ResumeAtsScore = BuildResumeAtsScore(profile, career, requirements, currentSkillLevels, educationFit, experienceFit),
            BaselineSummary = new BaselineSummaryDto
            {
                HasProfile = true,
                CompletenessScore = (hasEducation ? 35 : 0) + (hasSkills ? 35 : 0) + (profile.ExperienceYears.HasValue ? 30 : 0),
                ExtractedSkillsCount = profile.CurrentSkills.Count,
                ProjectsCount = profile.Certifications?.Count ?? 0,
                YearsOfExperience = (int)(profile.ExperienceYears ?? 0),
                Degree = !string.IsNullOrWhiteSpace(profile.Degree) ? profile.Degree : (!string.IsNullOrWhiteSpace(profile.EducationLevel) ? profile.EducationLevel : "Bachelor's"),
                BroadField = profile.BroadField ?? string.Empty
            },
            CareerFit = new CareerFitDto
            {
                EducationFit = educationFit.label,
                EducationFitDetail = educationFit.detail,
                ExperienceFit = experienceFit.label,
                ExperienceFitDetail = experienceFit.detail,
                SkillsFit = skillsFit,
            },
            Strengths = strengths,
            SkillGaps = gaps,
            MatchedSkills = strengths.Select(s => new SkillGapItemDto
            {
                SkillId = s.SkillId,
                SkillName = s.SkillName,
                Category = "Technical",
                IsRequired = true,
                UserProficiencyLevel = Math.Min(5, Math.Max(1, (int)Math.Round(s.CurrentLevel / 20.0))),
                RequiredProficiencyLevel = 4,
                EvidenceSource = s.Evidence
            }).ToList(),
            MissingSkills = gaps.Select(g => new SkillGapItemDto
            {
                SkillId = g.SkillId,
                SkillName = g.SkillName,
                Category = "Technical",
                IsRequired = true,
                UserProficiencyLevel = Math.Min(5, Math.Max(0, (int)Math.Round(g.CurrentLevel / 20.0))),
                RequiredProficiencyLevel = Math.Min(5, Math.Max(1, (int)Math.Round(g.RequiredLevel / 20.0))),
                Gap = g.Gap,
                PriorityRank = g.Priority,
                EvidenceSource = g.Evidence
            }).ToList(),
            Recommendations = recommendations,
            StrategicSummary = recommendations.ReadinessExplanation,
            RecommendedCertifications = career.Certifications ?? new(),
            LearningRoadmap = roadmap,
            SkillGapRoadmap = roadmap,
            IsLowDataProfile = false,
            AnalyzedAt = DateTime.UtcNow,
        };
    }

    // ─── Step 5: Estimate current skill levels from career profile ───────────

    private Dictionary<string, (int Level, string Evidence, double Confidence)> ComputeCurrentSkillLevels(CareerProfileDto profile)
    {
        var result = new Dictionary<string, (int, string, double)>(StringComparer.OrdinalIgnoreCase);

        double experienceYears = profile.ExperienceYears ?? 0;
        bool hasSignificantExperience = experienceYears >= 2;
        bool isStudent = profile.CurrentEmploymentStatus?.Equals("Student", StringComparison.OrdinalIgnoreCase) == true
                      || profile.EducationStatus?.Equals("Student", StringComparison.OrdinalIgnoreCase) == true;

        foreach (var skill in profile.CurrentSkills)
        {
            if (string.IsNullOrWhiteSpace(skill.Name)) continue;

            int baseLevel = ProficiencyMap.TryGetValue(skill.Proficiency ?? "intermediate", out var lv) ? lv : 55;

            // Evidence boost for experience
            double confidence = 0.65;
            string evidence = $"Listed in Career Profile (Proficiency: {skill.Proficiency ?? "Intermediate"})";

            if (hasSignificantExperience)
            {
                baseLevel = Math.Min(100, (int)(baseLevel * 1.10));
                confidence = 0.80;
                evidence += $"; {experienceYears} years of experience";
            }

            if (isStudent)
            {
                baseLevel = (int)(baseLevel * 0.90);
                confidence = 0.55;
                evidence += "; Student (academic context)";
            }

            // Certification boost
            bool hasCert = profile.Certifications?.Any(c =>
                c.Contains(skill.Name, StringComparison.OrdinalIgnoreCase)) == true;
            if (hasCert)
            {
                baseLevel = Math.Min(100, baseLevel + 5);
                confidence = Math.Min(1.0, confidence + 0.10);
                evidence += "; Certification found";
            }

            // Relevant experience text mention
            bool inRelevantExp = !string.IsNullOrWhiteSpace(profile.RelevantExperience) &&
                profile.RelevantExperience.Contains(skill.Name, StringComparison.OrdinalIgnoreCase);
            if (inRelevantExp)
            {
                baseLevel = Math.Min(100, baseLevel + 5);
                confidence = Math.Min(1.0, confidence + 0.05);
                evidence = $"Used in relevant experience: {TruncateWithContext(profile.RelevantExperience, skill.Name)}";
            }

            // Try to also match via alternate names in catalog
            var catalogSkill = _intelligence.FindSkillByNameAsync(skill.Name).Result;
            string key = catalogSkill?.Id ?? skill.Name;

            if (!result.ContainsKey(key) || result[key].Item1 < baseLevel)
            {
                result[key] = (baseLevel, evidence, confidence);
            }

            // Also index by display name for easier lookup
            if (!result.ContainsKey(skill.Name))
                result[skill.Name] = (baseLevel, evidence, confidence);
        }

        return result;
    }

    // ─── Step 6: Compute skill gaps ─────────────────────────────────────────

    private (List<SkillStrengthDto> Strengths, List<SkillGapItemDto> Gaps) ComputeSkillGaps(
        List<CareerSkillRequirementDto> requirements,
        Dictionary<string, (int Level, string Evidence, double Confidence)> currentLevels,
        CareerProfileDto profile)
    {
        var strengths = new List<SkillStrengthDto>();
        var gaps = new List<SkillGapItemDto>();

        foreach (var req in requirements)
        {
            // Try to find user's level for this skill (by id or name)
            (int userLevel, string evidence, double confidence) = FindUserLevel(req, currentLevels);

            int gap = Math.Max(0, req.RequiredLevel - userLevel);

            if (gap == 0 || userLevel >= req.RequiredLevel)
            {
                // Strength — user meets or exceeds requirement
                strengths.Add(new SkillStrengthDto
                {
                    SkillId = req.SkillId,
                    SkillName = req.SkillName,
                    CurrentLevel = userLevel,
                    Importance = req.Importance,
                    Evidence = evidence,
                });
            }
            else
            {
                // Gap
                string priority = ComputePriority(gap, req.Importance);
                gaps.Add(new SkillGapItemDto
                {
                    SkillId = req.SkillId,
                    SkillName = req.SkillName,
                    CurrentLevel = userLevel,
                    RequiredLevel = req.RequiredLevel,
                    Gap = gap,
                    Importance = req.Importance,
                    Priority = priority,
                    Evidence = userLevel == 0 ? "Not found in Career Profile" : evidence,
                    Confidence = confidence,
                });
            }
        }

        // Sort: Core gaps first, then by gap size desc
        gaps = gaps
            .OrderByDescending(g => ImportanceWeights.GetValueOrDefault(g.Importance, 1.0))
            .ThenByDescending(g => g.Gap)
            .ToList();

        // Sort strengths: by level desc
        strengths = strengths.OrderByDescending(s => s.CurrentLevel).ToList();

        return (strengths, gaps);
    }

    private (int Level, string Evidence, double Confidence) FindUserLevel(
        CareerSkillRequirementDto req,
        Dictionary<string, (int Level, string Evidence, double Confidence)> currentLevels)
    {
        // Try skill ID first
        if (currentLevels.TryGetValue(req.SkillId, out var byId))
            return byId;

        // Try skill name
        if (currentLevels.TryGetValue(req.SkillName, out var byName))
            return byName;

        // Try alternate names from catalog
        var catalogSkill = _intelligence.GetSkillByIdAsync(req.SkillId).Result;
        if (catalogSkill != null)
        {
            foreach (var alt in catalogSkill.AlternateNames)
            {
                if (currentLevels.TryGetValue(alt, out var byAlt))
                    return byAlt;
            }
        }

        // Not found — zero level with no evidence
        return (0, "Not found in Career Profile", 0.0);
    }

    // ─── Step 7: Compute readiness score ────────────────────────────────────

    private int ComputeReadinessScore(
        List<CareerSkillRequirementDto> requirements,
        Dictionary<string, (int Level, string Evidence, double Confidence)> currentLevels)
    {
        double weightedSum = 0;
        double totalWeight = 0;

        foreach (var req in requirements)
        {
            double weight = ImportanceWeights.GetValueOrDefault(req.Importance, 1.0);
            (int userLevel, _, _) = FindUserLevel(req, currentLevels);
            double coverage = req.RequiredLevel == 0 ? 1.0 : Math.Min(1.0, (double)userLevel / req.RequiredLevel);

            weightedSum += coverage * weight;
            totalWeight += weight;
        }

        if (totalWeight == 0) return 0;
        return (int)Math.Round((weightedSum / totalWeight) * 100);
    }

    private static string GetReadinessLabel(int score) => score switch
    {
        >= 90 => "Highly Prepared",
        >= 80 => "Job Ready",
        >= 60 => "Partially Ready",
        >= 40 => "Developing",
        _ => "Needs Significant Preparation"
    };

    private static string ComputePriority(int gap, string importance)
    {
        bool isCore = importance.Equals("Core", StringComparison.OrdinalIgnoreCase);
        if (isCore && gap >= 30) return "High";
        if (isCore || gap >= 35) return "High";
        if (gap >= 20) return "Medium";
        return "Low";
    }

    private static string GetSkillsFitLabel(List<SkillGapItemDto> gaps)
    {
        int coreGaps = gaps.Count(g => g.Importance.Equals("Core", StringComparison.OrdinalIgnoreCase));
        if (coreGaps == 0) return "Strong";
        if (coreGaps <= 2) return "Good";
        if (coreGaps <= 4) return "Several Gaps";
        return "Significant Gaps";
    }

    // ─── Step 8: Education Fit ───────────────────────────────────────────────

    private (string label, string detail) ComputeEducationFit(CareerProfileDto profile, CareerDetailDto career)
    {
        var userField = (profile.BroadField ?? string.Empty).ToLower();
        var userDegree = (profile.Degree ?? string.Empty).ToLower();
        var careerDomain = career.Domain.ToLower();
        var reqEducation = career.RequiredEducation;

        // string.Contains("") is always true. Guarding empty values prevents
        // a missing degree/field from being incorrectly labelled an exact match.
        bool exactMatch = reqEducation.Any(re =>
            (!string.IsNullOrWhiteSpace(userDegree) && re.Contains(userDegree, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrWhiteSpace(userField) && re.Contains(userField, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrWhiteSpace(userDegree) && userDegree.Contains(re.Split(' ').LastOrDefault() ?? "", StringComparison.OrdinalIgnoreCase)));

        if (exactMatch)
            return ("Strong Match", $"Your {profile.Degree ?? profile.BroadField ?? "education"} aligns with typical requirements for {career.Name}.");

        // Related field check
        bool relatedField = IsRelatedField(userField, careerDomain);
        if (relatedField)
            return ("Partial Match", $"Your background in {profile.BroadField ?? profile.Degree ?? "your field"} has relevant overlap with {career.Name} requirements.");

        // Entry-level / no experience required
        if (career.ExperienceRequirements.EntryLevel == 0)
            return ("Alternative Path Available", $"While a degree in {string.Join(" or ", reqEducation.Take(2))} is typical, entry-level {career.Name} positions are accessible through portfolio and certifications.");

        return ("Needs Additional Education", $"Typical {career.Name} roles require {string.Join(" or ", reqEducation.Take(2))}. Consider supplementing your education with certifications.");
    }

    private static bool IsRelatedField(string userField, string careerDomain)
    {
        var relatedMap = new Dictionary<string, List<string>>
        {
            ["cs"] = new() { "information technology", "it", "computer", "software", "engineering", "mathematics", "physics" },
            ["data_science"] = new() { "computer", "mathematics", "statistics", "it", "engineering", "physics" },
            ["finance"] = new() { "commerce", "economics", "business", "accounting", "mathematics" },
            ["accounting"] = new() { "commerce", "finance", "business", "economics" },
            ["business"] = new() { "commerce", "management", "economics", "finance" },
            ["management"] = new() { "business", "commerce", "economics", "engineering" },
            ["law"] = new() { "political science", "social sciences", "humanities", "arts" },
            ["healthcare"] = new() { "science", "biology", "chemistry", "nursing", "pharmacy" },
            ["design"] = new() { "arts", "fine arts", "visual communication", "media" },
        };

        if (!relatedMap.TryGetValue(careerDomain, out var related)) return false;
        return related.Any(r => userField.Contains(r, StringComparison.OrdinalIgnoreCase));
    }

    private static int ComputeEducationFitScore(CareerProfileDto profile, CareerDetailDto career, string fitLabel)
    {
        var hasEducation = !string.IsNullOrWhiteSpace(profile.Degree)
            || !string.IsNullOrWhiteSpace(profile.BroadField)
            || !string.IsNullOrWhiteSpace(profile.EducationLevel);
        if (!hasEducation) return 0;

        if (fitLabel.StartsWith("Strong", StringComparison.OrdinalIgnoreCase)) return 95;
        if (fitLabel.StartsWith("Partial", StringComparison.OrdinalIgnoreCase)) return 70;
        if (fitLabel.StartsWith("Alternative", StringComparison.OrdinalIgnoreCase)) return 35;
        return 20;
    }

    // ─── Step 9: Experience Fit ──────────────────────────────────────────────

    private (string label, string detail) ComputeExperienceFit(CareerProfileDto profile, CareerDetailDto career, string currentLevel)
    {
        double userYears = profile.ExperienceYears ?? 0;
        int entryRequired = career.ExperienceRequirements.EntryLevel;
        int midRequired = career.ExperienceRequirements.MidLevel;

        bool isStudentOrFresher = currentLevel is "Student" or "Fresher" or "Beginner";

        if (isStudentOrFresher && entryRequired == 0)
            return ("Entry-Level Suitable", $"{career.Name} has entry-level positions available for freshers and students.");

        if (userYears >= midRequired)
            return ("Strong", $"Your {userYears} years of experience meets mid-level {career.Name} requirements.");

        if (userYears >= entryRequired)
            return ("Moderate", $"Your {userYears} years of experience qualifies for entry-level {career.Name} roles.");

        if (entryRequired == 0)
            return ("Entry-Level Suitable", $"No prior experience is required for entry-level {career.Name} positions.");

        return ("Needs More Experience", $"{career.Name} typically requires at least {entryRequired}+ years of experience. Building internships and projects will help.");
    }

    private static int ComputeExperienceFitScore(CareerProfileDto profile, CareerDetailDto career, string currentLevel)
    {
        var years = Math.Max(0, profile.ExperienceYears ?? 0);
        var entryRequired = Math.Max(0, career.ExperienceRequirements.EntryLevel);
        var midRequired = Math.Max(entryRequired + 1, career.ExperienceRequirements.MidLevel);
        var isStudentOrFresher = currentLevel is "Student" or "Fresher" or "Beginner";

        // A zero-year candidate can be eligible for an entry-level opening, but
        // should not display the same readiness as someone with proven experience.
        if (years <= 0)
            return entryRequired == 0 ? (isStudentOrFresher ? 30 : 35) : 0;

        if (years < entryRequired)
            return Math.Clamp((int)Math.Round((years / entryRequired) * 45), 5, 45);

        if (years >= midRequired)
            return 100;

        // From the entry threshold to the mid-level threshold, progress from
        // 60% to 90%, based on actual years rather than a fixed label.
        var range = Math.Max(1, midRequired - entryRequired);
        var progress = (years - entryRequired) / range;
        return Math.Clamp((int)Math.Round(60 + (progress * 30)), 60, 90);
    }

    // ─── Step 10: Learning Roadmap ───────────────────────────────────────────

    private List<LearningPhaseDto> BuildLearningRoadmap(CareerDetailDto career, List<SkillGapItemDto> gaps, string timeline)
    {
        if (!gaps.Any()) return new();

        var phases = new List<LearningPhaseDto>();
        int phaseNum = 1;

        // Phase 1: Core High-Priority gaps
        var phase1Skills = gaps
            .Where(g => g.Importance.Equals("Core", StringComparison.OrdinalIgnoreCase) && g.Priority == "High")
            .Take(3).ToList();
        if (phase1Skills.Any())
        {
            phases.Add(new LearningPhaseDto
            {
                Phase = phaseNum++,
                Title = $"{career.Name} Foundation",
                Description = $"Close the highest-priority {career.Name} gaps first: {string.Join(", ", phase1Skills.Select(s => s.SkillName))}.",
                Skills = phase1Skills.Select(s => s.SkillName).ToList(),
                TargetSkills = phase1Skills.Select(s => s.SkillName).ToList(),
                ActionItems = BuildActionItems(career, phase1Skills, "foundation"),
                EstimatedTime = EstimateTime(phase1Skills, timeline),
                EstimatedDuration = EstimateTime(phase1Skills, timeline),
                Priority = "High",
            });
        }

        // Phase 2: Remaining Core gaps
        var phase2Skills = gaps
            .Where(g => g.Importance.Equals("Core", StringComparison.OrdinalIgnoreCase) && g.Priority != "High")
            .Take(3).ToList();
        if (phase2Skills.Any())
        {
            phases.Add(new LearningPhaseDto
            {
                Phase = phaseNum++,
                Title = $"Build {career.Name} Core Competence",
                Description = $"Strengthen the remaining core requirements for {career.Name}: {string.Join(", ", phase2Skills.Select(s => s.SkillName))}.",
                Skills = phase2Skills.Select(s => s.SkillName).ToList(),
                TargetSkills = phase2Skills.Select(s => s.SkillName).ToList(),
                ActionItems = BuildActionItems(career, phase2Skills, "core practice"),
                EstimatedTime = EstimateTime(phase2Skills, timeline),
                EstimatedDuration = EstimateTime(phase2Skills, timeline),
                Priority = "High",
            });
        }

        // Phase 3: Supporting skills
        var phase3Skills = gaps
            .Where(g => g.Importance.Equals("Supporting", StringComparison.OrdinalIgnoreCase))
            .Take(4).ToList();
        if (phase3Skills.Any())
        {
            phases.Add(new LearningPhaseDto
            {
                Phase = phaseNum++,
                Title = $"Supporting Skills for {career.Name}",
                Description = $"Add the supporting capabilities that make your {career.Name} profile complete: {string.Join(", ", phase3Skills.Select(s => s.SkillName))}.",
                Skills = phase3Skills.Select(s => s.SkillName).ToList(),
                TargetSkills = phase3Skills.Select(s => s.SkillName).ToList(),
                ActionItems = BuildActionItems(career, phase3Skills, "applied practice"),
                EstimatedTime = EstimateTime(phase3Skills, timeline),
                EstimatedDuration = EstimateTime(phase3Skills, timeline),
                Priority = "Medium",
            });
        }

        // Phase 4: Advanced / project phase
        var phase4Skills = gaps
            .Where(g => g.Importance.Equals("Advanced", StringComparison.OrdinalIgnoreCase))
            .Take(3).ToList();
        if (phase4Skills.Any())
        {
            phases.Add(new LearningPhaseDto
            {
                Phase = phaseNum++,
                Title = $"{career.Name} Specialization",
                Description = $"Differentiate your profile with advanced capabilities in {string.Join(", ", phase4Skills.Select(s => s.SkillName))}.",
                Skills = phase4Skills.Select(s => s.SkillName).ToList(),
                TargetSkills = phase4Skills.Select(s => s.SkillName).ToList(),
                ActionItems = BuildActionItems(career, phase4Skills, "specialization"),
                EstimatedTime = EstimateTime(phase4Skills, timeline),
                EstimatedDuration = EstimateTime(phase4Skills, timeline),
                Priority = "Low",
            });
        }

        // Final phase: Real-world application
        if (phases.Count > 0)
        {
            phases.Add(new LearningPhaseDto
            {
                Phase = phaseNum,
                Title = $"Demonstrate {career.Name} Readiness",
                Description = $"Turn your new skills into credible evidence for {career.Name} applications through role-relevant work samples, supervised practice, internships, or certifications.",
                Skills = new() { "Portfolio Projects", "Certifications", "Internship / Work Experience" },
                TargetSkills = gaps.Take(3).Select(s => s.SkillName).ToList(),
                ActionItems = new()
                {
                    $"Create one {career.Name}-relevant work sample demonstrating {string.Join(", ", gaps.Take(2).Select(s => s.SkillName))}.",
                    $"Tailor your resume evidence to the {career.Name} requirements and measurable outcomes.",
                    "Seek feedback from a mentor, recruiter, or domain professional before applying."
                },
                EstimatedTime = "Ongoing",
                EstimatedDuration = "Ongoing",
                Priority = "Medium",
            });
        }

        return phases;
    }

    private static string EstimateTime(List<SkillGapItemDto> skills, string timeline)
    {
        int avgGap = skills.Any() ? (int)skills.Average(s => s.Gap) : 30;
        if (avgGap >= 50) return "6–10 weeks";
        if (avgGap >= 30) return "3–6 weeks";
        return "1–3 weeks";
    }

    private static List<string> BuildActionItems(CareerDetailDto career, List<SkillGapItemDto> skills, string phase)
    {
        return skills.Take(3).Select(skill =>
            $"Complete a {phase} exercise for {skill.SkillName} relevant to {career.Name}, then record the outcome as portfolio or resume evidence.")
            .ToList();
    }

    /// <summary>
    /// Enriches the deterministic roadmap structure with concise, role-specific
    /// learning subtopics. Groq can improve the instructional detail but never
    /// changes the phase order, target skills, duration, or readiness score.
    /// </summary>
    private async Task<List<LearningPhaseDto>> EnrichRoadmapWithGroqAsync(
        CareerDetailDto career,
        CareerProfileDto profile,
        SkillAnalyzerRequest request,
        List<LearningPhaseDto> roadmap)
    {
        if (!roadmap.Any()) return roadmap;

        try
        {
            const string systemPrompt = @"You design precise, job-relevant learning roadmaps. Return ONLY valid JSON:
{
  ""phases"": [
    { ""phase"": 1, ""subtopics"": [""topic 1"", ""topic 2"", ""topic 3""] }
  ]
}
Return one entry for every supplied phase and exactly three concise subtopics per entry. Each subtopic must name a concrete concept, technique, workflow, or work artifact relevant to the target role and that phase's target skills. Do not repeat the phase title or merely restate a skill name. Do not invent certifications, employers, regulations, scores, or durations.";

            var prompt = new StringBuilder();
            prompt.AppendLine($"Target role: {career.Name} ({career.Domain})");
            prompt.AppendLine($"Learner level: {request.CurrentLevel}; timeline: {request.Timeline}");
            prompt.AppendLine($"Education: {JoinNonEmpty(profile.EducationLevel, profile.Degree, profile.BroadField, profile.Specialization)}");
            prompt.AppendLine($"Experience: {profile.ExperienceYears?.ToString() ?? "0"} years; {ValueOrNotProvided(profile.RelevantExperience)}");
            prompt.AppendLine("Roadmap phases:");
            foreach (var phase in roadmap)
                prompt.AppendLine($"  - Phase {phase.Phase}: {phase.Title}. Target skills: {string.Join(", ", phase.TargetSkills)}. Purpose: {phase.Description}");

            var rawJson = await _groqService.GetSkillAnalyzerResponseAsync(systemPrompt, prompt.ToString());
            using var document = JsonDocument.Parse(CleanJson(rawJson));
            if (!document.RootElement.TryGetProperty("phases", out var phases) || phases.ValueKind != JsonValueKind.Array)
                return roadmap;

            var subtopicsByPhase = phases.EnumerateArray()
                .Where(item => item.TryGetProperty("phase", out var phase) && phase.TryGetInt32(out _))
                .ToDictionary(
                    item => item.GetProperty("phase").GetInt32(),
                    item => item.TryGetProperty("subtopics", out var topics) && topics.ValueKind == JsonValueKind.Array
                        ? topics.EnumerateArray().Select(topic => topic.GetString()?.Trim() ?? string.Empty).Where(topic => topic.Length > 0).Take(3).ToList()
                        : new List<string>());

            foreach (var phase in roadmap)
                if (subtopicsByPhase.TryGetValue(phase.Phase, out var subtopics) && subtopics.Any())
                    phase.Subtopics = subtopics;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Groq roadmap subtopic generation failed. Showing the deterministic roadmap.");
        }

        return roadmap;
    }

    private static ResumeAtsScoreDto? BuildResumeAtsScore(
        CareerProfileDto profile,
        CareerDetailDto career,
        List<CareerSkillRequirementDto> requirements,
        Dictionary<string, (int Level, string Evidence, double Confidence)> currentSkillLevels,
        (string label, string detail) educationFit,
        (string label, string detail) experienceFit)
    {
        if (!profile.ProfileSource.Equals("resume", StringComparison.OrdinalIgnoreCase)) return null;

        var matched = requirements.Where(requirement =>
            currentSkillLevels.TryGetValue(requirement.SkillId, out var level) && level.Level > 0)
            .Select(requirement => requirement.SkillName).Distinct().ToList();
        var missing = requirements.Where(requirement => !matched.Contains(requirement.SkillName))
            .Select(requirement => requirement.SkillName).Take(8).ToList();

        var keywordScore = requirements.Count == 0 ? 0 : (int)Math.Round(60.0 * matched.Count / requirements.Count);
        var educationScore = educationFit.label.StartsWith("Strong", StringComparison.OrdinalIgnoreCase) ? 20
            : educationFit.label.StartsWith("Partial", StringComparison.OrdinalIgnoreCase) ? 14 : 8;
        var experienceScore = experienceFit.label.StartsWith("Strong", StringComparison.OrdinalIgnoreCase) ? 10
            : experienceFit.label.StartsWith("Moderate", StringComparison.OrdinalIgnoreCase) || experienceFit.label.StartsWith("Entry", StringComparison.OrdinalIgnoreCase) ? 7 : 3;
        var completenessScore = Math.Min(10, (profile.CurrentSkills.Count >= 8 ? 5 : profile.CurrentSkills.Count * 5 / 8)
            + (!string.IsNullOrWhiteSpace(profile.RelevantExperience) ? 3 : 0)
            + (profile.Certifications.Any() ? 2 : 0));
        var score = Math.Min(100, keywordScore + educationScore + experienceScore + completenessScore);

        return new ResumeAtsScoreDto
        {
            Score = score,
            Label = score >= 75 ? "Strong ATS Match" : score >= 55 ? "Competitive ATS Match" : "Needs Keyword Targeting",
            ResumeFileName = profile.ResumeFileName,
            Summary = $"Resume-to-{career.Name} match estimated from {matched.Count} matched role keywords, education, experience, and profile completeness.",
            MatchedKeywords = matched.Take(10).ToList(),
            MissingKeywords = missing,
        };
    }

    // ─── Step 12: AI Recommendations (narrative only) ───────────────────────

    private async Task<AIRecommendationDto> GetAIRecommendationsAsync(
        CareerDetailDto career,
        CareerProfileDto profile,
        List<SkillGapItemDto> gaps,
        List<SkillStrengthDto> strengths,
        (string label, string detail) educationFit,
        (string label, string detail) experienceFit,
        SkillAnalyzerRequest request,
        int readinessScore)
    {
        try
        {
            var systemPrompt = @"You are a career advisor AI. You receive structured career readiness analysis data and must provide personalized, actionable recommendations.
Return ONLY valid JSON with this exact structure:
{
  ""readinessExplanation"": ""A concise readiness assessment of no more than 60 words"",
  ""whatToLearnFirst"": [""skill1"", ""skill2"", ""skill3""],
  ""skillAdvice"": [
    { ""skill"": ""SkillName"", ""whyItMatters"": ""brief reason"", ""howToLearn"": ""concrete learning suggestion"" }
  ],
  ""studyTopics"": [""topic1"", ""topic2"", ""topic3"", ""topic4""],
  ""projectSuggestions"": [""project idea 1"", ""project idea 2""],
  ""careerAdvice"": [""advice point 1"", ""advice point 2"", ""advice point 3""]
}
Return exactly 3 short items in whatToLearnFirst, exactly 3 skillAdvice objects, exactly 4 studyTopics, exactly 2 projectSuggestions, and exactly 3 careerAdvice items. Keep whyItMatters under 25 words and howToLearn under 45 words. Be specific, practical, and encouraging. Ground every recommendation in the supplied learner profile/resume evidence and target role; acknowledge relevant skills, education, experience, or constraints when present. Translate broad catalog labels into the target role's actual technical work—for example, for a geotechnical role use soil investigation, foundation design, ground improvement, or slope stability where relevant. Focus on the top skill gaps. For every skillAdvice item, name a concrete role-relevant activity or work artifact the learner can complete; never use vague phrases such as 'take online courses', 'watch tutorials', or 'practice through projects'. Explain the business or delivery decision that the skill supports in this target role. Make careerAdvice a sequenced next-step plan, not generic motivational advice. Do not invent scores, credentials, employers, regulations, or numbers.";

            // Send enough context for recommendations to cover the complete role
            // profile. The deterministic matrix returned to the UI is never capped.
            var topGaps = gaps.Take(12).ToList();
            var topStrengths = strengths.Take(5).ToList();

            var prompt = new StringBuilder();
            prompt.AppendLine($"Target Career: {career.Name} ({career.Domain})");
            prompt.AppendLine($"Overall Readiness Score: {readinessScore}%");
            prompt.AppendLine($"Preparation Goal: {request.PreparationType}");
            prompt.AppendLine($"Current Level: {request.CurrentLevel}");
            prompt.AppendLine($"Timeline: {request.Timeline}");
            prompt.AppendLine($"Education Fit: {educationFit.label}");
            prompt.AppendLine($"Experience Fit: {experienceFit.label}");
            prompt.AppendLine();
            prompt.AppendLine("LEARNER PROFILE / RESUME EVIDENCE:");
            prompt.AppendLine($"  Education: {JoinNonEmpty(profile.EducationLevel, profile.Degree, profile.BroadField, profile.Specialization)}");
            prompt.AppendLine($"  Current status: {JoinNonEmpty(profile.CurrentEmploymentStatus, profile.CurrentOccupation, profile.CurrentRole, profile.CurrentIndustry)}");
            prompt.AppendLine($"  Experience: {profile.ExperienceYears?.ToString() ?? "0"} years. Relevant evidence: {ValueOrNotProvided(profile.RelevantExperience)}");
            prompt.AppendLine($"  Previous roles: {ValueOrNotProvided(profile.PreviousRoles)}");
            prompt.AppendLine($"  Resume source: {ValueOrNotProvided(profile.ProfileSource)}; resume attached: {(string.IsNullOrWhiteSpace(profile.ResumeFileName) ? "no" : "yes")}");
            prompt.AppendLine($"  Extracted/self-reported skills: {FormatSkills(profile.CurrentSkills)}");
            prompt.AppendLine($"  Certifications: {FormatList(profile.Certifications)}");
            prompt.AppendLine($"  Career goal: {ValueOrNotProvided(profile.CareerGoals)}");
            prompt.AppendLine($"  Preferences: likes {FormatList(profile.CareerLikes)}; work mode {ValueOrNotProvided(profile.PreferredWorkMode)}; constraints {FormatList(profile.Constraints)}");
            prompt.AppendLine();
            prompt.AppendLine("TOP SKILL GAPS:");
            foreach (var g in topGaps)
                prompt.AppendLine($"  - {g.SkillName}: Current={g.CurrentLevel}%, Required={g.RequiredLevel}%, Gap={g.Gap}%, Importance={g.Importance}");
            prompt.AppendLine();
            prompt.AppendLine("CURRENT STRENGTHS:");
            foreach (var s in topStrengths)
                prompt.AppendLine($"  - {s.SkillName}: {s.CurrentLevel}%");
            if (request.InterestAreas.Any())
                prompt.AppendLine($"\nInterest Areas: {string.Join(", ", request.InterestAreas)}");

            // The action plan is generated by Groq from the learner's profile,
            // target role, priorities, gaps, and strengths. The readiness score
            // itself remains deterministic and is never AI-generated.
            var rawJson = await _groqService.GetSkillAnalyzerResponseAsync(systemPrompt, prompt.ToString());

            // Parse AI response
            rawJson = CleanJson(rawJson);
            var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            return new AIRecommendationDto
            {
                ReadinessExplanation = GetString(root, "readinessExplanation"),
                WhatToLearnFirst = GetStringList(root, "whatToLearnFirst"),
                SkillAdvice = GetSkillAdvice(root),
                StudyTopics = GetStringList(root, "studyTopics"),
                ProjectSuggestions = GetStringList(root, "projectSuggestions"),
                CareerAdvice = GetStringList(root, "careerAdvice"),
                IsAIGenerated = true,
                AIFailed = false,
                AIProvider = "Groq",
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI recommendation generation failed. Using fallback.");
            return BuildFallbackRecommendations(career, gaps, strengths, readinessScore);
        }
    }

    private AIRecommendationDto BuildFallbackRecommendations(
        CareerDetailDto career, List<SkillGapItemDto> gaps, List<SkillStrengthDto> strengths, int score)
    {
        var topGaps = gaps.Take(3).Select(g => g.SkillName).ToList();
        return new AIRecommendationDto
        {
            ReadinessExplanation = BuildFallbackReadinessSummary(career, gaps, strengths, score),
            WhatToLearnFirst = topGaps,
            SkillAdvice = gaps.Take(3).Select(g => new SkillAdviceDto
            {
                Skill = g.SkillName,
                WhyItMatters = GetFallbackWhyItMatters(career, g.SkillName, g.Importance),
                HowToLearn = GetFocusedLearningAction(career, g.SkillName),
            }).ToList(),
            StudyTopics = gaps.Take(4).Select(g => g.SkillName).ToList(),
            ProjectSuggestions = new() { BuildPortfolioProject(career, topGaps), $"Create a one-page {career.Name} case study that explains your assumptions, work process, outcomes, and the skills you applied." },
            CareerAdvice = new() { $"Start with {topGaps.FirstOrDefault() ?? "the highest-priority gap"} and complete one role-relevant work sample before moving to the next skill.", "Use the same work sample to demonstrate at least two priority skills, then capture your method and outcome in a portfolio case study.", "After completing the first learning phase, retake this analysis to measure progress against the {career.Name} requirements." },
            IsAIGenerated = false,
            AIFailed = true,
            AIProvider = "",
            AIFailureMessage = "AI recommendations temporarily unavailable. Showing basic guidance.",
        };
    }

    private static string BuildFallbackReadinessSummary(CareerDetailDto career, List<SkillGapItemDto> gaps, List<SkillStrengthDto> strengths, int score)
    {
        var prioritySkill = gaps.FirstOrDefault()?.SkillName;
        var strength = strengths.FirstOrDefault()?.SkillName;
        var firstStep = string.IsNullOrWhiteSpace(prioritySkill) ? "the remaining role requirements" : prioritySkill;
        var strengthNote = string.IsNullOrWhiteSpace(strength) ? "" : $" Your existing {strength} evidence is a useful base to build on.";
        return $"You are currently {GetReadinessLabel(score)} for the {career.Name} role at {score}% readiness. Prioritize {firstStep} first because it is the largest immediate gap for this role, then build evidence through a role-relevant work sample.{strengthNote}";
    }

    private static string GetFocusedLearningAction(CareerDetailDto career, string skill)
    {
        var normalized = skill.Trim().ToLowerInvariant();
        var role = career.Name.ToLowerInvariant();
        var isMachineLearning = role.Contains("machine learning") || role.Contains("ai engineer") || role.Contains("nlp") || role.Contains("computer vision");

        if (isMachineLearning && (normalized.Contains("python") || normalized.Contains("pandas") || normalized.Contains("sql")))
            return $"Build a reproducible training-data notebook for a small {career.Name} use case: load the data, clean it, document leakage checks, and save the SQL/query or transformation steps.";
        if (isMachineLearning && (normalized.Contains("machine learning") || normalized.Contains("statistics") || normalized.Contains("model evaluation") || normalized.Contains("feature engineering")))
            return $"Train and compare two baseline models for one {career.Name} use case; report the evaluation metric, error slices, feature decisions, and the reason for selecting one model.";
        if (isMachineLearning && (normalized.Contains("deployment") || normalized.Contains("mlops") || normalized.Contains("cloud")))
            return $"Package one small model behind a prediction endpoint, add an input-validation check and prediction log, then document how you would monitor drift or failed requests.";
        if (normalized.Contains("engineering mathematics"))
            return $"Work through a small {career.Name} calculation pack: define assumptions, solve quantities or loads, check units, and present the result in a one-page calculation sheet.";
        if (normalized.Contains("problem solving"))
            return $"Analyse three realistic {career.Name} constraints, document the root cause, compare two options, and justify the recommended action using cost, safety, quality, or schedule impact.";
        if (normalized.Contains("project management"))
            return $"Create a work-breakdown structure, two-week schedule, risk log, and progress update for a small {career.Name} scenario; use these as a single portfolio work sample.";
        if (normalized.Contains("communication"))
            return $"Write a concise stakeholder update for a {career.Name} scenario that states the issue, evidence, decision required, owner, and next deadline.";
        if (normalized.Contains("data analysis"))
            return $"Use a small role-relevant dataset to calculate key measures, identify one insight, and present the conclusion with a clear chart or decision note.";
        if (normalized.Contains("design"))
            return $"Produce one annotated {career.Name} design or workflow artefact, explain your constraints and choices, then request feedback against the role requirements.";
        return $"Complete one focused {skill} exercise, then apply it in a short {career.Name} work sample. Document the objective, method, assumptions, and outcome so it can become portfolio evidence.";
    }

    private static string GetFallbackWhyItMatters(CareerDetailDto career, string skill, string importance)
    {
        var role = career.Name.ToLowerInvariant();
        var normalizedSkill = skill.ToLowerInvariant();

        if (role.Contains("machine learning"))
        {
            if (normalizedSkill.Contains("python")) return "Python is the working language for training pipelines, experiments, and production inference in this role.";
            if (normalizedSkill.Contains("machine learning")) return "You need to choose, train, and tune suitable models rather than only consume pre-built predictions.";
            if (normalizedSkill.Contains("statistics")) return "Statistics helps you validate that an apparent model improvement is real, useful, and not caused by noisy data.";
            if (normalizedSkill.Contains("pandas") || normalizedSkill.Contains("data preparation")) return "Reliable data preparation prevents leakage, inconsistent features, and training-serving mismatches.";
            if (normalizedSkill.Contains("evaluation")) return "Evaluation and error analysis determine whether a model is safe and useful enough to deploy.";
            if (normalizedSkill.Contains("feature")) return "Feature engineering connects raw business data to model signals that can improve predictive performance.";
            if (normalizedSkill.Contains("deployment")) return "An ML engineer is responsible for turning a model into a monitored service, not just a notebook result.";
            if (normalizedSkill.Contains("sql")) return "SQL is used to inspect, join, and validate the training data that powers model experiments.";
        }

        return $"{skill} supports the day-to-day decisions and deliverables expected of a {career.Name}; it is marked {importance.ToLowerInvariant()} for this target role.";
    }

    private static string BuildPortfolioProject(CareerDetailDto career, List<string> topGaps)
    {
        var skills = topGaps.Count > 0 ? string.Join(", ", topGaps.Take(3)) : "the priority skills";
        return $"Build a realistic {career.Name} work sample that combines {skills}; include the brief, your process, decisions, and a review checklist.";
    }

    private static string JoinNonEmpty(params string?[] values)
    {
        var available = values.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        return available.Count > 0 ? string.Join(" | ", available) : "Not provided";
    }

    private static string ValueOrNotProvided(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Not provided" : value;

    private static string FormatList(IEnumerable<string>? values)
    {
        var available = values?.Where(value => !string.IsNullOrWhiteSpace(value)).Take(12).ToList() ?? new List<string>();
        return available.Count > 0 ? string.Join(", ", available) : "None provided";
    }

    private static string FormatSkills(IEnumerable<CareerSkillDto>? skills)
    {
        var available = skills?.Where(skill => !string.IsNullOrWhiteSpace(skill.Name)).Take(20)
            .Select(skill => $"{skill.Name} ({skill.Proficiency})").ToList() ?? new List<string>();
        return available.Count > 0 ? string.Join(", ", available) : "None provided";
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static string CleanJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "{}";
        var match = Regex.Match(raw.Trim(), @"```(?:json)?\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : raw.Trim();
    }

    private static string GetString(JsonElement root, string prop)
    {
        try { return root.TryGetProperty(prop, out var el) ? el.GetString() ?? "" : ""; }
        catch { return ""; }
    }

    private static List<string> GetStringList(JsonElement root, string prop)
    {
        try
        {
            if (!root.TryGetProperty(prop, out var el) || el.ValueKind != JsonValueKind.Array) return new();
            return el.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList();
        }
        catch { return new(); }
    }

    private static List<SkillAdviceDto> GetSkillAdvice(JsonElement root)
    {
        try
        {
            if (!root.TryGetProperty("skillAdvice", out var el) || el.ValueKind != JsonValueKind.Array) return new();
            return el.EnumerateArray().Select(e => new SkillAdviceDto
            {
                Skill = GetString(e, "skill"),
                WhyItMatters = GetString(e, "whyItMatters"),
                HowToLearn = GetString(e, "howToLearn"),
            }).ToList();
        }
        catch { return new(); }
    }

    private static string TruncateWithContext(string text, string keyword)
    {
        var idx = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return text.Length > 80 ? text[..80] + "..." : text;
        var start = Math.Max(0, idx - 20);
        var end = Math.Min(text.Length, idx + keyword.Length + 40);
        var excerpt = text[start..end];
        return (start > 0 ? "..." : "") + excerpt + (end < text.Length ? "..." : "");
    }
}
