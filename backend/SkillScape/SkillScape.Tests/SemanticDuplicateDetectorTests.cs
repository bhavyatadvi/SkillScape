using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;
using SkillScape.Infrastructure.Services;
using Xunit;

namespace SkillScape.Tests
{
    public class SemanticDuplicateDetectorTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly SemanticDuplicateDetector _detector;
        private readonly LocalSemanticSimilarityService _similarityService;

        public SemanticDuplicateDetectorTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _similarityService = new LocalSemanticSimilarityService();
            _detector = new SemanticDuplicateDetector(_context, _similarityService);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task IsDuplicateAsync_ExactDuplicateText_ReturnsTrue()
        {
            var answered = new QuizQuestionV2 { Id = "Q1", Text = "What is your preferred working style?" };
            _context.QuizQuestionsV2.Add(answered);
            await _context.SaveChangesAsync();

            var candidate = new QuizQuestionV2 { Id = "Q2", Text = "What is your preferred working style?" };
            
            bool result = await _detector.IsDuplicateAsync(candidate, new List<string> { "Q1" });
            
            Assert.True(result);
        }

        [Fact]
        public async Task IsDuplicateAsync_ExactDuplicateId_ReturnsTrue()
        {
            var answered = new QuizQuestionV2 { Id = "Q1", Text = "Question A" };
            _context.QuizQuestionsV2.Add(answered);
            await _context.SaveChangesAsync();

            var candidate = new QuizQuestionV2 { Id = "Q1", Text = "Question B" };
            
            bool result = await _detector.IsDuplicateAsync(candidate, new List<string> { "Q1" });
            
            Assert.True(result);
        }

        [Fact]
        public async Task IsDuplicateAsync_ParaphrasedDuplicate_ReturnsTrue()
        {
            // Specifically testing the synonym mapping feature
            var answered = new QuizQuestionV2 { Id = "Q1", CompetencyId = "Security", Text = "Would you enjoy monitoring network traffic for attacks?" };
            _context.QuizQuestionsV2.Add(answered);
            await _context.SaveChangesAsync();

            var candidate = new QuizQuestionV2 { Id = "Q2", CompetencyId = "Security", Text = "How interested are you in watching network logs for suspicious activity?" };
            
            bool result = await _detector.IsDuplicateAsync(candidate, new List<string> { "Q1" });
            
            Assert.True(result, "Paraphrased text should be considered a duplicate within the same competency.");
        }

        [Fact]
        public async Task IsDuplicateAsync_SameScenario_ReturnsTrue()
        {
            var answered = new QuizQuestionV2 { Id = "Q1", QuestionType = "Scenario", Text = "You are leading a project and a team member quits. What do you do?" };
            _context.QuizQuestionsV2.Add(answered);
            await _context.SaveChangesAsync();

            var candidate = new QuizQuestionV2 { Id = "Q2", QuestionType = "Scenario", Text = "You lead a project and a colleague resigns. How do you respond?" };
            
            bool result = await _detector.IsDuplicateAsync(candidate, new List<string> { "Q1" });
            
            Assert.True(result, "Scenarios with same premise should be rejected.");
        }

        [Fact]
        public async Task IsDuplicateAsync_SameConceptRepeated_ReturnsTrue()
        {
            var answered1 = new QuizQuestionV2 { Id = "Q1", ConceptId = "C_NET", Text = "Q1" };
            var answered2 = new QuizQuestionV2 { Id = "Q2", ConceptId = "C_NET", Text = "Q2" };
            _context.QuizQuestionsV2.AddRange(answered1, answered2);
            await _context.SaveChangesAsync();

            var candidate = new QuizQuestionV2 { Id = "Q3", ConceptId = "C_NET", Text = "Q3" };
            
            // Limit is 2 for concepts, so 3rd should be rejected
            bool result = await _detector.IsDuplicateAsync(candidate, new List<string> { "Q1", "Q2" });
            
            Assert.True(result);
        }

        [Fact]
        public async Task IsDuplicateAsync_SameCompetencyRepeated_ReturnsTrue()
        {
            var answered1 = new QuizQuestionV2 { Id = "Q1", CompetencyId = "COMP_SEC", Text = "Q1" };
            var answered2 = new QuizQuestionV2 { Id = "Q2", CompetencyId = "COMP_SEC", Text = "Q2" };
            var answered3 = new QuizQuestionV2 { Id = "Q3", CompetencyId = "COMP_SEC", Text = "Q3" };
            _context.QuizQuestionsV2.AddRange(answered1, answered2, answered3);
            await _context.SaveChangesAsync();

            var candidate = new QuizQuestionV2 { Id = "Q4", CompetencyId = "COMP_SEC", Text = "Q4" };
            
            // Limit is 3 for competencies, so 4th should be rejected
            bool result = await _detector.IsDuplicateAsync(candidate, new List<string> { "Q1", "Q2", "Q3" });
            
            Assert.True(result);
        }

        [Fact]
        public async Task IsDuplicateAsync_ValidDifferentQuestion_ReturnsFalse()
        {
            var answered = new QuizQuestionV2 { Id = "Q1", CompetencyId = "COMP_1", ConceptId = "C_1", Text = "What is your favorite color?" };
            _context.QuizQuestionsV2.Add(answered);
            await _context.SaveChangesAsync();

            var candidate = new QuizQuestionV2 { Id = "Q2", CompetencyId = "COMP_2", ConceptId = "C_2", Text = "How do you handle conflict?" };
            
            bool result = await _detector.IsDuplicateAsync(candidate, new List<string> { "Q1" });
            
            Assert.False(result);
        }
    }
}
