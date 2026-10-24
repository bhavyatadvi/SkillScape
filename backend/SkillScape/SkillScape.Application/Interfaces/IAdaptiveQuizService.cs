using System.Collections.Generic;
using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

/// <summary>
/// Core interface for the V2 adaptive quiz system.
/// Implements context-aware, hierarchical, adaptive, evidence-based career prediction.
/// </summary>
public interface IAdaptiveQuizService
{
    /// <summary>
    /// Start a new quiz session for the given phase and initial profile.
    /// Returns the session ID and the first question.
    /// </summary>
    Task<(AdaptiveQuizSessionDto Session, AdaptiveQuizNextResponse FirstQuestion)> StartSessionAsync(
        string userId,
        AdaptiveQuizStartRequest request);

    /// <summary>
    /// Submit an answer for the current question and get the next question.
    /// If IsComplete=true on the response, call SubmitSessionAsync to get results.
    /// </summary>
    Task<AdaptiveQuizNextResponse> AnswerAndGetNextAsync(
        string userId,
        AdaptiveQuizAnswerRequest request);

    /// <summary>
    /// Finalize the session, run the scoring engine, and return the full prediction result.
    /// </summary>
    Task<QuizV2ResultDto> SubmitSessionAsync(
        string userId,
        string sessionId);

    /// <summary>Get the most recent completed quiz result for a user.</summary>
    Task<QuizV2ResultDto?> GetLatestResultAsync(string userId);

    /// <summary>Get all completed quiz results for a user.</summary>
    Task<List<QuizV2ResultDto>> GetResultsAsync(string userId);

    /// <summary>Get a specific quiz result by ID.</summary>
    Task<QuizV2ResultDto?> GetResultByIdAsync(string userId, string resultId);

    /// <summary>Delete a specific quiz result by ID.</summary>
    Task<bool> DeleteResultAsync(string userId, string resultId);

    /// <summary>Get available phase information including sub-phases.</summary>
    Task<List<PhaseInfoDto>> GetPhasesAsync();
}
