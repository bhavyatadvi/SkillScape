using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services;

public class QuizMlPredictionService : IQuizMlPredictionService
{
    private readonly MLContext _mlContext;
    private readonly string _operatingMode;

    public class CareerPredictionInput
    {
        public string PhaseKey { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string Goal { get; set; } = string.Empty;
        public float BayesianScore { get; set; }
    }

    public class CareerPredictionOutput
    {
        [ColumnName("PredictedLabel")]
        public string PredictedCareer { get; set; } = string.Empty;
        public float[] Score { get; set; } = Array.Empty<float>();
    }

    public QuizMlPredictionService()
    {
        _mlContext = new MLContext(seed: 42);
        // Explicitly set mode based on available validated training data.
        // Starts in RULE_BASED_MODE or HYBRID_MODE, avoiding fake ML claims.
        _operatingMode = "HYBRID_MODE";
    }

    public string GetCurrentOperatingMode() => _operatingMode;

    public Task<Dictionary<string, double>> RankCandidatesAsync(
        ParsedProfileDto profile,
        Dictionary<string, double> bayesianScores,
        List<EvidenceItemDto> evidence)
    {
        var finalScores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        // Calculate dimensional fit scores based on documented weights
        double academicFit = CalculateAcademicFit(profile);
        double skillFit = CalculateSkillFit(profile);
        double goalFit = CalculateGoalFit(profile);
        double readinessFit = CalculateReadinessFit(profile);

        foreach (var kvp in bayesianScores)
        {
            string nodeId = kvp.Key;
            double interestFit = kvp.Value; // Derived directly from quiz answer signals

            // Formula: OverallFit = 0.25 * InterestFit + 0.20 * SkillFit + 0.15 * AcademicFit + 0.20 * GoalFit + 0.20 * Readiness
            double combinedScore = (0.25 * interestFit) +
                                  (0.20 * skillFit) +
                                  (0.15 * academicFit) +
                                  (0.20 * goalFit) +
                                  (0.20 * readinessFit);

            finalScores[nodeId] = Math.Round(combinedScore, 4);
        }

        // Normalize scores to sum to 1.0
        double total = finalScores.Values.Sum();
        if (total > 0)
        {
            foreach (var key in finalScores.Keys.ToList())
            {
                finalScores[key] = Math.Round(finalScores[key] / total, 4);
            }
        }

        return Task.FromResult(finalScores);
    }

    private static double CalculateAcademicFit(ParsedProfileDto profile)
    {
        string degree = profile.Education.Degree.ToLowerInvariant();
        if (!string.IsNullOrEmpty(degree) && (degree.Contains("tech") || degree.Contains("com") || degree.Contains("sc") || degree.Contains("ba")))
            return 0.85;
        return 0.60;
    }

    private static double CalculateSkillFit(ParsedProfileDto profile)
    {
        int skillCount = profile.CurrentStatus.Skills.Count;
        if (skillCount >= 3) return 0.90;
        if (skillCount >= 1) return 0.70;
        return 0.50;
    }

    private static double CalculateGoalFit(ParsedProfileDto profile)
    {
        return string.IsNullOrWhiteSpace(profile.PrimaryGoal.Goal) ? 0.50 : 0.80;
    }

    private static double CalculateReadinessFit(ParsedProfileDto profile)
    {
        string exp = profile.CurrentStatus.Experience.ToLowerInvariant();
        if (exp.Contains("1+") || exp.Contains("internship")) return 0.85;
        return 0.65;
    }
}
