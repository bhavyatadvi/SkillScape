using System.Collections.Generic;
using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface IQuizMlPredictionService
{
    /// <summary>
    /// Evaluates candidate career scores using ML.NET feature pipeline & hybrid scoring model.
    /// Operates explicitly in RULE_BASED_MODE, HYBRID_MODE, or ML_MODE.
    /// Computes overall fit using weighted formula:
    /// 0.25 * InterestFit + 0.20 * SkillFit + 0.15 * AcademicFit + 0.20 * GoalFit + 0.20 * Readiness.
    /// </summary>
    Task<Dictionary<string, double>> RankCandidatesAsync(
        ParsedProfileDto profile,
        Dictionary<string, double> bayesianScores,
        List<EvidenceItemDto> evidence);

    /// <summary>
    /// Gets current ML system operating mode (RULE_BASED_MODE, HYBRID_MODE, ML_MODE).
    /// </summary>
    string GetCurrentOperatingMode();
}
