using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SkillScape.Domain.Entities;

namespace SkillScape.Infrastructure.Services
{
    public interface IMultiDimensionalScoringEngine
    {
        Task UpdateScoresAsync(AdaptiveQuizSession session, QuizQuestionV2 question, int selectedOptionIndex);
        Task<QuizResultV2> FinalizeSessionAsync(AdaptiveQuizSession session);
    }

    public class MultiDimensionalScoringEngine : IMultiDimensionalScoringEngine
    {
        private class CareerScoreTracker
        {
            public double RawScore { get; set; } = 0;
            public double MaxPossibleScore { get; set; } = 0;
        }

        private class SubdomainScoreTracker
        {
            public double RawScore { get; set; } = 0;
            public double MaxPossibleScore { get; set; } = 0;
            public int EvidenceCount { get; set; } = 0;
        }

        private class OptionMapping
        {
            public int index { get; set; }
            public string text { get; set; } = string.Empty;
            public List<OptionSignal> signals { get; set; } = new();
        }

        private class OptionSignal
        {
            public string nodeId { get; set; } = string.Empty;
            public string trait
            {
                set
                {
                    if (string.IsNullOrWhiteSpace(nodeId))
                        nodeId = value ?? string.Empty;
                }
            }
            public double weight { get; set; }
        }

        // ── Career Name to Taxonomy ID Mapping ────────────────────────────────
        // Maps user-entered names ("SOC Analyst") to taxonomy node IDs ("sci.eng.cs.soc")
        private static readonly Dictionary<string, string[]> _careerNameToNodeIds
            = new(StringComparer.OrdinalIgnoreCase)
        {
            // Cybersecurity
            ["soc analyst"]               = new[] { "sci.eng.cs.soc", "sci.eng.cs.ir" },
            ["security operations"]        = new[] { "sci.eng.cs.soc" },
            ["penetration tester"]         = new[] { "sci.eng.cs.pentest", "sci.eng.cs.redteam" },
            ["penetration testing"]        = new[] { "sci.eng.cs.pentest" },
            ["ethical hacker"]             = new[] { "sci.eng.cs.pentest" },
            ["digital forensics analyst"]  = new[] { "sci.eng.cs.forensics" },
            ["digital forensics"]          = new[] { "sci.eng.cs.forensics" },
            ["forensics analyst"]          = new[] { "sci.eng.cs.forensics" },
            ["grc analyst"]                = new[] { "sci.eng.cs.grc" },
            ["governance risk compliance"] = new[] { "sci.eng.cs.grc" },
            ["cloud security engineer"]    = new[] { "sci.eng.cs.cloudsec" },
            ["cloud security"]             = new[] { "sci.eng.cs.cloudsec" },
            ["security architect"]         = new[] { "sci.eng.cs.secarch" },
            ["application security"]       = new[] { "sci.eng.cs.appsec" },
            ["appsec"]                     = new[] { "sci.eng.cs.appsec" },
            ["threat intelligence analyst"]= new[] { "sci.eng.cs.threat" },
            ["threat hunter"]              = new[] { "sci.eng.cs.threat" },
            ["incident responder"]         = new[] { "sci.eng.cs.ir" },
            ["incident response"]          = new[] { "sci.eng.cs.ir" },

            // Medicine
            ["general medicine"]           = new[] { "sci.med.gen_medicine", "sci.med.internal" },
            ["physician"]                  = new[] { "sci.med.gen_medicine" },
            ["internal medicine"]          = new[] { "sci.med.internal", "sci.med.gen_medicine" },
            ["general surgery"]            = new[] { "sci.med.surgery" },
            ["surgeon"]                    = new[] { "sci.med.surgery" },
            ["surgery"]                    = new[] { "sci.med.surgery" },
            ["pediatrics"]                 = new[] { "sci.med.pediatrics" },
            ["pediatrician"]               = new[] { "sci.med.pediatrics" },
            ["emergency medicine"]         = new[] { "sci.med.emergency" },
            ["emergency physician"]        = new[] { "sci.med.emergency" },
            ["psychiatry"]                 = new[] { "sci.med.psychiatry" },
            ["psychiatrist"]               = new[] { "sci.med.psychiatry" },
            ["neurology"]                  = new[] { "sci.med.neurology" },
            ["cardiology"]                 = new[] { "sci.med.cardiology" },
            ["medical research"]           = new[] { "sci.med.research" },
            ["public health"]              = new[] { "sci.med.public_health" },

            // Software
            ["software engineer"]          = new[] { "sci.eng.cs.software" },
            ["backend developer"]          = new[] { "sci.eng.cs.software" },
            ["backend engineer"]           = new[] { "sci.eng.cs.software" },
            ["frontend developer"]         = new[] { "sci.eng.cs.software" },
            ["frontend engineer"]          = new[] { "sci.eng.cs.software" },
            ["full stack developer"]       = new[] { "sci.eng.cs.software" },
            ["devops engineer"]            = new[] { "sci.eng.cs.cloud" },
            ["devops"]                     = new[] { "sci.eng.cs.cloud" },
            ["site reliability engineer"]  = new[] { "sci.eng.cs.cloud" },
            ["sre"]                        = new[] { "sci.eng.cs.cloud" },
            ["mobile developer"]           = new[] { "sci.eng.cs.software" },
            ["data engineer"]              = new[] { "sci.eng.cs.data" },
            ["data scientist"]             = new[] { "sci.eng.cs.data" },
            ["machine learning engineer"]  = new[] { "sci.eng.cs.ai_ml" },
            ["ml engineer"]                = new[] { "sci.eng.cs.ai_ml" },
            ["ai engineer"]                = new[] { "sci.eng.cs.ai_ml" },
            ["data analyst"]               = new[] { "sci.eng.cs.data" },

            // Finance
            ["investment banker"]          = new[] { "com.finance.ib" },
            ["financial analyst"]          = new[] { "com.finance", "com.finance.analyst" },
            ["ca"]                         = new[] { "com.ca" },
            ["chartered accountant"]       = new[] { "com.ca" },
            ["banker"]                     = new[] { "com.banking" },
        };

