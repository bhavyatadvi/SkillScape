using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SkillScape.Application.DTOs;
using SkillScape.Domain.Entities;
using Xunit;

namespace SkillScape.Tests
{
    public class FinalResultGenerationTests
    {
        private readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        [Fact]
        public void FinalResult_MapsScoreExactly()
        {
            var careerScores = new Dictionary<string, object>
            {
                { "cyber", new { MaxPossibleScore = 100, RawScore = 89 } }
            };

            var (bestMatch, scoreText, isClose) = ParseResults(JsonSerializer.Serialize(careerScores));
            Assert.Equal("cyber", bestMatch);
            Assert.Equal("89/100 current profile alignment.", scoreText);
            Assert.False(isClose);
        }

        [Fact]
        public void FinalResult_DetectsCloseMatch()
        {
            var careerScores = new Dictionary<string, object>
            {
                { "med.pediatrics", new { MaxPossibleScore = 100, RawScore = 89 } },
                { "med.general", new { MaxPossibleScore = 100, RawScore = 86 } }
            };

            var (bestMatch, scoreText, isClose) = ParseResults(JsonSerializer.Serialize(careerScores));
            Assert.Equal("med.pediatrics", bestMatch);
            Assert.Equal("89/100 current profile alignment.", scoreText);
            Assert.True(isClose);
        }

        [Fact]
        public void FinalResult_DoesNotReportProbabilityOfSuccess()
        {
            var careerScores = new Dictionary<string, object>
            {
                { "cyber", new { MaxPossibleScore = 100, RawScore = 89 } }
            };

            var (bestMatch, scoreText, isClose) = ParseResults(JsonSerializer.Serialize(careerScores));
            Assert.DoesNotContain("probability of success", scoreText, StringComparison.OrdinalIgnoreCase);
        }

        private (string BestMatch, string ScoreText, bool IsClose) ParseResults(string careerScoresJson)
        {
            var careerScores = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(careerScoresJson, _jsonOpts) ?? new();
            var parsedTargetCareers = new List<ScoredCareerDto>();
            foreach (var kvp in careerScores)
            {
                double maxScore = kvp.Value.TryGetProperty("MaxPossibleScore", out JsonElement maxEl) ? maxEl.GetDouble() : 0;
                double rawScore = kvp.Value.TryGetProperty("RawScore", out JsonElement rawEl) ? rawEl.GetDouble() : 0;
                if (maxScore > 0)
                {
                    var norm = (int)Math.Round((rawScore / maxScore) * 100);
                    parsedTargetCareers.Add(new ScoredCareerDto { CareerName = kvp.Key, Score = norm });
                }
            }
            parsedTargetCareers = parsedTargetCareers.OrderByDescending(c => c.Score).ToList();

            string bestMatchName = "Unknown";
            string compScoreText = "0/100 current profile alignment.";
            bool isClose = false;

            if (parsedTargetCareers.Any())
            {
                bestMatchName = parsedTargetCareers[0].CareerName;
                compScoreText = $"{parsedTargetCareers[0].Score}/100 current profile alignment.";
                if (parsedTargetCareers.Count > 1 && (parsedTargetCareers[0].Score - parsedTargetCareers[1].Score) < 5)
                {
                    isClose = true;
                }
            }
            return (bestMatchName, compScoreText, isClose);
        }
    }
}
