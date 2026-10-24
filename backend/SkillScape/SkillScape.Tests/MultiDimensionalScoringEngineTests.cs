using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Services;
using Xunit;

namespace SkillScape.Tests
{
    public class MultiDimensionalScoringEngineTests
    {
        private readonly MultiDimensionalScoringEngine _engine;

        public MultiDimensionalScoringEngineTests()
        {
            _engine = new MultiDimensionalScoringEngine();
        }

        private QuizQuestionV2 CreateQuestion(string type, List<string> targets, double signalA, double signalB, double signalC)
        {
            var options = new List<object>
            {
                new
                {
                    index = 0,
                    text = "Option 0",
                    signals = new List<object>
                    {
                        new { nodeId = "CareerA", weight = signalA },
                        new { nodeId = "CareerB", weight = signalB },
                        new { nodeId = "CareerC", weight = signalC }
                    }
                }
            };

            return new QuizQuestionV2
            {
                Id = Guid.NewGuid().ToString(),
                QuestionType = type,
                TargetDiscriminatesJson = JsonSerializer.Serialize(targets),
                OptionsJson = JsonSerializer.Serialize(options)
            };
        }

        [Fact]
        public async Task UpdateScoresAsync_AllTargetCareersScoredIndependently()
        {
            var session = new AdaptiveQuizSession
            {
                TargetCareersJson = JsonSerializer.Serialize(new List<string> { "CareerA", "CareerB", "CareerC" })
            };

            var question = CreateQuestion("Competency", new List<string> { "CareerA", "CareerB", "CareerC" }, 1.0, 0.5, 0.0);
            
            await _engine.UpdateScoresAsync(session, question, 0);
            var result = await _engine.FinalizeSessionAsync(session);

            // Output should have 3 predictions
            Assert.Contains("CareerA", result.TopPredictionsJson);
            Assert.Contains("CareerB", result.TopPredictionsJson);
            Assert.Contains("CareerC", result.TopPredictionsJson);
        }

        [Fact]
        public async Task UpdateScoresAsync_UnrelatedCareersAreExcluded()
        {
            var session = new AdaptiveQuizSession
            {
                TargetCareersJson = JsonSerializer.Serialize(new List<string> { "CareerA", "CareerB" }) // CareerC is excluded!
            };

            var question = CreateQuestion("Competency", new List<string> { "CareerA", "CareerB", "CareerC" }, 1.0, 1.0, 1.0);
            
            await _engine.UpdateScoresAsync(session, question, 0);
            var result = await _engine.FinalizeSessionAsync(session);

            Assert.Contains("CareerA", result.TopPredictionsJson);
            Assert.Contains("CareerB", result.TopPredictionsJson);
            Assert.DoesNotContain("CareerC", result.TopPredictionsJson); // Even though the signal has C, it's not a target
        }

        [Fact]
        public async Task FinalizeSessionAsync_ScoresAreStrictlyBounded0To100()
        {
            var session = new AdaptiveQuizSession
            {
                TargetCareersJson = JsonSerializer.Serialize(new List<string> { "CareerA" })
            };

            // Test overflow (e.g. signal > 1.0 somehow)
            var question = CreateQuestion("Interest", new List<string> { "CareerA" }, 1.5, 0, 0); 
            
            await _engine.UpdateScoresAsync(session, question, 0);
            var result = await _engine.FinalizeSessionAsync(session);

            var preds = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result.TopPredictionsJson);
            var matchPercent = double.Parse(preds[0]["matchPercent"].ToString());

            // Even if signal is 1.5, the logic might result in > 100, but Finalize bounds it
            Assert.True(matchPercent <= 100.0);
            Assert.True(matchPercent >= 0.0);
        }

        [Fact]
        public async Task UpdateScoresAsync_UntestedCareerMaxScoreIsNotPenalized()
        {
            var session = new AdaptiveQuizSession
            {
                TargetCareersJson = JsonSerializer.Serialize(new List<string> { "CareerA", "CareerB" })
            };

            // Question only tests CareerA. CareerB is NOT in discriminates array.
            var question = CreateQuestion("Competency", new List<string> { "CareerA" }, 1.0, 0.0, 0.0); 
            
            await _engine.UpdateScoresAsync(session, question, 0);
            var result = await _engine.FinalizeSessionAsync(session);

            var preds = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result.TopPredictionsJson);
            
            var scoreA = double.Parse(preds.First(p => p["nodeId"].ToString() == "CareerA")["matchPercent"].ToString());
            var scoreB = double.Parse(preds.First(p => p["nodeId"].ToString() == "CareerB")["matchPercent"].ToString());

            Assert.Equal(100.0, scoreA); // 1.0 / 1.0 = 100%
            Assert.Equal(0.0, scoreB);   // 0.0 / 0.0 = 0% (Since max score for B remains 0, Finalize outputs 0)
        }

        [Fact]
        public async Task UpdateScoresAsync_AnswerContributionsAreTraceableInState()
        {
            var session = new AdaptiveQuizSession
            {
                TargetCareersJson = JsonSerializer.Serialize(new List<string> { "CareerA" })
            };

            var question = CreateQuestion("Reasoning", new List<string> { "CareerA" }, 0.5, 0.0, 0.0); 
            
            await _engine.UpdateScoresAsync(session, question, 0);

            // Weight for Reasoning is 0.10.
            // Raw score should be 0.5 * 0.10 = 0.05
            // Max score should be 1.0 * 0.10 = 0.10
            Assert.Contains("\"RawScore\":0.05", session.CareerScoresJson);
            Assert.Contains("\"MaxPossibleScore\":0.1", session.CareerScoresJson);
            
            var result = await _engine.FinalizeSessionAsync(session);
            
            // (0.05 / 0.10) * 100 = 50%
            var preds = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result.TopPredictionsJson);
            var scoreA = double.Parse(preds.First(p => p["nodeId"].ToString() == "CareerA")["matchPercent"].ToString());
            Assert.Equal(50.0, scoreA);
        }
    }
}
