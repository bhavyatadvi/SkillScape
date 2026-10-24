using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;
using System.Text.Json;


namespace SkillScape.API.Data;

public static class DatabaseSeeder
{
    /// <summary>
    /// Adds clearly marked sample quiz results for up to three students who have
    /// no saved result in either quiz engine. This is additive and idempotent; it
    /// never replaces or edits an existing student's quiz history.
    /// </summary>
    public static async Task SeedAdminAnalyticsSampleResultsAsync(ApplicationDbContext context)
    {
        if (await context.QuizResults.AnyAsync(result => result.RecommendationReason.StartsWith("ADMIN_ANALYTICS_SAMPLE:")))
            return;

        var students = await context.Users
            .Where(user => user.Role == "Student")
            .OrderBy(user => user.CreatedAt)
            .ToListAsync();
        var domains = await context.CareerDomains
            .Where(domain => domain.IsActive)
            .OrderBy(domain => domain.DisplayOrder)
            .Take(3)
            .ToListAsync();

        if (students.Count == 0 || domains.Count == 0)
            return;

        var legacyAttemptUserIds = await context.QuizResults
            .Select(result => result.UserId)
            .Distinct()
            .ToListAsync();
        var adaptiveAttemptUserIds = await context.QuizResultsV2
            .Select(result => result.UserId)
            .Distinct()
            .ToListAsync();
        var attemptedUserIds = legacyAttemptUserIds.Concat(adaptiveAttemptUserIds).ToHashSet();
        var studentsNeedingSample = students
            .Where(student => !attemptedUserIds.Contains(student.Id))
            .Take(3)
            .ToList();
        if (studentsNeedingSample.Count == 0)
            return;

        var now = DateTime.UtcNow;
        var sampleScores = new[] { 78.0, 84.0, 71.0 };
        var results = studentsNeedingSample.Select((student, index) => new QuizResult
        {
            Id = $"admin-analytics-sample-{student.Id}",
            UserId = student.Id,
            RecommendedDomainId = domains[index % domains.Count].Id,
            ScoresJson = JsonSerializer.Serialize(new Dictionary<string, double>
            {
                [domains[index % domains.Count].Name] = sampleScores[index]
            }),
            RecommendationReason = "ADMIN_ANALYTICS_SAMPLE: Sample result added for dashboard preview; this is not a student's submitted assessment.",
            CompletedAt = now.AddDays(-index)
        });

        context.QuizResults.AddRange(results);
        await context.SaveChangesAsync();
    }

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed Career Map data only when the table is empty. The seed method
        // rebuilds its own dataset, so never run it on every application start
        // or existing user career data could be removed.
        if (!context.CareerPaths.Any())
        {
            await SeedCareerPathsAsync(context);
        }
        await SeedHierarchicalQuizQuestionsAsync(context);

        // Seed Placement Readiness Module data (idempotent)
        await SeedPlacementDataAsync(context);

        // Seed Upskilling Data
        await UpskillingSeeder.SeedAsync(context);

        // Learning Circles are user-created content. Deactivate legacy rows
        // whose creator is not an account in this database; never create groups
        // under fabricated or missing creator identities during startup.
        await DeactivateOrphanedLearningCirclesAsync(context);

        // Refresh career salary data with domain-accurate benchmarks
        await EnsureRealisticCareerSalariesAsync(context);
        
        // Populate authentic education, duration and degree mapping data (Phase 6B)
        await EnsureRealisticEducationDataAsync(context);

