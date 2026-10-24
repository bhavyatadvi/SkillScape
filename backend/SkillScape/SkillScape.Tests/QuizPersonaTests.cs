using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;
using SkillScape.Infrastructure.Services;
using Xunit;

namespace SkillScape.Tests;

public class QuizPersonaTests
{
    private readonly ApplicationDbContext _context;
    private readonly AdaptiveQuizService _quizService;
    private readonly ProfileParserService _profileParser;
    private readonly GoalValidationEngine _goalValidator;
    private readonly Mock<IQuestionGenerationService> _mockGenService;
    private readonly Mock<IDuplicateDetector> _mockDupDetector;

    private class TestOptionSignal
    {
        [JsonPropertyName("nodeId")]
        public string NodeId { get; set; } = string.Empty;
        
        [JsonPropertyName("weight")]
        public double Weight { get; set; }
    }

    private class TestQuizOptionData
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("signals")]
        public List<TestOptionSignal> Signals { get; set; } = new();
    }

    public QuizPersonaTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        
        _mockGenService = new Mock<IQuestionGenerationService>();
        _mockGenService.Setup(x => x.GetValidatedCandidatesAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, double>>(),
            It.IsAny<List<string>>()))
            .ReturnsAsync(new List<QuizQuestionV2>());

        _mockDupDetector = new Mock<IDuplicateDetector>();

        _profileParser = new ProfileParserService();
        _goalValidator = new GoalValidationEngine(_context);

        var mockLlmGenerator = new Mock<IDynamicLlmQuestionGenerator>();
        mockLlmGenerator.Setup(x => x.GenerateNextQuestionAsync(
            It.IsAny<ParsedProfileDto>(), It.IsAny<Dictionary<string, double>>(),
            It.IsAny<List<EvidenceItemDto>>(), It.IsAny<AssessmentSlot>(),
            It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>()))
            .ReturnsAsync(new QuizQuestionV2 { Id = "llm_q1", Text = "Dummy LLM question", OptionsJson = "[{\"index\":0,\"text\":\"Op1\"},{\"index\":1,\"text\":\"Op2\"}]", TargetDiscriminatesJson = "[]" });

        var mockRoadmap = new Mock<IDynamicRoadmapEngine>();
        var mockPrediction = new Mock<IQuizMlPredictionService>();
        var mockBlueprint = new Mock<IAssessmentBlueprintService>();
        var mockProfileMapper = new Mock<IProfileMapperService>();
        var mockProfileValidator = new Mock<IProfileValidationEngine>();
        mockProfileValidator.Setup(x => x.IsQuestionAllowed(It.IsAny<QuizQuestionV2>(), It.IsAny<AssessmentContext>(), It.IsAny<List<string>>(), out It.Ref<string>.IsAny)).Returns(true);

        _quizService = new AdaptiveQuizService(
            _context, 
            _mockGenService.Object, 
            _mockDupDetector.Object, 
            _profileParser, 
            _goalValidator,
            mockPrediction.Object,
            mockRoadmap.Object,
            mockLlmGenerator.Object,
            null, // scoringEngine
            mockBlueprint.Object,
            null, // deterministicSelector
            mockProfileMapper.Object,
            mockProfileValidator.Object);

        SeedDeterministicData();
    }

    private void SeedDeterministicData()
    {
        string phaseJson = "[\"After12th\",\"AfterGraduation\",\"After10th\"]";
        var nodes = new List<CareerTaxonomyV2>
        {
            new CareerTaxonomyV2 { Id = "sci.eng.cs", Name = "Computer Science", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "sci.eng.civil", Name = "Civil Engineering", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "sci.eng.civil.struct", Name = "Structural Engineering", PhasesApplicableJson = phaseJson, IsActive = true, Level = 3 },
            new CareerTaxonomyV2 { Id = "sci.eng.civil.construction", Name = "Construction Management", PhasesApplicableJson = phaseJson, IsActive = true, Level = 3 },
            new CareerTaxonomyV2 { Id = "sci.med.mbbs", Name = "Medicine (MBBS)", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "com.fin.ca", Name = "Chartered Accountancy", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "com.fin.analyst", Name = "Financial Analyst", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "com.bus.bba", Name = "Business Administration", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "art.hum.soc", Name = "Social Sciences", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "art.hum.psych", Name = "Psychology", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "art.des.gd", Name = "Graphic Design", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "sci.eng.mech", Name = "Mechanical Engineering", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "sci.math.pure", Name = "Pure Mathematics", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "sci.chem.res", Name = "Chemistry Research", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "voc.diploma", Name = "Polytechnic Diploma", PhasesApplicableJson = phaseJson, IsActive = true, Level = 2 }
        };

        _context.CareerTaxonomyV2.AddRange(nodes);

        var q1 = new QuizQuestionV2 { Id = "q_cs", PhaseKey = "After12th", Text = "Programming?", IsActive = true, OptionsJson = SerializeOptions("sci.eng.cs", "art.hum.soc") };
        var q2 = new QuizQuestionV2 { Id = "q_civil", PhaseKey = "After12th", Text = "Structures?", IsActive = true, OptionsJson = SerializeOptions("sci.eng.civil", "art.des.gd") };
        var q3 = new QuizQuestionV2 { Id = "q_med", PhaseKey = "After12th", Text = "Biology?", IsActive = true, OptionsJson = SerializeOptions("sci.med.mbbs", "sci.eng.mech") };
        var q4 = new QuizQuestionV2 { Id = "q_ca", PhaseKey = "After12th", Text = "Accounting?", IsActive = true, OptionsJson = SerializeOptions("com.fin.ca", "sci.math.pure") };

        _context.QuizQuestionsV2.AddRange(q1, q2, q3, q4);
        _context.SaveChanges();
    }

    private string SerializeOptions(string targetYes, string targetNo)
    {
        var list = new List<TestQuizOptionData>
        {
            new TestQuizOptionData 
            { 
                Index = 0, 
                Text = "Yes", 
                Signals = new List<TestOptionSignal> { new TestOptionSignal { NodeId = targetYes, Weight = 0.9 }, new TestOptionSignal { NodeId = targetNo, Weight = 0.1 } } 
            },
            new TestQuizOptionData 
            { 
                Index = 1, 
                Text = "No", 
                Signals = new List<TestOptionSignal> { new TestOptionSignal { NodeId = targetNo, Weight = 0.9 }, new TestOptionSignal { NodeId = targetYes, Weight = 0.1 } } 
            }
        };
        return JsonSerializer.Serialize(list);
    }

    [Fact]
    public async Task Guarantee_FirstQuestionIsNeverNull_EvenIfCandidatesAreEmpty()
    {
        var startReq = new AdaptiveQuizStartRequest
        {
            PhaseKey = "AfterGraduation",
            Profile = new PhaseProfileInputDto
            {
                PhaseKey = "AfterGraduation",
                Fields = new Dictionary<string, string>
                {
                    { "currentStatus", "B.Tech Computer Engineering graduate" },
                    { "primaryGoal", "I want a software development job" }
                }
            }
        };

        var (session, firstQuestion) = await _quizService.StartSessionAsync("user123", startReq);

        Assert.NotNull(session);
        Assert.NotNull(firstQuestion);
        Assert.NotNull(firstQuestion.Question);
        Assert.False(string.IsNullOrWhiteSpace(firstQuestion.Question.Id));
        Assert.False(string.IsNullOrWhiteSpace(firstQuestion.Question.Text));
        Assert.True(firstQuestion.Question.Options.Count >= 2);
    }

    [Fact]
    public void ProfileParser_ParsesNaturalLanguageStatusAndGoal()
    {
        string status = "B.Tech Computer Engineering graduate, unemployed, completed 1 internship in PHP and React";
        string goal = "I want to get a software development job";

        var parsed = _profileParser.ParseProfile("AfterGraduation", status, goal);

        Assert.Equal("B.Tech", parsed.Education.Degree);
        Assert.Contains("Computer", parsed.Education.Specialization);
        Assert.Contains("PHP", parsed.CurrentStatus.Skills);
        Assert.Equal(goal, parsed.PrimaryGoal.Goal);
    }

    [Fact]
    public async Task GoalValidation_IdentifiesInconsistentGoal_AndRecommendsAlternatives()
    {
        var profile = new ParsedProfileDto
        {
            Education = new ParsedEducationDto { Level = "AfterGraduation", Degree = "B.A.", Specialization = "Psychology" },
            CurrentStatus = new ParsedCurrentStatusDto { Role = "Graduate", EmploymentStatus = "unemployed", Skills = new List<string> { "WRITING", "PUBLIC SPEAKING" } },
            PrimaryGoal = new ParsedPrimaryGoalDto { Goal = "Data Scientist", TargetDomain = "sci.eng.cs.ai" }
        };

        var validation = await _goalValidator.ValidateGoalAsync(profile);

        Assert.False(validation.IsGoodFit);
        Assert.Contains(validation.AlignmentRating, new[] { "WEAK_FIT", "CURRENTLY_NOT_ALIGNED" });
        Assert.NotEmpty(validation.RecommendedAlternatives);
    }

    [Fact]
    public async Task Case1_BTechCS_To_SoftwareDeveloper()
    {
        var parsed = _profileParser.ParseProfile("AfterGraduation", "B.Tech CS graduate", "Software Developer");
        var validation = await _goalValidator.ValidateGoalAsync(parsed);
        Assert.True(validation.IsGoodFit);
    }

    [Fact]
    public async Task Case2_BTechCivil_To_StructuralEngineer()
    {
        var parsed = _profileParser.ParseProfile("AfterGraduation", "B.Tech Civil graduate with STAAD skills", "Structural Engineer");
        var validation = await _goalValidator.ValidateGoalAsync(parsed);
        Assert.Equal("sci.eng.civil.struct", parsed.PrimaryGoal.TargetDomain);
    }

    [Fact]
    public async Task Case4_BCom_To_Finance()
    {
        var parsed = _profileParser.ParseProfile("AfterGraduation", "B.Com graduate with Accounting skills", "Financial Analyst");
        var validation = await _goalValidator.ValidateGoalAsync(parsed);
        Assert.True(validation.IsGoodFit);
    }

    [Fact]
    public void Case15_UserSpellingMistakes_HandledGracefully()
    {
        var parsed = _profileParser.ParseProfile("After12th", "I stdied btech cs and have basic python", "sofware enginer job");
        Assert.Equal("B.Tech", parsed.Education.Degree);
        Assert.Equal("Computer Science & Engineering", parsed.Education.Specialization);
    }
}