        public async Task UpdateScoresAsync(AdaptiveQuizSession session, QuizQuestionV2 question, int selectedOptionIndex)
        {
            // Parse existing scores
            var competencyScores = string.IsNullOrEmpty(session.CompetencyScoresJson)
                ? new Dictionary<string, double>()
                : JsonSerializer.Deserialize<Dictionary<string, double>>(session.CompetencyScoresJson) ?? new();

            var subdomainScores = string.IsNullOrEmpty(session.SubdomainScoresJson)
                ? new Dictionary<string, SubdomainScoreTracker>()
                : JsonSerializer.Deserialize<Dictionary<string, SubdomainScoreTracker>>(session.SubdomainScoresJson) ?? new();

            var careerScores = string.IsNullOrEmpty(session.CareerScoresJson)
                ? new Dictionary<string, CareerScoreTracker>()
                : JsonSerializer.Deserialize<Dictionary<string, CareerScoreTracker>>(session.CareerScoresJson) ?? new();

            var targetCareers = string.IsNullOrEmpty(session.TargetCareersJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(session.TargetCareersJson) ?? new();

            var discriminates = string.IsNullOrEmpty(question.TargetDiscriminatesJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(question.TargetDiscriminatesJson) ?? new();

            var options = string.IsNullOrEmpty(question.OptionsJson)
                ? new List<OptionMapping>()
                : JsonSerializer.Deserialize<List<OptionMapping>>(question.OptionsJson) ?? new();

            // Question type weight — higher weight for scenario/competency questions
            double weight = question.QuestionType switch
            {
                "Situational"    => 0.30,
                "Competency"     => 0.30,
                "Reasoning"      => 0.25,
                "WorkPreference" => 0.20,
                "Interest"       => 0.20,
                "TaskPreference" => 0.15,
                "WorkStyle"      => 0.15,
                _                => 0.15
            };

            var selectedOption = options.FirstOrDefault(o => o.index == selectedOptionIndex);
            var signals = selectedOption?.signals ?? new List<OptionSignal>();

            // Build signal lookup: nodeId → weight
            var signalLookup = signals.ToDictionary(
                s => s.nodeId, s => s.weight, StringComparer.OrdinalIgnoreCase);

            // Build career name → node ID mapping for target careers
            var careerNodeMapping = BuildCareerNodeMapping(targetCareers);

            // Initialize all target careers
            foreach (var career in targetCareers)
            {
                if (!careerScores.ContainsKey(career))
                    careerScores[career] = new CareerScoreTracker();
            }

            // ── Career Scoring ────────────────────────────────────────────────
            // For each target career, find the best-matching signal from the chosen answer
            foreach (var career in targetCareers)
            {
                var tracker = careerScores[career];
                var nodeIds = careerNodeMapping.TryGetValue(career, out var ids) ? ids : new[] { career };

                // Find the maximum signal this career can receive from any option
                double maxPossibleSignal = 0;
                foreach (var opt in options)
                {
                    foreach (var sig in opt.signals)
                    {
                        if (nodeIds.Any(n => n.Equals(sig.nodeId, StringComparison.OrdinalIgnoreCase) ||
                                             n.StartsWith(sig.nodeId + ".", StringComparison.OrdinalIgnoreCase) ||
                                             sig.nodeId.StartsWith(n + ".", StringComparison.OrdinalIgnoreCase)))
                        {
                            maxPossibleSignal = Math.Max(maxPossibleSignal, sig.weight);
                        }
                    }
                }

                // Check if question discriminates between this career
                bool questionIsRelevant = discriminates.Any(d =>
                    nodeIds.Any(n => n.Equals(d, StringComparison.OrdinalIgnoreCase) ||
                                     n.StartsWith(d + ".", StringComparison.OrdinalIgnoreCase) ||
                                     d.StartsWith(n + ".", StringComparison.OrdinalIgnoreCase)));

                // Find the actual signal received from the chosen option
                double receivedSignal = 0;
                if (selectedOption != null)
                {
                    foreach (var sig in selectedOption.signals)
                    {
                        if (nodeIds.Any(n => n.Equals(sig.nodeId, StringComparison.OrdinalIgnoreCase) ||
                                             n.StartsWith(sig.nodeId + ".", StringComparison.OrdinalIgnoreCase) ||
                                             sig.nodeId.StartsWith(n + ".", StringComparison.OrdinalIgnoreCase)))
                        {
                            receivedSignal = Math.Max(receivedSignal, sig.weight);
                        }
                    }
                }

                tracker.MaxPossibleScore += maxPossibleSignal * weight;
                tracker.RawScore += receivedSignal * weight;
            }

            // ── Subdomain Scoring ─────────────────────────────────────────────
            // Track all signal node IDs from the chosen answer as subdomain scores
            foreach (var signal in signals)
            {
                var nodeId = signal.nodeId;
                if (string.IsNullOrEmpty(nodeId)) continue;

                if (!subdomainScores.ContainsKey(nodeId))
                    subdomainScores[nodeId] = new SubdomainScoreTracker();

                var tracker = subdomainScores[nodeId];
                tracker.RawScore += signal.weight * weight;
                tracker.EvidenceCount++;

                // Max possible signal for this subdomain across all options
                double maxForSubdomain = options
                    .SelectMany(o => o.signals)
                    .Where(s => s.nodeId.Equals(nodeId, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.weight)
                    .DefaultIfEmpty(0)
                    .Max();
                tracker.MaxPossibleScore += maxForSubdomain * weight;
            }

            // ── Competency Scoring ────────────────────────────────────────────
            if (!string.IsNullOrEmpty(question.CompetencyId))
            {
                if (!competencyScores.ContainsKey(question.CompetencyId))
                    competencyScores[question.CompetencyId] = 0;
                double avgSignal = signals.Any() ? signals.Average(s => s.weight) : 0;
                competencyScores[question.CompetencyId] += avgSignal * weight;
            }

            // ── Confidence Tracking ───────────────────────────────────────────
            var answeredCount = string.IsNullOrEmpty(session.AnsweredQuestionIdsJson)
                ? 1
                : (JsonSerializer.Deserialize<List<string>>(session.AnsweredQuestionIdsJson)?.Count ?? 0) + 1;
            session.ConfidenceScore = Math.Min(100.0, (answeredCount / 15.0) * 100.0);

            // Save state
            session.CompetencyScoresJson = JsonSerializer.Serialize(competencyScores);
            session.SubdomainScoresJson = JsonSerializer.Serialize(subdomainScores);
            session.CareerScoresJson = JsonSerializer.Serialize(careerScores);

            await Task.CompletedTask;
        }

        public async Task<QuizResultV2> FinalizeSessionAsync(AdaptiveQuizSession session)
        {
            var careerScores = string.IsNullOrEmpty(session.CareerScoresJson)
                ? new Dictionary<string, CareerScoreTracker>()
                : JsonSerializer.Deserialize<Dictionary<string, CareerScoreTracker>>(session.CareerScoresJson) ?? new();

            var predictions = new List<object>();
            string primaryNodeId = string.Empty;
            double maxScore = -1;

            // Step 1: Calculate raw normalized ratios (0 to 1) for each career or subdomain
            var rawRatios = new Dictionary<string, double>();
            
            var subdomainScores = string.IsNullOrEmpty(session.SubdomainScoresJson)
                ? new Dictionary<string, SubdomainScoreTracker>()
                : JsonSerializer.Deserialize<Dictionary<string, SubdomainScoreTracker>>(session.SubdomainScoresJson) ?? new();

            if (careerScores.Any())
            {
                foreach (var kvp in careerScores)
                {
                    var tracker = kvp.Value;
                    // A minimum evidence threshold of 2.0 prevents careers with only 1 trivial signal from scoring 100%.
                    double effectiveDenominator = Math.Max(tracker.MaxPossibleScore, 5.0);
                    
                    double ratio = effectiveDenominator > 0 ? (tracker.RawScore / effectiveDenominator) : 0.0;
                    Console.WriteLine($"[Scoring Trace] Career: {kvp.Key} | RawScore: {tracker.RawScore} | MaxPossibleScore: {tracker.MaxPossibleScore} | EffectiveDenominator: {effectiveDenominator} | MatchPercent: {ratio * 100}");

                    rawRatios[kvp.Key] = ratio;
                }
            }
            else
            {
                // Fallback for discovery mode (e.g., After 10th) where no target careers were specified
                foreach (var kvp in subdomainScores.OrderByDescending(x => x.Value.RawScore).Take(5))
                {
                    var tracker = kvp.Value;
                    double ratio = tracker.MaxPossibleScore > 0 ? (tracker.RawScore / tracker.MaxPossibleScore) : 0.0;
                    rawRatios[kvp.Key] = ratio;
                }
            }

            foreach (var kvp in rawRatios)
            {
                double rawPercent = Math.Round(kvp.Value * 100.0, 1);

                predictions.Add(new
                {
                    nodeId = kvp.Key,
                    probability = kvp.Value,
                    matchPercent = rawPercent
                });

                if (rawPercent > maxScore)
                {
                    maxScore = rawPercent;
                    primaryNodeId = kvp.Key;
                }
            }

            predictions = predictions.OrderByDescending(p => ((dynamic)p).probability).ToList();

            // SubPredictions logic remains unchanged, but we already deserialized it above
            var subPredictions = subdomainScores.Select(kvp =>
            {
                var tracker = kvp.Value;
                double norm = tracker.MaxPossibleScore > 0
                    ? Math.Round((tracker.RawScore / tracker.MaxPossibleScore) * 100.0, 1)
                    : 0.0;
                norm = Math.Max(0, Math.Min(100, norm));
                return (object)new { nodeId = kvp.Key, matchPercent = norm, evidenceCount = tracker.EvidenceCount };
            })
            .OrderByDescending(p => ((dynamic)p).matchPercent)
            .ToList();

            var result = new QuizResultV2
            {
                Id = Guid.NewGuid().ToString(),
                SessionId = session.Id,
                UserId = session.UserId,
                PhaseKey = session.PhaseKey,
                PrimaryTaxonomyNodeId = primaryNodeId,
                TopPredictionsJson = JsonSerializer.Serialize(predictions),
                TopSubdomainsJson = JsonSerializer.Serialize(subPredictions),
                ConfidenceScore = Math.Round(session.ConfidenceScore, 1)
            };

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Resolves user-entered career names ("SOC Analyst") to taxonomy node IDs ("sci.eng.cs.soc").
        /// Falls back to using the name as-is if no match is found (handles pre-resolved IDs).
        /// </summary>
        private static Dictionary<string, string[]> BuildCareerNodeMapping(List<string> careerNames)
        {
            var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in careerNames)
            {
                var trimmed = name.Trim();
                if (_careerNameToNodeIds.TryGetValue(trimmed, out var ids))
                {
                    result[name] = ids;
                }
                else
                {
                    // Try partial match
                    var matchKey = _careerNameToNodeIds.Keys.FirstOrDefault(k =>
                        trimmed.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                        k.Contains(trimmed, StringComparison.OrdinalIgnoreCase));
                    result[name] = matchKey != null ? _careerNameToNodeIds[matchKey] : new[] { trimmed };
                }
            }
            return result;
        }
    }
}
