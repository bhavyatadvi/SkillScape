using System;
using System.Collections.Generic;
using System.Linq;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Drives the quiz through 5 explicit stages.
/// Transitions are based on probability distribution confidence and question count.
/// The state machine is the single source of truth for "what kind of question to ask next."
/// </summary>
public enum QuizStage
{
    /// <summary>Q1-3: Discover the broad career field</summary>
    BroadFieldDiscovery,
    /// <summary>Q4-5: Confirm the leading field with targeted scenario questions</summary>
    FieldConfirmation,
    /// <summary>Q6-10: Discover the specific subdomain within the confirmed field</summary>
    SubdomainDiscovery,
    /// <summary>Q11-12: Final validation to resolve remaining ambiguity</summary>
    FinalValidation,
    /// <summary>Terminal: result can be generated</summary>
    Completed
}

public class QuizStageEngine
{
    private const double FieldConfirmThreshold = 0.45;
    private const double SubdomainThreshold    = 0.58;
    private const double FinalValidationThresh = 0.65;
    private const double CompletionThreshold   = 0.72;
    private const double CompletionGap         = 0.22;

    private const int MinBeforeFieldConfirm    = 3; // Enforce at least 3 broad questions
    private const int MinBeforeSubdomain       = 5;
    private const int MinBeforeFinalValidation = 7;
    private const int MinBeforeCompletion      = 5;
    private const int MaxQuestions             = 12;

    public QuizStage GetCurrentStage(
        Dictionary<string, double> prior,
        int answeredCount,
        string? savedStage = null)
    {
        if (answeredCount >= MaxQuestions) return QuizStage.Completed;
        if (answeredCount >= MinBeforeCompletion && CanComplete(prior))
            return QuizStage.Completed;

        if (!string.IsNullOrEmpty(savedStage) &&
            Enum.TryParse<QuizStage>(savedStage, out var saved))
        {
            var computed = ComputeStageFromDistribution(prior, answeredCount);
            return (int)computed >= (int)saved ? computed : saved;
        }

        return ComputeStageFromDistribution(prior, answeredCount);
    }

    private static QuizStage ComputeStageFromDistribution(
        Dictionary<string, double> prior,
        int answeredCount)
    {
        if (!prior.Any()) return QuizStage.BroadFieldDiscovery;

        var sorted = prior.OrderByDescending(kv => kv.Value).ToList();
        double top1 = sorted.Count > 0 ? sorted[0].Value : 0;

        if (answeredCount >= MinBeforeFinalValidation && top1 >= FinalValidationThresh)
            return QuizStage.FinalValidation;
        if (answeredCount >= MinBeforeSubdomain && top1 >= SubdomainThreshold)
            return QuizStage.SubdomainDiscovery;
        if (answeredCount >= MinBeforeFieldConfirm && top1 >= FieldConfirmThreshold)
            return QuizStage.FieldConfirmation;

        return QuizStage.BroadFieldDiscovery;
    }

    private static bool CanComplete(Dictionary<string, double> prior)
    {
        if (!prior.Any()) return false;
        var sorted = prior.OrderByDescending(kv => kv.Value).ToList();
        double top1 = sorted.Count > 0 ? sorted[0].Value : 0;
        double top2 = sorted.Count > 1 ? sorted[1].Value : 0;
        return top1 >= CompletionThreshold && (top1 - top2) >= CompletionGap;
    }

    public static string GetLeadingField(Dictionary<string, double> prior)
    {
        if (!prior.Any()) return string.Empty;
        var top = prior.OrderByDescending(kv => kv.Value).First();
        var parts = top.Key.Split('.');
        return parts.Length > 3 ? string.Join(".", parts.Take(3)) : top.Key;
    }

    public static List<string> GetTopCandidates(Dictionary<string, double> prior, int count = 5)
    {
        return prior
            .OrderByDescending(kv => kv.Value)
            .Take(count)
            .Select(kv => kv.Key)
            .ToList();
    }
}
