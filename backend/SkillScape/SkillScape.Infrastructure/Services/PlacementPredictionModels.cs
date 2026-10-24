using Microsoft.ML.Data;

namespace SkillScape.Infrastructure.Services;

// =============================================
// ML.NET Data Models for Placement Readiness Predictor
// Uses SDCA Regression trained on synthetic placement data
// =============================================

/// <summary>
/// Input features for PlacementReadinessPredictor model.
/// Each float represents a skill category score (0-100).
/// </summary>
public class PlacementData
{
    [LoadColumn(0)] public float DsaScore { get; set; }
    [LoadColumn(1)] public float ProgrammingScore { get; set; }
    [LoadColumn(2)] public float ProblemSolvingScore { get; set; }
    [LoadColumn(3)] public float AptitudeScore { get; set; }
    [LoadColumn(4)] public float SoftSkillsScore { get; set; }
    [LoadColumn(5)] public float SystemDesignScore { get; set; }
    [LoadColumn(6)] public float DatabasesScore { get; set; }
    [LoadColumn(7)] public float ReadinessScore { get; set; } // Label column
}

/// <summary>
/// Output from PlacementReadinessPredictor ML model.
/// Score is the predicted placement readiness percentage (0-100).
/// </summary>
public class PlacementPrediction
{
    [ColumnName("Score")]
    public float ReadinessScore { get; set; }
}
