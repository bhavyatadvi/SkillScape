using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class SemanticDuplicateDetector : IDuplicateDetector
{
    private readonly ApplicationDbContext _context;
    private readonly ISemanticSimilarityService _similarityService;
    
    // Limits based on Phase 4 specification
    private const double DUPLICATE_THRESHOLD = 0.85;
    private const int MAX_CONCEPT_COUNT = 2;
    private const int MAX_COMPETENCY_COUNT = 3;

    public SemanticDuplicateDetector(ApplicationDbContext context, ISemanticSimilarityService similarityService)
    {
        _context = context;
        _similarityService = similarityService;
    }

    public async Task<bool> IsDuplicateAsync(QuizQuestionV2 candidate, List<string> answeredQuestionIds)
    {
        if (!answeredQuestionIds.Any()) return false;

        var answeredQuestions = await _context.QuizQuestionsV2
            .AsNoTracking()
            .Where(q => answeredQuestionIds.Contains(q.Id))
            .ToListAsync();

        // Count current saturation
        int conceptCount = answeredQuestions.Count(q => !string.IsNullOrEmpty(q.ConceptId) && q.ConceptId == candidate.ConceptId);
        int competencyCount = answeredQuestions.Count(q => !string.IsNullOrEmpty(q.CompetencyId) && q.CompetencyId == candidate.CompetencyId);

        // Rule 4: same concept already sufficiently tested
        if (!string.IsNullOrEmpty(candidate.ConceptId) && conceptCount >= MAX_CONCEPT_COUNT)
        {
            return true;
        }

        // Rule 5: same competency repeated without adding meaningful information
        if (!string.IsNullOrEmpty(candidate.CompetencyId) && competencyCount >= MAX_COMPETENCY_COUNT)
        {
            return true;
        }

        foreach (var answered in answeredQuestions)
        {
            // Rule 1: exact duplicate
            if (candidate.Id == answered.Id || candidate.Text.Trim().Equals(answered.Text.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Rule 3: same scenario with minor wording changes
            if (candidate.QuestionType == "Scenario" && answered.QuestionType == "Scenario")
            {
                double scenarioSimilarity = await _similarityService.ComputeSimilarityAsync(candidate.Text, answered.Text);
                if (scenarioSimilarity > 0.40) // Lower threshold for scenarios testing same premise
                {
                    return true;
                }
            }

            double similarity = await _similarityService.ComputeSimilarityAsync(candidate.Text, answered.Text);
            double dimensionOverlap = CalculateDimensionOverlap(candidate, answered);
            
            // Rule 2: semantically equivalent (same competency + high semantic similarity)
            if (candidate.CompetencyId == answered.CompetencyId && similarity > 0.65)
            {
                return true; // Too similar within the same competency
            }
            
            if (similarity > DUPLICATE_THRESHOLD || (similarity > 0.6 && dimensionOverlap > 0.8))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<double> CalculateNoveltyScoreAsync(QuizQuestionV2 candidate, List<string> answeredQuestionIds)
    {
        if (!answeredQuestionIds.Any()) return 1.0;

        var answeredQuestions = await _context.QuizQuestionsV2
            .AsNoTracking()
            .Where(q => answeredQuestionIds.Contains(q.Id))
            .ToListAsync();

        double maxSimilarity = 0.0;
        foreach (var answered in answeredQuestions)
        {
            double similarity = await _similarityService.ComputeSimilarityAsync(candidate.Text, answered.Text);
            if (similarity > maxSimilarity) maxSimilarity = similarity;
        }

        return 1.0 - maxSimilarity;
    }

    private double CalculateDimensionOverlap(QuizQuestionV2 q1, QuizQuestionV2 q2)
    {
        try
        {
            var t1 = JsonSerializer.Deserialize<List<string>>(q1.TargetDiscriminatesJson) ?? new();
            var t2 = JsonSerializer.Deserialize<List<string>>(q2.TargetDiscriminatesJson) ?? new();
            
            if (!t1.Any() || !t2.Any()) return 0.0;
            
            var intersect = t1.Intersect(t2).Count();
            var union = t1.Union(t2).Count();
            
            return (double)intersect / union;
        }
        catch
        {
            return 0.0;
        }
    }
}

public class LocalSemanticSimilarityService : ISemanticSimilarityService
{
    private static readonly Dictionary<string, string> Synonyms = new(StringComparer.OrdinalIgnoreCase)
    {
        { "monitoring", "watch" },
        { "watching", "watch" },
        { "monitor", "watch" },
        { "network", "network" },
        { "traffic", "logs" },
        { "logs", "logs" },
        { "attacks", "suspicious" },
        { "suspicious", "suspicious" },
        { "activity", "activity" },
        { "interested", "enjoy" },
        { "enjoy", "enjoy" },
        { "leading", "lead" },
        { "team", "colleague" },
        { "member", "colleague" },
        { "quits", "resigns" },
        { "respond", "do" }
    };

    public Task<double> ComputeSimilarityAsync(string text1, string text2)
    {
        if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
            return Task.FromResult(0.0);

        var t1 = NormalizeText(text1);
        var t2 = NormalizeText(text2);

        if (t1 == t2) return Task.FromResult(1.0);

        var tokens1 = t1.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var tokens2 = t2.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        if (tokens1.Count == 0 || tokens2.Count == 0) return Task.FromResult(0.0);

        var intersection = tokens1.Intersect(tokens2).Count();
        var union = tokens1.Union(tokens2).Count();
        
        double jaccard = (double)intersection / union;

        return Task.FromResult(jaccard);
    }

    private static string NormalizeText(string text)
    {
        text = text.ToLowerInvariant();
        text = Regex.Replace(text, @"[^\w\s]", ""); // Remove punctuation
        
        var stopWords = new HashSet<string> { "what", "which", "do", "you", "the", "a", "an", "is", "are", "most", "for", "how", "in", "would" };
        
        var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                         .Where(t => !stopWords.Contains(t))
                         .Select(t => Synonyms.TryGetValue(t, out var syn) ? syn : t);
                         
        return string.Join(" ", tokens);
    }
}
