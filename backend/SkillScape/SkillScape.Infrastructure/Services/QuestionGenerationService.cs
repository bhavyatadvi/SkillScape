using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class QuestionGenerationService : IQuestionGenerationService
{
    private readonly ApplicationDbContext _context;
    private readonly IQuestionQualityValidator _qualityValidator;
    private readonly IQuestionRelevanceValidator _relevanceValidator;
    private readonly IAgeLevelValidator _ageValidator;
    private readonly IAnswerOptionValidator _optionValidator;
    private readonly IDuplicateDetector _duplicateDetector;

    public QuestionGenerationService(
        ApplicationDbContext context,
        IQuestionQualityValidator qualityValidator,
        IQuestionRelevanceValidator relevanceValidator,
        IAgeLevelValidator ageValidator,
        IAnswerOptionValidator optionValidator,
        IDuplicateDetector duplicateDetector)
    {
        _context = context;
        _qualityValidator = qualityValidator;
        _relevanceValidator = relevanceValidator;
        _ageValidator = ageValidator;
        _optionValidator = optionValidator;
        _duplicateDetector = duplicateDetector;
    }

    public async Task<List<QuizQuestionV2>> GetValidatedCandidatesAsync(
        string phaseKey, 
        string subPhaseKey, 
        Dictionary<string, double> prior, 
        List<string> answeredQuestionIds)
    {
        // 1. Fetch raw candidates (In a full implementation, this could invoke an LLM generation agent.
        // For now, we pull from the DB bank which acts as our generated pool).
        var rawCandidates = await _context.QuizQuestionsV2
            .Where(q => q.IsActive 
                && q.PhaseKey == phaseKey 
                && (q.SubPhaseKey == subPhaseKey || q.SubPhaseKey == string.Empty || string.IsNullOrEmpty(subPhaseKey))
                && (!((phaseKey == "AfterGraduation" || phaseKey == "AfterPostGraduation"))
                    || (!q.QuestionType.ToLower().Contains("direction")
                        && !q.Text.ToLower().Contains("primary goal")))
                && !answeredQuestionIds.Contains(q.Id))
            .ToListAsync();

        var validCandidates = new List<QuizQuestionV2>();

        // 2. Run the strict validation pipeline
        foreach (var q in rawCandidates)
        {
            if (!_qualityValidator.IsValid(q)) continue;
            if (!_relevanceValidator.IsRelevant(q, prior)) continue;
            if (!_ageValidator.IsAgeAppropriate(q, phaseKey)) continue;
            if (!_optionValidator.AreOptionsValid(q)) continue;
            
            if (await _duplicateDetector.IsDuplicateAsync(q, answeredQuestionIds)) continue;

            validCandidates.Add(q);
        }

        return validCandidates;
    }
}

// ── Basic Validator Implementations ───────────────────────────────────────

public class QuestionQualityValidator : IQuestionQualityValidator
{
    public bool IsValid(QuizQuestionV2 question)
    {
        return !string.IsNullOrWhiteSpace(question.Id) 
            && !string.IsNullOrWhiteSpace(question.Text)
            && !string.IsNullOrWhiteSpace(question.PhaseKey);
    }
}

public class QuestionRelevanceValidator : IQuestionRelevanceValidator
{
    public bool IsRelevant(QuizQuestionV2 question, Dictionary<string, double> prior)
    {
        // A question is relevant if it discriminates targets that are still possible (> 0)
        try
        {
            var discriminates = JsonSerializer.Deserialize<List<string>>(question.TargetDiscriminatesJson) ?? new();
            if (!discriminates.Any()) return true; // General question
            
            return discriminates.Any(d => prior.TryGetValue(d, out var prob) && prob > 0.001);
        }
        catch
        {
            return false;
        }
    }
}

public class AgeLevelValidator : IAgeLevelValidator
{
    public bool IsAgeAppropriate(QuizQuestionV2 question, string phaseKey)
    {
        // Placeholder for complexity checks (Flesch-Kincaid etc).
        // For now, assume questions are pre-filtered by PhaseKey which represents age grouping.
        return question.PhaseKey == phaseKey;
    }
}

public class AnswerOptionValidator : IAnswerOptionValidator
{
    public bool AreOptionsValid(QuizQuestionV2 question)
    {
        try
        {
            var options = JsonSerializer.Deserialize<List<OptionV2>>(question.OptionsJson);
            if (options == null || options.Count < 2) return false;
            
            // Validate all options have distinct text and at least some signals
            var texts = new HashSet<string>();
            foreach (var opt in options)
            {
                if (string.IsNullOrWhiteSpace(opt.Text)) return false;
                if (!texts.Add(opt.Text.ToLowerInvariant())) return false; // Duplicate options
                if (opt.Signals == null || !opt.Signals.Any()) return false;
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private class OptionV2
    {
        public string Text { get; set; } = string.Empty;
        public List<object> Signals { get; set; } = new();
    }
}

