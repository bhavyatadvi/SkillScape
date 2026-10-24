using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

/// <summary>
/// Core service that computes career readiness and skill gaps.
/// The authoritative readiness score is calculated deterministically here.
/// AI is only used for narrative recommendations.
/// </summary>
public interface ISkillGapAnalyzerService
{
    /// <summary>
    /// Returns the profile status for the authenticated user.
    /// Used by the entry screen to decide whether to show the empty state.
    /// </summary>
    Task<ProfileStatusDto> GetProfileStatusAsync(string userId);

    /// <summary>
    /// Performs the full career readiness and skill gap analysis.
    /// Loads the user's CareerProfile from the database.
    /// Loads career requirements from CareerIntelligence JSON.
    /// Computes deterministic scores.
    /// Calls AI for narrative recommendations only.
    /// </summary>
    Task<SkillAnalyzerResultDto> AnalyzeAsync(string userId, SkillAnalyzerRequest request);
}
