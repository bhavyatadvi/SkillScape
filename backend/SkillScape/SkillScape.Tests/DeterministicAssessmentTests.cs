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

namespace SkillScape.Tests;

public class DeterministicAssessmentTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly QuestionValidationEngine _validator;
    private readonly DeterministicQuestionSelector _selector;
    private readonly MultiDimensionalScoringEngine _scoringEngine;

    public DeterministicAssessmentTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"DeterministicAssessmentDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _validator = new QuestionValidationEngine();
        _selector = new DeterministicQuestionSelector(_context, _validator, new ProfileValidationEngine(NullLogger<ProfileValidationEngine>.Instance), NullLogger<DeterministicQuestionSelector>.Instance);
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
        // 1. Seed Taxonomy
        _context.CareerTaxonomyV2.AddRange(
            new CareerTaxonomyV2 { Id = "med.clinical.gen_med", Name = "General Medicine", Domain = "Healthcare", Subdomain = "Clinical Medicine", Level = 2, IsActive = true },
            new CareerTaxonomyV2 { Id = "med.surg.gen_surg", Name = "General Surgery", Domain = "Healthcare", Subdomain = "Surgical Medicine", Level = 2, IsActive = true },
            new CareerTaxonomyV2 { Id = "med.clinical.peds", Name = "Pediatrics", Domain = "Healthcare", Subdomain = "Clinical Medicine", Level = 2, IsActive = true },
            new CareerTaxonomyV2 { Id = "tech.cyber.soc", Name = "SOC Analyst", Domain = "Technology", Subdomain = "Security Operations", Level = 2, IsActive = true },
            new CareerTaxonomyV2 { Id = "tech.cyber.pentest", Name = "Penetration Tester", Domain = "Technology", Subdomain = "Offensive Security", Level = 2, IsActive = true },
            new CareerTaxonomyV2 { Id = "tech.cyber.forensics", Name = "Digital Forensics Analyst", Domain = "Technology", Subdomain = "Digital Forensics", Level = 2, IsActive = true }
        );

        // 2. Seed Questions
        _context.QuizQuestionsV2.AddRange(
            new QuizQuestionV2
            {
                Id = "Q_MED_001",
                PhaseKey = "AfterGraduation",
                SubPhaseKey = "medical",
                Domain = "Healthcare",
                Text = "A patient presents with interacting symptoms. How do you approach the clinical diagnosis?",
                TargetDiscriminatesJson = JsonSerializer.Serialize(new[] { "med.clinical.gen_med", "med.surg.gen_surg", "med.clinical.peds" }),
                OptionsJson = JsonSerializer.Serialize(new[]
                {
                    new { index = 0, text = "Longitudinal medical history and differential diagnosis.", signals = new[] { new { nodeId = "med.clinical.gen_med", weight = 1.0 } } },
                    new { index = 1, text = "Immediate surgical procedural intervention.", signals = new[] { new { nodeId = "med.surg.gen_surg", weight = 1.0 } } },
                    new { index = 2, text = "Pediatric developmental assessment and family counseling.", signals = new[] { new { nodeId = "med.clinical.peds", weight = 1.0 } } }
                }),
                IsActive = true
            },
            new QuizQuestionV2
            {
                Id = "Q_CYBER_001",
                PhaseKey = "AfterGraduation",
                SubPhaseKey = "tech",
                Domain = "Technology",
                Text = "Suspicious network traffic indicates credential compromise across systems. What is your priority?",
                TargetDiscriminatesJson = JsonSerializer.Serialize(new[] { "tech.cyber.soc", "tech.cyber.pentest", "tech.cyber.forensics" }),
                OptionsJson = JsonSerializer.Serialize(new[]
                {
                    new { index = 0, text = "Triage SIEM alerts and isolate host.", signals = new[] { new { nodeId = "tech.cyber.soc", weight = 1.0 } } },
                    new { index = 1, text = "Exploit the entry point to test system resilience.", signals = new[] { new { nodeId = "tech.cyber.pentest", weight = 1.0 } } },
                    new { index = 2, text = "Perform memory and disk artifact reconstruction.", signals = new[] { new { nodeId = "tech.cyber.forensics", weight = 1.0 } } }
                }),
                IsActive = true
            }
        );

        _context.SaveChanges();
    }

    [Fact]
    public async Task Test1_MBBS_Profile_SelectsMedicalDiscriminatorQuestions()
    {
        var session = new AdaptiveQuizSession
        {
            PhaseKey = "AfterGraduation",
            Mode = "TargetCareer",
            TargetCareersJson = JsonSerializer.Serialize(new[] { "med.clinical.gen_med", "med.surg.gen_surg", "med.clinical.peds" })
        };

        var profile = new ParsedProfileDto
        {
            Education = new ParsedEducationDto { Degree = "MBBS", Specialization = "Medicine", Level = "Graduation" }
        };

        var prior = new Dictionary<string, double>
        {
            ["med.clinical.gen_med"] = 0.33,
            ["med.surg.gen_surg"] = 0.33,
            ["med.clinical.peds"] = 0.34
        };

        var selectedQuestion = await _selector.SelectNextQuestionAsync(
            session, new List<string>(), profile, prior,
            new List<string> { "med.clinical.gen_med", "med.surg.gen_surg", "med.clinical.peds" }, "TargetCareer");

        Assert.NotNull(selectedQuestion);
        Assert.Equal("Q_MED_001", selectedQuestion.Id);
        Assert.Equal("Healthcare", selectedQuestion.Domain);
    }

    [Fact]
    public async Task Test2_ComputerScience_Profile_SelectsCybersecurityQuestions()
    {
        var session = new AdaptiveQuizSession
        {
            PhaseKey = "AfterGraduation",
            Mode = "TargetCareer",
            TargetCareersJson = JsonSerializer.Serialize(new[] { "tech.cyber.soc", "tech.cyber.pentest", "tech.cyber.forensics" })
        };

        var profile = new ParsedProfileDto
        {
            Education = new ParsedEducationDto { Degree = "B.Tech", Specialization = "Computer Science", Level = "Graduation" }
        };

        var prior = new Dictionary<string, double>
        {
            ["tech.cyber.soc"] = 0.33,
            ["tech.cyber.pentest"] = 0.33,
            ["tech.cyber.forensics"] = 0.34
        };

        var selectedQuestion = await _selector.SelectNextQuestionAsync(
            session, new List<string>(), profile, prior,
            new List<string> { "tech.cyber.soc", "tech.cyber.pentest", "tech.cyber.forensics" }, "TargetCareer");

        Assert.NotNull(selectedQuestion);
        Assert.Equal("Q_CYBER_001", selectedQuestion.Id);
        Assert.Equal("Technology", selectedQuestion.Domain);
    }

    [Fact]
    public void Test3_QuestionValidator_RejectsPlaceholderKeywords()
    {
        var invalidQ1 = new QuizQuestionV2
        {
            Id = "Q_BAD_1",
            Text = "Based on your profile, how would you solve threat_detection?",
            OptionsJson = "[{\"text\": \"I would focus on aspects related to 1.\"}, {\"text\": \"I would focus on aspects related to 2.\"}]"
        };

        var result1 = _validator.ValidateQuestion(invalidQ1);
        Assert.False(result1.IsValid);

        var validQ = new QuizQuestionV2
        {
            Id = "Q_GOOD_1",
            Text = "How do you prefer analyzing complex security logs in a corporate setting?",
            OptionsJson = "[{\"text\": \"Methodical query construction in a SIEM platform.\"}, {\"text\": \"Automated scripting with Python.\"}]"
        };

        var result2 = _validator.ValidateQuestion(validQ);
        Assert.True(result2.IsValid);
    }

    [Fact]
    public async Task Test4_ScoringIsDeterministicAndReproducible()
    {
        var session1 = new AdaptiveQuizSession
        {
            Id = "sess_1",
            TargetCareersJson = JsonSerializer.Serialize(new[] { "tech.cyber.soc", "tech.cyber.pentest" })
        };

        var session2 = new AdaptiveQuizSession
        {
            Id = "sess_2",
            TargetCareersJson = JsonSerializer.Serialize(new[] { "tech.cyber.soc", "tech.cyber.pentest" })
        };

        var question = _context.QuizQuestionsV2.First(q => q.Id == "Q_CYBER_001");

        // User chooses Option 0 in both sessions
        await _scoringEngine.UpdateScoresAsync(session1, question, 0);
        await _scoringEngine.UpdateScoresAsync(session2, question, 0);

        var res1 = await _scoringEngine.FinalizeSessionAsync(session1);
        var res2 = await _scoringEngine.FinalizeSessionAsync(session2);

        Assert.Equal(res1.PrimaryTaxonomyNodeId, res2.PrimaryTaxonomyNodeId);
        Assert.Equal(res1.TopPredictionsJson, res2.TopPredictionsJson);
    }

    [Fact]
    public async Task Test5_QuestionSelector_NeverRepeatsAnsweredQuestions()
    {
        var session = new AdaptiveQuizSession
        {
            PhaseKey = "AfterGraduation",
            TargetCareersJson = JsonSerializer.Serialize(new[] { "tech.cyber.soc", "tech.cyber.pentest" })
        };

        var profile = new ParsedProfileDto
        {
            Education = new ParsedEducationDto { Degree = "B.Tech", Specialization = "Computer Science" }
        };

        var prior = new Dictionary<string, double> { ["tech.cyber.soc"] = 0.5, ["tech.cyber.pentest"] = 0.5 };

        // Mark Q_CYBER_001 as already answered
        var answeredIds = new List<string> { "Q_CYBER_001" };

        var nextQuestion = await _selector.SelectNextQuestionAsync(
            session, answeredIds, profile, prior,
            new List<string> { "tech.cyber.soc", "tech.cyber.pentest" }, "TargetCareer");

        // Q_CYBER_001 should NOT be returned
        Assert.NotEqual("Q_CYBER_001", nextQuestion?.Id);
    }
}
