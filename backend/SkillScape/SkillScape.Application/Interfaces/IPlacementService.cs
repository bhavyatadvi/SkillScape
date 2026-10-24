using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

/// <summary>
/// Service interface for the Placement Readiness & Skill Gap Analysis Module
/// </summary>
public interface IPlacementService
{
    /// <summary>Returns all available target job roles with metadata</summary>
    Task<List<PlacementRoleDto>> GetAvailableRolesAsync();

    /// <summary>Returns role-specific assessment questions (without correct answers)</summary>
    Task<List<PlacementQuestionDto>> GetAssessmentQuestionsAsync(string role);

    /// <summary>
    /// Scores submitted answers, calls ML model, saves result, returns full breakdown
    /// </summary>
    Task<PlacementResultDto> SubmitAssessmentAsync(string userId, SubmitPlacementRequest request);

    /// <summary>Returns the authenticated user's past assessment history</summary>
    Task<List<PlacementHistoryDto>> GetMyAssessmentHistoryAsync(string userId);

    /// <summary>Returns a specific assessment result by ID</summary>
    Task<PlacementResultDto> GetAssessmentByIdAsync(string userId, string assessmentId);
}
