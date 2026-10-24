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
    public class SubdomainDetectionTests
    {
        private readonly MultiDimensionalScoringEngine _engine;

        public SubdomainDetectionTests()
        {
            _engine = new MultiDimensionalScoringEngine();
        }

        private QuizQuestionV2 CreateSubdomainQuestion(string type, List<string> targetSubdomains, string signalNodeId, double signalWeight)
        {
            var options = new List<object>
            {
                new
                {
                    index = 0,
                    text = "Subdomain specific choice",
                    signals = new List<object>
                    {
                        new { nodeId = signalNodeId, weight = signalWeight }
                    }
                }
            };

            return new QuizQuestionV2
            {
                Id = Guid.NewGuid().ToString(),
                QuestionType = type,
                SubdomainIdsJson = JsonSerializer.Serialize(targetSubdomains),
                OptionsJson = JsonSerializer.Serialize(options)
            };
        }

        [Fact]
        public async Task FinalizeSessionAsync_InsufficientEvidence_ReturnsWarning()
        {
            var session = new AdaptiveQuizSession();
            
            // Only 1 independent question contributes to the "cyber.soc" subdomain
            var q1 = CreateSubdomainQuestion("Subdomain", new List<string> { "cyber.soc", "cyber.network" }, "cyber.soc", 1.0);
            
            await _engine.UpdateScoresAsync(session, q1, 0);
            var result = await _engine.FinalizeSessionAsync(session);

            // Because EvidenceCount is 1 (< 2), the first item should be INSUFFICIENT_EVIDENCE
            var subPreds = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result.TopSubdomainsJson);
            Assert.NotNull(subPreds);
            Assert.True(subPreds.Count > 0);
            Assert.Equal("INSUFFICIENT_EVIDENCE", subPreds[0]["nodeId"].ToString());
            Assert.Equal("Not enough evidence to confidently distinguish a specialization.", subPreds[0]["warning"].ToString());
        }

        [Fact]
        public async Task FinalizeSessionAsync_SufficientEvidence_ReturnsValidSubdomain()
        {
            var session = new AdaptiveQuizSession();
            
            // 2 independent questions contribute to the "cyber.soc" subdomain
            var q1 = CreateSubdomainQuestion("Subdomain", new List<string> { "cyber.soc", "cyber.network" }, "cyber.soc", 1.0);
            var q2 = CreateSubdomainQuestion("Subdomain", new List<string> { "cyber.soc", "cyber.forensics" }, "cyber.soc", 0.8);
            
            await _engine.UpdateScoresAsync(session, q1, 0);
            await _engine.UpdateScoresAsync(session, q2, 0);
            var result = await _engine.FinalizeSessionAsync(session);

            // Because EvidenceCount is 2 (>= 2), it should correctly resolve to cyber.soc
            var subPreds = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result.TopSubdomainsJson);
            Assert.NotNull(subPreds);
            Assert.True(subPreds.Count > 0);
            Assert.Equal("cyber.soc", subPreds[0]["nodeId"].ToString());
            
            var evidenceCount = int.Parse(subPreds[0]["evidenceCount"].ToString());
            Assert.Equal(2, evidenceCount);
        }

        [Fact]
        public async Task UpdateScoresAsync_TracksSubdomainIndependenceCorrectly()
        {
            var session = new AdaptiveQuizSession();
            
            // 3 questions, 1 for network, 2 for forensics
            var q1 = CreateSubdomainQuestion("Subdomain", new List<string> { "cyber.network", "cyber.forensics" }, "cyber.network", 1.0);
            var q2 = CreateSubdomainQuestion("Subdomain", new List<string> { "cyber.forensics" }, "cyber.forensics", 1.0);
            var q3 = CreateSubdomainQuestion("Subdomain", new List<string> { "cyber.forensics", "cyber.soc" }, "cyber.forensics", 0.5);
            
            await _engine.UpdateScoresAsync(session, q1, 0);
            await _engine.UpdateScoresAsync(session, q2, 0);
            await _engine.UpdateScoresAsync(session, q3, 0);
            
            var result = await _engine.FinalizeSessionAsync(session);
            var subPreds = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result.TopSubdomainsJson);

            // Top subdomain should be forensics since it has 2 evidence counts
            Assert.Equal("cyber.forensics", subPreds[0]["nodeId"].ToString());
            Assert.Equal(2, int.Parse(subPreds[0]["evidenceCount"].ToString())); // q2 and q3

            // Network should have 1 evidence count, so it is filtered out by the >= 2 rule.
            var network = subPreds.FirstOrDefault(p => p["nodeId"].ToString() == "cyber.network");
            Assert.Null(network);
        }
    }
}
