using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SkillScape.Application.DTOs;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;
using SkillScape.Infrastructure.Services;
using Xunit;
using Xunit.Abstractions;

namespace SkillScape.Tests;

public class ComprehensiveScenarioTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly QuestionValidationEngine _qValidator;
    private readonly ProfileValidationEngine _pValidator;
    private readonly DeterministicQuestionSelector _selector;
    private readonly MultiDimensionalScoringEngine _scoringEngine;
    private readonly ITestOutputHelper _output;

    public ComprehensiveScenarioTests(ITestOutputHelper output)
    {
        _output = output;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"ComprehensiveTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _qValidator = new QuestionValidationEngine();
        _pValidator = new ProfileValidationEngine(NullLogger<ProfileValidationEngine>.Instance);
        _selector = new DeterministicQuestionSelector(_context, _qValidator, _pValidator, NullLogger<DeterministicQuestionSelector>.Instance);
        _scoringEngine = new MultiDimensionalScoringEngine();

        SeedDatabase();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SeedDatabase()
    {
        string allPhases = "[\"After10th\",\"After12th\",\"AfterGraduation\",\"AfterPostGraduation\"]";

        var taxonomy = new List<CareerTaxonomyV2>
        {
            new CareerTaxonomyV2 { Id = "tech", Name = "Technology", Domain = "Technology", PhasesApplicableJson = allPhases, IsActive = true, Level = 1 },
            new CareerTaxonomyV2 { Id = "tech.cs", Name = "Computer Science", Domain = "Technology", Subdomain = "Computer Science", PhasesApplicableJson = allPhases, IsActive = true, Level = 2, ParentId = "tech" },
            new CareerTaxonomyV2 { Id = "tech.cs.ds", Name = "Data Science", Domain = "Technology", Subdomain = "Computer Science", PhasesApplicableJson = allPhases, IsActive = true, Level = 3, ParentId = "tech.cs" },
            new CareerTaxonomyV2 { Id = "tech.cs.cyber", Name = "Cybersecurity", Domain = "Technology", Subdomain = "Computer Science", PhasesApplicableJson = allPhases, IsActive = true, Level = 3, ParentId = "tech.cs" },
            new CareerTaxonomyV2 { Id = "tech.cs.ai", Name = "Artificial Intelligence", Domain = "Technology", Subdomain = "Computer Science", PhasesApplicableJson = allPhases, IsActive = true, Level = 3, ParentId = "tech.cs" },
            new CareerTaxonomyV2 { Id = "tech.civil", Name = "Civil Engineering", Domain = "Technology", Subdomain = "Civil", PhasesApplicableJson = allPhases, IsActive = true, Level = 2, ParentId = "tech" },
            
            new CareerTaxonomyV2 { Id = "sci.med", Name = "Medicine", Domain = "Healthcare", Subdomain = "Clinical Medicine", PhasesApplicableJson = allPhases, IsActive = true, Level = 2 },
            new CareerTaxonomyV2 { Id = "sci.med.surg", Name = "Surgery", Domain = "Healthcare", Subdomain = "Clinical Medicine", PhasesApplicableJson = allPhases, IsActive = true, Level = 3, ParentId = "sci.med" },
        };
        _context.CareerTaxonomyV2.AddRange(taxonomy);

        var questions = new List<QuizQuestionV2>();
        
        for (int i=1; i<=10; i++)
        {
            questions.Add(new QuizQuestionV2
            {
                Id = $"Q_GRAD_CS_{i}",
                PhaseKey = "AfterGraduation",
                Domain = "Technology",
                Subdomain = "Computer Science",
                Text = $"This is a long enough CS Question {i}",
                QuestionType = "MultipleChoice",
                OptionsJson = JsonSerializer.Serialize(new[]
                {
                    new { index = 0, text = "Data Science is great", signals = new[] { new { nodeId = "tech.cs.ds", weight = 1.0 } } },
                    new { index = 1, text = "Cybersecurity is fun", signals = new[] { new { nodeId = "tech.cs.cyber", weight = 1.0 } } },
                    new { index = 2, text = "AI is the future now", signals = new[] { new { nodeId = "tech.cs.ai", weight = 1.0 } } }
                }),
                TargetDiscriminatesJson = "[\"tech.cs.ds\", \"tech.cs.cyber\", \"tech.cs.ai\"]",
                IsActive = true
            });
        }
        
        questions.Add(new QuizQuestionV2
        {
            Id = $"Q_GRAD_CIVIL_1",
            PhaseKey = "AfterGraduation",
            Domain = "Technology",
            Subdomain = "Civil",
            Text = $"This is a long enough Civil Question 1",
            QuestionType = "MultipleChoice",
            OptionsJson = JsonSerializer.Serialize(new[]
            {
                new { index = 0, text = "Structural Eng is cool", signals = new[] { new { nodeId = "tech.civil", weight = 1.0 } } },
                new { index = 1, text = "Other engineering is bad", signals = new[] { new { nodeId = "tech.civil", weight = 0.0 } } }
            }),
            TargetDiscriminatesJson = "[\"tech.civil\"]",
            IsActive = true
        });

        questions.Add(new QuizQuestionV2
        {
            Id = $"Q_GRAD_MED_1",
            PhaseKey = "AfterGraduation",
            Domain = "Healthcare",
            Subdomain = "Clinical Medicine",
            Text = $"This is a long enough Med Question 1",
            QuestionType = "MultipleChoice",
            OptionsJson = JsonSerializer.Serialize(new[]
            {
                new { index = 0, text = "Surgery is very precise", signals = new[] { new { nodeId = "sci.med.surg", weight = 1.0 } } },
                new { index = 1, text = "Other medicine is fine", signals = new[] { new { nodeId = "sci.med.surg", weight = 0.0 } } }
            }),
            TargetDiscriminatesJson = "[\"sci.med.surg\"]",
            IsActive = true
        });

        _context.QuizQuestionsV2.AddRange(questions);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Scenario17_BTechCSE_To_DataScience_Cybersecurity_AI()
    {
        var session = new AdaptiveQuizSession
        {
            Id = "S17",
            PhaseKey = "AfterGraduation",
            AssessmentContextJson = JsonSerializer.Serialize(new AssessmentContext
            {
                EducationLevel = "AfterGraduation",
                Degree = "B.Tech",
                Major = "Computer Science and Engineering",
                LockedDomain = "Technology",
                PreferenceMode = "SELECTED",
                SelectedFields = new List<string> { "Computer Science", "Data Science", "Cybersecurity", "Artificial Intelligence" },
                TargetCareers = new List<string>(),
                EligibleDomains = new List<string> { "Technology" },
                EligibleSubdomains = new List<string> { "Computer Science" }
            })
        };

        var ctx = JsonSerializer.Deserialize<AssessmentContext>(session.AssessmentContextJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var testQuestion = _context.QuizQuestionsV2.FirstOrDefault(q => q.Id == "Q_GRAD_CS_1");
        bool isAllowed = _pValidator.IsQuestionAllowed(testQuestion, ctx, new List<string>(), out string reason);
        Assert.True(isAllowed, $"Validation failed: {reason}");

        var q = await _selector.SelectNextQuestionAsync(session, new List<string>(), new ParsedProfileDto(), new Dictionary<string, double>(), new List<string>(), "SELECTED");

        if (q == null)
        {
            _output.WriteLine("SelectNextQuestionAsync returned null.");
            
            // Re-simulate candidate questions
            var candidates = _context.QuizQuestionsV2.Where(x => x.PhaseKey == "AfterGraduation").ToList();
            _output.WriteLine($"DB PhaseKey candidates: {candidates.Count}");
            
            candidates = candidates.Where(x => _qValidator.ValidateQuestion(x).IsValid).ToList();
            _output.WriteLine($"Valid candidates: {candidates.Count}");
            
            var allowed = candidates.Where(x => _pValidator.IsQuestionAllowed(x, ctx, new List<string>(), out _)).ToList();
            _output.WriteLine($"Profile allowed candidates: {allowed.Count}");
            
            Assert.Fail("q was null. Look at output");
        }
        
        Assert.NotNull(q);
        Assert.Equal("Technology", q.Domain);
        Assert.Equal("Computer Science", q.Subdomain);
        
        Assert.NotEqual("Civil", q.Subdomain);
        Assert.NotEqual("Healthcare", q.Domain);
    }
}