        // Always ensure passwords are set for test users
        var testUsers = context.Users.Where(u => u.Email == "student@example.com" || u.Email == "admin@example.com").ToList();
        bool passwordUpdated = false;
        foreach (var user in testUsers)
        {
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                user.PasswordHash = SkillScape.Infrastructure.Services.AuthService.HashPassword("Password123!");
                passwordUpdated = true;
            }
        }
        if (passwordUpdated)
        {
            await context.SaveChangesAsync();
        }

        // Skip if data already exists
        if (context.CareerDomains.Any())
        {
            return;
        }

        // Seed Career Domains
        var domains = SeedDomains(context);
        context.CareerDomains.AddRange(domains);
        await context.SaveChangesAsync();

        // Seed Skills
        var skills = SeedSkills(context, domains);
        context.Skills.AddRange(skills);
        await context.SaveChangesAsync();

        // Seed Quiz Questions and Options
        var (questions, options) = SeedQuiz(context, domains);
        context.QuizQuestions.AddRange(questions);
        context.QuizOptions.AddRange(options);
        await context.SaveChangesAsync();

        // Seed Badges
        var badges = SeedBadges();
        context.Badges.AddRange(badges);
        await context.SaveChangesAsync();

        // Seed Roadmap Steps
        var roadmapSteps = SeedRoadmapSteps(context, domains, skills);
        context.RoadmapSteps.AddRange(roadmapSteps);
        await context.SaveChangesAsync();

        // Seed Sample Users
        var users = SeedUsers();
        context.Users.AddRange(users);
        await context.SaveChangesAsync();

    }

    public static async Task EnsureRealisticEducationDataAsync(ApplicationDbContext context)
    {
        var allTaxonomy = await context.CareerTaxonomyV2.ToListAsync();
        bool updated = false;

        var degreeMap = allTaxonomy.Where(t => t.NodeType == "degree").ToList();
        foreach (var deg in degreeMap)
        {
            if (string.IsNullOrEmpty(deg.TimeToCompleteQualification))
            {
                string name = deg.Name.ToLowerInvariant();
                if (name.Contains("mbbs")) deg.TimeToCompleteQualification = "5 years";
                else if (name.Contains("b.arch") || name.Contains("bachelor of architecture") || name.Contains("ba llb") || name.Contains("b.a. ll.b")) deg.TimeToCompleteQualification = "5 years";
                else if (name.Contains("b.tech") || name.Contains("b.e") || name.Contains("bdes") || name.Contains("b.des") || name.Contains("pharm")) deg.TimeToCompleteQualification = "4 years";
                else if (name.Contains("b.sc") || name.Contains("b.com") || name.Contains("b.a") || name.Contains("bca") || name.Contains("bba")) deg.TimeToCompleteQualification = "3 years";
                else deg.TimeToCompleteQualification = "3 years";
                
                updated = true;
            }
        }

        var careers = allTaxonomy.Where(t => t.NodeType == "career" && t.Level >= 2).ToList();
        foreach (var career in careers)
        {
            if (string.IsNullOrEmpty(career.RequiredDegreesJson) || career.RequiredDegreesJson == "[]")
            {
                var id = career.Id;
                string requiredDegKey = "";
                
                if (id.StartsWith("sci.eng.cs") || id.StartsWith("tech.cs")) requiredDegKey = "deg.btech.cs";
                else if (id.StartsWith("sci.eng.civil")) requiredDegKey = "deg.btech.civil";
                else if (id.StartsWith("sci.eng.mech")) requiredDegKey = "deg.btech.mech";
                else if (id.StartsWith("sci.eng") || id.StartsWith("tech")) requiredDegKey = "deg.btech.general";
                else if (id.StartsWith("sci.med.mbbs") || id.StartsWith("sci.med.surg") || id.StartsWith("sci.med") || id.StartsWith("med.")) requiredDegKey = "deg.mbbs";
                else if (id.StartsWith("com.fin.ca") || id.StartsWith("com.fin") || id.StartsWith("com.bus.bba")) requiredDegKey = "deg.bcom.general";
                else if (id.StartsWith("art.hum.psych")) requiredDegKey = "deg.ba.psych";
                else if (id.StartsWith("art.hum.soc")) requiredDegKey = "deg.ba.polsci";
                else if (id.StartsWith("art.des")) requiredDegKey = "deg.bdes.general";
                else requiredDegKey = "deg.bcom.general"; // fallback
                
                if (!string.IsNullOrEmpty(requiredDegKey) && degreeMap.Any(d => d.Id == requiredDegKey))
                {
                    career.RequiredDegreesJson = $"[\"{requiredDegKey}\"]";
                    updated = true;
                }
            }
        }

        if (updated)
        {
            await context.SaveChangesAsync();
        }
    }

    public static async Task DeactivateOrphanedLearningCirclesAsync(ApplicationDbContext context)
    {
        var circles = await context.LearningCircles.ToListAsync();
        var creatorIds = circles
            .Where(circle => !string.IsNullOrWhiteSpace(circle.CreatorId))
            .Select(circle => circle.CreatorId)
            .Distinct()
            .ToList();
        var registeredUserIds = await context.Users
            .Where(user => creatorIds.Contains(user.Id))
            .Select(user => user.Id)
            .ToListAsync();
        var validCreatorIds = registeredUserIds.ToHashSet(StringComparer.Ordinal);
        var orphanedCircles = circles
            .Where(circle => string.IsNullOrWhiteSpace(circle.CreatorId) || !validCreatorIds.Contains(circle.CreatorId))
            .ToList();

        if (orphanedCircles.Count == 0) return;

        // Preserve existing discussions, notes, tasks, and membership records.
        // Invalid rows are hidden from the public circle directory instead of
        // cascading a destructive delete through user-generated content.
        foreach (var circle in orphanedCircles)
        {
            circle.IsPublic = false;
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"Deactivated {orphanedCircles.Count} learning circles without a registered creator.");
    }

    private static List<CareerDomain> SeedDomains(ApplicationDbContext context)
    {
        return new List<CareerDomain>
        {
            new CareerDomain
            {
                Id = "frontend",
                Name = "Frontend Development",
                Description = "Build beautiful and interactive user interfaces with modern web technologies",
                Color = "bg-pink-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=frontend",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "backend",
                Name = "Backend Development",
                Description = "Build robust APIs and server-side applications that power modern applications",
                Color = "bg-blue-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=backend",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "fullstack",
                Name = "Full Stack Development",
                Description = "Master both frontend and backend to become a complete developer",
                Color = "bg-purple-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=fullstack",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "data",
                Name = "Data Science",
                Description = "Analyze data and extract insights using Python, SQL, and statistical methods",
                Color = "bg-green-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=data",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "devops",
                Name = "DevOps & Cloud",
                Description = "Master deployment, scaling, and infrastructure automation",
                Color = "bg-orange-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=devops",
                DisplayOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "ml",
                Name = "Machine Learning",
                Description = "Build intelligent systems using machine learning algorithms",
                Color = "bg-indigo-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=ml",
                DisplayOrder = 6,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "business",
                Name = "Business & Management",
                Description = "Lead companies, manage budgets, and drive strategic growth",
                Color = "bg-yellow-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=business",
                DisplayOrder = 7,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "design",
                Name = "Creative & Design",
                Description = "Create stunning visual graphics, brands, and UI/UX",
                Color = "bg-red-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=design",
                DisplayOrder = 8,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "healthcare",
                Name = "Healthcare & Medicine",
                Description = "Treat patients, innovate medical research, and save lives",
                Color = "bg-teal-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=healthcare",
                DisplayOrder = 9,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CareerDomain
            {
                Id = "education",
                Name = "Education & Teaching",
                Description = "Inspire students, foster learning, and cultivate growth",
                Color = "bg-lime-500",
                IconUrl = "https://api.dicebear.com/7.x/icons/svg?seed=education",
                DisplayOrder = 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    // Classes to deserialize the new curriculum JSON
    private class JsonCurriculumDomain
    {
        public string domainId { get; set; } = string.Empty;
        public List<JsonCurriculumSkill> skills { get; set; } = new();
    }
    private class JsonCurriculumSkill
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string difficultyLevel { get; set; } = string.Empty;
        public int xpReward { get; set; }
        public int displayOrder { get; set; }
        public List<JsonCurriculumStep> steps { get; set; } = new();
    }
    private class JsonCurriculumStep
    {
        public string id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int estimatedHours { get; set; }
        public int stepNumber { get; set; }
        public List<JsonCurriculumTopic> topics { get; set; } = new();
    }
    private class JsonCurriculumTopic
    {
        public string title { get; set; } = string.Empty;
        public int displayOrder { get; set; }
    }

    private static List<JsonCurriculumDomain> LoadCurriculum()
    {
        var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "roadmap_curriculum.json");
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"Curriculum file not found at {jsonFilePath}");
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var fileContent = File.ReadAllText(jsonFilePath);
        return JsonSerializer.Deserialize<List<JsonCurriculumDomain>>(fileContent, jsonOptions) ?? new List<JsonCurriculumDomain>();
    }

    private class RoadmapPhase
    {
        public string Phase { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public List<string> Topics { get; set; } = new();
        public string ProjectSuggestion { get; set; } = string.Empty;
    }

    private static Dictionary<string, List<RoadmapPhase>>? _roadmapTemplates;

    private static Dictionary<string, List<RoadmapPhase>> LoadRoadmapTemplates()
    {
        if (_roadmapTemplates != null) return _roadmapTemplates;

        var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "career_roadmaps.json");
        if (!File.Exists(jsonFilePath))
        {
            var altPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "career_roadmaps.json");
            if (File.Exists(altPath)) jsonFilePath = altPath;
            else throw new FileNotFoundException($"Roadmap templates file not found at {jsonFilePath}");
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var fileContent = File.ReadAllText(jsonFilePath);
        _roadmapTemplates = JsonSerializer.Deserialize<Dictionary<string, List<RoadmapPhase>>>(fileContent, jsonOptions);
        return _roadmapTemplates ?? new Dictionary<string, List<RoadmapPhase>>();
    }

    private static string GetRoadmapCategory(string id)
    {
        id = id.ToLower();
        if (id.Contains("ai") || id.Contains("ml") || id.Contains("ds") || id.Contains("data"))
            return "ai_data";
        if (id.Contains("eng") || id.Contains("cs") || id.Contains("tech") || id.Contains("cloud") || id.Contains("cyber") || id.Contains("sre") || id.Contains("block") || id.Contains("game") || id.Contains("arvr") || id.Contains("embed") || id.Contains("prod") || id.Contains("arch") || id.Contains("cto") || id.Contains("ms"))
            return "tech_software";
        if (id.Contains("pcb") || id.Contains("med") || id.Contains("hc") || id.Contains("pharm") || id.Contains("nurse") || id.Contains("physio") || id.Contains("vet") || id.Contains("clinical") || id.Contains("medical") || id.Contains("doctor") || id.Contains("dentist") || id.Contains("bams") || id.Contains("bhms") || id.Contains("bpt") || id.Contains("biomed") || id.Contains("neuro") || id.Contains("immuno") || id.Contains("radio") || id.Contains("path"))
            return "medical";
        if (id.Contains("com") || id.Contains("fin") || id.Contains("bus") || id.Contains("bank") || id.Contains("acct") || id.Contains("wealth") || id.Contains("quant") || id.Contains("mba") || id.Contains("mgmt") || id.Contains("d2c") || id.Contains("ca") || id.Contains("cma") || id.Contains("cfa") || id.Contains("frm") || id.Contains("cfp") || id.Contains("wealth") || id.Contains("auditor") || id.Contains("tax") || id.Contains("retail") || id.Contains("econ"))
            return "commerce_finance";
        if (id.Contains("des") || id.Contains("art") || id.Contains("gd") || id.Contains("anim") || id.Contains("photo") || id.Contains("creative") || id.Contains("fashion") || id.Contains("interior") || id.Contains("palette") || id.Contains("media") || id.Contains("journal") || id.Contains("creator") || id.Contains("film") || id.Contains("paint") || id.Contains("sculpt"))
            return "design_creative";
        if (id.Contains("law") || id.Contains("legal") || id.Contains("corporate") || id.Contains("criminal") || id.Contains("judge") || id.Contains("court") || id.Contains("judiciary"))
            return "law";
        if (id.Contains("gov") || id.Contains("civil") || id.Contains("state") || id.Contains("def") || id.Contains("army") || id.Contains("navy") || id.Contains("iaf") || id.Contains("nda"))
            return "government";
        if (id.Contains("ent") || id.Contains("startup") || id.Contains("founder") || id.Contains("entrepreneur"))
            return "entrepreneurship";
        if (id.Contains("dip") || id.Contains("iti") || id.Contains("voc") || id.Contains("vocational") || id.Contains("trade") || id.Contains("elec") || id.Contains("fitter") || id.Contains("welder") || id.Contains("plumb") || id.Contains("cul") || id.Contains("hosp") || id.Contains("mech") || id.Contains("auto") || id.Contains("civil") || id.Contains("entc") || id.Contains("mecha") || id.Contains("text") || id.Contains("chem") || id.Contains("petr") || id.Contains("aero") || id.Contains("marine") || id.Contains("biotech") || id.Contains("nano") || id.Contains("env") || id.Contains("arch") || id.Contains("urban"))
            return "vocational";
        return "tech_software";
    }

    private static List<Skill> SeedSkills(ApplicationDbContext context, List<CareerDomain> domains)
    {
        var skillsDb = new List<Skill>();
        var curriculum = LoadCurriculum();

        foreach (var cDomain in curriculum)
        {
            var domain = domains.FirstOrDefault(d => d.Id == cDomain.domainId);
            if (domain == null) continue;

            foreach (var cSkill in cDomain.skills)
            {
                skillsDb.Add(new Skill
                {
                    Id = cSkill.id,
                    CareerDomainId = domain.Id,
                    Name = cSkill.name,
                    Description = cSkill.description,
                    DifficultyLevel = cSkill.difficultyLevel,
                    XPReward = cSkill.xpReward,
                    DisplayOrder = cSkill.displayOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        return skillsDb;
    }

    private static List<RoadmapStep> SeedRoadmapSteps(ApplicationDbContext context, List<CareerDomain> domains, List<Skill> skills)
    {
        var stepsDb = new List<RoadmapStep>();
        var curriculum = LoadCurriculum();

        foreach (var cDomain in curriculum)
        {
            var domain = domains.FirstOrDefault(d => d.Id == cDomain.domainId);
            if (domain == null) continue;

            foreach (var cSkill in cDomain.skills)
            {
                var skill = skills.FirstOrDefault(s => s.Id == cSkill.id);
                if (skill == null) continue;

                foreach (var cStep in cSkill.steps)
                {
                    var roadmapStep = new RoadmapStep
                    {
                        Id = cStep.id,
                        CareerDomainId = domain.Id,
                        SkillId = skill.Id,
                        Title = cStep.title,
                        Description = cStep.description,
                        StepNumber = cStep.stepNumber,
                        EstimatedHours = cStep.estimatedHours,
                        IsActive = true,
                        Topics = new List<RoadmapTopic>()
                    };

                    foreach (var cTopic in cStep.topics)
                    {
                        roadmapStep.Topics.Add(new RoadmapTopic
                        {
                            Id = Guid.NewGuid().ToString(),
                            ModuleId = roadmapStep.Id,
                            Title = cTopic.title,
                            DisplayOrder = cTopic.displayOrder,
                            ResourceUrl = "https://developer.mozilla.org"
                        });
                    }
                    
                    stepsDb.Add(roadmapStep);
                }
            }
        }

        return stepsDb;
    }

    private class JsonQuizQuestion
    {
        public string id { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public List<JsonQuizOption> options { get; set; } = new();
    }
    
    private class JsonQuizOption
    {
        public string id { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
        public string domain { get; set; } = string.Empty;
    }

    private static (List<QuizQuestion>, List<QuizOption>) SeedQuiz(ApplicationDbContext context, List<CareerDomain> domains)
    {
        var questionsDb = new List<QuizQuestion>();
        var optionsDb = new List<QuizOption>();

        // Try to load from JSON first, fallback to hardcoded detailed questions
        var jsonFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "quiz_150_questions.json");
        if (File.Exists(jsonFilePath))
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var fileContent = File.ReadAllText(jsonFilePath);
            var parsedQuestions = JsonSerializer.Deserialize<List<JsonQuizQuestion>>(fileContent, jsonOptions);

            if (parsedQuestions != null)
            {
                int qOrder = 1;
                foreach (var q in parsedQuestions)
                {
                    var dbQ = new QuizQuestion 
                    { 
                        Id = q.id, 
                        Text = q.text, 
                        Category = q.category, 
                        DisplayOrder = qOrder++, 
                        IsActive = true, 
                        CareerDomainId = domains[0].Id 
                    };
                    questionsDb.Add(dbQ);

                    int optOrder = 1;
                    foreach (var opt in q.options)
                    {
                        optionsDb.Add(new QuizOption 
                        { 
                            Id = opt.id, 
                            QuizQuestionId = dbQ.Id, 
                            Text = opt.text,
                            DomainWeightJson = JsonSerializer.Serialize(new Dictionary<string, int> { { opt.domain, 1 } }), 
                            DisplayOrder = optOrder++ 
                        });
                    }
                }
                return (questionsDb, optionsDb);
            }
        }

        // Fallback: Create comprehensive detailed quiz questions
        return CreateDetailedComprehensiveQuiz(questionsDb, optionsDb, domains);
    }

    private static (List<QuizQuestion>, List<QuizOption>) CreateDetailedComprehensiveQuiz(
        List<QuizQuestion> questionsDb, List<QuizOption> optionsDb, List<CareerDomain> domains)
    {
        int qOrder = 1;
        var detailedQuestions = GetDetailedQuizQuestions();

        foreach (var q in detailedQuestions)
        {
            var dbQ = new QuizQuestion 
            { 
                Id = $"q_{qOrder}", 
                Text = q.Text, 
                Category = q.Category, 
                DisplayOrder = qOrder, 
                IsActive = true, 
                CareerDomainId = domains[0].Id 
            };
            questionsDb.Add(dbQ);

            int optOrder = 1;
            foreach (var opt in q.Options)
            {
                optionsDb.Add(new QuizOption 
                { 
                    Id = $"o_{qOrder}_{optOrder}", 
                    QuizQuestionId = dbQ.Id, 
                    Text = opt.Text,
                    DomainWeightJson = JsonSerializer.Serialize(opt.DomainWeights), 
                    DisplayOrder = optOrder++ 
                });
            }
            qOrder++;
        }

        return (questionsDb, optionsDb);
    }

    private class DetailedQuizQuestion
    {
        public string Text { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<DetailedQuizOption> Options { get; set; } = new();
    }

    private class DetailedQuizOption
    {
        public string Text { get; set; } = string.Empty;
        public Dictionary<string, int> DomainWeights { get; set; } = new();
    }

    private static List<DetailedQuizQuestion> GetDetailedQuizQuestions()
    {
        return new List<DetailedQuizQuestion>
        {
            // Question 1: Work Preference
            new DetailedQuizQuestion
            {
                Text = "What aspect of technology excites you the most?",
                Category = "Interests & Passions",
                Options = new List<DetailedQuizOption>
                {
                    new DetailedQuizOption { Text = "Creating beautiful, interactive user experiences", DomainWeights = new() { { "frontend", 40 }, { "design", 30 }, { "fullstack", 20 } } },
                    new DetailedQuizOption { Text = "Building powerful APIs and server-side logic", DomainWeights = new() { { "backend", 40 }, { "fullstack", 30 }, { "devops", 15 } } },
                    new DetailedQuizOption { Text = "Working across the entire stack from UI to databases", DomainWeights = new() { { "fullstack", 50 }, { "backend", 25 }, { "frontend", 25 } } },
                    new DetailedQuizOption { Text = "Analyzing data and finding patterns", DomainWeights = new() { { "data", 50 }, { "ml", 30 }, { "backend", 15 } } },
                    new DetailedQuizOption { Text = "Infrastructure, deployment, and system reliability", DomainWeights = new() { { "devops", 45 }, { "backend", 30 }, { "fullstack", 15 } } },
                    new DetailedQuizOption { Text = "Building intelligent systems with AI/ML algorithms", DomainWeights = new() { { "ml", 50 }, { "data", 35 }, { "backend", 15 } } }
                }
            },

            // Question 2: Problem Solving Style
            new DetailedQuizQuestion
            {
                Text = "How do you prefer to solve problems?",
                Category = "Problem-Solving Approach",
                Options = new List<DetailedQuizOption>
                {
                    new DetailedQuizOption { Text = "By optimizing the visual and user experience", DomainWeights = new() { { "frontend", 50 }, { "design", 40 }, { "fullstack", 10 } } },
                    new DetailedQuizOption { Text = "By designing efficient algorithms and data structures", DomainWeights = new() { { "backend", 45 }, { "fullstack", 30 }, { "ml", 15 } } },
                    new DetailedQuizOption { Text = "By understanding the complete system architecture", DomainWeights = new() { { "fullstack", 45 }, { "backend", 35 }, { "devops", 15 } } },
                    new DetailedQuizOption { Text = "By analyzing data trends and generating insights", DomainWeights = new() { { "data", 55 }, { "ml", 30 }, { "backend", 10 } } },
                    new DetailedQuizOption { Text = "By ensuring systems are scalable and reliable", DomainWeights = new() { { "devops", 50 }, { "backend", 35 }, { "fullstack", 10 } } },
                    new DetailedQuizOption { Text = "By training models and optimizing predictions", DomainWeights = new() { { "ml", 55 }, { "data", 35 }, { "backend", 10 } } }
                }
            },

            // Question 3: Learning Preference
            new DetailedQuizQuestion
            {
                Text = "What's your preferred learning style?",
                Category = "Learning & Growth",
                Options = new List<DetailedQuizOption>
                {
                    new DetailedQuizOption { Text = "Visual design systems, CSS frameworks, UI libraries", DomainWeights = new() { { "frontend", 50 }, { "design", 35 }, { "fullstack", 15 } } },
                    new DetailedQuizOption { Text = "Databases, APIs, server architectures, best practices", DomainWeights = new() { { "backend", 50 }, { "fullstack", 30 }, { "data", 15 } } },
                    new DetailedQuizOption { Text = "Both frontend and backend technologies equally", DomainWeights = new() { { "fullstack", 60 }, { "backend", 25 }, { "frontend", 15 } } },
                    new DetailedQuizOption { Text = "Data analysis tools, SQL, statistics, visualization", DomainWeights = new() { { "data", 55 }, { "ml", 30 }, { "fullstack", 10 } } },
                    new DetailedQuizOption { Text = "Docker, Kubernetes, CI/CD pipelines, cloud platforms", DomainWeights = new() { { "devops", 55 }, { "backend", 30 }, { "fullstack", 10 } } },
                    new DetailedQuizOption { Text = "Machine learning libraries, neural networks, statistics", DomainWeights = new() { { "ml", 55 }, { "data", 35 }, { "backend", 10 } } }
                }
            },

            // Question 4: Work Environment Preference
            new DetailedQuizQuestion
            {
                Text = "What's your ideal work environment?",
                Category = "Work Style",
                Options = new List<DetailedQuizOption>
                {
                    new DetailedQuizOption { Text = "Fast-paced, designing cutting-edge interfaces", DomainWeights = new() { { "frontend", 45 }, { "design", 40 }, { "fullstack", 10 } } },
                    new DetailedQuizOption { Text = "Focused on code quality and system design", DomainWeights = new() { { "backend", 50 }, { "fullstack", 30 }, { "devops", 15 } } },
                    new DetailedQuizOption { Text = "Collaborative, working on multiple layers", DomainWeights = new() { { "fullstack", 55 }, { "backend", 25 }, { "frontend", 15 } } },
                    new DetailedQuizOption { Text = "Research-oriented, exploring data patterns", DomainWeights = new() { { "data", 50 }, { "ml", 40 }, { "backend", 10 } } },
                    new DetailedQuizOption { Text = "Problem-solving focused on reliability and scale", DomainWeights = new() { { "devops", 50 }, { "backend", 35 }, { "fullstack", 15 } } },
                    new DetailedQuizOption { Text = "Experimental, building intelligent algorithms", DomainWeights = new() { { "ml", 50 }, { "data", 40 }, { "backend", 10 } } }
                }
            },

            // Question 5: Technical Interest Depth
            new DetailedQuizQuestion
            {
                Text = "Which technical domain interests you the most?",
                Category = "Technical Expertise",
                Options = new List<DetailedQuizOption>
                {
                    new DetailedQuizOption { Text = "React, Vue, Angular, responsive design, animations", DomainWeights = new() { { "frontend", 55 }, { "design", 30 }, { "fullstack", 15 } } },
                    new DetailedQuizOption { Text = "Node.js, Python, Java, databases, authentication", DomainWeights = new() { { "backend", 55 }, { "fullstack", 30 }, { "devops", 10 } } },
                    new DetailedQuizOption { Text = "Everything from HTML to databases and APIs", DomainWeights = new() { { "fullstack", 60 }, { "backend", 25 }, { "frontend", 15 } } },
                    new DetailedQuizOption { Text = "Python, SQL, Pandas, data visualization tools", DomainWeights = new() { { "data", 60 }, { "ml", 30 }, { "backend", 10 } } },
                    new DetailedQuizOption { Text = "Docker, AWS, Kubernetes, monitoring, automation", DomainWeights = new() { { "devops", 60 }, { "backend", 30 }, { "fullstack", 10 } } },
                    new DetailedQuizOption { Text = "TensorFlow, PyTorch, scikit-learn, NLP, CV", DomainWeights = new() { { "ml", 60 }, { "data", 30 }, { "backend", 10 } } }
                }
            },

            // Question 6: Career Goals
            new DetailedQuizQuestion
            {
                Text = "What's your primary career aspiration?",
                Category = "Career Goals",
                Options = new List<DetailedQuizOption>
                {
                    new DetailedQuizOption { Text = "Lead design systems and create exceptional UX", DomainWeights = new() { { "frontend", 50 }, { "design", 40 }, { "fullstack", 10 } } },
                    new DetailedQuizOption { Text = "Become a backend architect or tech lead", DomainWeights = new() { { "backend", 50 }, { "fullstack", 30 }, { "devops", 15 } } },
                    new DetailedQuizOption { Text = "Become a versatile full-stack engineer", DomainWeights = new() { { "fullstack", 60 }, { "backend", 25 }, { "frontend", 15 } } },
                    new DetailedQuizOption { Text = "Lead data science or analytics initiatives", DomainWeights = new() { { "data", 55 }, { "ml", 35 }, { "backend", 10 } } },
                    new DetailedQuizOption { Text = "Architect cloud solutions and DevOps pipelines", DomainWeights = new() { { "devops", 55 }, { "backend", 35 }, { "fullstack", 10 } } },
                    new DetailedQuizOption { Text = "Lead ML research and AI product development", DomainWeights = new() { { "ml", 55 }, { "data", 35 }, { "backend", 10 } } }
                }
            },

            // Question 7: Challenge Type
            new DetailedQuizQuestion
            {
                Text = "What type of challenges do you enjoy most?",
                Category = "Problem Types",
                Options = new List<DetailedQuizOption>
                {
                    new DetailedQuizOption { Text = "Performance optimization and smooth interactions", DomainWeights = new() { { "frontend", 50 }, { "fullstack", 30 }, { "backend", 15 } } },
                    new DetailedQuizOption { Text = "Designing scalable systems and handling millions of requests", DomainWeights = new() { { "backend", 50 }, { "devops", 35 }, { "fullstack", 15 } } },
                    new DetailedQuizOption { Text = "Balancing frontend and backend optimization", DomainWeights = new() { { "fullstack", 55 }, { "backend", 30 }, { "frontend", 15 } } },
                    new DetailedQuizOption { Text = "Extracting insights from complex datasets", DomainWeights = new() { { "data", 55 }, { "ml", 35 }, { "backend", 10 } } },
                    new DetailedQuizOption { Text = "Managing infrastructure and ensuring system reliability", DomainWeights = new() { { "devops", 55 }, { "backend", 30 }, { "fullstack", 15 } } },
                    new DetailedQuizOption { Text = "Building models that solve real-world problems", DomainWeights = new() { { "ml", 55 }, { "data", 35 }, { "backend", 10 } } }
                }
            },

            // Question 8: Technology Stack Interest
            new DetailedQuizQuestion
            {
                Text = "Which technology stack interests you most?",
                Category = "Tech Stack Preferences",
                Options = new List<DetailedQuizOption>
                {
                    new DetailedQuizOption { Text = "JavaScript/TypeScript ecosystem (React, Vue, Next.js)", DomainWeights = new() { { "frontend", 55 }, { "fullstack", 35 }, { "backend", 10 } } },
                    new DetailedQuizOption { Text = "Backend languages (.NET, Java, Node.js, Python Django)", DomainWeights = new() { { "backend", 55 }, { "fullstack", 30 }, { "devops", 10 } } },
                    new DetailedQuizOption { Text = "Full-stack frameworks (Next.js, Nuxt, Django+React)", DomainWeights = new() { { "fullstack", 60 }, { "backend", 25 }, { "frontend", 15 } } },
                    new DetailedQuizOption { Text = "Data science stack (Pandas, Scikit-learn, Jupyter)", DomainWeights = new() { { "data", 60 }, { "ml", 35 }, { "backend", 5 } } },
                    new DetailedQuizOption { Text = "DevOps tools (Docker, Kubernetes, Terraform, CI/CD)", DomainWeights = new() { { "devops", 60 }, { "backend", 35 }, { "fullstack", 5 } } },
                    new DetailedQuizOption { Text = "ML frameworks (TensorFlow, PyTorch, Keras)", DomainWeights = new() { { "ml", 65 }, { "data", 30 }, { "backend", 5 } } }
                }
            },

            // Question 9: Interaction with Users
            new DetailedQuizQuestion
            {
                Text = "How do you prefer to interact with your work's impact?",
                Category = "Impact & Visibility",
                Options = new List<DetailedQuizOption>
                {
                    new DetailedQuizOption { Text = "See users interact with beautiful interfaces I built", DomainWeights = new() { { "frontend", 55 }, { "design", 35 }, { "fullstack", 10 } } },
                    new DetailedQuizOption { Text = "Know my code powers critical backend systems", DomainWeights = new() { { "backend", 55 }, { "fullstack", 30 }, { "devops", 15 } } },
                    new DetailedQuizOption { Text = "See end-to-end features working seamlessly", DomainWeights = new() { { "fullstack", 60 }, { "backend", 25 }, { "frontend", 15 } } },
                    new DetailedQuizOption { Text = "Present data insights that drive decisions", DomainWeights = new() { { "data", 60 }, { "ml", 35 }, { "backend", 5 } } },
                    new DetailedQuizOption { Text = "Ensure systems run reliably without incidents", DomainWeights = new() { { "devops", 60 }, { "backend", 35 }, { "fullstack", 5 } } },
                    new DetailedQuizOption { Text = "See AI/ML solutions solve complex problems", DomainWeights = new() { { "ml", 60 }, { "data", 35 }, { "backend", 5 } } }
                }
            },

            // Question 10: Team Collaboration
            new DetailedQuizQuestion
            {
                Text = "What's your preferred team structure?",
                Category = "Collaboration",
                Options = new List<DetailedQuizOption>
                {
                    new DetailedQuizOption { Text = "Work with designers and focus on user experience", DomainWeights = new() { { "frontend", 50 }, { "design", 40 }, { "fullstack", 10 } } },
                    new DetailedQuizOption { Text = "Work with other backend engineers on system design", DomainWeights = new() { { "backend", 55 }, { "fullstack", 30 }, { "devops", 15 } } },
                    new DetailedQuizOption { Text = "Collaborate closely with both frontend and backend teams", DomainWeights = new() { { "fullstack", 60 }, { "backend", 25 }, { "frontend", 15 } } },
                    new DetailedQuizOption { Text = "Work with domain experts and business analysts", DomainWeights = new() { { "data", 55 }, { "ml", 35 }, { "backend", 10 } } },
                    new DetailedQuizOption { Text = "Work with developers and infrastructure teams", DomainWeights = new() { { "devops", 55 }, { "backend", 35 }, { "fullstack", 10 } } },
                    new DetailedQuizOption { Text = "Work with researchers and data engineers", DomainWeights = new() { { "ml", 55 }, { "data", 35 }, { "backend", 10 } } }
                }
            }
        };
    }

    private static List<Badge> SeedBadges()
    {
        return new List<Badge>
        {
            new Badge { Id = Guid.NewGuid().ToString(), Name = "First Steps", Description = "Complete your first skill", IconUrl = "ðŸ‘£", Rarity = "Common", XPRequired = 0, SkillsCompletedRequired = 1, IsActive = true },
            new Badge { Id = Guid.NewGuid().ToString(), Name = "Quick Learner", Description = "Complete 5 skills", IconUrl = "âš¡", Rarity = "Rare", XPRequired = 0, SkillsCompletedRequired = 5, IsActive = true },
            new Badge { Id = Guid.NewGuid().ToString(), Name = "Skill Master", Description = "Complete 10 skills", IconUrl = "ðŸ†", Rarity = "Epic", XPRequired = 0, SkillsCompletedRequired = 10, IsActive = true },
            new Badge { Id = Guid.NewGuid().ToString(), Name = "Legendary", Description = "Reach 500 XP", IconUrl = "ðŸ‘‘", Rarity = "Legendary", XPRequired = 500, IsActive = true },
            new Badge { Id = Guid.NewGuid().ToString(), Name = "Domain Expert", Description = "Complete a full domain", IconUrl = "ðŸ”¥", Rarity = "Epic", XPRequired = 100, IsActive = true }
        };
    }

    private static List<ApplicationUser> SeedUsers()
    {
        var users = new List<ApplicationUser>
        {
            new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "student@example.com",
                PasswordHash = SkillScape.Infrastructure.Services.AuthService.HashPassword("Password123!"),
                FullName = "John Learner",
                Role = "Student",
                IsBlocked = false,
                BlockedReason = null,
                Level = 1,
                TotalXP = 0,
                CurrentStreak = 0,
                IsActive = true,
                ProfileCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "admin@example.com",
                PasswordHash = SkillScape.Infrastructure.Services.AuthService.HashPassword("Password123!"),
                FullName = "Admin User",
                Role = "Admin",
                Level = 20,
                TotalXP = 5000,
                IsActive = true,
                ProfileCompleted = true,
                IsBlocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        return users;
    }

    private static async Task SeedCareerPathsAsync(ApplicationDbContext context)
    {
        // Clear existing to refresh seeder data
        if (context.UserCareerProfiles.Any())
        {
            context.UserCareerProfiles.RemoveRange(context.UserCareerProfiles);
            await context.SaveChangesAsync();
        }
        if (context.CareerPaths.Any())
        {
            context.CareerPaths.RemoveRange(context.CareerPaths);
            await context.SaveChangesAsync();
        }

        var allPaths = new List<CareerPath>();

        // Level 1: Root entry-point categories
        // After 10th
        AddPath(allPaths, "10th-sci", "Science Stream", "After10th", "flask", "blue", 1, null);
        AddPath(allPaths, "10th-com", "Commerce Stream", "After10th", "banknote", "green", 1, null);
        AddPath(allPaths, "10th-art", "Arts & Humanities", "After10th", "palette", "purple", 1, null);
        AddPath(allPaths, "10th-dip", "Diploma Studies", "After10th", "briefcase", "amber", 1, null);
        AddPath(allPaths, "10th-iti", "ITI Trades", "After10th", "server", "rose", 1, null);
        AddPath(allPaths, "10th-voc", "Vocational Training", "After10th", "sparkles", "cyan", 1, null);

        // After 12th
        AddPath(allPaths, "12th-pcm", "Science PCM Stream", "After12th", "flask", "blue", 1, null);
        AddPath(allPaths, "12th-pcb", "Science PCB Stream", "After12th", "heart-pulse", "red", 1, null);
        AddPath(allPaths, "12th-pcmb", "Science PCMB Stream", "After12th", "flask", "blue", 1, null);
        AddPath(allPaths, "12th-com", "Commerce Stream", "After12th", "banknote", "green", 1, null);
        AddPath(allPaths, "12th-art", "Arts Stream", "After12th", "palette", "purple", 1, null);

        // After Graduation
        AddPath(allPaths, "grad-eng", "Engineering Specializations", "AfterGraduation", "cpu", "blue", 1, null);
        AddPath(allPaths, "grad-cs", "Computer Science & IT", "AfterGraduation", "code", "cyan", 1, null);
        AddPath(allPaths, "grad-bus", "Business Administration", "AfterGraduation", "banknote", "green", 1, null);
        AddPath(allPaths, "grad-med", "Clinical & Healthcare Practice", "AfterGraduation", "heart-pulse", "red", 1, null);
        AddPath(allPaths, "grad-art", "Creative Arts & Design", "AfterGraduation", "palette", "purple", 1, null);

        // After Post Graduation
        AddPath(allPaths, "pg-phd", "PhD & Academic Research", "AfterPostGraduation", "flask", "blue", 1, null);
        AddPath(allPaths, "pg-prof", "Advanced Professional Practice", "AfterPostGraduation", "briefcase", "green", 1, null);

        // Career Switch
        AddPath(allPaths, "swi-tech", "Switching to Software Engineering", "CareerSwitch", "code", "rose", 1, null);
        AddPath(allPaths, "swi-mgmt", "Switching to Management & PM", "CareerSwitch", "briefcase", "amber", 1, null);
        AddPath(allPaths, "swi-des", "Switching to Creative Design", "CareerSwitch", "palette", "purple", 1, null);

        // Government Jobs
        AddPath(allPaths, "gov-civil", "Civil Services", "GovernmentJobs", "briefcase", "amber", 1, null);
        AddPath(allPaths, "gov-bank", "Banking PO & RBI Services", "GovernmentJobs", "banknote", "green", 1, null);
        AddPath(allPaths, "gov-def", "Defence Commission", "GovernmentJobs", "shield-alert", "blue", 1, null);

        // International Education
        AddPath(allPaths, "int-ms", "MS in STEM Fields", "InternationalEducation", "flask", "blue", 1, null);
        AddPath(allPaths, "int-mba", "Global MBA Programs", "InternationalEducation", "banknote", "green", 1, null);

        // Level 2: Subdomains
        var level2List = new List<(string Id, string Title, string ParentId, string StreamType, string Icon, string Color)>();

        // After 10th
        level2List.Add(("10th-sci-pcm", "PCM (Physics, Chemistry, Math)", "10th-sci", "After10th", "flask", "blue"));
        level2List.Add(("10th-sci-pcb", "PCB (Physics, Chemistry, Biology)", "10th-sci", "After10th", "flask", "blue"));
        level2List.Add(("10th-sci-pcmb", "PCMB (Bio-Math Combination)", "10th-sci", "After10th", "flask", "blue"));
        
        level2List.Add(("10th-com-math", "Commerce with Mathematics", "10th-com", "After10th", "banknote", "green"));
        level2List.Add(("10th-com-nomath", "Commerce without Mathematics", "10th-com", "After10th", "banknote", "green"));

        level2List.Add(("10th-art-psych", "Psychology Branch", "10th-art", "After10th", "palette", "purple"));
        level2List.Add(("10th-art-soc", "Sociology Branch", "10th-art", "After10th", "palette", "purple"));
        level2List.Add(("10th-art-polsci", "Political Science Branch", "10th-art", "After10th", "palette", "purple"));
        level2List.Add(("10th-art-hist", "History Branch", "10th-art", "After10th", "palette", "purple"));
        level2List.Add(("10th-art-geog", "Geography Branch", "10th-art", "After10th", "palette", "purple"));
        level2List.Add(("10th-art-phil", "Philosophy Branch", "10th-art", "After10th", "palette", "purple"));

        level2List.Add(("10th-dip-comp", "Computer Engineering Diploma", "10th-dip", "After10th", "briefcase", "amber"));
        level2List.Add(("10th-dip-mech", "Mechanical Engineering Diploma", "10th-dip", "After10th", "briefcase", "amber"));
        level2List.Add(("10th-dip-civil", "Civil Engineering Diploma", "10th-dip", "After10th", "briefcase", "amber"));
        level2List.Add(("10th-dip-elec", "Electrical Engineering Diploma", "10th-dip", "After10th", "briefcase", "amber"));
        level2List.Add(("10th-dip-entc", "Electronics Engineering Diploma", "10th-dip", "After10th", "briefcase", "amber"));
        level2List.Add(("10th-dip-auto", "Automobile Engineering Diploma", "10th-dip", "After10th", "briefcase", "amber"));
        level2List.Add(("10th-dip-mecha", "Mechatronics Diploma", "10th-dip", "After10th", "briefcase", "amber"));
        level2List.Add(("10th-dip-text", "Textile Technology Diploma", "10th-dip", "After10th", "briefcase", "amber"));
        level2List.Add(("10th-dip-chem", "Chemical Engineering Diploma", "10th-dip", "After10th", "briefcase", "amber"));

        level2List.Add(("10th-iti-elec", "Electrician Trade ITI", "10th-iti", "After10th", "server", "rose"));
        level2List.Add(("10th-iti-fitter", "Fitter Trade ITI", "10th-iti", "After10th", "server", "rose"));
        level2List.Add(("10th-iti-welder", "Welder Trade ITI", "10th-iti", "After10th", "server", "rose"));
        level2List.Add(("10th-iti-mech", "Mechanic Trade ITI", "10th-iti", "After10th", "server", "rose"));
        level2List.Add(("10th-iti-plumb", "Plumber Trade ITI", "10th-iti", "After10th", "server", "rose"));
        level2List.Add(("10th-iti-tech", "Technician Trade ITI", "10th-iti", "After10th", "server", "rose"));

        level2List.Add(("10th-voc-gd", "Graphic Design", "10th-voc", "After10th", "sparkles", "cyan"));
        level2List.Add(("10th-voc-dm", "Digital Marketing", "10th-voc", "After10th", "sparkles", "cyan"));
        level2List.Add(("10th-voc-anim", "Animation & VFX", "10th-voc", "After10th", "sparkles", "cyan"));
        level2List.Add(("10th-voc-photo", "Photography Skills", "10th-voc", "After10th", "sparkles", "cyan"));
        level2List.Add(("10th-voc-cul", "Culinary Arts", "10th-voc", "After10th", "sparkles", "cyan"));
        level2List.Add(("10th-voc-hosp", "Hospitality Operations", "10th-voc", "After10th", "sparkles", "cyan"));

        // After 12th
        level2List.Add(("12th-pcm-eng", "Engineering (B.Tech/B.E)", "12th-pcm", "After12th", "flask", "blue"));
        level2List.Add(("12th-pcm-arch", "Architecture Studies", "12th-pcm", "After12th", "flask", "blue"));
        level2List.Add(("12th-pcm-def", "Defence Commissioning", "12th-pcm", "After12th", "flask", "blue"));
        level2List.Add(("12th-pcm-av", "Aviation Tracks", "12th-pcm", "After12th", "flask", "blue"));
        level2List.Add(("12th-pcm-res", "Core Research Sciences", "12th-pcm", "After12th", "flask", "blue"));

        level2List.Add(("12th-pcb-med", "Medical Sciences (MBBS/BDS)", "12th-pcb", "After12th", "heart-pulse", "red"));
        level2List.Add(("12th-pcb-hc", "Healthcare Professions", "12th-pcb", "After12th", "heart-pulse", "red"));
        level2List.Add(("12th-pcb-res", "Biological Research", "12th-pcb", "After12th", "heart-pulse", "red"));

        // PCMB is a combined eligibility background. These pathways are narrowed
        // later by the learner's discovery answers, not by the stream alone.
        level2List.Add(("12th-pcmb-eng", "Engineering & Technology", "12th-pcmb", "After12th", "flask", "blue"));
        level2List.Add(("12th-pcmb-med", "Medical & Healthcare", "12th-pcmb", "After12th", "heart-pulse", "blue"));
        level2List.Add(("12th-pcmb-life", "Life Sciences & Biotechnology", "12th-pcmb", "After12th", "flask", "blue"));
        level2List.Add(("12th-pcmb-sci", "Pure Sciences & Research", "12th-pcmb", "After12th", "flask", "blue"));
        level2List.Add(("12th-pcmb-env", "Environment, Agriculture & Food Science", "12th-pcmb", "After12th", "flask", "blue"));

        level2List.Add(("12th-com-fin", "Finance Specialties", "12th-com", "After12th", "banknote", "green"));
        level2List.Add(("12th-com-bus", "Business & Startups", "12th-com", "After12th", "banknote", "green"));
        level2List.Add(("12th-com-bank", "Banking Services", "12th-com", "After12th", "banknote", "green"));
        level2List.Add(("12th-com-econ", "Economics & Analysis", "12th-com", "After12th", "banknote", "green"));
        level2List.Add(("12th-com-acct", "Accounting & Taxation", "12th-com", "After12th", "banknote", "green"));
        level2List.Add(("12th-com-fintech", "Financial Technology", "12th-com", "After12th", "banknote", "green"));

        level2List.Add(("12th-art-psych", "Psychology Practices", "12th-art", "After12th", "palette", "purple"));
        level2List.Add(("12th-art-law", "Legal Studies & Judiciary", "12th-art", "After12th", "palette", "purple"));
        level2List.Add(("12th-art-media", "Media & Journalism", "12th-art", "After12th", "palette", "purple"));
        level2List.Add(("12th-art-des", "Creative Design Studies", "12th-art", "After12th", "palette", "purple"));
        level2List.Add(("12th-art-soc", "Social Sciences & Policy", "12th-art", "After12th", "palette", "purple"));
        level2List.Add(("12th-art-civil", "Civil Services Preparations", "12th-art", "After12th", "palette", "purple"));
        level2List.Add(("12th-art-edu", "Education & Teaching", "12th-art", "After12th", "palette", "purple"));

        // Build Level 2 paths for other remaining streams
        var remainingRoots = allPaths.Where(p => p.Level == 1 && !p.Id.StartsWith("10th-") && !p.Id.StartsWith("12th-")).ToList();
        foreach (var r in remainingRoots)
        {
            level2List.Add(($"{r.Id}-sd1", $"{r.Title} - Core Domain", r.Id, r.StreamType, r.Icon, r.Color));
            level2List.Add(($"{r.Id}-sd2", $"{r.Title} - Advanced Studies", r.Id, r.StreamType, r.Icon, r.Color));
        }

        // Add Level 2 paths to database
        foreach (var l2 in level2List)
        {
            AddPath(allPaths, l2.Id, l2.Title, l2.StreamType, l2.Icon, l2.Color, 2, l2.ParentId);
        }

        // Level 3: Fields under Level 2
        var level3List = new List<(string Id, string Title, string ParentId, string StreamType, string Icon, string Color)>();

        // Under 12th-pcm-eng (Level 2 Engineering PCM): 21 fields
        level3List.Add(("12th-pcm-eng-cs", "Computer Science Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "code", "blue"));
        level3List.Add(("12th-pcm-eng-ai", "Artificial Intelligence Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "sparkles", "blue"));
        level3List.Add(("12th-pcm-eng-ml", "Machine Learning Systems", "12th-pcm-eng", "After12th,AfterGraduation", "sparkles", "blue"));
        level3List.Add(("12th-pcm-eng-ds", "Data Science Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "flask", "blue"));
        level3List.Add(("12th-pcm-eng-cyber", "Cyber Security Systems", "12th-pcm-eng", "After12th,AfterGraduation", "shield-alert", "blue"));
        level3List.Add(("12th-pcm-eng-cloud", "Cloud Computing Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "server", "blue"));
        level3List.Add(("12th-pcm-eng-robo", "Robotics Systems Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "cpu", "blue"));
        level3List.Add(("12th-pcm-eng-se", "Software Engineering Practice", "12th-pcm-eng", "After12th,AfterGraduation", "code", "blue"));
        level3List.Add(("12th-pcm-eng-elec", "Electronics Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "cpu", "blue"));
        level3List.Add(("12th-pcm-eng-electrical", "Electrical Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "cpu", "blue"));
        level3List.Add(("12th-pcm-eng-mech", "Mechanical Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "briefcase", "blue"));
        level3List.Add(("12th-pcm-eng-civil", "Civil & Structural Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "briefcase", "blue"));
        level3List.Add(("12th-pcm-eng-chem", "Chemical Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "flask", "blue"));
        level3List.Add(("12th-pcm-eng-petr", "Petroleum & Gas Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "flask", "blue"));
        level3List.Add(("12th-pcm-eng-aero", "Aerospace & Aeronautical Eng", "12th-pcm-eng", "After12th,AfterGraduation", "flask", "blue"));
        level3List.Add(("12th-pcm-eng-auto", "Automobile Design Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "briefcase", "blue"));
        level3List.Add(("12th-pcm-eng-marine", "Marine Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "briefcase", "blue"));
        level3List.Add(("12th-pcm-eng-biotech", "Biotechnology Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "heart-pulse", "blue"));
        level3List.Add(("12th-pcm-eng-mecha", "Mechatronics Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "cpu", "blue"));
        level3List.Add(("12th-pcm-eng-nano", "Nanotechnology Systems", "12th-pcm-eng", "After12th,AfterGraduation", "flask", "blue"));
        level3List.Add(("12th-pcm-eng-env", "Environmental Engineering", "12th-pcm-eng", "After12th,AfterGraduation", "flask", "blue"));

        // Under 12th-pcm-arch: B.Arch, Interior Design, Urban Planning
        level3List.Add(("12th-pcm-arch-barch", "B.Arch Architecture", "12th-pcm-arch", "After12th", "palette", "blue"));
        level3List.Add(("12th-pcm-arch-interior", "Interior Designing", "12th-pcm-arch", "After12th", "palette", "blue"));
        level3List.Add(("12th-pcm-arch-urban", "Urban & City Planning", "12th-pcm-arch", "After12th", "palette", "blue"));

        // Under 12th-pcm-def: NDA, Navy, Air Force, Army
        level3List.Add(("12th-pcm-def-nda", "NDA Academy Commissions", "12th-pcm-def", "After12th", "shield-alert", "blue"));
        level3List.Add(("12th-pcm-def-navy", "Indian Navy", "12th-pcm-def", "After12th", "shield-alert", "blue"));
        level3List.Add(("12th-pcm-def-iaf", "Indian Air Force", "12th-pcm-def", "After12th", "shield-alert", "blue"));
        level3List.Add(("12th-pcm-def-army", "Indian Army", "12th-pcm-def", "After12th", "shield-alert", "blue"));

        // Under 12th-pcm-av: Pilot, Aircraft Maintenance, Aviation Management
        level3List.Add(("12th-pcm-av-pilot", "Commercial & Fighter Pilot", "12th-pcm-av", "After12th", "flask", "blue"));
        level3List.Add(("12th-pcm-av-maint", "Aircraft Maintenance", "12th-pcm-av", "After12th", "flask", "blue"));
        level3List.Add(("12th-pcm-av-mgmt", "Aviation Operations Management", "12th-pcm-av", "After12th", "flask", "blue"));

        // Under 12th-pcm-res: Physics, Mathematics, Astronomy, Space Science
        level3List.Add(("12th-pcm-res-phys", "Physics Research", "12th-pcm-res", "After12th", "flask", "blue"));
        level3List.Add(("12th-pcm-res-math", "Mathematics Research", "12th-pcm-res", "After12th", "flask", "blue"));
        level3List.Add(("12th-pcm-res-astro", "Astronomy Research", "12th-pcm-res", "After12th", "flask", "blue"));
        level3List.Add(("12th-pcm-res-space", "Space Science & Technology", "12th-pcm-res", "After12th", "flask", "blue"));

        // Under 12th-pcb-med: MBBS, BDS, BAMS, BHMS, BPT, Nursing, Pharmacy, Veterinary, Biotechnology, Genetics
        level3List.Add(("12th-pcb-med-mbbs", "MBBS Doctor", "12th-pcb-med", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-med-bds", "BDS Dentist", "12th-pcb-med", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-med-bams", "BAMS Ayurvedic Practitioner", "12th-pcb-med", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-med-bhms", "BHMS Homeopathic Doctor", "12th-pcb-med", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-med-bpt", "BPT Physiotherapist", "12th-pcb-med", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-med-nurse", "B.Sc Nursing Care", "12th-pcb-med", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-med-pharm", "Pharmacy Practice", "12th-pcb-med", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-med-vet", "Veterinary Medicine", "12th-pcb-med", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-med-biotech", "Biotechnology Studies", "12th-pcb-med", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-med-genetics", "Genetics Research", "12th-pcb-med", "After12th", "heart-pulse", "red"));

        // Under 12th-pcb-hc: Radiology, Pathology, Physiotherapy, Occupational Therapy
        level3List.Add(("12th-pcb-hc-radio", "Radiology Tech", "12th-pcb-hc", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-hc-path", "Pathology Lab Diagnostics", "12th-pcb-hc", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-hc-physio", "Physiotherapy & Rehab", "12th-pcb-hc", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-hc-occup", "Occupational Therapy", "12th-pcb-hc", "After12th", "heart-pulse", "red"));

        // Under 12th-pcb-res: Biomedical Science, Neuroscience, Immunology
        level3List.Add(("12th-pcb-res-biomed", "Biomedical Science Studies", "12th-pcb-res", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-res-neuro", "Neuroscience Research", "12th-pcb-res", "After12th", "heart-pulse", "red"));
        level3List.Add(("12th-pcb-res-immuno", "Immunology & Vaccine Studies", "12th-pcb-res", "After12th", "heart-pulse", "red"));

        // Under 12th-com-fin: CA, CMA, CS, CFA, FRM, CFP
        level3List.Add(("12th-com-fin-ca", "Chartered Accountant (CA)", "12th-com-fin", "After12th,AfterGraduation", "banknote", "green"));
        level3List.Add(("12th-com-fin-cma", "Cost & Management Accountant", "12th-com-fin", "After12th", "banknote", "green"));
        level3List.Add(("12th-com-fin-cs", "Company Secretary (CS)", "12th-com-fin", "After12th", "banknote", "green"));
        level3List.Add(("12th-com-fin-cfa", "Chartered Financial Analyst", "12th-com-fin", "After12th,AfterGraduation", "banknote", "green"));
        level3List.Add(("12th-com-fin-frm", "Financial Risk Manager", "12th-com-fin", "After12th", "banknote", "green"));
        level3List.Add(("12th-com-fin-cfp", "Certified Financial Planner", "12th-com-fin", "After12th", "banknote", "green"));

        // Under 12th-com-bus: BBA, MBA, Entrepreneurship, Startup Founder
        level3List.Add(("12th-com-bus-bba", "BBA Management studies", "12th-com-bus", "After12th", "banknote", "green"));
        level3List.Add(("12th-com-bus-mba", "MBA Corporate Finance", "12th-com-bus", "After12th,AfterGraduation", "banknote", "green"));
        level3List.Add(("12th-com-bus-ent", "Business Entrepreneurship", "12th-com-bus", "After12th,AfterGraduation", "banknote", "green"));
        level3List.Add(("12th-com-bus-founder", "Startup Founder", "12th-com-bus", "After12th,AfterGraduation,CareerSwitch", "banknote", "green"));

        // Under 12th-com-bank: Investment Banking, Retail Banking
        level3List.Add(("12th-com-bank-ib", "Investment Banking Specialist", "12th-com-bank", "After12th,AfterGraduation", "banknote", "green"));
        level3List.Add(("12th-com-bank-retail", "Retail & Branch Banking", "12th-com-bank", "After12th", "banknote", "green"));

        // Under 12th-com-econ: Economist, Policy Analyst
        level3List.Add(("12th-com-econ-economist", "Professional Economist", "12th-com-econ", "After12th", "banknote", "green"));
        level3List.Add(("12th-com-econ-policy", "Economic Policy Analyst", "12th-com-econ", "After12th,AfterGraduation", "banknote", "green"));

        // Under 12th-com-acct: Auditor, Tax Consultant
        level3List.Add(("12th-com-acct-auditor", "Statutory Auditor", "12th-com-acct", "After12th", "banknote", "green"));
        level3List.Add(("12th-com-acct-tax", "Taxation Consultant", "12th-com-acct", "After12th", "banknote", "green"));

        // Under 12th-com-fintech: Financial Analyst, Wealth Manager, Quant Analyst
        level3List.Add(("12th-com-fintech-analyst", "Financial Analyst", "12th-com-fintech", "After12th,AfterGraduation", "banknote", "green"));
        level3List.Add(("12th-com-fintech-wealth", "Wealth Manager", "12th-com-fintech", "After12th", "banknote", "green"));
        level3List.Add(("12th-com-fintech-quant", "Quant Analytics Specialist", "12th-com-fintech", "After12th,AfterGraduation", "banknote", "green"));

        // Under 12th-art-psych: Clinical Psychologist, Counsellor
        level3List.Add(("12th-art-psych-clinical", "Clinical Psychologist", "12th-art-psych", "After12th,AfterGraduation", "palette", "purple"));
        level3List.Add(("12th-art-psych-counsel", "Family & Career Counsellor", "12th-art-psych", "After12th", "palette", "purple"));

        // Under 12th-art-law: Corporate Lawyer, Criminal Lawyer, Judge
        level3List.Add(("12th-art-law-corporate", "Corporate Lawyer", "12th-art-law", "After12th,AfterGraduation", "palette", "purple"));
        level3List.Add(("12th-art-law-criminal", "Criminal Litigation Lawyer", "12th-art-law", "After12th", "palette", "purple"));
        level3List.Add(("12th-art-law-judge", "Judicial Magistrate", "12th-art-law", "After12th,AfterGraduation", "palette", "purple"));

        // Under 12th-art-media: Journalism, Content Creator, Film Making
        level3List.Add(("12th-art-media-journal", "Print & Digital Journalism", "12th-art-media", "After12th", "palette", "purple"));
        level3List.Add(("12th-art-media-creator", "Digital Content Creator", "12th-art-media", "After12th,CareerSwitch", "palette", "purple"));
        level3List.Add(("12th-art-media-film", "Cinematography & Film Making", "12th-art-media", "After12th", "palette", "purple"));

        // Under 12th-art-des: UI UX, Product Design, Fashion Design, Interior Design
        level3List.Add(("12th-art-des-uiux", "UI/UX Designer", "12th-art-des", "After12th,AfterGraduation,CareerSwitch", "palette", "purple"));
        level3List.Add(("12th-art-des-product", "Industrial Product Design", "12th-art-des", "After12th", "palette", "purple"));
        level3List.Add(("12th-art-des-fashion", "Fashion & Apparel Design", "12th-art-des", "After12th", "palette", "purple"));
        level3List.Add(("12th-art-des-interior", "Commercial Interior Design", "12th-art-des", "After12th", "palette", "purple"));

        // Under 12th-art-soc: Sociology, Anthropology, Public Policy
        level3List.Add(("12th-art-soc-sociology", "Sociology Research", "12th-art-soc", "After12th", "palette", "purple"));
        level3List.Add(("12th-art-soc-anthro", "Anthropology Studies", "12th-art-soc", "After12th", "palette", "purple"));
        level3List.Add(("12th-art-soc-policy", "Public Policy Advisor", "12th-art-soc", "After12th,AfterGraduation", "palette", "purple"));

        // Under 12th-art-civil: UPSC, State PSC
        level3List.Add(("12th-art-civil-upsc", "UPSC IAS Preparation", "12th-art-civil", "After12th,AfterGraduation,GovernmentJobs", "palette", "purple"));
        level3List.Add(("12th-art-civil-state", "State Service Commission", "12th-art-civil", "After12th,AfterGraduation,GovernmentJobs", "palette", "purple"));

        // Under 12th-art-edu: Professor, Lecturer, Teacher
        level3List.Add(("12th-art-edu-prof", "University Professor", "12th-art-edu", "After12th,AfterGraduation", "palette", "purple"));
        level3List.Add(("12th-art-edu-lecturer", "College Lecturer", "12th-art-edu", "After12th", "palette", "purple"));
        level3List.Add(("12th-art-edu-teacher", "High School Teacher", "12th-art-edu", "After12th", "palette", "purple"));

        // Generate Level 3 fields for other Level 2 nodes recursively
        foreach (var l2 in level2List)
        {
            if (!level3List.Any(l3 => l3.ParentId == l2.Id))
            {
                level3List.Add(($"{l2.Id}-f1", $"{l2.Title} - Core Field", l2.Id, l2.StreamType, l2.Icon, l2.Color));
            }
        }

        // Add Level 3 paths to database
        foreach (var l3 in level3List)
        {
            AddPath(allPaths, l3.Id, l3.Title, l3.StreamType, l3.Icon, l3.Color, 3, l3.ParentId);
        }

        // Level 4: Specializations under Level 3
        var level4List = new List<(string Id, string Title, string ParentId, string StreamType, string Icon, string Color)>();

        // Under CS Engineering (12th-pcm-eng-cs): 16 specializations
        string csId = "12th-pcm-eng-cs";
        level4List.Add(($"{csId}-front", "Frontend Engineer", csId, "After12th,AfterGraduation,CareerSwitch", "code", "blue"));
        level4List.Add(($"{csId}-back", "Backend Engineer", csId, "After12th,AfterGraduation,CareerSwitch", "code", "blue"));
        level4List.Add(($"{csId}-full", "Full Stack Engineer", csId, "After12th,AfterGraduation,CareerSwitch", "code", "blue"));
        level4List.Add(($"{csId}-ai", "AI Engineer", csId, "After12th,AfterGraduation,CareerSwitch,Upskilling", "sparkles", "blue"));
        level4List.Add(($"{csId}-ml", "ML Engineer", csId, "After12th,AfterGraduation,CareerSwitch,Upskilling", "sparkles", "blue"));
        level4List.Add(($"{csId}-data", "Data Engineer", csId, "After12th,AfterGraduation,Upskilling", "flask", "blue"));
        level4List.Add(($"{csId}-cloud", "Cloud Engineer", csId, "After12th,AfterGraduation,Upskilling", "server", "blue"));
        level4List.Add(($"{csId}-devops", "DevOps Engineer", csId, "After12th,AfterGraduation,Upskilling", "server", "blue"));
        level4List.Add(($"{csId}-sre", "Site Reliability Engineer", csId, "After12th,AfterGraduation", "server", "blue"));
        level4List.Add(($"{csId}-block", "Blockchain Developer", csId, "After12th,AfterGraduation", "code", "blue"));
        level4List.Add(($"{csId}-game", "Game Developer", csId, "After12th,AfterGraduation", "code", "blue"));
        level4List.Add(($"{csId}-arvr", "AR/VR Developer", csId, "After12th,AfterGraduation", "code", "blue"));
        level4List.Add(($"{csId}-embed", "Embedded Engineer", csId, "After12th,AfterGraduation", "cpu", "blue"));
        level4List.Add(($"{csId}-prod", "Product Engineer", csId, "After12th,AfterGraduation", "briefcase", "blue"));
        level4List.Add(($"{csId}-arch", "Solution Architect", csId, "After12th,AfterGraduation", "briefcase", "blue"));
        level4List.Add(($"{csId}-cto", "CTO Path", csId, "After12th,AfterGraduation", "sparkles", "blue"));

        // Generate Level 4 paths for other Level 3 nodes recursively
        foreach (var l3 in level3List)
        {
            if (!level4List.Any(l4 => l4.ParentId == l3.Id))
            {
                level4List.Add(($"{l3.Id}-s1", $"{l3.Title} - Specialization", l3.Id, l3.StreamType, l3.Icon, l3.Color));
            }
        }

        // Add Level 4 paths to database
        foreach (var l4 in level4List)
        {
            AddPath(allPaths, l4.Id, l4.Title, l4.StreamType, l4.Icon, l4.Color, 4, l4.ParentId);
        }

        // Level 5: Job Roles under Level 4
        var level5List = new List<(string Id, string Title, string ParentId, string StreamType, string Icon, string Color)>();

        // Generate 2 job roles for every Level 4 specialization programmatically
        foreach (var l4 in level4List)
        {
            level5List.Add(($"{l4.Id}-r1", $"Lead {l4.Title}", l4.Id, l4.StreamType, l4.Icon, l4.Color));
            level5List.Add(($"{l4.Id}-r2", $"Senior {l4.Title} Architect", l4.Id, l4.StreamType, l4.Icon, l4.Color));
        }

        // Add Level 5 paths to database
        foreach (var l5 in level5List)
        {
            AddPath(allPaths, l5.Id, l5.Title, l5.StreamType, l5.Icon, l5.Color, 5, l5.ParentId);
        }

        // Populate RelatedCareerIds based on sibling nodes in Level 5
        var level5Paths = allPaths.Where(p => p.Level == 5).ToList();
        for (int i = 0; i < level5Paths.Count; i++)
        {
            var p = level5Paths[i];
            var siblings = level5Paths.Where(x => x.ParentCareerId == p.ParentCareerId && x.Id != p.Id).Select(x => x.Id).ToList();
            p.RelatedCareerIds = string.Join(",", siblings);
        }

        // Save in batches to database
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        int batchSize = 100;
        for (int i = 0; i < allPaths.Count; i += batchSize)
        {
            var batch = allPaths.Skip(i).Take(batchSize);
            context.CareerPaths.AddRange(batch);
            await context.SaveChangesAsync();
        }
        context.ChangeTracker.AutoDetectChangesEnabled = true;
    }

    private static void AddPath(
        List<CareerPath> list,
        string id,
        string title,
        string streamType,
        string icon,
        string color,
        int level,
        string? parentId)
    {
        string desc = $"Professional career pathway in {title} at Level {level}, highlighting growth tracks, certifications, and recommended skills.";
        string diffLevel = level switch {
            1 => "Easy",
            2 => "Medium",
            3 => "Medium",
            4 => "Hard",
            _ => "Extremely Hard"
        };
        double risk = level switch {
            1 => 15.0,
            2 => 20.0,
            3 => 25.0,
            4 => 30.0,
            _ => 10.0
        };
        double demand = 70.0 + (level * 5.0);
        string category = GetRoadmapCategory(id);
        var (salaryJson, cagr) = GetDomainSalaryAndGrowth(category, id, level);

        var templates = LoadRoadmapTemplates();
        string roadmapJsonString = "[]";

        if (templates.TryGetValue(category, out var phases))
        {
            var customizedPhases = phases.Select(p => new RoadmapPhase
            {
                Phase = p.Phase.Replace("{Title}", title),
                Duration = p.Duration,
                Topics = p.Topics.Select(t => t.Replace("{Title}", title)).ToList(),
                ProjectSuggestion = p.ProjectSuggestion.Replace("{Title}", title)
            }).ToList();

            roadmapJsonString = JsonSerializer.Serialize(customizedPhases, new JsonSerializerOptions { WriteIndented = false });
        }
        else
        {
            roadmapJsonString = $"[{{\"Phase\":\"Phase 1: Foundations of {title}\",\"Duration\":\"3 Months\",\"Topics\":[\"Introduction to {title}\",\"Core Concepts and Terminology\"],\"ProjectSuggestion\":\"Build a review deck on {title} fundamentals.\"}}]";
        }

        string skillsRequiredJson = "[\"Communication\", \"Problem Solving\", \"Critical Thinking\"]";
        if (category == "tech_software") skillsRequiredJson = "[\"Programming\", \"Git/GitHub\", \"Databases\", \"API Design\"]";
        else if (category == "ai_data") skillsRequiredJson = "[\"Python\", \"Pandas/NumPy\", \"Mathematics\", \"Machine Learning\"]";
        else if (category == "medical") skillsRequiredJson = "[\"Anatomy\", \"Pathology\", \"Pharmacology\", \"Diagnostics\"]";
        else if (category == "commerce_finance") skillsRequiredJson = "[\"Accounting\", \"Taxation\", \"Financial Valuation\", \"Strategic Mgmt\"]";
        else if (category == "design_creative") skillsRequiredJson = "[\"Design Thinking\", \"Typography\", \"Figma Prototyping\", \"User Research\"]";
        else if (category == "law") skillsRequiredJson = "[\"Constitutional Law\", \"Legal Drafting\", \"Criminal/Civil Code\", \"Courtroom Advocacy\"]";
        else if (category == "government") skillsRequiredJson = "[\"Indian Polity\", \"CSAT Aptitude\", \"Answer Writing\", \"Current Affairs\"]";
        else if (category == "entrepreneurship") skillsRequiredJson = "[\"Lean Startup\", \"Product Execution\", \"Growth Marketing\", \"Fundraising\"]";
        else if (category == "vocational") skillsRequiredJson = "[\"Trade Tools\", \"Workshop Controls\", \"Blueprint Schematics\", \"Safety Auditing\"]";

        list.Add(new CareerPath
        {
            Id = id,
            Title = title,
            Description = desc,
            Level = level,
            ParentCareerId = string.IsNullOrEmpty(parentId) ? null : parentId,
            StreamType = streamType,
            Tags = id.Replace("-", ","),
            SkillsRequired = skillsRequiredJson,
            FutureScope = $"Excellent scope driven by advancements and market demand for {title}.",
            SalaryDataJson = salaryJson,
            IndustryGrowth = cagr,
            RoadmapJson = roadmapJsonString,
            Certifications = "Professional Standard Certifications",
            Colleges = "Top National & Global Universities",
            DemandIndex = demand,
            Icon = icon,
            Color = color,
            WhatYouWillStudy = $"Theoretical foundations and applied skills of {title}.",
            RecommendedSubjects = "[\"Core Theory\", \"Electives\"]",
            TopCompanies = "[\"MNCs\", \"Startups\", \"Public Sector\"]",
            RequiredDegrees = "[\"Bachelor's / Associate degree\"]",
            WorkEnvironment = "Hybrid",
            RemoteWorkPossibility = 45.0,
            EntrepreneurshipPotential = 50.0,
            CareerFlexibility = 60.0,
            LearningResources = "[\"Introductory Books\", \"Online Reference Course\"]",
            YoutubeResources = "[\"Educational Tutorials\", \"Crash Course\"]",
            RoadmapTimeline = "3-4 Years",
            DifficultyLevel = diffLevel,
            PersonalityMatch = "INTJ, ENTJ, INTP, ENFP",
            RelatedCareerIds = "",
            AutomationRisk = risk
        });
    }
    private class QuestionTemplate
    {
        public string TextTemplate { get; set; } = string.Empty;
        public string AptitudeType { get; set; } = string.Empty;
        public List<OptionTemplate> Options { get; set; } = new();
    }

    private class OptionTemplate
    {
        public string TextTemplate { get; set; } = string.Empty;
        public Dictionary<string, int> WeightModifiers { get; set; } = new();
        public string NextLevelTags { get; set; } = string.Empty;
    }

    private static List<QuestionTemplate> GetTemplatesForCategory(string category)
    {
        var list = new List<QuestionTemplate>();

        switch (category.ToLower())
        {
            case "ai_data":
                // 3 Interest Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "What area of data science or machine learning excites you the most when studying {Title}?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Designing deep neural networks, transformers, and generative AI models.", WeightModifiers = new() { { "primary", 5 }, { "ai", 5 } }, NextLevelTags = "ai" },
                        new() { TextTemplate = "Building large-scale data pipelines, ETL workflows, and data lakehouses.", WeightModifiers = new() { { "primary", 3 }, { "data", 5 } }, NextLevelTags = "data" },
                        new() { TextTemplate = "Performing statistical analysis, hypothesis testing, and forecasting trends.", WeightModifiers = new() { { "primary", 3 }, { "ds", 5 } }, NextLevelTags = "ds" },
                        new() { TextTemplate = "Deploying model inference services with extremely low latency at the edge.", WeightModifiers = new() { { "primary", 2 }, { "ml", 5 } }, NextLevelTags = "ml" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "Which type of dataset would you enjoy analyzing the most in a {Title} project?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Massive text corpora for training large language models (LLMs).", WeightModifiers = new() { { "primary", 5 }, { "ai", 5 } }, NextLevelTags = "ai" },
                        new() { TextTemplate = "Structured financial transaction records for fraud detection models.", WeightModifiers = new() { { "primary", 4 }, { "ds", 4 } }, NextLevelTags = "ds" },
                        new() { TextTemplate = "Raw sensor data streams from autonomous robots or IoT devices.", WeightModifiers = new() { { "primary", 4 }, { "ml", 5 } }, NextLevelTags = "ml" },
                        new() { TextTemplate = "Biomedical genomics datasets to discover new therapeutic proteins.", WeightModifiers = new() { { "primary", 3 }, { "biotech", 4 } }, NextLevelTags = "biotech" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What kind of analytical breakthrough sounds most fulfilling to you in {Title}?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Improving model accuracy on generative tasks by fine-tuning neural weights.", WeightModifiers = new() { { "primary", 5 }, { "ai", 5 } }, NextLevelTags = "ai" },
                        new() { TextTemplate = "Optimizing database query speeds for millions of analytical rows.", WeightModifiers = new() { { "primary", 4 }, { "data", 5 } }, NextLevelTags = "data" },
                        new() { TextTemplate = "Publishing a paper proving a new statistical correlation in customer behavior.", WeightModifiers = new() { { "primary", 4 }, { "ds", 5 } }, NextLevelTags = "ds" },
                        new() { TextTemplate = "Structuring clean schema configurations for streaming telemetry.", WeightModifiers = new() { { "primary", 3 }, { "data", 4 } }, NextLevelTags = "data" }
                    }
                });

                // 3 WorkStyle Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you prefer to approach modeling tasks in a {Title} project?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Deeply researching model architectures, loss functions, and hyperparameters.", WeightModifiers = new() { { "primary", 5 }, { "ml", 5 } } },
                        new() { TextTemplate = "Cleaning messy datasets, handling missing values, and engineering features.", WeightModifiers = new() { { "primary", 5 }, { "ds", 5 } } },
                        new() { TextTemplate = "Writing clean, production-ready Python pipelines and MLOps build scripts.", WeightModifiers = new() { { "primary", 4 }, { "ml", 5 } } },
                        new() { TextTemplate = "Creating interactive dashboards (Streamlit/Tableau) to present findings.", WeightModifiers = new() { { "primary", 3 }, { "ds", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What is your approach to dealing with raw data quality issues in {Title}?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Automating validation rules and data contracts at the ingestion layer.", WeightModifiers = new() { { "primary", 5 }, { "data", 5 } } },
                        new() { TextTemplate = "Imputing values using statistical distributions and correlation matrices.", WeightModifiers = new() { { "primary", 5 }, { "ds", 5 } } },
                        new() { TextTemplate = "Consulting domain experts to understand data entry anomalies.", WeightModifiers = new() { { "primary", 4 }, { "ds", 4 } } },
                        new() { TextTemplate = "Manually pruning outliers and labeling data for active learning loops.", WeightModifiers = new() { { "primary", 3 }, { "ml", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When presenting analytical insights for {Title}, what do you focus on?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Visual charts, clear trends, and actionable business recommendations.", WeightModifiers = new() { { "primary", 5 }, { "ds", 5 } } },
                        new() { TextTemplate = "Explaining the mathematical principles, weights, and confidence intervals.", WeightModifiers = new() { { "primary", 5 }, { "ml", 5 } } },
                        new() { TextTemplate = "Demonstrating the scalability and cloud latency metrics of the model.", WeightModifiers = new() { { "primary", 4 }, { "data", 4 } } },
                        new() { TextTemplate = "Detailing the data cleaning and source lineage documentation.", WeightModifiers = new() { { "primary", 3 }, { "data", 5 } } }
                    }
                });

                // 3 Aptitude Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "If your machine learning model is overfitting the training data in {Title}, how do you fix it?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Applying regularization techniques, dropout layers, or gathering more training samples.", WeightModifiers = new() { { "primary", 5 }, { "ml", 5 } } },
                        new() { TextTemplate = "Reducing the number of features and selecting only highly correlated variables.", WeightModifiers = new() { { "primary", 5 }, { "ds", 5 } } },
                        new() { TextTemplate = "Setting up cross-validation folds and hyperparameter grid searches.", WeightModifiers = new() { { "primary", 4 }, { "ml", 5 } } },
                        new() { TextTemplate = "Pruning decision trees and simplifying the neural network architecture.", WeightModifiers = new() { { "primary", 3 }, { "ml", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you evaluate if a data pipeline is performing optimally in {Title}?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Measuring throughput, latency, partition distribution, and cluster resources.", WeightModifiers = new() { { "primary", 5 }, { "data", 5 } } },
                        new() { TextTemplate = "Checking precision, recall, F1-scores, and ROC-AUC curves of downstream models.", WeightModifiers = new() { { "primary", 5 }, { "ml", 5 } } },
                        new() { TextTemplate = "Running data integrity checks and schema validation tests.", WeightModifiers = new() { { "primary", 4 }, { "data", 5 } } },
                        new() { TextTemplate = "Verifying statistical significance (p-values) of experimental tests.", WeightModifiers = new() { { "primary", 3 }, { "ds", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When a model's prediction accuracy drifts in production for {Title}, what is your diagnostic flow?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Comparing current production feature distributions with training distributions (Data Drift).", WeightModifiers = new() { { "primary", 5 }, { "ml", 5 } } },
                        new() { TextTemplate = "Inspecting raw database inputs for format changes or missing values.", WeightModifiers = new() { { "primary", 5 }, { "data", 5 } } },
                        new() { TextTemplate = "Re-training the model on the latest batch of transactions.", WeightModifiers = new() { { "primary", 4 }, { "ml", 5 } } },
                        new() { TextTemplate = "Conducting manual audits of misclassified edge cases to update rules.", WeightModifiers = new() { { "primary", 3 }, { "ds", 5 } } }
                    }
                });
                break;

            case "medical":
                // 3 Interest Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "What area of clinical practice or patient care excites you the most in {Title}?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Diagnosing complex patient symptoms and designing custom treatment plans.", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "mbbs" },
                        new() { TextTemplate = "Conducting laboratory tests, analyzing blood work, and researching pathogens.", WeightModifiers = new() { { "primary", 4 }, { "path", 5 } }, NextLevelTags = "path" },
                        new() { TextTemplate = "Caring for patients during recovery, administering medicine, and bedside care.", WeightModifiers = new() { { "primary", 3 }, { "nurse", 5 } }, NextLevelTags = "nurse" },
                        new() { TextTemplate = "Performing surgical procedures, stitches, and manual medical interventions.", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "mbbs" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "If you were to attend a medical seminar on {Title}, what topic would you choose?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "New drug discoveries, pharmacokinetics, and therapeutic applications.", WeightModifiers = new() { { "primary", 5 }, { "pharm", 5 } }, NextLevelTags = "pharm" },
                        new() { TextTemplate = "Advanced radiological imaging techniques, MRI, and diagnostics.", WeightModifiers = new() { { "primary", 5 }, { "radio", 5 } }, NextLevelTags = "radio" },
                        new() { TextTemplate = "Genomics, gene editing, and cell-based therapeutic advancements.", WeightModifiers = new() { { "primary", 5 }, { "genetics", 5 } }, NextLevelTags = "genetics" },
                        new() { TextTemplate = "Neurological pathways, brain-computer interfaces, and cognitive rehab.", WeightModifiers = new() { { "primary", 5 }, { "neuro", 5 } }, NextLevelTags = "neuro" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What aspect of healthcare system operations in {Title} interests you?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Preventing disease outbreaks, community vaccination, and public health policy.", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "mbbs" },
                        new() { TextTemplate = "Analyzing lab results, tissue biopsies, and writing pathology reports.", WeightModifiers = new() { { "primary", 5 }, { "path", 5 } }, NextLevelTags = "path" },
                        new() { TextTemplate = "Rehabilitating physical injuries, muscle strengthening, and sports medicine.", WeightModifiers = new() { { "primary", 5 }, { "physio", 5 } }, NextLevelTags = "physio" },
                        new() { TextTemplate = "Managing medical staff, hospital resources, and healthcare software.", WeightModifiers = new() { { "primary", 3 } }, NextLevelTags = "nurse" }
                    }
                });

                // 3 WorkStyle Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you prefer to handle patient interactions or clinical schedules in {Title}?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Patiently explaining diagnostics, options, and comfort care.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Focusing on fast diagnostic execution, lab scans, and writing reports.", WeightModifiers = new() { { "primary", 5 }, { "path", 5 } } },
                        new() { TextTemplate = "Coordinating ward schedules, medicine dosages, and nurse rotations.", WeightModifiers = new() { { "primary", 4 }, { "nurse", 5 } } },
                        new() { TextTemplate = "Undertaking high-pressure emergency surgeries and shifts.", WeightModifiers = new() { { "primary", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What is your approach to medical guidelines and research in {Title}?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Sticking strictly to standard clinical protocols (evidence-based medicine).", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Participating in research trials and developing new diagnostic methods.", WeightModifiers = new() { { "primary", 5 }, { "genetics", 5 } } },
                        new() { TextTemplate = "Evaluating patient posture, mobility, and adjusting physical therapy.", WeightModifiers = new() { { "primary", 5 }, { "physio", 5 } } },
                        new() { TextTemplate = "Compounding formulations and verifying drug chemical safety logs.", WeightModifiers = new() { { "primary", 5 }, { "pharm", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When joining a new clinic or hospital for {Title}, what do you prioritize?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Reviewing patient histories, case files, and medication records.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Inspecting laboratory equipment, microscopes, and chemical reagents.", WeightModifiers = new() { { "primary", 5 }, { "path", 5 } } },
                        new() { TextTemplate = "Verifying ICU equipment monitors, ventilators, and emergency supplies.", WeightModifiers = new() { { "primary", 4 }, { "nurse", 5 } } },
                        new() { TextTemplate = "Calibrating X-ray, CT, or MRI imaging scanners.", WeightModifiers = new() { { "primary", 5 }, { "radio", 5 } } }
                    }
                });

                // 3 Aptitude Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "If a patient experiences an unexpected allergic reaction in {Title}, what is your first response?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Quickly administering emergency adrenaline and stabilizing airway.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Checking drug interaction charts and notifying the pharmacist.", WeightModifiers = new() { { "primary", 5 }, { "pharm", 5 } } },
                        new() { TextTemplate = "Analyzing the chemical composition of the allergen in a pathology lab.", WeightModifiers = new() { { "primary", 4 }, { "path", 5 } } },
                        new() { TextTemplate = "Calming the patient and updating their charts for follow-up studies.", WeightModifiers = new() { { "primary", 3 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you evaluate if a treatment plan is working for {Title}?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Monitoring patient vital signs, fever charts, and physical recovery.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Running follow-up blood tests, cultures, and diagnostic imaging.", WeightModifiers = new() { { "primary", 5 }, { "path", 5 } } },
                        new() { TextTemplate = "Measuring joint range of motion and muscle strength recovery.", WeightModifiers = new() { { "primary", 5 }, { "physio", 5 } } },
                        new() { TextTemplate = "Assessing if symptoms have reduced without drug side effects.", WeightModifiers = new() { { "primary", 5 }, { "pharm", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When a clinical audit occurs for {Title}, what is your focus?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Verifying patient consent forms, prescription sheets, and medical records.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Inspecting sterilization logs, surgical wards, and hygiene reviews.", WeightModifiers = new() { { "primary", 5 }, { "nurse", 5 } } },
                        new() { TextTemplate = "Vetting drug inventory logs, storage temperatures, and expiry dates.", WeightModifiers = new() { { "primary", 5 }, { "pharm", 5 } } },
                        new() { TextTemplate = "Confirming radiology calibration logs and radiation safety shielding.", WeightModifiers = new() { { "primary", 5 }, { "radio", 5 } } }
                    }
                });
                break;

            case "commerce_finance":
                // 3 Interest Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "Which financial or business domain excites you the most in {Title}?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Analyzing tax laws, auditing ledgers, and certifying corporate accounts.", WeightModifiers = new() { { "primary", 5 }, { "ca", 5 }, { "acct", 5 } }, NextLevelTags = "ca" },
                        new() { TextTemplate = "Valuing stocks, structuring corporate mergers, and investment strategies.", WeightModifiers = new() { { "primary", 5 }, { "cfa", 5 }, { "ib", 5 } }, NextLevelTags = "cfa" },
                        new() { TextTemplate = "Coordinating corporate teams, setting project budgets, and scaling operations.", WeightModifiers = new() { { "primary", 4 }, { "mba", 5 } }, NextLevelTags = "mba" },
                        new() { TextTemplate = "Developing automated quantitative trading algorithms and risk models.", WeightModifiers = new() { { "primary", 3 }, { "quant", 5 } }, NextLevelTags = "quant" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What project would you enjoy leading in {Title}?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Conducting a comprehensive risk assessment for a new investment portfolio.", WeightModifiers = new() { { "primary", 5 }, { "frm", 5 }, { "wealth", 5 } }, NextLevelTags = "frm" },
                        new() { TextTemplate = "Formulating a long-term macro-economic policy proposal for inflation control.", WeightModifiers = new() { { "primary", 5 }, { "economist", 5 }, { "econ", 5 } }, NextLevelTags = "econ" },
                        new() { TextTemplate = "Establishing corporate compliance standards and board resolution minutes.", WeightModifiers = new() { { "primary", 5 }, { "cs", 5 } }, NextLevelTags = "cs" },
                        new() { TextTemplate = "Developing a new payment gateway or digital lending application.", WeightModifiers = new() { { "primary", 4 }, { "fintech", 5 } }, NextLevelTags = "fintech" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What kind of analysis sounds most interesting to you in {Title}?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Calculating cash flow projections, EBITDA, and internal rate of return (IRR).", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "cfa" },
                        new() { TextTemplate = "Investigating asset records, balance sheets, and discovering tax savings.", WeightModifiers = new() { { "primary", 5 }, { "ca", 5 } }, NextLevelTags = "ca" },
                        new() { TextTemplate = "Researching market size, competitive advantages, and customer engagement metrics.", WeightModifiers = new() { { "primary", 4 } }, NextLevelTags = "mba" },
                        new() { TextTemplate = "Measuring standard deviations, tracking market indices, and hedging risk.", WeightModifiers = new() { { "primary", 4 }, { "frm", 5 } }, NextLevelTags = "frm" }
                    }
                });

                // 3 WorkStyle Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you prefer to coordinate your team tasks in {Title}?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Working on audit spreadsheets, validating receipts, and balancing ledgers.", WeightModifiers = new() { { "primary", 5 }, { "ca", 5 } } },
                        new() { TextTemplate = "Conducting deal meetings with clients and pitching investment models.", WeightModifiers = new() { { "primary", 5 }, { "ib", 5 }, { "cfa", 5 } } },
                        new() { TextTemplate = "Establishing weekly standups, delegation charts, and project milestones.", WeightModifiers = new() { { "primary", 4 }, { "mba", 5 } } },
                        new() { TextTemplate = "Writing automated scripts to query financial APIs and fetch stock data.", WeightModifiers = new() { { "primary", 3 }, { "quant", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What is your approach to financial risk and market updates in {Title}?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Prudently avoiding high-risk assets and planning secure, long-term reserves.", WeightModifiers = new() { { "primary", 5 }, { "frm", 5 } } },
                        new() { TextTemplate = "Leveraging market opportunities using mathematical models and algorithmic trading.", WeightModifiers = new() { { "primary", 5 }, { "quant", 5 } } },
                        new() { TextTemplate = "Adapting business strategies based on tax amendments and compliance updates.", WeightModifiers = new() { { "primary", 5 }, { "ca", 5 } } },
                        new() { TextTemplate = "Drafting policy reports to influence government banking rates.", WeightModifiers = new() { { "primary", 4 }, { "econ", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When joining a new corporate team for {Title}, what is your initial step?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Reviewing the tax logs, financial statements, and ledger entries.", WeightModifiers = new() { { "primary", 5 }, { "ca", 5 } } },
                        new() { TextTemplate = "Reviewing the capitalization table, investor agreements, and bylaws.", WeightModifiers = new() { { "primary", 5 }, { "cs", 5 } } },
                        new() { TextTemplate = "Inspecting the database structure of the transactional ledger software.", WeightModifiers = new() { { "primary", 4 }, { "fintech", 5 } } },
                        new() { TextTemplate = "Interviewing department heads to map business workflow bottlenecks.", WeightModifiers = new() { { "primary", 3 }, { "mba", 5 } } }
                    }
                });

                // 3 Aptitude Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "If a company's budget is overdrawn in {Title}, how do you diagnose the issue?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Performing a forensic audit of department ledger expenses.", WeightModifiers = new() { { "primary", 5 }, { "ca", 5 } } },
                        new() { TextTemplate = "Analyzing fixed vs variable costs and optimizing product shipping models.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Reviewing marketing CAC vs customer LTV and stopping high-burn campaigns.", WeightModifiers = new() { { "primary", 4 } } },
                        new() { TextTemplate = "Negotiating refinancing options or short-term bank credit lines.", WeightModifiers = new() { { "primary", 4 }, { "ib", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you evaluate if a business investment is successful in {Title}?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Calculating the ROI, Net Present Value (NPV), and payback period.", WeightModifiers = new() { { "primary", 5 }, { "cfa", 5 } } },
                        new() { TextTemplate = "Verifying if all operations comply with tax laws and corporate governance codes.", WeightModifiers = new() { { "primary", 5 }, { "ca", 5 }, { "cs", 5 } } },
                        new() { TextTemplate = "Measuring customer acquisition growth rates and market share expansion.", WeightModifiers = new() { { "primary", 4 } } },
                        new() { TextTemplate = "Checking standard deviations of the stock portfolio against beta indices.", WeightModifiers = new() { { "primary", 3 }, { "frm", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When a financial audit reveals discrepancies in {Title}, what is your response?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Tracing transactions from bank statements to source vouchers.", WeightModifiers = new() { { "primary", 5 }, { "ca", 5 } } },
                        new() { TextTemplate = "Reviewing board authorization minutes and compliance certificates.", WeightModifiers = new() { { "primary", 5 }, { "cs", 5 } } },
                        new() { TextTemplate = "Analyzing database logs of the accounting software for manual overrides.", WeightModifiers = new() { { "primary", 4 }, { "fintech", 5 } } },
                        new() { TextTemplate = "Recalculating tax liabilities and filing amended tax returns.", WeightModifiers = new() { { "primary", 4 }, { "ca", 4 } } }
                    }
                });
                break;

            case "design_creative":
                // 3 Interest Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "Which creative process in {Title} do you enjoy the most?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Mapping user personas, wireframing screens, and creating clickable mockups.", WeightModifiers = new() { { "primary", 5 }, { "uiux", 5 } }, NextLevelTags = "uiux" },
                        new() { TextTemplate = "Drawing digital illustrations, editing high-fidelity photos, and graphic styling.", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "gd" },
                        new() { TextTemplate = "Rigging 3D models, editing video sequences, and rendering movie VFX.", WeightModifiers = new() { { "primary", 4 }, { "anim", 5 } }, NextLevelTags = "anim" },
                        new() { TextTemplate = "Designing apparel collections, sketching fashion drapes, or selecting fabric textures.", WeightModifiers = new() { { "primary", 3 }, { "fashion", 5 } }, NextLevelTags = "fashion" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "If you had a free day to practice {Title}, what would you do?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Redesigning the interior layout, furniture, and lighting of a commercial cafe.", WeightModifiers = new() { { "primary", 5 }, { "interior", 5 } }, NextLevelTags = "interior" },
                        new() { TextTemplate = "Cooking and presenting a multi-course gourmet menu with precise plating.", WeightModifiers = new() { { "primary", 5 }, { "cul", 5 } }, NextLevelTags = "cul" },
                        new() { TextTemplate = "Writing a screenplay, filming documentary scenes, or adjusting video color grades.", WeightModifiers = new() { { "primary", 4 }, { "film", 5 } }, NextLevelTags = "film" },
                        new() { TextTemplate = "Analyzing social trends, writing copy, and launching marketing campaigns.", WeightModifiers = new() { { "primary", 3 }, { "dm", 5 } }, NextLevelTags = "dm" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What outcome from a {Title} project makes you proud?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Seeing users interact effortlessly with an app without getting confused.", WeightModifiers = new() { { "primary", 5 }, { "uiux", 5 } }, NextLevelTags = "uiux" },
                        new() { TextTemplate = "Having a logo or billboard design go viral and establish a brand identity.", WeightModifiers = new() { { "primary", 5 }, { "dm", 5 } }, NextLevelTags = "dm" },
                        new() { TextTemplate = "Creating a beautiful physical space that makes people feel comfortable and inspired.", WeightModifiers = new() { { "primary", 4 }, { "interior", 5 } }, NextLevelTags = "interior" },
                        new() { TextTemplate = "Serving a delicious meal that receives excellent guest feedback.", WeightModifiers = new() { { "primary", 3 }, { "cul", 5 } }, NextLevelTags = "cul" }
                    }
                });

                // 3 WorkStyle Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you organize your workflow during a {Title} project?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Staging user testing sessions and gathering feedback on rough wireframes.", WeightModifiers = new() { { "primary", 5 }, { "uiux", 5 } } },
                        new() { TextTemplate = "Creating multiple style guidelines and visual mood boards before final design.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Setting up rendering queues, layer groups, and timeline markers.", WeightModifiers = new() { { "primary", 4 }, { "anim", 5 } } },
                        new() { TextTemplate = "Creating recipe batch calculations and testing menu items.", WeightModifiers = new() { { "primary", 3 }, { "cul", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What is your approach to feedback from clients or directors in {Title}?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Analyzing the feedback using analytical metrics (like conversion rate or user tests).", WeightModifiers = new() { { "primary", 5 }, { "uiux", 5 } } },
                        new() { TextTemplate = "Iterating visual palettes, fonts, and layouts to match the client's vision.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Adapting space planning to accommodate building code and client needs.", WeightModifiers = new() { { "primary", 4 }, { "interior", 5 } } },
                        new() { TextTemplate = "Polishing food taste, spices, and plate presentation details.", WeightModifiers = new() { { "primary", 3 }, { "cul", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When starting a new {Title} project, what do you look at first?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "The design brief, brand guidelines, and visual references.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "User research data, survey responses, and customer journey maps.", WeightModifiers = new() { { "primary", 5 }, { "uiux", 5 } } },
                        new() { TextTemplate = "Physical dimensions of the space, window directions, and layout constraints.", WeightModifiers = new() { { "primary", 4 }, { "interior", 5 } } },
                        new() { TextTemplate = "Kitchen inventory logs, ingredient freshness, and menu pricing.", WeightModifiers = new() { { "primary", 3 }, { "cul", 5 } } }
                    }
                });

                // 3 Aptitude Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "If users find your {Title} layout hard to navigate, how do you diagnose it?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Running heatmaps, scroll-tracking tests, and recording user sessions.", WeightModifiers = new() { { "primary", 5 }, { "uiux", 5 } } },
                        new() { TextTemplate = "Reviewing page grid layouts, line spacing, and contrast ratios.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Performing a walk-through audit of the physical floor space bottlenecks.", WeightModifiers = new() { { "primary", 4 }, { "interior", 5 } } },
                        new() { TextTemplate = "Reviewing social media bounce rates, click-through rates, and drop-offs.", WeightModifiers = new() { { "primary", 3 }, { "dm", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you evaluate if a visual asset in {Title} is successful?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Confirming that it meets all accessibility standards (WCAG contrast, size).", WeightModifiers = new() { { "primary", 5 }, { "uiux", 5 } } },
                        new() { TextTemplate = "Measuring audience engagement, shares, and conversion improvements.", WeightModifiers = new() { { "primary", 5 }, { "dm", 5 } } },
                        new() { TextTemplate = "Inspecting structural safety, material durability, and lighting lumens.", WeightModifiers = new() { { "primary", 4 }, { "interior", 5 } } },
                        new() { TextTemplate = "Verifying consistent food quality, temperature control, and cost margins.", WeightModifiers = new() { { "primary", 3 }, { "cul", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When a client complains about a design delay in {Title}, what is your recovery flow?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Showing progress wireframes immediately to align on the next design direction.", WeightModifiers = new() { { "primary", 5 }, { "uiux", 5 } } },
                        new() { TextTemplate = "Providing a set of alternative visual mockups within a short deadline.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Re-evaluating spatial layouts and presenting a simplified floor blueprint.", WeightModifiers = new() { { "primary", 4 }, { "interior", 5 } } },
                        new() { TextTemplate = "Offering a complimentary tasting session or replacing the menu item.", WeightModifiers = new() { { "primary", 3 }, { "cul", 5 } } }
                    }
                });
                break;

            case "law":
                // 3 Interest Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "What legal domain or practice area in {Title} interests you the most?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Drafting corporate contracts, commercial compliance, and shareholder agreements.", WeightModifiers = new() { { "primary", 5 }, { "corporate", 5 } }, NextLevelTags = "corporate" },
                        new() { TextTemplate = "Defending rights in criminal trials, analyzing evidence, and presenting in court.", WeightModifiers = new() { { "primary", 5 }, { "criminal", 5 } }, NextLevelTags = "criminal" },
                        new() { TextTemplate = "Reviewing lower court judgments, writing legal opinions, and judicial processes.", WeightModifiers = new() { { "primary", 5 }, { "judge", 5 } }, NextLevelTags = "judge" },
                        new() { TextTemplate = "Mediating disputes, settlement negotiations, and family counseling.", WeightModifiers = new() { { "primary", 4 } }, NextLevelTags = "corporate" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "If you were preparing for a trial in {Title}, how would you prepare?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Searching case law databases (SCC/Manupatra) for precedents and judgments.", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "judge" },
                        new() { TextTemplate = "Interviewing witnesses, reviewing crime scene logs, and validating cross-examinations.", WeightModifiers = new() { { "primary", 5 }, { "criminal", 5 } }, NextLevelTags = "criminal" },
                        new() { TextTemplate = "Structuring corporate compliance sheets and reviewing company bylaws.", WeightModifiers = new() { { "primary", 5 }, { "corporate", 5 } }, NextLevelTags = "corporate" },
                        new() { TextTemplate = "Writing legal summaries, briefs, and coordinating administrative courtroom filing.", WeightModifiers = new() { { "primary", 4 } }, NextLevelTags = "judge" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What career milestone in {Title} do you target?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Partner at a tier-1 corporate law firm advising on major acquisitions.", WeightModifiers = new() { { "primary", 5 }, { "corporate", 5 } }, NextLevelTags = "corporate" },
                        new() { TextTemplate = "Leading criminal lawyer representing high-profile justice trials.", WeightModifiers = new() { { "primary", 5 }, { "criminal", 5 } }, NextLevelTags = "criminal" },
                        new() { TextTemplate = "Passing the judicial services exam to become a magistrate in the court.", WeightModifiers = new() { { "primary", 5 }, { "judge", 5 } }, NextLevelTags = "judge" },
                        new() { TextTemplate = "Legal advisor to a prominent international NGO or government policy think-tank.", WeightModifiers = new() { { "primary", 4 } }, NextLevelTags = "corporate" }
                    }
                });

                // 3 WorkStyle Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you organize your legal research in {Title}?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Categorizing judgments by court hierarchy, year, and relevance.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Drafting bulletproof contracts with precise definitions and indemnities.", WeightModifiers = new() { { "primary", 5 }, { "corporate", 5 } } },
                        new() { TextTemplate = "Mapping out defense arguments against prosecution points sequentially.", WeightModifiers = new() { { "primary", 5 }, { "criminal", 5 } } },
                        new() { TextTemplate = "Detailing statutory codes, local regulations, and administrative guidelines.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What is your approach to negotiations or trials in {Title}?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Seeking compromise, amicable mediation, and fast settle terms.", WeightModifiers = new() { { "primary", 4 } } },
                        new() { TextTemplate = "Using strong logical rhetoric, citing precedents, and asserting courtroom presence.", WeightModifiers = new() { { "primary", 5 }, { "criminal", 5 } } },
                        new() { TextTemplate = "Carefully reviewing corporate liabilities to minimize risk exposure.", WeightModifiers = new() { { "primary", 5 }, { "corporate", 5 } } },
                        new() { TextTemplate = "Analyzing all details objectively to deliver a fair, neutral verdict.", WeightModifiers = new() { { "primary", 5 }, { "judge", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When starting on a new client case in {Title}, what do you look at first?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "The exact legal charge sheet, police records, or first information reports.", WeightModifiers = new() { { "primary", 5 }, { "criminal", 5 } } },
                        new() { TextTemplate = "The underlying commercial agreement, equity structures, and financial terms.", WeightModifiers = new() { { "primary", 5 }, { "corporate", 5 } } },
                        new() { TextTemplate = "Constitutional statutes, civil codes, and historical landmark judgments.", WeightModifiers = new() { { "primary", 5 }, { "judge", 5 } } },
                        new() { TextTemplate = "The timeline of events, client testimonials, and written correspondence.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });

                // 3 Aptitude Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "If a contract clause is disputed in {Title}, how do you resolve it?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Analyzing case precedents on similar clauses and proposing an addendum.", WeightModifiers = new() { { "primary", 5 }, { "corporate", 5 } } },
                        new() { TextTemplate = "Filing a motion in court to interpret the statutory meaning of the clause.", WeightModifiers = new() { { "primary", 5 }, { "judge", 5 } } },
                        new() { TextTemplate = "Negotiating a commercial settlement with the opposing legal counsel.", WeightModifiers = new() { { "primary", 4 }, { "corporate", 5 } } },
                        new() { TextTemplate = "Investigating the intent of the parties during the draft communications.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you evaluate if a legal strategy is successful in {Title}?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Whether it successfully secured a favorable verdict or case dismissal.", WeightModifiers = new() { { "primary", 5 }, { "criminal", 5 } } },
                        new() { TextTemplate = "Whether it protected the client's corporate assets from litigation claims.", WeightModifiers = new() { { "primary", 5 }, { "corporate", 5 } } },
                        new() { TextTemplate = "Whether the legal opinion remains consistent with constitutional standards.", WeightModifiers = new() { { "primary", 5 }, { "judge", 5 } } },
                        new() { TextTemplate = "Whether the dispute was resolved with minimal court costs and delays.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When a new regulatory bill is passed in {Title}, what is your reaction?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Updating corporate training guidelines and amending template contracts.", WeightModifiers = new() { { "primary", 5 }, { "corporate", 5 } } },
                        new() { TextTemplate = "Reviewing the bill's constitutionality and drafting public interest challenges.", WeightModifiers = new() { { "primary", 5 }, { "judge", 5 } } },
                        new() { TextTemplate = "Analyzing how it impacts existing litigation cases on the court docket.", WeightModifiers = new() { { "primary", 5 }, { "judge", 5 } } },
                        new() { TextTemplate = "Publishing a legal commentary explaining the practical changes of the law.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });
                break;

            case "government":
                // 3 Interest Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "What area of public service or national administration in {Title} excites you?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Managing district administration, implementing policy, and resolving public grievances.", WeightModifiers = new() { { "primary", 5 }, { "civil", 5 }, { "upsc", 5 } }, NextLevelTags = "civil" },
                        new() { TextTemplate = "Leading military operations, commanding platoons, and guarding national borders.", WeightModifiers = new() { { "primary", 5 }, { "def", 5 } }, NextLevelTags = "def" },
                        new() { TextTemplate = "Inspecting bank operations, audit ledgers, and monetary compliance.", WeightModifiers = new() { { "primary", 5 }, { "bank", 5 } }, NextLevelTags = "bank" },
                        new() { TextTemplate = "Writing state reports, analyzing rural development, and state government operations.", WeightModifiers = new() { { "primary", 4 }, { "state", 5 } }, NextLevelTags = "state" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "How would you prefer to prepare for the {Title} entry process?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Deeply studying Indian Polity, History, Geography, and current affairs.", WeightModifiers = new() { { "primary", 5 }, { "civil", 5 }, { "upsc", 5 } }, NextLevelTags = "civil" },
                        new() { TextTemplate = "Training physical fitness, leadership drills, and passing SSB military boards.", WeightModifiers = new() { { "primary", 5 }, { "def", 5 } }, NextLevelTags = "def" },
                        new() { TextTemplate = "Practicing numerical aptitude, banking law, and general financial awareness.", WeightModifiers = new() { { "primary", 5 }, { "bank", 5 } }, NextLevelTags = "bank" },
                        new() { TextTemplate = "Writing long essay answers on social reforms and public policy.", WeightModifiers = new() { { "primary", 4 }, { "civil", 5 } }, NextLevelTags = "civil" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What responsibility in {Title} sounds most appealing?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Having the authority to lead developmental changes in rural villages.", WeightModifiers = new() { { "primary", 5 }, { "civil", 5 }, { "upsc", 5 } }, NextLevelTags = "civil" },
                        new() { TextTemplate = "Piloting air force fighter jets, commanding naval destroyers, or leading army troops.", WeightModifiers = new() { { "primary", 5 }, { "def", 5 } }, NextLevelTags = "def" },
                        new() { TextTemplate = "Overseeing transactions at a major public sector branch and managing loans.", WeightModifiers = new() { { "primary", 5 }, { "bank", 5 } }, NextLevelTags = "bank" },
                        new() { TextTemplate = "Managing tax collections, state revenues, and administrative audits.", WeightModifiers = new() { { "primary", 4 }, { "civil", 5 } }, NextLevelTags = "civil" }
                    }
                });

                // 3 WorkStyle Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you handle public communication in a {Title} role?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Addressing public assemblies, explaining welfare plans, and resolving protests.", WeightModifiers = new() { { "primary", 5 }, { "civil", 5 }, { "upsc", 5 } } },
                        new() { TextTemplate = "Issuing precise tactical orders and maintaining military chain-of-command.", WeightModifiers = new() { { "primary", 5 }, { "def", 5 } } },
                        new() { TextTemplate = "Explaining interest rates, loan rules, and resolving customer account claims.", WeightModifiers = new() { { "primary", 5 }, { "bank", 5 } } },
                        new() { TextTemplate = "Drafting official press statements and publishing budget files.", WeightModifiers = new() { { "primary", 4 }, { "state", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What is your approach to administrative rules and protocols in {Title}?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Following state rules and official code-of-conduct manuals.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Executing orders with discipline, courage, and tactical operations.", WeightModifiers = new() { { "primary", 5 }, { "def", 5 } } },
                        new() { TextTemplate = "Ensuring absolute monetary audit checks and ledger balances.", WeightModifiers = new() { { "primary", 5 }, { "bank", 5 } } },
                        new() { TextTemplate = "Proposing policy adaptations based on field challenges.", WeightModifiers = new() { { "primary", 5 }, { "civil", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When starting a new deployment in {Title}, what is your focus?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Meeting local village heads, checking budget allocations, and auditing facilities.", WeightModifiers = new() { { "primary", 5 }, { "civil", 5 }, { "upsc", 5 } } },
                        new() { TextTemplate = "Inspecting weapon systems, ammunition reserves, and base security walls.", WeightModifiers = new() { { "primary", 5 }, { "def", 5 } } },
                        new() { TextTemplate = "Reviewing the branch transaction logs and cash vaults.", WeightModifiers = new() { { "primary", 5 }, { "bank", 5 } } },
                        new() { TextTemplate = "Studying local crop yields, irrigation projects, and land record files.", WeightModifiers = new() { { "primary", 4 }, { "state", 5 } } }
                    }
                });

                // 3 Aptitude Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "If a developmental project is delayed due to local protests in {Title}, what is your response?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Holding public discussions with community heads and adjusting compensation.", WeightModifiers = new() { { "primary", 5 }, { "civil", 5 }, { "upsc", 5 } } },
                        new() { TextTemplate = "Deploying patrol forces to guard the site boundaries and maintain order.", WeightModifiers = new() { { "primary", 5 }, { "def", 5 } } },
                        new() { TextTemplate = "Auditing the local project budget to check for financial leakage.", WeightModifiers = new() { { "primary", 4 }, { "civil", 5 } } },
                        new() { TextTemplate = "Filing a legal response defending state project authorizations.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you evaluate if a public policy is successful in {Title}?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Whether it reduced poverty metrics, improved literacy, and helped village heads.", WeightModifiers = new() { { "primary", 5 }, { "civil", 5 }, { "upsc", 5 } } },
                        new() { TextTemplate = "Whether the military platoon achieved its strategic defense objectives safely.", WeightModifiers = new() { { "primary", 5 }, { "def", 5 } } },
                        new() { TextTemplate = "Whether the banking branch met its annual loan recovery targets.", WeightModifiers = new() { { "primary", 5 }, { "bank", 5 } } },
                        new() { TextTemplate = "Whether it increased state revenue collections without tax conflicts.", WeightModifiers = new() { { "primary", 4 }, { "state", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When a sudden emergency occurs in your jurisdiction in {Title}, what is your flow?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Establishing a disaster control center, allocating food, and coordinating police.", WeightModifiers = new() { { "primary", 5 }, { "civil", 5 }, { "upsc", 5 } } },
                        new() { TextTemplate = "Enforcing immediate tactical lockdowns and initiating counter-ops.", WeightModifiers = new() { { "primary", 5 }, { "def", 5 } } },
                        new() { TextTemplate = "Freezing disputed accounts and reporting transactions to the central bank.", WeightModifiers = new() { { "primary", 5 }, { "bank", 5 } } },
                        new() { TextTemplate = "Drafting an emergency report to the state ministry requesting funds.", WeightModifiers = new() { { "primary", 4 }, { "state", 5 } } }
                    }
                });
                break;

            case "entrepreneurship":
                // 3 Interest Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "What aspect of startup building excites you the most in {Title}?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Validating a tech product idea, coding the MVP, and launching to users.", WeightModifiers = new() { { "primary", 5 }, { "startup", 5 } }, NextLevelTags = "startup" },
                        new() { TextTemplate = "Designing product packaging, sourcing inventory, and scaling D2C e-commerce.", WeightModifiers = new() { { "primary", 5 }, { "d2c", 5 } }, NextLevelTags = "d2c" },
                        new() { TextTemplate = "Running growth marketing campaigns, SEO hacks, and scaling customer acquisition.", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "startup" },
                        new() { TextTemplate = "Pitching business plans to venture capitalists and raising funding rounds.", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "startup" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "Which challenge would you enjoy tackling in {Title}?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Finding product-market fit by interviewing early users and pivoting features.", WeightModifiers = new() { { "primary", 5 }, { "startup", 5 } }, NextLevelTags = "startup" },
                        new() { TextTemplate = "Optimizing supply chain shipping costs and manufacturing pipeline efficiency.", WeightModifiers = new() { { "primary", 5 }, { "d2c", 5 } }, NextLevelTags = "d2c" },
                        new() { TextTemplate = "Recruiting a core founding team of developers and designers.", WeightModifiers = new() { { "primary", 4 } }, NextLevelTags = "startup" },
                        new() { TextTemplate = "Analyzing financial runways, burn rates, and unit economics metrics.", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "startup" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What is your primary goal as a {Title} founder?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Building a high-growth technology company that disrupts an industry.", WeightModifiers = new() { { "primary", 5 }, { "startup", 5 } }, NextLevelTags = "startup" },
                        new() { TextTemplate = "Creating a beloved consumer brand with premium, sustainable products.", WeightModifiers = new() { { "primary", 5 }, { "d2c", 5 } }, NextLevelTags = "d2c" },
                        new() { TextTemplate = "Achieving financial independence and building a profitable lifestyle business.", WeightModifiers = new() { { "primary", 4 } }, NextLevelTags = "d2c" },
                        new() { TextTemplate = "Developing an innovative solution that drives significant social impact.", WeightModifiers = new() { { "primary", 4 } }, NextLevelTags = "startup" }
                    }
                });

                // 3 WorkStyle Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you organize daily operations as a {Title} founder?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Focusing on product design, developer sprints, and testing code features.", WeightModifiers = new() { { "primary", 5 }, { "startup", 5 } } },
                        new() { TextTemplate = "Managing shipping tracking logs, factory manufacturing, and inventory orders.", WeightModifiers = new() { { "primary", 5 }, { "d2c", 5 } } },
                        new() { TextTemplate = "Reviewing daily sales funnels, marketing copy, and customer support charts.", WeightModifiers = new() { { "primary", 4 } } },
                        new() { TextTemplate = "Conducting investor updates, financial modeling, and strategy workshops.", WeightModifiers = new() { { "primary", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What is your approach to dealing with competitive market changes in {Title}?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Quickly iterating software features and launching product upgrades.", WeightModifiers = new() { { "primary", 5 }, { "startup", 5 } } },
                        new() { TextTemplate = "Differentiating product quality, packaging, and brand storytelling.", WeightModifiers = new() { { "primary", 5 }, { "d2c", 5 } } },
                        new() { TextTemplate = "Aggressively adjusting price discounts and running marketing blitzes.", WeightModifiers = new() { { "primary", 4 } } },
                        new() { TextTemplate = "Focusing on niche customer retention and building community loyalty.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When launching a new startup initiative for {Title}, what do you do first?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Building a quick landing page to gather email signups and validate interest.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Inspecting competitive products, pricing, and manufacturing options.", WeightModifiers = new() { { "primary", 5 }, { "d2c", 5 } } },
                        new() { TextTemplate = "Creating a detailed pro-forma financial sheet showing cash runway.", WeightModifiers = new() { { "primary", 4 } } },
                        new() { TextTemplate = "Recruiting a technical co-founder to lead the product build.", WeightModifiers = new() { { "primary", 5 }, { "startup", 5 } } }
                    }
                });

                // 3 Aptitude Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "If user growth plateaus for {Title}, how do you diagnose it?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Analyzing funnel drop-offs, registration flows, and user retention charts.", WeightModifiers = new() { { "primary", 5 }, { "startup", 5 } } },
                        new() { TextTemplate = "Reviewing customer feedback on product quality, shipping, and packaging.", WeightModifiers = new() { { "primary", 5 }, { "d2c", 5 } } },
                        new() { TextTemplate = "A/B testing marketing copy, ad placements, and discount structures.", WeightModifiers = new() { { "primary", 4 } } },
                        new() { TextTemplate = "Analyzing customer churn rate and net promoter scores (NPS).", WeightModifiers = new() { { "primary", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you evaluate if your {Title} startup is ready to scale?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "When customer lifetime value (LTV) is at least three times the acquisition cost (CAC).", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "When product manufacturing can be automated and inventory runs smoothly.", WeightModifiers = new() { { "primary", 5 }, { "d2c", 5 } } },
                        new() { TextTemplate = "When you have achieved consistent month-over-month revenue growth of over 15%.", WeightModifiers = new() { { "primary", 5 }, { "startup", 5 } } },
                        new() { TextTemplate = "When you have a fully functional team that operates without founder intervention.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When cash runway drops below 3 months in {Title}, what is your action?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Immediately reducing operational expenses and pausing marketing experiments.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Launching a crowdfunding campaign or running an inventory clearance sale.", WeightModifiers = new() { { "primary", 5 }, { "d2c", 5 } } },
                        new() { TextTemplate = "Accelerating pitch meetings with seed investors and venture capital firms.", WeightModifiers = new() { { "primary", 5 }, { "startup", 5 } } },
                        new() { TextTemplate = "Pivoting the business model to focus only on highly profitable services.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });
                break;

            case "vocational":
            default:
                // 3 Interest Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "What vocational trade or hands-on practice in {Title} excites you?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Assembling electrical wiring layouts, switchboards, and power controls.", WeightModifiers = new() { { "primary", 5 }, { "elec", 5 } }, NextLevelTags = "elec" },
                        new() { TextTemplate = "Operating lathes, fitting machinery components, and checking dimensions.", WeightModifiers = new() { { "primary", 5 }, { "fitter", 5 } }, NextLevelTags = "fitter" },
                        new() { TextTemplate = "Performing arc welding, structural steel joining, and blueprint reading.", WeightModifiers = new() { { "primary", 5 }, { "welder", 5 } }, NextLevelTags = "welder" },
                        new() { TextTemplate = "Designing piping layouts, fitting valves, and hot-water heating plumbing.", WeightModifiers = new() { { "primary", 5 }, { "plumb", 5 } }, NextLevelTags = "plumb" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What hands-on task would you prefer to perform in {Title}?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Installing solar panel converters and residential circuit breakers.", WeightModifiers = new() { { "primary", 5 }, { "elec", 5 } }, NextLevelTags = "elec" },
                        new() { TextTemplate = "Diagnosing engine failures, transmission leaks, and doing vehicle tune-ups.", WeightModifiers = new() { { "primary", 5 }, { "mech", 5 } }, NextLevelTags = "mech" },
                        new() { TextTemplate = "Fabricating metal support gates and welding heavy machinery parts.", WeightModifiers = new() { { "primary", 5 }, { "welder", 5 } }, NextLevelTags = "welder" },
                        new() { TextTemplate = "Installing central heating systems, sewer drain pipes, and water fixtures.", WeightModifiers = new() { { "primary", 5 }, { "plumb", 5 } }, NextLevelTags = "plumb" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What working environment do you prefer in {Title}?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "On-site industrial workshop troubleshooting automation hardware.", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "fitter" },
                        new() { TextTemplate = "Construction site overseeing electrical or plumbing installations.", WeightModifiers = new() { { "primary", 5 }, { "elec", 5 } }, NextLevelTags = "elec" },
                        new() { TextTemplate = "Manufacturing factory floor supervising machinery calibration.", WeightModifiers = new() { { "primary", 5 }, { "fitter", 5 } }, NextLevelTags = "fitter" },
                        new() { TextTemplate = "Automobile repair garage fixing mechanical systems.", WeightModifiers = new() { { "primary", 5 }, { "mech", 5 } }, NextLevelTags = "mech" }
                    }
                });

                // 3 WorkStyle Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you organize your work day in a {Title} trade role?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Reviewing blueprint schematics and preparing tool lists.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Inspecting wiring diagrams, testing currents, and calibrating voltage.", WeightModifiers = new() { { "primary", 5 }, { "elec", 5 } } },
                        new() { TextTemplate = "Running lathe tests, checking calipers, and lubricating machinery parts.", WeightModifiers = new() { { "primary", 5 }, { "fitter", 5 } } },
                        new() { TextTemplate = "Inspecting welding joints using dye penetrants and safety audits.", WeightModifiers = new() { { "primary", 5 }, { "welder", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What is your approach to work safety and tool maintenance in {Title}?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Sticking strictly to standard safety guidelines, wearing PPE, and safety locks.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Keeping tools organized, clean, and calibrated after every shift.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Replacing worn machinery parts and running scheduled maintenance.", WeightModifiers = new() { { "primary", 5 }, { "mech", 5 } } },
                        new() { TextTemplate = "Verifying pipe pressure thresholds and testing for leaks.", WeightModifiers = new() { { "primary", 5 }, { "plumb", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When starting a new installation job for {Title}, what do you verify first?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "The structural blueprints, technical drawings, and tool specs.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "The main power grid isolation, circuit breakers, and grounding.", WeightModifiers = new() { { "primary", 5 }, { "elec", 5 } } },
                        new() { TextTemplate = "The machinery alignment, lubrication levels, and bolt torques.", WeightModifiers = new() { { "primary", 5 }, { "mech", 5 } } },
                        new() { TextTemplate = "The water supply pressures, valve seals, and piping routes.", WeightModifiers = new() { { "primary", 5 }, { "plumb", 5 } } }
                    }
                });

                // 3 Aptitude Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "If a machine or circuit fails to start in {Title}, how do you diagnose it?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Tracing voltage drops using a multimeter across circuit links.", WeightModifiers = new() { { "primary", 5 }, { "elec", 5 } } },
                        new() { TextTemplate = "Checking mechanical logs, inspecting belts, and looking for mechanical wear.", WeightModifiers = new() { { "primary", 5 }, { "mech", 5 } } },
                        new() { TextTemplate = "Testing the structural integrity of welded brackets under stress.", WeightModifiers = new() { { "primary", 5 }, { "welder", 5 } } },
                        new() { TextTemplate = "Performing a pressure test to find pipes containing blocks or leaks.", WeightModifiers = new() { { "primary", 5 }, { "plumb", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you evaluate if a task is successfully completed in {Title}?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Verifying that circuit voltage levels are consistent and safe.", WeightModifiers = new() { { "primary", 5 }, { "elec", 5 } } },
                        new() { TextTemplate = "Measuring dimensions with a digital micrometer to match specifications.", WeightModifiers = new() { { "primary", 5 }, { "fitter", 5 } } },
                        new() { TextTemplate = "Ensuring welds are clean, free of cracks, and pass safety inspections.", WeightModifiers = new() { { "primary", 5 }, { "welder", 5 } } },
                        new() { TextTemplate = "Running a full pressure test on the piping network without any leaks.", WeightModifiers = new() { { "primary", 5 }, { "plumb", 5 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When a sudden component failure happens during operations in {Title}, what do you do?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Immediately triggering the main emergency power cut-off switch.", WeightModifiers = new() { { "primary", 5 }, { "elec", 5 } } },
                        new() { TextTemplate = "Stopping the engine and using mechanical lock-out tag-out protocols.", WeightModifiers = new() { { "primary", 5 }, { "mech", 5 } } },
                        new() { TextTemplate = "Inspecting the failed metal joint and planning a reinforce weld.", WeightModifiers = new() { { "primary", 5 }, { "welder", 5 } } },
                        new() { TextTemplate = "Closing the main water shut-off valve to prevent flooding.", WeightModifiers = new() { { "primary", 5 }, { "plumb", 5 } } }
                    }
                });
                break;

            case "tech_software":
                // 3 Interest Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "What type of software development tasks excite you the most when working with {Title}?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Building high-performance APIs and optimizing server databases.", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "back" },
                        new() { TextTemplate = "Designing responsive UI components, micro-frontends, and user flows.", WeightModifiers = new() { { "primary", 3 }, { "design", 5 } }, NextLevelTags = "front" },
                        new() { TextTemplate = "Architecting cloud infrastructure, serverless functions, and CI/CD pipelines.", WeightModifiers = new() { { "primary", 3 }, { "devops", 5 } }, NextLevelTags = "devops" },
                        new() { TextTemplate = "Working on game physics, graphics pipelines, or interactive simulations.", WeightModifiers = new() { { "primary", 2 } }, NextLevelTags = "game" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "If you were to work on a weekend hackathon project for {Title}, what would you choose?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Creating a real-time web application using a modern fullstack framework.", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "full" },
                        new() { TextTemplate = "Developing an embedded system driver for IoT devices or microcontrollers.", WeightModifiers = new() { { "primary", 4 } }, NextLevelTags = "embed" },
                        new() { TextTemplate = "Securing systems, performing vulnerability scans, and pen-testing APIs.", WeightModifiers = new() { { "primary", 4 } }, NextLevelTags = "cyber" },
                        new() { TextTemplate = "Writing smart contracts and deploying decentralized Web3 applications.", WeightModifiers = new() { { "primary", 3 } }, NextLevelTags = "block" }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "Which aspect of technology system design for {Title} interests you the most?",
                    AptitudeType = "Interest",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Scaling applications to handle millions of requests with message queues.", WeightModifiers = new() { { "primary", 5 } }, NextLevelTags = "arch" },
                        new() { TextTemplate = "Optimizing application bundle sizes and client-side rendering speeds.", WeightModifiers = new() { { "primary", 4 } }, NextLevelTags = "front" },
                        new() { TextTemplate = "Establishing zero-downtime deployments and containerized cluster orchestration.", WeightModifiers = new() { { "primary", 4 }, { "devops", 5 } }, NextLevelTags = "devops" },
                        new() { TextTemplate = "Writing clear, modular clean-code documentation for developers.", WeightModifiers = new() { { "primary", 3 } }, NextLevelTags = "se" }
                    }
                });

                // 3 WorkStyle Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you prefer to collaborate with your team when shipping {Title} features?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Focusing on writing clean code, pull request reviews, and refactoring.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Coordinating with product managers to scope features and translate requirements.", WeightModifiers = new() { { "primary", 4 } } },
                        new() { TextTemplate = "Troubleshooting infrastructure issues, on-call alert systems, and post-mortems.", WeightModifiers = new() { { "primary", 4 } } },
                        new() { TextTemplate = "Conducting design sprints and prototyping mockups with Figma.", WeightModifiers = new() { { "primary", 2 }, { "design", 4 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "What is your philosophy on adopting new developer tools and frameworks for {Title}?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Pioneering state-of-the-art libraries even if they have unstable ecosystems.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Standardizing mature, battle-tested programming patterns to ensure system longevity.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Automating script configurations and build steps to minimize developer friction.", WeightModifiers = new() { { "primary", 4 }, { "devops", 5 } } },
                        new() { TextTemplate = "Sticking strictly to official SDK guidelines and hardware spec sheets.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When joining a new {Title} project, what is your first step?",
                    AptitudeType = "WorkStyle",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Reviewing the repository codebase, folder structure, and existing unit tests.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Checking the system architecture diagram and database entity relations.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Running the project locally and inspecting the network calls in developer tools.", WeightModifiers = new() { { "primary", 4 } } },
                        new() { TextTemplate = "Inspecting the Dockerfile, env settings, and staging deployment pipeline.", WeightModifiers = new() { { "primary", 4 }, { "devops", 5 } } }
                    }
                });

                // 3 Aptitude Questions
                list.Add(new QuestionTemplate {
                    TextTemplate = "If you encounter a performance bottleneck in your {Title} application, how do you diagnose it?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Using profiling tools to trace slow database queries, indexes, and locking.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Inspecting chrome performance timelines for long tasks and main-thread blocks.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Monitoring server CPU, memory utilization, and container garbage collection.", WeightModifiers = new() { { "primary", 4 } } },
                        new() { TextTemplate = "Analyzing network bandwidth limits and CDN caching configurations.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "How do you ensure code safety, reliability, and security in your {Title} project?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Setting up automated unit/integration tests with high code coverage metrics.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Enforcing strict static typing, dependency injection, and architectural patterns.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Configuring security scan actions in CI/CD and implementing JWT/OAuth best practices.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Testing error handling bounds, hardware interrupts, and power consumption.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });
                list.Add(new QuestionTemplate {
                    TextTemplate = "When a critical bug is reported in production for {Title}, what is your debugging flow?",
                    AptitudeType = "Aptitude",
                    Options = new List<OptionTemplate> {
                        new() { TextTemplate = "Searching centralized server logs (Kibana/Datadog) for exact stack traces.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Isolating the issue using test suites and step-through debuggers.", WeightModifiers = new() { { "primary", 5 } } },
                        new() { TextTemplate = "Reverting the recent deployment branch instantly to restore system health.", WeightModifiers = new() { { "primary", 4 }, { "devops", 5 } } },
                        new() { TextTemplate = "Simulating user network throttles or local storage conditions to reproduce the bug.", WeightModifiers = new() { { "primary", 4 } } }
                    }
                });
                break;
        }

        return list;
    }

    private static async Task SeedHierarchicalQuizQuestionsAsync(ApplicationDbContext context)
    {
        if (context.HierarchicalQuizQuestions.Any())
        {
            context.HierarchicalQuizQuestions.RemoveRange(context.HierarchicalQuizQuestions);
            await context.SaveChangesAsync();
        }

        var paths = await context.CareerPaths.ToListAsync();
        var questionsList = new List<HierarchicalQuizQuestion>();

        // Seed base Level 1 questions (Interest & Aptitude) tailored for each of the 9 streams
        var level1Questions = new List<HierarchicalQuizQuestion>
        {
            // After 10th Grade
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-10th-i",
                QuestionText = "Which academic discipline or domain of study excites you the most as you think about your next step?",
                HierarchyLevel = 1,
                StreamType = "After10th",
                AptitudeType = "Interest",
                TargetCareerTags = "sci,com,art,dip",
                OptionsJson = "[" +
                              "{\"text\": \"Scientific concepts, laboratory experiments, or mathematics.\", \"weights\": {\"sci\": 3, \"pcm\": 2, \"pcb\": 2}, \"nextLevelTags\": \"sci\"}," +
                              "{\"text\": \"Corporate trade, accounting, household budgets, and business news.\", \"weights\": {\"com\": 3, \"finance\": 2}, \"nextLevelTags\": \"com\"}," +
                              "{\"text\": \"Writing stories, historical analysis, painting, or media journalism.\", \"weights\": {\"art\": 3, \"creative\": 2}, \"nextLevelTags\": \"art\"}," +
                              "{\"text\": \"Practical hands-on workshops, repair tools, or polytechnic trades.\", \"weights\": {\"dip\": 3, \"vocational\": 2}, \"nextLevelTags\": \"dip\"}" +
                              "]"
            },
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-10th-a",
                QuestionText = "How do you prefer to solve problems or complete school projects?",
                HierarchyLevel = 1,
                StreamType = "After10th",
                AptitudeType = "Aptitude",
                TargetCareerTags = "sci,com,art,dip",
                OptionsJson = "[" +
                              "{\"text\": \"By analyzing scientific data, formula equations, or logical proofs.\", \"weights\": {\"sci\": 3, \"pcm\": 2}, \"nextLevelTags\": \"sci\"}," +
                              "{\"text\": \"By managing resource allocations, cost estimation, or leadership organization.\", \"weights\": {\"com\": 3, \"finance\": 2}, \"nextLevelTags\": \"com\"}," +
                              "{\"text\": \"By designing visual displays, writing reports, or verbal presentations.\", \"weights\": {\"art\": 3, \"creative\": 2}, \"nextLevelTags\": \"art\"}," +
                              "{\"text\": \"By physically assembling components, fixing wires, or testing mechanisms.\", \"weights\": {\"dip\": 3, \"polytech\": 2}, \"nextLevelTags\": \"dip\"}" +
                              "]"
            },

            // After 12th Grade
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-12th-i",
                QuestionText = "Given your stream background in 11th-12th grade, what type of degree or domain matches your ambitions?",
                HierarchyLevel = 1,
                StreamType = "After12th",
                AptitudeType = "Interest",
                TargetCareerTags = "pcm,pcb,com,art",
                OptionsJson = "[" +
                              "{\"text\": \"Engineering, code development, architecture, or technical sciences.\", \"weights\": {\"pcm\": 3, \"eng\": 2}, \"nextLevelTags\": \"pcm\"}," +
                              "{\"text\": \"Clinical medicine, dental care, nursing, or biological research.\", \"weights\": {\"pcb\": 3, \"med\": 2}, \"nextLevelTags\": \"pcb\"}," +
                              "{\"text\": \"Chartered accountancy, stocks, corporate business, or economics.\", \"weights\": {\"com\": 3, \"fin\": 2}, \"nextLevelTags\": \"com\"}," +
                              "{\"text\": \"Legal advocacy, product design, humanities, or civil service study.\", \"weights\": {\"art\": 3, \"law\": 2}, \"nextLevelTags\": \"art\"}" +
                              "]"
            },
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-12th-a",
                QuestionText = "What kind of complex problems are you most eager to study and solve in your career?",
                HierarchyLevel = 1,
                StreamType = "After12th",
                AptitudeType = "Aptitude",
                TargetCareerTags = "pcm,pcb,com,art",
                OptionsJson = "[" +
                              "{\"text\": \"Designing algorithms, debugging software scripts, or building structures.\", \"weights\": {\"pcm\": 3, \"eng\": 2}, \"nextLevelTags\": \"pcm\"}," +
                              "{\"text\": \"Diagnosing patient symptoms, reviewing pathology, or drug chemical profiles.\", \"weights\": {\"pcb\": 3, \"med\": 2}, \"nextLevelTags\": \"pcb\"}," +
                              "{\"text\": \"Analyzing financial statements, auditing ledgers, or business trends.\", \"weights\": {\"com\": 3, \"fin\": 2}, \"nextLevelTags\": \"com\"}," +
                              "{\"text\": \"Drafting legal defense cases, wireframing digital apps, or writing policy.\", \"weights\": {\"art\": 3, \"law\": 2}, \"nextLevelTags\": \"art\"}" +
                              "]"
            },

            // After Graduation
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-grad-i",
                QuestionText = "As a university graduate, which professional sector align with your post-graduation career goals?",
                HierarchyLevel = 1,
                StreamType = "AfterGraduation",
                AptitudeType = "Interest",
                TargetCareerTags = "eng,cs,bus,med,art",
                OptionsJson = "[" +
                              "{\"text\": \"Technical software engineering, data science pipelines, or IT infrastructure.\", \"weights\": {\"eng\": 3, \"cs\": 2}, \"nextLevelTags\": \"cs\"}," +
                              "{\"text\": \"Business operations, MBA consulting, corporate finance, or marketing.\", \"weights\": {\"bus\": 3, \"mgmt\": 2}, \"nextLevelTags\": \"bus\"}," +
                              "{\"text\": \"Clinical healthcare practice, pharmacy operations, or nursing care.\", \"weights\": {\"med\": 3, \"hc\": 2}, \"nextLevelTags\": \"med\"}," +
                              "{\"text\": \"Creative brand design, interior layouts, movie VFX, or photography.\", \"weights\": {\"art\": 3, \"des\": 2}, \"nextLevelTags\": \"art\"}" +
                              "]"
            },
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-grad-a",
                QuestionText = "Which advanced professional skill set do you wish to leverage daily?",
                HierarchyLevel = 1,
                StreamType = "AfterGraduation",
                AptitudeType = "Aptitude",
                TargetCareerTags = "eng,cs,bus,med,art",
                OptionsJson = "[" +
                              "{\"text\": \"Fullstack coding scripts, database profiling, or DevOps builds.\", \"weights\": {\"eng\": 3, \"cs\": 2}, \"nextLevelTags\": \"cs\"}," +
                              "{\"text\": \"Corporate team coordination, budgeting spreadsheets, or brand strategy.\", \"weights\": {\"bus\": 3, \"mgmt\": 2}, \"nextLevelTags\": \"bus\"}," +
                              "{\"text\": \"Patient clinical diagnosis, radiological scanning, or pharmaceutical audits.\", \"weights\": {\"med\": 3, \"hc\": 2}, \"nextLevelTags\": \"med\"}," +
                              "{\"text\": \"UI/UX screen wireframing, industrial prototype models, or fashion sketches.\", \"weights\": {\"art\": 3, \"des\": 2}, \"nextLevelTags\": \"art\"}" +
                              "]"
            },

            // After Post Graduation
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-pg-i",
                QuestionText = "With your postgraduate degree completed, what type of advanced work environment do you target?",
                HierarchyLevel = 1,
                StreamType = "AfterPostGraduation",
                AptitudeType = "Interest",
                TargetCareerTags = "phd,prof",
                OptionsJson = "[" +
                              "{\"text\": \"Academic research labs, writing thesis papers, or university teaching.\", \"weights\": {\"phd\": 3, \"academic\": 2}, \"nextLevelTags\": \"phd\"}," +
                              "{\"text\": \"Specialist corporate consulting, lead engineering practice, or clinical consulting.\", \"weights\": {\"prof\": 3, \"practice\": 2}, \"nextLevelTags\": \"prof\"}" +
                              "]"
            },
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-pg-a",
                QuestionText = "How do you prefer to apply your advanced specialist knowledge?",
                HierarchyLevel = 1,
                StreamType = "AfterPostGraduation",
                AptitudeType = "Aptitude",
                TargetCareerTags = "phd,prof",
                OptionsJson = "[" +
                              "{\"text\": \"By publishing scientific discoveries, reviews, and leading lectures.\", \"weights\": {\"phd\": 3, \"academic\": 2}, \"nextLevelTags\": \"phd\"}," +
                              "{\"text\": \"By resolving enterprise problems, patents, and senior team deliverables.\", \"weights\": {\"prof\": 3, \"practice\": 2}, \"nextLevelTags\": \"prof\"}" +
                              "]"
            },

            // Career Switch
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-switch-i",
                QuestionText = "Which high-growth digital industry are you looking to switch into?",
                HierarchyLevel = 1,
                StreamType = "CareerSwitch",
                AptitudeType = "Interest",
                TargetCareerTags = "tech,mgmt,des",
                OptionsJson = "[" +
                              "{\"text\": \"Software development, mobile apps coding, or cloud architecture.\", \"weights\": {\"tech\": 3, \"cs\": 2}, \"nextLevelTags\": \"tech\"}," +
                              "{\"text\": \"Product management, business analysis, or scrum facilitation.\", \"weights\": {\"mgmt\": 3, \"prod\": 2}, \"nextLevelTags\": \"mgmt\"}," +
                              "{\"text\": \"User interface (UI/UX) design, graphic wireframes, or digital assets creation.\", \"weights\": {\"des\": 3, \"uiux\": 2}, \"nextLevelTags\": \"des\"}" +
                              "]"
            },
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-switch-a",
                QuestionText = "What is your main transferable skill or alignment for this career switch?",
                HierarchyLevel = 1,
                StreamType = "CareerSwitch",
                AptitudeType = "Aptitude",
                TargetCareerTags = "tech,mgmt,des",
                OptionsJson = "[" +
                              "{\"text\": \"I enjoy writing automation scripts, logic, and building functional tools.\", \"weights\": {\"tech\": 3, \"cs\": 2}, \"nextLevelTags\": \"tech\"}," +
                              "{\"text\": \"I excel at coordinate schedules, translating guidelines, and public communication.\", \"weights\": {\"mgmt\": 3, \"prod\": 2}, \"nextLevelTags\": \"mgmt\"}," +
                              "{\"text\": \"I have a strong eye for visual layouts, color patterns, and user flows.\", \"weights\": {\"des\": 3, \"uiux\": 2}, \"nextLevelTags\": \"des\"}" +
                              "]"
            },

            // Upskilling
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-upskill-i",
                QuestionText = "Which tech specialization area represents your target upskilling certification?",
                HierarchyLevel = 1,
                StreamType = "Upskilling",
                AptitudeType = "Interest",
                TargetCareerTags = "cloud,ai,cyber",
                OptionsJson = "[" +
                              "{\"text\": \"Cloud environments (AWS/Azure/GCP), container pipelines, or DevOps.\", \"weights\": {\"cloud\": 3, \"devops\": 2}, \"nextLevelTags\": \"cloud\"}," +
                              "{\"text\": \"Deep learning models, neural weights tuning, Python data profiling, or ML.\", \"weights\": {\"ai\": 3, \"ml\": 2}, \"nextLevelTags\": \"ai\"}," +
                              "{\"text\": \"System vulnerability scanning, API pen-testing, or security compliance.\", \"weights\": {\"cyber\": 3, \"security\": 2}, \"nextLevelTags\": \"cyber\"}" +
                              "]"
            },
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-upskill-a",
                QuestionText = "Which technical challenge aligns with your current level of professional upgrade?",
                HierarchyLevel = 1,
                StreamType = "Upskilling",
                AptitudeType = "Aptitude",
                TargetCareerTags = "cloud,ai,cyber",
                OptionsJson = "[" +
                              "{\"text\": \"Automating CI/CD pipelines, scaling database nodes, or server config.\", \"weights\": {\"cloud\": 3, \"devops\": 2}, \"nextLevelTags\": \"cloud\"}," +
                              "{\"text\": \"Fine-tuning neural transformer parameters or clean dataset parsing.\", \"weights\": {\"ai\": 3, \"ml\": 2}, \"nextLevelTags\": \"ai\"}," +
                              "{\"text\": \"Setting up firewalls, verifying audit logs, or patching vulnerabilities.\", \"weights\": {\"cyber\": 3, \"security\": 2}, \"nextLevelTags\": \"cyber\"}" +
                              "]"
            },

            // Entrepreneurship
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-ent-i",
                QuestionText = "What type of startup business model do you wish to launch as a founder?",
                HierarchyLevel = 1,
                StreamType = "Entrepreneurship",
                AptitudeType = "Interest",
                TargetCareerTags = "startup,d2c",
                OptionsJson = "[" +
                              "{\"text\": \"High-growth software-as-a-service (SaaS) or mobile app tools.\", \"weights\": {\"startup\": 3, \"tech\": 2}, \"nextLevelTags\": \"startup\"}," +
                              "{\"text\": \"Consumer brand (D2C), direct manufacturing, or e-commerce shop.\", \"weights\": {\"d2c\": 3, \"brand\": 2}, \"nextLevelTags\": \"d2c\"}" +
                              "]"
            },
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-ent-a",
                QuestionText = "How do you plan to validate your business idea and launch to early users?",
                HierarchyLevel = 1,
                StreamType = "Entrepreneurship",
                AptitudeType = "Aptitude",
                TargetCareerTags = "startup,d2c",
                OptionsJson = "[" +
                              "{\"text\": \"By writing a clean software prototype and hosting it on startup directories.\", \"weights\": {\"startup\": 3, \"tech\": 2}, \"nextLevelTags\": \"startup\"}," +
                              "{\"text\": \"By sourcing initial supplier samples, packaging, and social marketing campaigns.\", \"weights\": {\"d2c\": 3, \"brand\": 2}, \"nextLevelTags\": \"d2c\"}" +
                              "]"
            },

            // Government Jobs
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-gov-i",
                QuestionText = "Which public administration sector represents your career goal?",
                HierarchyLevel = 1,
                StreamType = "GovernmentJobs",
                AptitudeType = "Interest",
                TargetCareerTags = "civil,bank,def",
                OptionsJson = "[" +
                              "{\"text\": \"IAS/IPS civil services administrative desks, public welfare, and state policy.\", \"weights\": {\"civil\": 3, \"upsc\": 2}, \"nextLevelTags\": \"civil\"}," +
                              "{\"text\": \"Banking officer (PO) branch operations, audit compliance, or RBI reserves.\", \"weights\": {\"bank\": 3, \"finance\": 2}, \"nextLevelTags\": \"bank\"}," +
                              "{\"text\": \"Military platoon command, fighter jet operations, or naval commission.\", \"weights\": {\"def\": 3, \"military\": 2}, \"nextLevelTags\": \"def\"}" +
                              "]"
            },
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-gov-a",
                QuestionText = "Which national level screening process best aligns with your strengths?",
                HierarchyLevel = 1,
                StreamType = "GovernmentJobs",
                AptitudeType = "Aptitude",
                TargetCareerTags = "civil,bank,def",
                OptionsJson = "[" +
                              "{\"text\": \"Solving GS descriptive essay questions, history, polity, and interviews.\", \"weights\": {\"civil\": 3, \"upsc\": 2}, \"nextLevelTags\": \"civil\"}," +
                              "{\"text\": \"Speed quantitative aptitude tests, banking codes, and logical puzzles.\", \"weights\": {\"bank\": 3, \"finance\": 2}, \"nextLevelTags\": \"bank\"}," +
                              "{\"text\": \"SSB officer commission tasks, physical drills, and tactical command tests.\", \"weights\": {\"def\": 3, \"military\": 2}, \"nextLevelTags\": \"def\"}" +
                              "]"
            },

            // International Education
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-intl-i",
                QuestionText = "Which global postgraduate program format are you planning to apply for?",
                HierarchyLevel = 1,
                StreamType = "InternationalEducation",
                AptitudeType = "Interest",
                TargetCareerTags = "ms,mba",
                OptionsJson = "[" +
                              "{\"text\": \"Master of Science (MS) in STEM domains focusing on laboratory research.\", \"weights\": {\"ms\": 3, \"stem\": 2}, \"nextLevelTags\": \"ms\"}," +
                              "{\"text\": \"Global Master of Business Administration (MBA) focusing on consulting/strategy.\", \"weights\": {\"mba\": 3, \"business\": 2}, \"nextLevelTags\": \"mba\"}" +
                              "]"
            },
            new HierarchicalQuizQuestion
            {
                Id = "hq-lvl1-intl-a",
                QuestionText = "What is your primary expected post-study career placement outcome?",
                HierarchyLevel = 1,
                StreamType = "InternationalEducation",
                AptitudeType = "Aptitude",
                TargetCareerTags = "ms,mba",
                OptionsJson = "[" +
                              "{\"text\": \"Joining corporate R&D divisions or specialized technical roles abroad.\", \"weights\": {\"ms\": 3, \"stem\": 2}, \"nextLevelTags\": \"ms\"}," +
                              "{\"text\": \"Entering international investment banks, global consulting, or strategy desks.\", \"weights\": {\"mba\": 3, \"business\": 2}, \"nextLevelTags\": \"mba\"}" +
                              "]"
            }
        };
        questionsList.AddRange(level1Questions);

        // For each career path at Level 2, 3, 4, 5, generate domain-specific questions
        var targetPaths = paths.Where(p => p.Level >= 2).ToList();

        foreach (var path in targetPaths)
        {
            var pathTags = path.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            string primaryTag = pathTags.LastOrDefault() ?? "general";
            string category = GetRoadmapCategory(path.Id);
            var templates = GetTemplatesForCategory(category);

            // Generate 3 Interest questions
            for (int i = 1; i <= 3; i++)
            {
                var template = templates[i - 1];
                var optionsList = new List<object>();

                foreach (var opt in template.Options)
                {
                    var finalWeights = new Dictionary<string, int>();
                    foreach (var kv in opt.WeightModifiers)
                    {
                        string key = kv.Key == "primary" ? primaryTag : kv.Key;
                        finalWeights[key] = kv.Value;
                    }

                    optionsList.Add(new {
                        text = opt.TextTemplate,
                        weights = finalWeights,
                        nextLevelTags = !string.IsNullOrEmpty(opt.NextLevelTags) ? opt.NextLevelTags : primaryTag
                    });
                }

                questionsList.Add(new HierarchicalQuizQuestion
                {
                    Id = $"hq-g-{path.Id}-i{i}",
                    QuestionText = template.TextTemplate.Replace("{Title}", path.Title),
                    HierarchyLevel = path.Level,
                    StreamType = path.StreamType,
                    AptitudeType = "Interest",
                    TargetCareerTags = path.Tags.ToLower(),
                    OptionsJson = JsonSerializer.Serialize(optionsList)
                });
            }

            // Generate 3 WorkStyle questions
            for (int i = 1; i <= 3; i++)
            {
                var template = templates[i + 2];
                var optionsList = new List<object>();

                foreach (var opt in template.Options)
                {
                    var finalWeights = new Dictionary<string, int>();
                    foreach (var kv in opt.WeightModifiers)
                    {
                        string key = kv.Key == "primary" ? primaryTag : kv.Key;
                        finalWeights[key] = kv.Value;
                    }

                    optionsList.Add(new {
                        text = opt.TextTemplate,
                        weights = finalWeights,
                        nextLevelTags = !string.IsNullOrEmpty(opt.NextLevelTags) ? opt.NextLevelTags : primaryTag
                    });
                }

                questionsList.Add(new HierarchicalQuizQuestion
                {
                    Id = $"hq-g-{path.Id}-w{i}",
                    QuestionText = template.TextTemplate.Replace("{Title}", path.Title),
                    HierarchyLevel = path.Level,
                    StreamType = path.StreamType,
                    AptitudeType = "WorkStyle",
                    TargetCareerTags = path.Tags.ToLower(),
                    OptionsJson = JsonSerializer.Serialize(optionsList)
                });
            }

            // Generate 3 Aptitude questions
            for (int i = 1; i <= 3; i++)
            {
                var template = templates[i + 5];
                var optionsList = new List<object>();

                foreach (var opt in template.Options)
                {
                    var finalWeights = new Dictionary<string, int>();
                    foreach (var kv in opt.WeightModifiers)
                    {
                        string key = kv.Key == "primary" ? primaryTag : kv.Key;
                        finalWeights[key] = kv.Value;
                    }

                    optionsList.Add(new {
                        text = opt.TextTemplate,
                        weights = finalWeights,
                        nextLevelTags = !string.IsNullOrEmpty(opt.NextLevelTags) ? opt.NextLevelTags : primaryTag
                    });
                }

                questionsList.Add(new HierarchicalQuizQuestion
                {
                    Id = $"hq-g-{path.Id}-a{i}",
                    QuestionText = template.TextTemplate.Replace("{Title}", path.Title),
                    HierarchyLevel = path.Level,
                    StreamType = path.StreamType,
                    AptitudeType = "Aptitude",
                    TargetCareerTags = path.Tags.ToLower(),
                    OptionsJson = JsonSerializer.Serialize(optionsList)
                });
            }
        }

        // Save in batches
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        int batchSize = 200;
        for (int i = 0; i < questionsList.Count; i += batchSize)
        {
            var batch = questionsList.Skip(i).Take(batchSize);
            context.HierarchicalQuizQuestions.AddRange(batch);
            await context.SaveChangesAsync();
        }
        context.ChangeTracker.AutoDetectChangesEnabled = true;
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // PLACEMENT READINESS & SKILL GAP ANALYSIS MODULE â€” SEED DATA
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    private static async Task SeedPlacementDataAsync(ApplicationDbContext context)
    {
        // Idempotent: only seed if no role skill profiles exist yet
        if (context.RoleSkillProfiles.Any()) return;

        // 1. Seed RoleSkillProfiles
        var profiles = SeedRoleSkillProfiles();
        context.RoleSkillProfiles.AddRange(profiles);
        await context.SaveChangesAsync();

        // 2. Seed PlacementAssessmentQuestions
        var questions = SeedPlacementAssessmentQuestions();
        context.PlacementAssessmentQuestions.AddRange(questions);
        await context.SaveChangesAsync();
    }

    private static List<RoleSkillProfile> SeedRoleSkillProfiles()
    {
        var profiles = new List<RoleSkillProfile>();
        string Req(List<SkillRequirement> reqs) => JsonSerializer.Serialize(reqs);

        profiles.Add(new RoleSkillProfile { Id = "rsp-software-dev", RoleName = "Software Developer",
            SkillsJson = Req(new List<SkillRequirement> {
                new() { SkillName = "Data Structures", Category = "Technical", RequiredLevel = 85, Weightage = 25 },
                new() { SkillName = "Problem Solving", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "OOP", Category = "Technical", RequiredLevel = 75, Weightage = 15 },
                new() { SkillName = "Database", Category = "Technical", RequiredLevel = 70, Weightage = 15 },
                new() { SkillName = "System Design", Category = "Technical", RequiredLevel = 65, Weightage = 10 },
                new() { SkillName = "Aptitude", Category = "Aptitude", RequiredLevel = 70, Weightage = 10 },
                new() { SkillName = "Soft Skills", Category = "SoftSkill", RequiredLevel = 65, Weightage = 5 },
            }) });
        profiles.Add(new RoleSkillProfile { Id = "rsp-fullstack-dev", RoleName = "Full Stack Developer",
            SkillsJson = Req(new List<SkillRequirement> {
                new() { SkillName = "JavaScript", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "REST APIs", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "Database", Category = "Technical", RequiredLevel = 75, Weightage = 15 },
                new() { SkillName = "Problem Solving", Category = "Technical", RequiredLevel = 70, Weightage = 15 },
                new() { SkillName = "System Design", Category = "Technical", RequiredLevel = 65, Weightage = 10 },
                new() { SkillName = "Aptitude", Category = "Aptitude", RequiredLevel = 65, Weightage = 10 },
                new() { SkillName = "Soft Skills", Category = "SoftSkill", RequiredLevel = 70, Weightage = 10 },
            }) });
        profiles.Add(new RoleSkillProfile { Id = "rsp-data-scientist", RoleName = "Data Scientist",
            SkillsJson = Req(new List<SkillRequirement> {
                new() { SkillName = "Python", Category = "Technical", RequiredLevel = 85, Weightage = 25 },
                new() { SkillName = "Statistics", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "Machine Learning", Category = "Technical", RequiredLevel = 75, Weightage = 20 },
                new() { SkillName = "SQL", Category = "Technical", RequiredLevel = 70, Weightage = 15 },
                new() { SkillName = "Data Visualization", Category = "Technical", RequiredLevel = 65, Weightage = 10 },
                new() { SkillName = "Aptitude", Category = "Aptitude", RequiredLevel = 75, Weightage = 5 },
                new() { SkillName = "Soft Skills", Category = "SoftSkill", RequiredLevel = 65, Weightage = 5 },
            }) });
        profiles.Add(new RoleSkillProfile { Id = "rsp-ai-engineer", RoleName = "AI Engineer",
            SkillsJson = Req(new List<SkillRequirement> {
                new() { SkillName = "Python", Category = "Technical", RequiredLevel = 90, Weightage = 25 },
                new() { SkillName = "Deep Learning", Category = "Technical", RequiredLevel = 85, Weightage = 25 },
                new() { SkillName = "Machine Learning", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "NLP", Category = "Technical", RequiredLevel = 70, Weightage = 15 },
                new() { SkillName = "Problem Solving", Category = "Technical", RequiredLevel = 75, Weightage = 10 },
                new() { SkillName = "Soft Skills", Category = "SoftSkill", RequiredLevel = 60, Weightage = 5 },
            }) });
        profiles.Add(new RoleSkillProfile { Id = "rsp-cybersecurity", RoleName = "Cybersecurity Analyst",
            SkillsJson = Req(new List<SkillRequirement> {
                new() { SkillName = "Networking", Category = "Technical", RequiredLevel = 85, Weightage = 25 },
                new() { SkillName = "Linux", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "Cryptography", Category = "Technical", RequiredLevel = 75, Weightage = 20 },
                new() { SkillName = "Problem Solving", Category = "Technical", RequiredLevel = 70, Weightage = 15 },
                new() { SkillName = "Aptitude", Category = "Aptitude", RequiredLevel = 70, Weightage = 10 },
                new() { SkillName = "Soft Skills", Category = "SoftSkill", RequiredLevel = 65, Weightage = 10 },
            }) });
        profiles.Add(new RoleSkillProfile { Id = "rsp-cloud-engineer", RoleName = "Cloud Engineer",
            SkillsJson = Req(new List<SkillRequirement> {
                new() { SkillName = "Networking", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "Linux", Category = "Technical", RequiredLevel = 75, Weightage = 20 },
                new() { SkillName = "Docker", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "System Design", Category = "Technical", RequiredLevel = 75, Weightage = 15 },
                new() { SkillName = "Problem Solving", Category = "Technical", RequiredLevel = 70, Weightage = 10 },
                new() { SkillName = "Aptitude", Category = "Aptitude", RequiredLevel = 65, Weightage = 10 },
                new() { SkillName = "Soft Skills", Category = "SoftSkill", RequiredLevel = 65, Weightage = 5 },
            }) });
        profiles.Add(new RoleSkillProfile { Id = "rsp-devops-engineer", RoleName = "DevOps Engineer",
            SkillsJson = Req(new List<SkillRequirement> {
                new() { SkillName = "Linux", Category = "Technical", RequiredLevel = 85, Weightage = 20 },
                new() { SkillName = "Docker", Category = "Technical", RequiredLevel = 85, Weightage = 20 },
                new() { SkillName = "Networking", Category = "Technical", RequiredLevel = 75, Weightage = 15 },
                new() { SkillName = "System Design", Category = "Technical", RequiredLevel = 75, Weightage = 15 },
                new() { SkillName = "Problem Solving", Category = "Technical", RequiredLevel = 70, Weightage = 15 },
                new() { SkillName = "Aptitude", Category = "Aptitude", RequiredLevel = 65, Weightage = 10 },
                new() { SkillName = "Soft Skills", Category = "SoftSkill", RequiredLevel = 65, Weightage = 5 },
            }) });

        // â”€â”€ NEW ROLES â”€â”€
        profiles.Add(new RoleSkillProfile { Id = "rsp-doctor-mbbs", RoleName = "Doctor (MBBS)",
            SkillsJson = Req(new List<SkillRequirement> {
                new() { SkillName = "Anatomy", Category = "Technical", RequiredLevel = 85, Weightage = 20 },
                new() { SkillName = "Physiology", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "Pharmacology", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "Clinical Diagnosis", Category = "Technical", RequiredLevel = 85, Weightage = 20 },
                new() { SkillName = "Patient Care", Category = "SoftSkill", RequiredLevel = 80, Weightage = 10 },
                new() { SkillName = "Aptitude", Category = "Aptitude", RequiredLevel = 70, Weightage = 10 },
            }) });
        profiles.Add(new RoleSkillProfile { Id = "rsp-psychiatrist", RoleName = "Psychiatrist",
            SkillsJson = Req(new List<SkillRequirement> {
                new() { SkillName = "Psychology", Category = "Technical", RequiredLevel = 85, Weightage = 25 },
                new() { SkillName = "Psychopathology", Category = "Technical", RequiredLevel = 80, Weightage = 25 },
                new() { SkillName = "Counseling", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "Pharmacotherapy", Category = "Technical", RequiredLevel = 75, Weightage = 15 },
                new() { SkillName = "Ethics", Category = "SoftSkill", RequiredLevel = 80, Weightage = 10 },
                new() { SkillName = "Aptitude", Category = "Aptitude", RequiredLevel = 70, Weightage = 5 },
            }) });
        profiles.Add(new RoleSkillProfile { Id = "rsp-pharmacist", RoleName = "Pharmacist",
            SkillsJson = Req(new List<SkillRequirement> {
                new() { SkillName = "Pharmacology", Category = "Technical", RequiredLevel = 90, Weightage = 25 },
                new() { SkillName = "Drug Interactions", Category = "Technical", RequiredLevel = 85, Weightage = 25 },
                new() { SkillName = "Chemistry", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "Patient Counseling", Category = "SoftSkill", RequiredLevel = 75, Weightage = 15 },
                new() { SkillName = "Regulations", Category = "SoftSkill", RequiredLevel = 70, Weightage = 10 },
                new() { SkillName = "Aptitude", Category = "Aptitude", RequiredLevel = 70, Weightage = 5 },
            }) });
        profiles.Add(new RoleSkillProfile { Id = "rsp-chartered-accountant", RoleName = "Chartered Accountant",
            SkillsJson = Req(new List<SkillRequirement> {
                new() { SkillName = "Accounting", Category = "Technical", RequiredLevel = 90, Weightage = 25 },
                new() { SkillName = "Taxation", Category = "Technical", RequiredLevel = 85, Weightage = 25 },
                new() { SkillName = "Auditing", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "Financial Analysis", Category = "Technical", RequiredLevel = 75, Weightage = 15 },
                new() { SkillName = "Business Law", Category = "SoftSkill", RequiredLevel = 70, Weightage = 10 },
                new() { SkillName = "Aptitude", Category = "Aptitude", RequiredLevel = 75, Weightage = 5 },
            }) });
        profiles.Add(new RoleSkillProfile { Id = "rsp-graphic-designer", RoleName = "Graphic Designer",
            SkillsJson = Req(new List<SkillRequirement> {
                new() { SkillName = "Visual Design", Category = "Technical", RequiredLevel = 85, Weightage = 25 },
                new() { SkillName = "Typography", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "Color Theory", Category = "Technical", RequiredLevel = 80, Weightage = 20 },
                new() { SkillName = "Adobe Suite", Category = "Technical", RequiredLevel = 85, Weightage = 20 },
                new() { SkillName = "UI/UX", Category = "Technical", RequiredLevel = 75, Weightage = 10 },
                new() { SkillName = "Soft Skills", Category = "SoftSkill", RequiredLevel = 65, Weightage = 5 },
            }) });

        return profiles;
    }

    private static List<PlacementAssessmentQuestion> SeedPlacementAssessmentQuestions()
    {
        var q = new List<PlacementAssessmentQuestion>();
        int order = 1;

        PlacementAssessmentQuestion Q(
            string roles, string text, List<string> opts, string correct,
            string skill, string category, string difficulty) =>
            new PlacementAssessmentQuestion
            {
                Id = $"paq-{order++}",
                ApplicableRoles = roles,
                QuestionText = text,
                OptionsJson = JsonSerializer.Serialize(opts),
                CorrectAnswer = correct,
                SkillMapped = skill,
                Category = category,
                Difficulty = difficulty,
                IsActive = true,
                DisplayOrder = order - 1
            };

        const string SD = "Software Developer";
        const string FS = "Full Stack Developer";
        const string DS = "Data Scientist";
        const string AI = "AI Engineer";
        const string CY = "Cybersecurity Analyst";
        const string CL = "Cloud Engineer";
        const string DO = "DevOps Engineer";
        const string DR = "Doctor (MBBS)";
        const string PS = "Psychiatrist";
        const string PH = "Pharmacist";
        const string CA = "Chartered Accountant";
        const string GD = "Graphic Designer";
        string TECH = $"{SD},{FS},{AI},{CL},{DO}";
        string MED  = $"{DR},{PS},{PH}";
        string ALL  = $"{SD},{FS},{DS},{AI},{CY},{CL},{DO},{DR},{PS},{PH},{CA},{GD}";

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // SOFTWARE DEVELOPER â€” Data Structures
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        q.Add(Q($"{SD},{FS}", "What is the time complexity of binary search on a sorted array?",
            new(){"O(n)","O(log n)","O(nÂ²)","O(1)"}, "O(log n)", "Data Structures", "Technical", "Easy"));
        q.Add(Q($"{SD},{FS}", "Which data structure uses LIFO ordering?",
            new(){"Queue","Linked List","Stack","Heap"}, "Stack", "Data Structures", "Technical", "Easy"));
        q.Add(Q($"{SD},{FS}", "What is the worst-case time complexity of QuickSort?",
            new(){"O(n log n)","O(n)","O(nÂ²)","O(log n)"}, "O(nÂ²)", "Data Structures", "Technical", "Medium"));
        q.Add(Q($"{SD},{FS}", "Which traversal of a BST gives nodes in sorted order?",
            new(){"Pre-order","Post-order","In-order","Level-order"}, "In-order", "Data Structures", "Technical", "Medium"));
        q.Add(Q($"{SD},{FS}", "A hash table with chaining has average-case lookup time complexity of:",
            new(){"O(n)","O(log n)","O(1)","O(n log n)"}, "O(1)", "Data Structures", "Technical", "Medium"));
        q.Add(Q($"{SD},{FS}", "In Dijkstra's shortest-path algorithm, which data structure gives optimal performance?",
            new(){"Stack","Queue","Min-Heap (Priority Queue)","Linked List"}, "Min-Heap (Priority Queue)", "Data Structures", "Technical", "Hard"));

        // â”€â”€ Problem Solving â”€â”€
        q.Add(Q($"{SD},{FS},{AI}", "Dynamic programming is best applied when a problem has:",
            new(){"No repeated sub-problems","Overlapping sub-problems and optimal substructure","Only greedy choices","Linear recursion"},
            "Overlapping sub-problems and optimal substructure", "Problem Solving", "Technical", "Medium"));
        q.Add(Q($"{SD},{FS},{AI}", "Which approach solves the 0/1 Knapsack problem optimally?",
            new(){"Greedy","Dynamic Programming","Divide and Conquer","Backtracking"},
            "Dynamic Programming", "Problem Solving", "Technical", "Medium"));

        // â”€â”€ OOP / System Design â”€â”€
        q.Add(Q($"{SD},{FS},{DO}", "Which SOLID principle states a class should have only one reason to change?",
            new(){"Open/Closed","Single Responsibility","Liskov Substitution","Dependency Inversion"},
            "Single Responsibility", "OOP", "Technical", "Easy"));
        q.Add(Q($"{SD},{FS},{DO}", "Which design pattern ensures only one instance of a class exists?",
            new(){"Factory","Observer","Singleton","Strategy"}, "Singleton", "OOP", "Technical", "Easy"));
        q.Add(Q($"{SD},{FS},{CL},{DO}", "CAP theorem states a distributed system can guarantee at most how many of: Consistency, Availability, Partition Tolerance?",
            new(){"All three","Two","One","None"}, "Two", "System Design", "Technical", "Medium"));
        q.Add(Q($"{SD},{FS},{CL},{DO}", "Which architectural pattern decomposes an application into small independently deployable services?",
            new(){"Monolithic","Microservices","Serverless","Event-Driven"}, "Microservices", "System Design", "Technical", "Easy"));

        // â”€â”€ Database / SQL â”€â”€
        q.Add(Q($"{SD},{FS},{DS}", "Which SQL clause filters groups after GROUP BY?",
            new(){"WHERE","HAVING","FILTER","ORDER BY"}, "HAVING", "Database", "Technical", "Easy"));
        q.Add(Q($"{SD},{FS},{DS}", "What does ACID stand for in database transactions?",
            new(){"Atomicity, Consistency, Isolation, Durability","Access, Control, Integrity, Data","Atomicity, Concurrency, Indexing, Durability","Availability, Consistency, Isolation, Data"},
            "Atomicity, Consistency, Isolation, Durability", "Database", "Technical", "Medium"));
        q.Add(Q($"{SD},{FS},{DS}", "In SQL, which JOIN returns all rows from both tables with NULLs where no match exists?",
            new(){"INNER JOIN","LEFT JOIN","FULL OUTER JOIN","CROSS JOIN"}, "FULL OUTER JOIN", "SQL", "Technical", "Medium"));

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // FULL STACK â€” REST APIs & JavaScript
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        q.Add(Q($"{FS}", "Which HTTP method is idempotent and used to fully update a resource?",
            new(){"POST","PATCH","PUT","GET"}, "PUT", "REST APIs", "Technical", "Medium"));
        q.Add(Q($"{FS}", "What does Promise.all([p1, p2, p3]) return in JavaScript?",
            new(){"The first resolved promise","A promise that resolves when all promises resolve","A promise that resolves to the fastest result","An array of pending promises"},
            "A promise that resolves when all promises resolve", "JavaScript", "Technical", "Medium"));
        q.Add(Q($"{FS}", "What is the primary purpose of JWT in web applications?",
            new(){"Storing session data on the server","Stateless authentication by encoding user claims in a signed token","Encrypting API responses","Caching database queries"},
            "Stateless authentication by encoding user claims in a signed token", "REST APIs", "Technical", "Medium"));

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // DATA SCIENTIST â€” Python, Statistics, ML
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        q.Add(Q($"{DS},{AI}", "Which Python library is primarily used for numerical computing?",
            new(){"pandas","matplotlib","numpy","scikit-learn"}, "numpy", "Python", "Technical", "Easy"));
        q.Add(Q($"{DS},{AI}", "What does df.dropna() do in pandas?",
            new(){"Drops all columns","Removes rows with missing values","Fills missing values with 0","Sorts the DataFrame"},
            "Removes rows with missing values", "Python", "Technical", "Easy"));
        q.Add(Q($"{DS},{AI}", "Which metric is most appropriate for evaluating a classifier on imbalanced data?",
            new(){"Accuracy","Mean Squared Error","F1-Score","R-Squared"}, "F1-Score", "Machine Learning", "Technical", "Medium"));
        q.Add(Q($"{DS},{AI}", "What is the purpose of train-test split in machine learning?",
            new(){"Increase training data","Evaluate model on unseen data to prevent overfitting","Speed up training","Reduce feature count"},
            "Evaluate model on unseen data to prevent overfitting", "Machine Learning", "Technical", "Easy"));
        q.Add(Q($"{DS},{AI}", "Which statistical measure is most affected by extreme outliers?",
            new(){"Median","Mode","Mean","IQR"}, "Mean", "Statistics", "Technical", "Easy"));
        q.Add(Q($"{DS},{AI}", "In a normal distribution, approximately what percentage of data falls within 2 standard deviations?",
            new(){"68%","95%","99.7%","50%"}, "95%", "Statistics", "Technical", "Medium"));
        q.Add(Q($"{DS}", "Which chart type best shows correlation between two continuous variables?",
            new(){"Bar chart","Pie chart","Scatter plot","Histogram"}, "Scatter plot", "Data Visualization", "Technical", "Easy"));

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // AI ENGINEER â€” Deep Learning, NLP
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        q.Add(Q($"{AI}", "Which activation function outputs values between 0 and 1 for binary classification?",
            new(){"ReLU","Tanh","Sigmoid","Softmax"}, "Sigmoid", "Deep Learning", "Technical", "Easy"));
        q.Add(Q($"{AI}", "What problem does the vanishing gradient primarily affect?",
            new(){"Convolutional Networks","Deep Recurrent Networks","Shallow Networks","Linear Regression"},
            "Deep Recurrent Networks", "Deep Learning", "Technical", "Medium"));
        q.Add(Q($"{AI}", "Which NLP technique converts words into dense vector representations capturing semantic meaning?",
            new(){"One-hot encoding","TF-IDF","Word Embeddings (Word2Vec)","Bag of Words"},
            "Word Embeddings (Word2Vec)", "NLP", "Technical", "Medium"));
        q.Add(Q($"{AI}", "The Transformer self-attention mechanism has time complexity of:",
            new(){"O(n)","O(n log n)","O(nÂ²)","O(1)"}, "O(nÂ²)", "Deep Learning", "Technical", "Hard"));

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // CYBERSECURITY / CLOUD / DEVOPS
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        q.Add(Q($"{CY},{CL},{DO}", "Which OSI layer handles routing between networks?",
            new(){"Data Link","Transport","Network","Session"}, "Network", "Networking", "Technical", "Easy"));
        q.Add(Q($"{CY},{CL},{DO}", "Which protocol provides reliable, connection-oriented communication?",
            new(){"UDP","ICMP","TCP","ARP"}, "TCP", "Networking", "Technical", "Easy"));
        q.Add(Q($"{CY},{CL},{DO}", "What is a subnet mask 255.255.255.0 equivalent to in CIDR notation?",
            new(){"/16","/24","/8","/32"}, "/24", "Networking", "Technical", "Medium"));
        q.Add(Q($"{CY},{CL},{DO}", "Which Linux command displays all running processes?",
            new(){"ls","ps aux","netstat","df"}, "ps aux", "Linux", "Technical", "Easy"));
        q.Add(Q($"{CY},{CL},{DO}", "What permission does chmod 755 grant?",
            new(){"Owner: rwx, Group: r-x, Others: r-x","Owner: rw-, Group: r--, Others: r--","Owner: rwx, Group: rwx, Others: r--","Owner: r-x, Group: r-x, Others: r-x"},
            "Owner: rwx, Group: r-x, Others: r-x", "Linux", "Technical", "Medium"));
        q.Add(Q($"{CY}", "Which encryption type uses the same key for encryption and decryption?",
            new(){"Asymmetric","Symmetric","Hashing","Digital Signature"}, "Symmetric", "Cryptography", "Technical", "Easy"));
        q.Add(Q($"{CY}", "Which OWASP vulnerability involves executing malicious SQL through user inputs?",
            new(){"XSS","IDOR","SQL Injection","CSRF"}, "SQL Injection", "Cryptography", "Technical", "Easy"));
        q.Add(Q($"{CY}", "TLS primarily protects against which attack?",
            new(){"SQL Injection","Man-in-the-Middle attacks during data transit","Cross-Site Scripting","Denial of Service"},
            "Man-in-the-Middle attacks during data transit", "Cryptography", "Technical", "Medium"));
        q.Add(Q($"{CL},{DO}", "What is the primary difference between a Docker image and container?",
            new(){"Images are running; containers are blueprints","Containers are running instances of images","They are the same","Images run on VMs only"},
            "Containers are running instances of images", "Docker", "Technical", "Easy"));
        q.Add(Q($"{CL},{DO}", "In Kubernetes, which resource manages identical pods and ensures a desired replica count?",
            new(){"Service","Deployment","ConfigMap","Ingress"}, "Deployment", "Docker", "Technical", "Medium"));
        q.Add(Q($"{CL},{DO}", "What does CI/CD stand for?",
            new(){"Code Integration / Code Deployment","Continuous Integration / Continuous Delivery","Continuous Inspection / Continuous Design","Core Integration / Core Deployment"},
            "Continuous Integration / Continuous Delivery", "System Design", "Technical", "Easy"));
        q.Add(Q($"{CL},{DO}", "Which cloud service model provides virtualized computing infrastructure over the internet?",
            new(){"SaaS","PaaS","IaaS","FaaS"}, "IaaS", "System Design", "Technical", "Easy"));

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // DOCTOR (MBBS)
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        q.Add(Q($"{DR}", "Which organ filters blood and produces urine?",
            new(){"Liver","Heart","Kidney","Spleen"}, "Kidney", "Anatomy", "Technical", "Easy"));
        q.Add(Q($"{DR}", "The brachial plexus supplies nerves primarily to which body region?",
            new(){"Lower limb","Upper limb","Thorax","Abdomen"}, "Upper limb", "Anatomy", "Technical", "Medium"));
        q.Add(Q($"{DR}", "Which cranial nerve controls movement of the tongue?",
            new(){"Vagus (X)","Glossopharyngeal (IX)","Hypoglossal (XII)","Facial (VII)"}, "Hypoglossal (XII)", "Anatomy", "Technical", "Hard"));
        q.Add(Q($"{DR}", "What is the primary pacemaker of the heart?",
            new(){"AV Node","Bundle of His","SA Node","Purkinje Fibers"}, "SA Node", "Physiology", "Technical", "Easy"));
        q.Add(Q($"{DR}", "Which hormone regulates blood glucose by promoting cellular uptake?",
            new(){"Glucagon","Cortisol","Insulin","Epinephrine"}, "Insulin", "Physiology", "Technical", "Easy"));
        q.Add(Q($"{DR}", "The Frank-Starling law states cardiac output increases when:",
            new(){"Heart rate increases","Venous return (preload) increases","Afterload decreases","Blood viscosity decreases"},
            "Venous return (preload) increases", "Physiology", "Technical", "Medium"));
        q.Add(Q($"{DR},{PH}", "Which class of antibiotics inhibits bacterial cell wall synthesis?",
            new(){"Aminoglycosides","Macrolides","Beta-lactams (Penicillins)","Tetracyclines"}, "Beta-lactams (Penicillins)", "Pharmacology", "Technical", "Easy"));
        q.Add(Q($"{DR},{PH}", "What is the mechanism of action of aspirin?",
            new(){"Blocks opioid receptors","Inhibits cyclooxygenase (COX) enzymes","Blocks calcium channels","Activates GABA receptors"},
            "Inhibits cyclooxygenase (COX) enzymes", "Pharmacology", "Technical", "Medium"));
        q.Add(Q($"{DR}", "A patient presents with chest pain, dyspnea, and elevated troponin. Most likely diagnosis?",
            new(){"Pneumothorax","Acute Myocardial Infarction","Pulmonary Embolism","Aortic Dissection"},
            "Acute Myocardial Infarction", "Clinical Diagnosis", "Technical", "Medium"));
        q.Add(Q($"{DR}", "Preferred imaging for acute ischemic stroke within first 6 hours?",
            new(){"X-Ray","MRI with diffusion-weighted imaging","Ultrasound","PET Scan"},
            "MRI with diffusion-weighted imaging", "Clinical Diagnosis", "Technical", "Hard"));
        q.Add(Q($"{DR}", "Glasgow Coma Scale assesses level of consciousness across which three domains?",
            new(){"Breathing, Pulse, Reflexes","Eye Opening, Verbal Response, Motor Response","Pain, Temperature, Touch","Memory, Orientation, Attention"},
            "Eye Opening, Verbal Response, Motor Response", "Clinical Diagnosis", "Technical", "Medium"));
        q.Add(Q($"{DR}", "Which communication model is recommended when breaking bad news to a patient?",
            new(){"SOAP notes","SPIKES protocol","APGAR score","ABCDE assessment"}, "SPIKES protocol", "Patient Care", "SoftSkill", "Medium"));

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // PSYCHIATRIST
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        q.Add(Q($"{PS}", "In Freud's structural model, which component operates on the reality principle?",
            new(){"Id","Ego","Superego","Unconscious"}, "Ego", "Psychology", "Technical", "Easy"));
        q.Add(Q($"{PS}", "Maslow's hierarchy places which need at the top of the pyramid?",
            new(){"Safety","Belonging","Self-actualization","Esteem"}, "Self-actualization", "Psychology", "Technical", "Easy"));
        q.Add(Q($"{PS}", "Which neurotransmitter is most associated with reward and motivation?",
            new(){"Serotonin","Dopamine","GABA","Acetylcholine"}, "Dopamine", "Psychology", "Technical", "Medium"));
        q.Add(Q($"{PS}", "CBT primarily focuses on:",
            new(){"Dream analysis","Identifying and restructuring negative thought patterns","Free association","Family systems dynamics"},
            "Identifying and restructuring negative thought patterns", "Psychology", "Technical", "Medium"));
        q.Add(Q($"{PS}", "Which disorder is characterized by persistent delusions, hallucinations, and disorganized thinking?",
            new(){"Major Depressive Disorder","Bipolar I Disorder","Schizophrenia","Generalized Anxiety Disorder"}, "Schizophrenia", "Psychopathology", "Technical", "Easy"));
        q.Add(Q($"{PS}", "DSM-5 classifies PTSD under which category?",
            new(){"Mood Disorders","Anxiety Disorders","Trauma- and Stressor-Related Disorders","Personality Disorders"},
            "Trauma- and Stressor-Related Disorders", "Psychopathology", "Technical", "Medium"));
        q.Add(Q($"{PS}", "Which Cluster B personality disorder is characterized by grandiosity and lack of empathy?",
            new(){"Borderline","Antisocial","Narcissistic","Histrionic"}, "Narcissistic", "Psychopathology", "Technical", "Medium"));
        q.Add(Q($"{PS}", "In motivational interviewing, the therapist should primarily use:",
            new(){"Direct confrontation","Open-ended questions and reflective listening","Prescribing medication first","Structured homework assignments"},
            "Open-ended questions and reflective listening", "Counseling", "Technical", "Medium"));
        q.Add(Q($"{PS}", "Which class of medication is first-line for Major Depressive Disorder?",
            new(){"Benzodiazepines","SSRIs","Antipsychotics","Mood stabilizers"}, "SSRIs", "Pharmacotherapy", "Technical", "Easy"));
        q.Add(Q($"{PS}", "Lithium is the gold-standard treatment for which psychiatric condition?",
            new(){"Schizophrenia","ADHD","Bipolar Disorder","OCD"}, "Bipolar Disorder", "Pharmacotherapy", "Technical", "Medium"));
        q.Add(Q($"{PS}", "A psychiatrist discovers a patient plans to harm a specific individual. Ethically the psychiatrist must:",
            new(){"Maintain strict confidentiality","Warn the potential victim and notify authorities (Duty to Warn)","Prescribe sedatives immediately","Terminate the relationship"},
            "Warn the potential victim and notify authorities (Duty to Warn)", "Ethics", "SoftSkill", "Medium"));

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // PHARMACIST
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        q.Add(Q($"{PH}", "What does 'bioavailability' refer to?",
            new(){"Speed of drug metabolism","Fraction of administered drug reaching systemic circulation","Drug binding to plasma proteins","Rate of renal excretion"},
            "Fraction of administered drug reaching systemic circulation", "Pharmacology", "Technical", "Easy"));
        q.Add(Q($"{PH}", "Which phase of drug metabolism involves oxidation, reduction, and hydrolysis?",
            new(){"Phase I","Phase II","Phase III","Phase 0"}, "Phase I", "Pharmacology", "Technical", "Medium"));
        q.Add(Q($"{PH}", "Therapeutic index is calculated as:",
            new(){"ED50 / LD50","LD50 / ED50","MIC / MBC","Cmax / Cmin"}, "LD50 / ED50", "Pharmacology", "Technical", "Medium"));
        q.Add(Q($"{PH}", "Combining warfarin with aspirin increases the risk of:",
            new(){"Hypertension","Bleeding","Liver toxicity","Seizures"}, "Bleeding", "Drug Interactions", "Technical", "Easy"));
        q.Add(Q($"{PH}", "Which cytochrome P450 enzyme metabolizes the largest number of drugs?",
            new(){"CYP1A2","CYP2D6","CYP3A4","CYP2C9"}, "CYP3A4", "Drug Interactions", "Technical", "Medium"));
        q.Add(Q($"{PH}", "Grapefruit juice is contraindicated with statins because it:",
            new(){"Increases renal excretion","Inhibits CYP3A4, increasing drug levels","Decreases absorption","Binds to the drug in GI tract"},
            "Inhibits CYP3A4, increasing drug levels", "Drug Interactions", "Technical", "Hard"));
        q.Add(Q($"{PH}", "What is the pH of a 0.01 M HCl solution?",
            new(){"1","2","3","4"}, "2", "Chemistry", "Technical", "Easy"));
        q.Add(Q($"{PH}", "Which bond holds the two strands of DNA together?",
            new(){"Covalent bonds","Ionic bonds","Hydrogen bonds","Van der Waals forces"}, "Hydrogen bonds", "Chemistry", "Technical", "Easy"));
        q.Add(Q($"{PH}", "When counseling a patient on metformin, which side effect should they be warned about?",
            new(){"Hair loss","Gastrointestinal upset (nausea, diarrhea)","Vision changes","Joint pain"},
            "Gastrointestinal upset (nausea, diarrhea)", "Patient Counseling", "SoftSkill", "Easy"));
        q.Add(Q($"{PH}", "Schedule H drugs require:",
            new(){"No prescription","A valid prescription from a registered medical practitioner","Over-the-counter availability","Patient self-declaration"},
            "A valid prescription from a registered medical practitioner", "Regulations", "SoftSkill", "Easy"));

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // CHARTERED ACCOUNTANT
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        q.Add(Q($"{CA}", "The fundamental accounting equation is:",
            new(){"Assets = Liabilities + Revenue","Assets = Liabilities + Owner's Equity","Assets + Liabilities = Equity","Revenue - Expenses = Equity"},
            "Assets = Liabilities + Owner's Equity", "Accounting", "Technical", "Easy"));
        q.Add(Q($"{CA}", "Which accounting principle requires revenue to be recorded when earned, not when cash is received?",
            new(){"Matching Principle","Accrual Principle","Conservatism Principle","Going Concern Principle"}, "Accrual Principle", "Accounting", "Technical", "Easy"));
        q.Add(Q($"{CA}", "Under double-entry bookkeeping, a purchase of equipment on credit would:",
            new(){"Debit Equipment, Credit Cash","Debit Equipment, Credit Accounts Payable","Debit Cash, Credit Equipment","Debit Accounts Payable, Credit Equipment"},
            "Debit Equipment, Credit Accounts Payable", "Accounting", "Technical", "Medium"));
        q.Add(Q($"{CA}", "Depreciation of a fixed asset is an example of:",
            new(){"Capital expenditure","Revenue expenditure","Deferred revenue","Contingent liability"}, "Revenue expenditure", "Accounting", "Technical", "Medium"));
        q.Add(Q($"{CA}", "Under Indian GST, the standard rate for most goods and services is:",
            new(){"5%","12%","18%","28%"}, "18%", "Taxation", "Technical", "Easy"));
        q.Add(Q($"{CA}", "Which section of the Indian Income Tax Act deals with TDS?",
            new(){"Section 80C","Section 194","Section 44AB","Section 10"}, "Section 194", "Taxation", "Technical", "Medium"));
        q.Add(Q($"{CA}", "The concept of 'transfer pricing' primarily relates to:",
            new(){"Tax on capital gains","Pricing of transactions between related entities across borders","Sales tax on imports","Depreciation of assets"},
            "Pricing of transactions between related entities across borders", "Taxation", "Technical", "Hard"));
        q.Add(Q($"{CA}", "Which audit opinion indicates financial statements are fairly presented in all material respects?",
            new(){"Adverse Opinion","Disclaimer of Opinion","Unqualified (Clean) Opinion","Qualified Opinion"}, "Unqualified (Clean) Opinion", "Auditing", "Technical", "Easy"));
        q.Add(Q($"{CA}", "Substantive audit procedures are designed to:",
            new(){"Test internal controls","Detect material misstatements in financial statements","Verify employee attendance","Assess management competency"},
            "Detect material misstatements in financial statements", "Auditing", "Technical", "Medium"));
        q.Add(Q($"{CA}", "Current Ratio is calculated as:",
            new(){"Total Assets / Total Liabilities","Current Assets / Current Liabilities","Net Income / Total Revenue","EBITDA / Interest Expense"},
            "Current Assets / Current Liabilities", "Financial Analysis", "Technical", "Easy"));
        q.Add(Q($"{CA}", "A Debt-to-Equity ratio of 2.5 indicates:",
            new(){"More equity than debt","The company has 2.5x more debt than equity","Strong liquidity","High profitability"},
            "The company has 2.5x more debt than equity", "Financial Analysis", "Technical", "Medium"));
        q.Add(Q($"{CA}", "Which body regulates and oversees chartered accountants in India?",
            new(){"SEBI","RBI","ICAI","MCA"}, "ICAI", "Business Law", "SoftSkill", "Easy"));

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // GRAPHIC DESIGNER
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        q.Add(Q($"{GD}", "The 'Rule of Thirds' in visual design helps to:",
            new(){"Choose color palettes","Create balanced, engaging compositions","Determine font sizes","Optimize file sizes"},
            "Create balanced, engaging compositions", "Visual Design", "Technical", "Easy"));
        q.Add(Q($"{GD}", "Which design principle ensures elements feel visually connected?",
            new(){"Contrast","Repetition","Unity","Hierarchy"}, "Unity", "Visual Design", "Technical", "Medium"));
        q.Add(Q($"{GD}", "White space (negative space) in design is used to:",
            new(){"Fill empty areas","Improve readability and draw attention to key elements","Reduce page load time","Increase element density"},
            "Improve readability and draw attention to key elements", "Visual Design", "Technical", "Easy"));
        q.Add(Q($"{GD}", "In typography, 'kerning' refers to:",
            new(){"Space between lines of text","Overall weight of a font","Adjusting space between individual letter pairs","Size of uppercase letters"},
            "Adjusting space between individual letter pairs", "Typography", "Technical", "Easy"));
        q.Add(Q($"{GD}", "Which font category is generally best for body text in print design?",
            new(){"Sans-serif","Serif","Display","Script"}, "Serif", "Typography", "Technical", "Medium"));
        q.Add(Q($"{GD}", "The difference between 'leading' and 'tracking' in typography:",
            new(){"Leading adjusts letter spacing; tracking adjusts line spacing","Leading adjusts line spacing; tracking adjusts overall letter spacing in a block","They are the same","Leading is for headlines; tracking for body text"},
            "Leading adjusts line spacing; tracking adjusts overall letter spacing in a block", "Typography", "Technical", "Medium"));
        q.Add(Q($"{GD}", "Which color model is used for print design?",
            new(){"RGB","HSL","CMYK","HEX"}, "CMYK", "Color Theory", "Technical", "Easy"));
        q.Add(Q($"{GD}", "Complementary colors are located:",
            new(){"Next to each other on the color wheel","Opposite each other on the color wheel","Three colors evenly spaced on the wheel","All warm tones"},
            "Opposite each other on the color wheel", "Color Theory", "Technical", "Easy"));
        q.Add(Q($"{GD}", "Which Adobe application is best suited for vector illustrations?",
            new(){"Photoshop","Premiere Pro","Illustrator","After Effects"}, "Illustrator", "Adobe Suite", "Technical", "Easy"));
        q.Add(Q($"{GD}", "In Adobe Photoshop, what is the purpose of layer masks?",
            new(){"To permanently delete parts of an image","To non-destructively hide or reveal parts of a layer","To merge all layers","To adjust image resolution"},
            "To non-destructively hide or reveal parts of a layer", "Adobe Suite", "Technical", "Medium"));
        q.Add(Q($"{GD}", "In UI/UX design, a 'wireframe' is:",
            new(){"A final high-fidelity mockup","A basic structural blueprint of a page layout","A coded prototype","A brand style guide"},
            "A basic structural blueprint of a page layout", "UI/UX", "Technical", "Easy"));
        q.Add(Q($"{GD}", "Fitts's Law in UX design states that:",
            new(){"Users read in an F-pattern","Larger and closer targets are easier to click","Dark mode is always preferred","More options lead to faster decisions"},
            "Larger and closer targets are easier to click", "UI/UX", "Technical", "Medium"));

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // ROLE-SPECIFIC APTITUDE
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

        // Tech aptitude â€” project/system reasoning
        q.Add(Q(TECH, "A software project has 5 modules, each taking 3 days. With 2 developers working in parallel, minimum completion time is:",
            new(){"15 days","8 days","9 days","7.5 days"}, "9 days", "Aptitude", "Aptitude", "Medium"));
        q.Add(Q(TECH, "A database grows 15% each month starting at 100 GB. Approximate size after 3 months:",
            new(){"130 GB","145 GB","152 GB","175 GB"}, "152 GB", "Aptitude", "Aptitude", "Medium"));
        q.Add(Q(TECH, "A server processes 500 requests/minute. Each request takes 200ms. Minimum threads to avoid a queue:",
            new(){"1","2","3","4"}, "2", "Aptitude", "Aptitude", "Hard"));

        // Medical aptitude â€” clinical reasoning and statistics
        q.Add(Q(MED, "A clinical trial shows a drug reduces mortality from 10% to 8%. The Absolute Risk Reduction (ARR) is:",
            new(){"20%","2%","8%","80%"}, "2%", "Aptitude", "Aptitude", "Medium"));
        q.Add(Q(MED, "A test with 90% sensitivity and 80% specificity is applied in a population with 10% disease prevalence. A positive result is most likely a:",
            new(){"True positive","False positive","True negative","False negative"}, "False positive", "Aptitude", "Aptitude", "Hard"));
        q.Add(Q(MED, "A patient's creatinine clearance is 40 mL/min (normal: 90-120). Which adjustment is most appropriate?",
            new(){"Double the dose","Reduce dose or extend the dosing interval","No change needed","Switch to IV administration"},
            "Reduce dose or extend the dosing interval", "Aptitude", "Aptitude", "Medium"));

        // Commerce aptitude â€” financial reasoning
        q.Add(Q($"{CA}", "An investment of â‚¹1,00,000 earns 10% annual compound interest. Value after 2 years?",
            new(){"â‚¹1,20,000","â‚¹1,21,000","â‚¹1,10,000","â‚¹1,25,000"}, "â‚¹1,21,000", "Aptitude", "Aptitude", "Easy"));
        q.Add(Q($"{CA}", "Revenue is â‚¹50 lakhs and cost of goods sold is â‚¹30 lakhs. Gross profit margin is:",
            new(){"60%","40%","30%","20%"}, "40%", "Aptitude", "Aptitude", "Easy"));
        q.Add(Q($"{CA}", "If a price is increased 20% then decreased 20%, the net effect is:",
            new(){"No change","4% decrease","4% increase","20% decrease"}, "4% decrease", "Aptitude", "Aptitude", "Medium"));

        // Design aptitude â€” spatial and visual reasoning
        q.Add(Q($"{GD}", "A poster needs 300 DPI for a 12Ã—18 inch print. Required pixel dimensions:",
            new(){"1200Ã—1800","3600Ã—5400","2400Ã—3600","6000Ã—9000"}, "3600Ã—5400", "Aptitude", "Aptitude", "Medium"));
        q.Add(Q($"{GD}", "A logo must scale from a business card to a billboard without quality loss. Which format should be used?",
            new(){"JPEG","PNG","SVG (vector)","GIF"}, "SVG (vector)", "Aptitude", "Aptitude", "Easy"));

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        // SOFT SKILLS (contextual)
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        q.Add(Q(ALL, "When a colleague disagrees with your approach, the best first action is:",
            new(){"Implement your approach anyway","Escalate immediately","Listen and discuss trade-offs collaboratively","Ask the team to vote"},
            "Listen and discuss trade-offs collaboratively", "Soft Skills", "SoftSkill", "Easy"));
        q.Add(Q(ALL, "You discover a critical issue 1 hour before a deadline. You should:",
            new(){"Deliver anyway and fix later","Inform your lead/supervisor with an impact assessment","Fix it silently","Blame someone else"},
            "Inform your lead/supervisor with an impact assessment", "Soft Skills", "SoftSkill", "Medium"));
        q.Add(Q($"{DR},{PS},{PH}", "A patient asks you to prescribe something you believe is unnecessary. The best approach is:",
            new(){"Prescribe it to keep the patient happy","Explain your clinical reasoning and discuss alternatives","Refuse without explanation","Ask a colleague to prescribe it"},
            "Explain your clinical reasoning and discuss alternatives", "Soft Skills", "SoftSkill", "Medium"));

        return q;
    }

    private static (string salaryJson, double cagr) GetDomainSalaryAndGrowth(string category, string id, int level)
    {
        id = id.ToLower();
        double starting = 500000;
        double mid = 1100000;
        double senior = 2400000;
        double cagr = 12.0;

        switch (category)
        {
            case "ai_data":
                cagr = 18.5 + (level * 0.5);
                switch (level)
                {
                    case 1: starting = 650000; mid = 1400000; senior = 2800000; break;
                    case 2: starting = 850000; mid = 1800000; senior = 3600000; break;
                    case 3: starting = 1100000; mid = 2200000; senior = 4500000; break;
                    case 4: starting = 1400000; mid = 2800000; senior = 6000000; break;
                    default: starting = 2600000; mid = 5500000; senior = 11000000; break;
                }
                break;

            case "tech_software":
                cagr = 14.5 + (level * 0.4);
                if (id.Contains("arch") || id.Contains("cto") || id.Contains("cloud") || id.Contains("sre") || id.Contains("devops"))
                {
                    switch (level)
                    {
                        case 1: starting = 550000; mid = 1200000; senior = 2400000; break;
                        case 2: starting = 750000; mid = 1600000; senior = 3200000; break;
                        case 3: starting = 950000; mid = 2000000; senior = 4000000; break;
                        case 4: starting = 1250000; mid = 2600000; senior = 5500000; break;
                        default: starting = 2400000; mid = 5000000; senior = 9500000; break;
                    }
                }
                else
                {
                    switch (level)
                    {
                        case 1: starting = 480000; mid = 1000000; senior = 2000000; break;
                        case 2: starting = 650000; mid = 1350000; senior = 2700000; break;
                        case 3: starting = 800000; mid = 1700000; senior = 3400000; break;
                        case 4: starting = 1050000; mid = 2200000; senior = 4600000; break;
                        default: starting = 2200000; mid = 4500000; senior = 8500000; break;
                    }
                }
                break;

            case "commerce_finance":
                cagr = 13.0 + (level * 0.3);
                if (id.Contains("quant") || id.Contains("fintech") || id.Contains("wealth") || id.Contains("bank"))
                {
                    switch (level)
                    {
                        case 1: starting = 600000; mid = 1300000; senior = 2600000; break;
                        case 2: starting = 800000; mid = 1700000; senior = 3500000; break;
                        case 3: starting = 1000000; mid = 2200000; senior = 4500000; break;
                        case 4: starting = 1350000; mid = 3000000; senior = 6500000; break;
                        default: starting = 2800000; mid = 6000000; senior = 12000000; break;
                    }
                }
                else
                {
                    switch (level)
                    {
                        case 1: starting = 400000; mid = 850000; senior = 1700000; break;
                        case 2: starting = 550000; mid = 1150000; senior = 2300000; break;
                        case 3: starting = 700000; mid = 1500000; senior = 3000000; break;
                        case 4: starting = 950000; mid = 2000000; senior = 4200000; break;
                        default: starting = 2000000; mid = 4200000; senior = 8000000; break;
                    }
                }
                break;

            case "medical":
                cagr = 11.5 + (level * 0.3);
                switch (level)
                {
                    case 1: starting = 500000; mid = 1100000; senior = 2200000; break;
                    case 2: starting = 700000; mid = 1450000; senior = 2900000; break;
                    case 3: starting = 850000; mid = 1800000; senior = 3600000; break;
                    case 4: starting = 1150000; mid = 2400000; senior = 5000000; break;
                    default: starting = 2200000; mid = 4800000; senior = 9500000; break;
                }
                break;

            case "design_creative":
                cagr = 12.0 + (level * 0.3);
                switch (level)
                {
                    case 1: starting = 380000; mid = 800000; senior = 1600000; break;
                    case 2: starting = 520000; mid = 1100000; senior = 2200000; break;
                    case 3: starting = 680000; mid = 1450000; senior = 2900000; break;
                    case 4: starting = 900000; mid = 1900000; senior = 3800000; break;
                    default: starting = 1900000; mid = 3800000; senior = 7000000; break;
                }
                break;

            case "law":
                cagr = 11.0 + (level * 0.3);
                switch (level)
                {
                    case 1: starting = 450000; mid = 950000; senior = 1900000; break;
                    case 2: starting = 600000; mid = 1300000; senior = 2600000; break;
                    case 3: starting = 780000; mid = 1650000; senior = 3300000; break;
                    case 4: starting = 1050000; mid = 2250000; senior = 4800000; break;
                    default: starting = 2200000; mid = 4800000; senior = 9500000; break;
                }
                break;

            case "government":
                cagr = 7.5 + (level * 0.2);
                switch (level)
                {
                    case 1: starting = 420000; mid = 850000; senior = 1500000; break;
                    case 2: starting = 550000; mid = 1100000; senior = 1900000; break;
                    case 3: starting = 700000; mid = 1350000; senior = 2300000; break;
                    case 4: starting = 900000; mid = 1650000; senior = 2900000; break;
                    default: starting = 1650000; mid = 2600000; senior = 4000000; break;
                }
                break;

            case "entrepreneurship":
                cagr = 16.0 + (level * 0.4);
                switch (level)
                {
                    case 1: starting = 550000; mid = 1200000; senior = 2400000; break;
                    case 2: starting = 750000; mid = 1600000; senior = 3200000; break;
                    case 3: starting = 980000; mid = 2050000; senior = 4200000; break;
                    case 4: starting = 1300000; mid = 2700000; senior = 5800000; break;
                    default: starting = 2600000; mid = 5500000; senior = 11000000; break;
                }
                break;

            case "vocational":
            default:
                if (id.Contains("mecha") || id.Contains("robot") || id.Contains("aero") || id.Contains("embed") || id.Contains("auto"))
                {
                    cagr = 13.0 + (level * 0.3);
                    switch (level)
                    {
                        case 1: starting = 450000; mid = 950000; senior = 1900000; break;
                        case 2: starting = 620000; mid = 1300000; senior = 2600000; break;
                        case 3: starting = 780000; mid = 1650000; senior = 3300000; break;
                        case 4: starting = 1000000; mid = 2100000; senior = 4400000; break;
                        default: starting = 2000000; mid = 4200000; senior = 8000000; break;
                    }
                }
                else
                {
                    cagr = 9.5 + (level * 0.2);
                    switch (level)
                    {
                        case 1: starting = 320000; mid = 650000; senior = 1250000; break;
                        case 2: starting = 420000; mid = 800000; senior = 1550000; break;
                        case 3: starting = 540000; mid = 1050000; senior = 2000000; break;
                        case 4: starting = 700000; mid = 1400000; senior = 2700000; break;
                        default: starting = 1500000; mid = 2800000; senior = 5000000; break;
                    }
                }
                break;
        }

        string json = $"{{\"Starting\": {Math.Round(starting)}, \"Mid-Career\": {Math.Round(mid)}, \"Senior\": {Math.Round(senior)}}}";
        return (json, Math.Round(cagr, 1));
    }

    private static async Task EnsureRealisticCareerSalariesAsync(ApplicationDbContext context)
    {
        var paths = await context.CareerPaths.ToListAsync();
        if (!paths.Any()) return;

        bool updated = false;
        foreach (var path in paths)
        {
            // If the path contains the legacy 700000/1400000 generic template
            if (path.SalaryDataJson.Contains("700000") || path.SalaryDataJson.Contains("900000") || path.SalaryDataJson.Contains("300000") || path.SalaryDataJson.Contains("400000") || path.SalaryDataJson.Contains("500000"))
            {
                string cat = GetRoadmapCategory(path.Id);
                var (salJson, cagr) = GetDomainSalaryAndGrowth(cat, path.Id, path.Level);
                path.SalaryDataJson = salJson;
                path.IndustryGrowth = cagr;
                updated = true;
            }
        }

        if (updated)
        {
            await context.SaveChangesAsync();
        }
    }

}
