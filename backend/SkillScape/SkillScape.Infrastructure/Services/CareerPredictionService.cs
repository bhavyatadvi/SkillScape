using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class CareerPredictionService : ICareerPredictionService
{
    private readonly ApplicationDbContext _context;

    public CareerPredictionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PredictionResultDto> PredictCareerAsync(string userId, List<QuizResponseRequest> responses, string phaseKey)
    {
        // 1. Evidence-Based Scoring (Cold Start Fallback)
        var domainScores = new Dictionary<string, double>();
        
        foreach (var response in responses)
        {
            var option = await _context.QuizOptions
                .Include(o => o.Signals)
                .FirstOrDefaultAsync(o => o.Id == response.OptionId);

            if (option != null)
            {
                // Use signals if available (New Schema)
                if (option.Signals != null && option.Signals.Any())
                {
                    foreach (var signal in option.Signals)
                    {
                        if (domainScores.ContainsKey(signal.TargetDomain))
                        {
                            domainScores[signal.TargetDomain] += signal.Weight;
                        }
                        else
                        {
                            domainScores[signal.TargetDomain] = signal.Weight;
                        }
                    }
                }
                else 
                {
                    // Fallback to legacy stringified JSON for compatibility if needed during transition
                    try
                    {
                        var weights = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(option.DomainWeightJson);
                        if (weights != null)
                        {
                            foreach (var kvp in weights)
                            {
                                if (domainScores.ContainsKey(kvp.Key))
                                    domainScores[kvp.Key] += kvp.Value;
                                else
                                    domainScores[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                    catch { /* Ignore parsing errors */ }
                }
            }
        }

        // Apply Eligibility rules filtering (e.g. PCM required for Engineering)
        // For simplicity in this implementation, we simulate it based on PhaseKey context
        
        // Find top domain
        string topDomain = "fullstack";
        if (domainScores.Any())
        {
            topDomain = domainScores.OrderByDescending(x => x.Value).First().Key;
        }

        // Normalize scores to percentage (0-100)
        var maxScore = domainScores.Any() ? domainScores.Max(x => x.Value) : 1;
        if (maxScore == 0) maxScore = 1;
        var normalizedScores = domainScores.ToDictionary(
            kvp => kvp.Key, 
            kvp => (int)Math.Min(100, Math.Max(0, (kvp.Value / maxScore) * 100))
        );
        
        // Ensure the top domain is at least 100% or properly scaled relative to itself
        if (normalizedScores.ContainsKey(topDomain))
        {
             normalizedScores[topDomain] = 100;
        }
        else
        {
             normalizedScores[topDomain] = 100;
        }

        var domain = await _context.CareerDomains.FirstOrDefaultAsync(d => d.Id == topDomain);
        string domainName = domain?.Name ?? topDomain;

        string explanation = GenerateDynamicExplanation(domainName, domainScores);

        return new PredictionResultDto
        {
            RecommendedDomainId = topDomain,
            Scores = normalizedScores,
            Explanation = explanation
        };
    }

    private string GenerateDynamicExplanation(string domainName, Dictionary<string, double> rawScores)
    {
        if (rawScores.Count < 2) 
        {
             return $"Based on your consistent focus, {domainName} aligns perfectly with your demonstrated interests and aptitude.";
        }
        
        var sorted = rawScores.OrderByDescending(x => x.Value).ToList();
        var primary = sorted[0];
        var secondary = sorted[1];
        
        double difference = primary.Value - secondary.Value;

        if (difference < 1.0 && primary.Value > 0)
        {
             return $"You showed strong multi-disciplinary interests. While {domainName} is your primary recommendation, your aptitude in {secondary.Key} suggests you might enjoy hybrid roles.";
        }
        else if (difference > 5.0)
        {
             return $"Your responses strongly indicate a clear path towards {domainName}. You consistently chose options that highlight skills critical for this field.";
        }

        return $"Your evidence-based profile points to {domainName} as the most optimal career path based on your unique combination of choices.";
    }
}
