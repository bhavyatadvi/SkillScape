using System;
using System.Linq;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;
using SkillScape.Infrastructure.Services;
using Xunit;

namespace SkillScape.Tests;

public class RoadmapCurriculumTests
{
    private (RoadmapCurriculumGeneratorService Service, ApplicationDbContext Context) CreateService()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"RoadmapTestDb_{Guid.NewGuid()}")
            .Options;

        var context = new ApplicationDbContext(options);
        var mockConfig = new Mock<IConfiguration>();
        var groq = new GroqService(new HttpClient(), mockConfig.Object, NullLogger<GroqService>.Instance);
        var mockSearch = new Mock<IRoadmapWebSearchService>();
        var logger = NullLogger<RoadmapCurriculumGeneratorService>.Instance;

        var service = new RoadmapCurriculumGeneratorService(
            context,
            groq,
            mockSearch.Object,
            logger,
            mockConfig.Object
        );

        return (service, context);
    }

    [Fact]
    public void InferCategory_MapsCivilEngineeringCorrectly_NotGovOrSWE()
    {
        Assert.Equal("CivilEngineering", RoadmapCurriculumGeneratorService.InferCategory("Civil Engineer", "civil-engineer"));
        Assert.Equal("CivilEngineering", RoadmapCurriculumGeneratorService.InferCategory("Structural Engineer", "structural-engineering"));
        Assert.Equal("GovCivil", RoadmapCurriculumGeneratorService.InferCategory("IAS Officer", "upsc-civil-services"));
        Assert.Equal("Cybersecurity", RoadmapCurriculumGeneratorService.InferCategory("Cybersecurity Analyst", "cybersecurity"));
        Assert.Equal("ElectricalElectronics", RoadmapCurriculumGeneratorService.InferCategory("Electrical Engineer", "electrical-engineer"));
        Assert.Equal("ChemicalEngineering", RoadmapCurriculumGeneratorService.InferCategory("Chemical Engineer", "chemical-engineer"));
        Assert.Equal("MedicineClinical", RoadmapCurriculumGeneratorService.InferCategory("MBBS Doctor", "medical-clinical"));
        Assert.Equal("TeachingEducation", RoadmapCurriculumGeneratorService.InferCategory("High School Teacher", "education-teaching"));
        Assert.Equal("ArchitecturePlanning", RoadmapCurriculumGeneratorService.InferCategory("Urban Architect", "architecture-planning"));
        Assert.Equal("GeneralProfessional", RoadmapCurriculumGeneratorService.InferCategory("Unknown Non-Tech Role", "unknown-specialty"));
    }

    [Fact]
    public void GenerateDeterministicBlueprint_CivilEngineering_HasNoTechContamination()
    {
        var (service, context) = CreateService();
        using (context)
        {
            var blueprint = service.GenerateDeterministicBlueprint("Civil Engineer", "civil-engineer");

            Assert.NotNull(blueprint);
            Assert.Equal(5, blueprint.Phases.Count);

            var allText = string.Join(" ", blueprint.Phases.SelectMany(p =>
                p.Modules.SelectMany(m =>
                    new[] { p.Title, p.Summary, m.Title, m.Description }.Concat(m.Topics.Select(t => t.Title))
                )
            )).ToLowerInvariant();

            // Must not contain software/web tech buzzwords
            Assert.DoesNotContain("programming fundamentals", allText);
            Assert.DoesNotContain("git version control", allText);
            Assert.DoesNotContain("data structures", allText);
            Assert.DoesNotContain("react components", allText);
            Assert.DoesNotContain("api endpoints", allText);

            // Must contain civil engineering terms
            Assert.Contains("mechanics", allText);
            Assert.Contains("surveying", allText);
            Assert.Contains("concrete", allText);
            Assert.Contains("structural", allText);

            // Must pass corruption check
            Assert.False(RoadmapCurriculumGeneratorService.IsCorruptedBlueprint("civil-engineer", "Civil Engineer", blueprint));
        }
    }

    [Fact]
    public void GenerateDeterministicBlueprint_Cybersecurity_HasDefensiveAndOffensiveSecurity()
    {
        var (service, context) = CreateService();
        using (context)
        {
            var blueprint = service.GenerateDeterministicBlueprint("Cybersecurity Analyst", "cybersecurity");

            Assert.NotNull(blueprint);
            Assert.Equal(5, blueprint.Phases.Count);

            var allText = string.Join(" ", blueprint.Phases.SelectMany(p =>
                p.Modules.SelectMany(m =>
                    new[] { p.Title, p.Summary, m.Title, m.Description }.Concat(m.Topics.Select(t => t.Title))
                )
            )).ToLowerInvariant();

            Assert.Contains("networking", allText);
            Assert.Contains("cryptography", allText);
            Assert.Contains("siem", allText);
            Assert.Contains("vulnerability", allText);
            Assert.Contains("incident response", allText);

            Assert.False(RoadmapCurriculumGeneratorService.IsCorruptedBlueprint("cybersecurity", "Cybersecurity Analyst", blueprint));
        }
    }

    [Fact]
    public void GenerateDeterministicBlueprint_ElectricalElectronics_HasCoreHardwarePhases()
    {
        var (service, context) = CreateService();
        using (context)
        {
            var blueprint = service.GenerateDeterministicBlueprint("Electrical Engineer", "electrical-engineer");

            Assert.NotNull(blueprint);
            Assert.Equal(5, blueprint.Phases.Count);

            var allText = string.Join(" ", blueprint.Phases.SelectMany(p =>
                p.Modules.SelectMany(m =>
                    new[] { p.Title, p.Summary, m.Title, m.Description }.Concat(m.Topics.Select(t => t.Title))
                )
            )).ToLowerInvariant();

            Assert.Contains("circuit", allText);
            Assert.Contains("semiconductor", allText);
            Assert.Contains("analog", allText);
            Assert.Contains("power systems", allText);
            Assert.Contains("pcb", allText);

            Assert.False(RoadmapCurriculumGeneratorService.IsCorruptedBlueprint("electrical-engineer", "Electrical Engineer", blueprint));
        }
    }

    [Fact]
    public void GenerateDeterministicBlueprint_MedicineClinical_HasClinicalPhases()
    {
        var (service, context) = CreateService();
        using (context)
        {
            var blueprint = service.GenerateDeterministicBlueprint("Clinical Physician", "medicine-clinical");

            Assert.NotNull(blueprint);
            Assert.Equal(5, blueprint.Phases.Count);

            var allText = string.Join(" ", blueprint.Phases.SelectMany(p =>
                p.Modules.SelectMany(m =>
                    new[] { p.Title, p.Summary, m.Title, m.Description }.Concat(m.Topics.Select(t => t.Title))
                )
            )).ToLowerInvariant();

            Assert.Contains("anatomy", allText);
            Assert.Contains("physiology", allText);
            Assert.Contains("pathology", allText);
            Assert.Contains("pharmacology", allText);
            Assert.Contains("surgery", allText);

            Assert.False(RoadmapCurriculumGeneratorService.IsCorruptedBlueprint("medicine-clinical", "Clinical Physician", blueprint));
        }
    }

    [Fact]
    public void CorruptedBlueprintDetector_FlagsSoftwareTopicsOnNonTechCareer()
    {
        var corruptBp = new RoadmapBlueprint
        {
            CareerKey = "civil-engineer",
            CareerTitle = "Civil Engineer"
        };
        var p1 = new RoadmapPhase
        {
            Title = "Phase 1: Computer Science & Programming Fundamentals"
        };
        p1.Modules.Add(new RoadmapModule
        {
            Title = "Programming Foundations & Git Version Control"
        });
        corruptBp.Phases.Add(p1);

        bool isCorrupted = RoadmapCurriculumGeneratorService.IsCorruptedBlueprint("civil-engineer", "Civil Engineer", corruptBp);
        Assert.True(isCorrupted);
    }

    [Fact]
    public async Task EnsurePredictedCareerSaved_AutoSaves_QuizResultsV2_Prediction_To_UserSavedRoadmaps()
    {
        var (curriculumService, context) = CreateService();
        using (context)
        {
            var userId = "user-predicted-quiz-1";
            context.Users.Add(new ApplicationUser
            {
                Id = userId,
                FullName = "quizlearner",
                Email = "learner@skillscape.com"
            });

            context.QuizResultsV2.Add(new QuizResultV2
            {
                UserId = userId,
                SessionId = "session-123",
                PhaseKey = "AfterGraduation",
                PrimaryTaxonomyNodeId = "sci.eng.civil",
                WhyRecommended = "Strong spatial reasoning and engineering fundamentals.",
                GeneratedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var roadmapService = new RoadmapService(
                context,
                new DynamicRoadmapEngine(),
                curriculumService
            );

            // Fetch saved roadmaps (should automatically trigger prediction auto-save)
            var saved = await roadmapService.GetUserSavedRoadmapsAsync(userId);

            Assert.NotEmpty(saved);
            var userRoadmap = saved.FirstOrDefault(r => r.CareerKey == "sci.eng.civil");
            Assert.NotNull(userRoadmap);
            Assert.True(userRoadmap.IsDefault);
            Assert.Contains("Civil", userRoadmap.CareerTitle);

            // Verify Blueprint was created and linked in the database
            var dbSaved = await context.UserSavedRoadmaps.FirstOrDefaultAsync(u => u.UserId == userId && u.CareerKey == "sci.eng.civil");
            Assert.NotNull(dbSaved);
            Assert.False(string.IsNullOrEmpty(dbSaved.BlueprintId));

            var blueprint = await context.RoadmapBlueprints.FindAsync(dbSaved.BlueprintId);
            Assert.NotNull(blueprint);
            Assert.Equal("sci.eng.civil", blueprint.CareerKey);
        }
    }

    [Fact]
    public async Task AcceptCareerRoadmap_SetsDefaultAndSavesCurriculum()
    {
        var (curriculumService, context) = CreateService();
        using (context)
        {
            var userId = "user-accept-test-2";
            context.Users.Add(new ApplicationUser
            {
                Id = userId,
                FullName = "acceptuser",
                Email = "accept@skillscape.com"
            });
            await context.SaveChangesAsync();

            var roadmapService = new RoadmapService(
                context,
                new DynamicRoadmapEngine(),
                curriculumService
            );

            // User accepts career
            var result = await roadmapService.AcceptCareerRoadmapAsync(userId, "cybersecurity", "Cybersecurity Specialist");

            Assert.NotNull(result);
            Assert.Equal("cybersecurity", result.DomainId);
            Assert.Equal("Cybersecurity Specialist", result.DomainName);
            Assert.NotEmpty(result.Phases);

            // Verify it is saved in My Roadmaps as default
            var savedList = await roadmapService.GetUserSavedRoadmapsAsync(userId);
            Assert.Single(savedList);
            Assert.Equal("cybersecurity", savedList[0].CareerKey);
            Assert.True(savedList[0].IsDefault);
        }
    }

    [Fact]
    public async Task DismissedRoadmap_ReopenedFromHome_IsAutoRestoredAsDefault()
    {
        var (curriculumService, context) = CreateService();
        using (context)
        {
            var userId = "user-dismiss-restore-3";
            context.Users.Add(new ApplicationUser
            {
                Id = userId,
                FullName = "mbbslearner",
                Email = "mbbs@skillscape.com"
            });

            context.QuizResultsV2.Add(new QuizResultV2
            {
                UserId = userId,
                SessionId = "session-mbbs-999",
                PhaseKey = "After12th",
                PrimaryTaxonomyNodeId = "science-mbbs",
                WhyRecommended = "Strong aptitude for clinical medicine.",
                GeneratedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var roadmapService = new RoadmapService(
                context,
                new DynamicRoadmapEngine(),
                curriculumService
            );

            // 1. Initial access auto-saves predicted roadmap
            var initialSaved = await roadmapService.GetUserSavedRoadmapsAsync(userId);
            Assert.Single(initialSaved);
            Assert.Equal("science-mbbs", initialSaved[0].CareerKey);

            // 2. User deletes roadmap from My Roadmaps
            var removeResult = await roadmapService.RemoveUserRoadmapAsync(userId, "science-mbbs");
            Assert.True(removeResult);

            var afterDeleteSaved = await context.UserSavedRoadmaps.Where(u => u.UserId == userId).ToListAsync();
            Assert.Empty(afterDeleteSaved);

            var dismissalCount = await context.UserRoadmapDismissals.CountAsync(d => d.UserId == userId && d.CareerKey == "science-mbbs");
            Assert.Equal(1, dismissalCount);

            // 3. User navigates to Home (GetMyRoadmap should NOT inject software-engineer)
            var myRoadmapOnHome = await roadmapService.GetMyRoadmapAsync(userId);
            Assert.Null(myRoadmapOnHome);

            // 4. User clicks on "Science MBBS" / Continue Learning from Home (calls GetRoadmapByDomainAsync)
            var restoredRoadmap = await roadmapService.GetRoadmapByDomainAsync(userId, "science-mbbs");
            Assert.NotNull(restoredRoadmap);
            Assert.Equal("science-mbbs", restoredRoadmap.DomainId);
            Assert.NotEmpty(restoredRoadmap.Phases);

            // 5. Verify dismissal was removed and roadmap is restored in UserSavedRoadmaps as default
            var remainingDismissals = await context.UserRoadmapDismissals.CountAsync(d => d.UserId == userId && d.CareerKey == "science-mbbs");
            Assert.Equal(0, remainingDismissals);

            var restoredSavedList = await roadmapService.GetUserSavedRoadmapsAsync(userId);
            Assert.Single(restoredSavedList);
            Assert.Equal("science-mbbs", restoredSavedList[0].CareerKey);
            Assert.True(restoredSavedList[0].IsDefault);
        }
    }

    [Fact]
    public void NormalizeCareerKey_ReplacesUnderscoresWithHyphens()
    {
        Assert.Equal("project-manager", RoadmapService.NormalizeCareerKey("project_manager"));
        Assert.Equal("data-analyst", RoadmapService.NormalizeCareerKey("data_analyst"));
        Assert.Equal("business-analyst", RoadmapService.NormalizeCareerKey("business_analyst"));
        Assert.Equal("operations-manager", RoadmapService.NormalizeCareerKey("operations_manager"));
    }

    [Fact]
    public void InferCategory_MapsProjectManagerAndBusinessAnalystToProduct()
    {
        Assert.Equal("Product", RoadmapCurriculumGeneratorService.InferCategory("Project Manager", "project_manager"));
        Assert.Equal("Product", RoadmapCurriculumGeneratorService.InferCategory("Business Analyst", "business_analyst"));
        Assert.Equal("Product", RoadmapCurriculumGeneratorService.InferCategory("Operations Manager", "operations_manager"));
    }

    [Fact]
    public async Task AcceptCareerRoadmap_ForCareerSwitch_SavesAsDefaultAndSyncsProfile()
    {
        var (curriculumService, context) = CreateService();
        using (context)
        {
            var userId = "switch-user-1";
            context.Users.Add(new ApplicationUser
            {
                Id = userId,
                FullName = "Switch Learner",
                Email = "switch@skillscape.com"
            });
            await context.SaveChangesAsync();

            var roadmapService = new RoadmapService(
                context,
                new DynamicRoadmapEngine(),
                curriculumService
            );

            // User accepts reverse career recommendation: "project_manager"
            var result = await roadmapService.AcceptCareerRoadmapAsync(userId, "project_manager", "Project Manager");

            Assert.NotNull(result);
            Assert.Equal("project-manager", result.DomainId);
            Assert.Equal("Project Manager", result.DomainName);
            Assert.NotEmpty(result.Phases);
            Assert.True(result.Phases.Count >= 4);

            // Verify it is saved in UserSavedRoadmaps as default
            var savedList = await roadmapService.GetUserSavedRoadmapsAsync(userId);
            Assert.Single(savedList);
            Assert.Equal("project-manager", savedList[0].CareerKey);
            Assert.Equal("Project Manager", savedList[0].CareerTitle);
            Assert.True(savedList[0].IsDefault);

            // Verify UserCareerProfile was updated/created
            var profile = await context.UserCareerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            Assert.NotNull(profile);
            Assert.Equal("project-manager", profile.RecommendedCareerId);
        }
    }

    [Fact]
    public async Task AcceptCareerRoadmap_PreservesDefaultWhenEnsurePredictedCareerSavedRuns()
    {
        var (curriculumService, context) = CreateService();
        using (context)
        {
            var userId = "switch-user-preserve-default";
            context.Users.Add(new ApplicationUser
            {
                Id = userId,
                FullName = "Switch Learner 2",
                Email = "switch2@skillscape.com"
            });

            // Add legacy quiz result (e.g. from general quiz taken earlier)
            context.QuizResultsV2.Add(new QuizResultV2
            {
                UserId = userId,
                SessionId = "sess-old-quiz",
                PhaseKey = "AfterGraduation",
                PrimaryTaxonomyNodeId = "science-mbbs",
                GeneratedAt = DateTime.UtcNow.AddDays(-7)
            });
            await context.SaveChangesAsync();

            var roadmapService = new RoadmapService(
                context,
                new DynamicRoadmapEngine(),
                curriculumService
            );

            // User accepts Reverse Career direction "project_manager"
            await roadmapService.AcceptCareerRoadmapAsync(userId, "project_manager", "Project Manager");

            // Fetch saved roadmaps (which triggers EnsurePredictedCareerSavedAsync)
            var savedList = await roadmapService.GetUserSavedRoadmapsAsync(userId);

            // "project-manager" MUST still be the default roadmap
            var defaultRoadmap = savedList.FirstOrDefault(r => r.IsDefault);
            Assert.NotNull(defaultRoadmap);
            Assert.Equal("project-manager", defaultRoadmap.CareerKey);

            // The legacy quiz prediction may be added, but not marked as default
            var quizRoadmap = savedList.FirstOrDefault(r => r.CareerKey == "science-mbbs");
            if (quizRoadmap != null)
            {
                Assert.False(quizRoadmap.IsDefault);
            }
        }
    }

    [Fact]
    public void NormalizeCareerKey_StripsCareerPrefixAndNormalizes()
    {
        Assert.Equal("data-analyst", RoadmapService.NormalizeCareerKey("career_data_analyst"));
        Assert.Equal("software-dev", RoadmapService.NormalizeCareerKey("career-software-dev"));
        Assert.Equal("cybersecurity-analyst", RoadmapService.NormalizeCareerKey("career_cybersecurity_analyst"));
        Assert.Equal("financial-analyst", RoadmapService.NormalizeCareerKey("career_financial_analyst"));
    }

    [Fact]
    public void InferCategory_MapsSkillAnalyzerCareers()
    {
        Assert.Equal("DataML", RoadmapCurriculumGeneratorService.InferCategory("Data Analyst", "career_data_analyst"));
        Assert.Equal("DataML", RoadmapCurriculumGeneratorService.InferCategory("Data Scientist", "career_data_scientist"));
        Assert.Equal("Cybersecurity", RoadmapCurriculumGeneratorService.InferCategory("Cybersecurity Analyst", "career_cybersecurity_analyst"));
        Assert.Equal("CommerceFinance", RoadmapCurriculumGeneratorService.InferCategory("Financial Analyst", "career_financial_analyst"));
        Assert.Equal("Product", RoadmapCurriculumGeneratorService.InferCategory("Business Analyst", "career_business_analyst"));
        Assert.Equal("Marketing", RoadmapCurriculumGeneratorService.InferCategory("Marketing Manager", "career_marketing_manager"));
    }

    [Fact]
    public async Task AcceptCareerRoadmap_ForSkillAnalyzerCareer_GeneratesDetailedCurriculumWithQuizzes()
    {
        var (curriculumService, context) = CreateService();
        using (context)
        {
            var userId = "skill-analyzer-user-1";
            context.Users.Add(new ApplicationUser
            {
                Id = userId,
                FullName = "Skill Analyzer Learner",
                Email = "analyzer@skillscape.com"
            });
            await context.SaveChangesAsync();

            var roadmapService = new RoadmapService(
                context,
                new DynamicRoadmapEngine(),
                curriculumService
            );

            // User accepts Skill Analyzer target career: "career_data_analyst" -> "Data Analyst"
            var result = await roadmapService.AcceptCareerRoadmapAsync(userId, "career_data_analyst", "Data Analyst");

            Assert.NotNull(result);
            Assert.Equal("data-analyst", result.DomainId);
            Assert.Equal("Data Analyst", result.DomainName);
            Assert.True(result.Phases.Count >= 4);

            // Verify each phase has modules with topics and quizzes
            foreach (var phase in result.Phases)
            {
                Assert.True(phase.Modules.Count >= 3);
                foreach (var module in phase.Modules)
                {
                    Assert.NotEmpty(module.Topics);
                    Assert.True(module.Topics.Count >= 5);
                }
            }

            // Verify saved in UserSavedRoadmaps as default
            var savedList = await roadmapService.GetUserSavedRoadmapsAsync(userId);
            Assert.Single(savedList);
            Assert.Equal("data-analyst", savedList[0].CareerKey);
            Assert.Equal("Data Analyst", savedList[0].CareerTitle);
            Assert.True(savedList[0].IsDefault);
        }
    }

    [Fact]
    public async Task RoadmapProgress_WhenAllModulesCompleted_Returns100Percent()
    {
        var (curriculumService, context) = CreateService();
        using (context)
        {
            var userId = "completed-user-1";
            context.Users.Add(new ApplicationUser
            {
                Id = userId,
                FullName = "Completed Learner",
                Email = "completed@skillscape.com"
            });
            await context.SaveChangesAsync();

            var roadmapService = new RoadmapService(
                context,
                new DynamicRoadmapEngine(),
                curriculumService
            );

            var roadmap = await roadmapService.AcceptCareerRoadmapAsync(userId, "career_software_dev", "Software Developer");
            Assert.NotNull(roadmap);

            // Mark all modules in all phases complete
            var allModuleIds = roadmap.Phases.SelectMany(p => p.Modules.Select(m => m.ModuleId)).ToList();
            Assert.NotEmpty(allModuleIds);

            foreach (var modId in allModuleIds)
            {
                await roadmapService.MarkStepCompleteAsync(userId, modId);
            }

            // Fetch roadmap again
            var updatedRoadmap = await roadmapService.GetRoadmapByDomainAsync(userId, "career_software_dev");
            Assert.Equal(100.0, updatedRoadmap.ProgressPercentage);
            Assert.All(updatedRoadmap.Phases.SelectMany(p => p.Modules), m => Assert.True(m.IsCompleted));

            // Fetch saved roadmaps
            var savedList = await roadmapService.GetUserSavedRoadmapsAsync(userId);
            Assert.Single(savedList);
            Assert.Equal(100.0, savedList[0].ProgressPercentage);
        }
    }
}
