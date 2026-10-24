using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkillScape.Application.Interfaces;
using SkillScape.Application.DTOs;
using SkillScape.Domain.Entities;

namespace SkillScape.ML.Models;

public class PredictionExplainerService : IPredictionExplainerService
{
    public string GenerateExplanation(
        string recommendedNodeId,
        string recommendedNodeName,
        List<PredictionScore> alternatives,
        Dictionary<string, double> featureContributions)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Recommended: {recommendedNodeName}");
        sb.AppendLine();
        
        sb.AppendLine("Why:");
        // In a real ML scenario, these would come directly from Feature Contributions (SHAP values)
        var topFeatures = featureContributions.OrderByDescending(f => f.Value).Take(4).ToList();
        foreach (var feature in topFeatures)
        {
            sb.AppendLine($"- Strong alignment with {feature.Key} (Weight: {feature.Value:F2})");
        }
        
        if (alternatives.Any())
        {
            sb.AppendLine();
            sb.AppendLine("Closest alternatives:");
            foreach (var alt in alternatives.Take(2))
            {
                sb.AppendLine($"- {alt.NodeName}: {alt.Score:P0}");
            }

            var closestAlt = alternatives.First();
            sb.AppendLine();
            sb.AppendLine($"Why {recommendedNodeName} won over {closestAlt.NodeName}:");
            sb.AppendLine($"Because the answers showed stronger alignment with {topFeatures.FirstOrDefault().Key} " +
                          $"than the typical requirements of {closestAlt.NodeName}.");
        }

        return sb.ToString();
    }
}
