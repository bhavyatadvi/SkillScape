using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkillScape.Application.Interfaces;

public interface IModelTrainingService
{
    Task TrainAllModelsAsync();
}

public interface IPredictionExplainerService
{
    string GenerateExplanation(
        string recommendedNodeId,
        string recommendedNodeName,
        List<PredictionScore> alternatives,
        Dictionary<string, double> featureContributions);
}

public class PredictionScore
{
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public double Score { get; set; }
}

public interface IEligibilityService
{
    Task<EligibilityResult> EvaluateEligibilityAsync(string profileJson, string targetNodeId);
}

public class EligibilityResult
{
    public bool IsEligible { get; set; }
    public string Reason { get; set; } = string.Empty;
}
