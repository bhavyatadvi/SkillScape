using System.Collections.Generic;
using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface IRoadmapService
{
    /// <summary>
    /// Gets the roadmap specifically tailored to the user's ML-predicted Career Domain.
    /// Includes completion status for each step based on UserSkill progress.
    /// Returns null if quiz not completed or domain not found.
    /// </summary>
    Task<RoadmapDto?> GetMyRoadmapAsync(string userId);

    /// <summary>
    /// Gets available roadmap domains for the user to switch between.
    /// </summary>
    Task<List<RoadmapOptionDto>> GetRoadmapOptionsAsync(string userId);

    /// <summary>
    /// Gets roadmap career keys intentionally removed by the user.
    /// </summary>
    Task<List<string>> GetDismissedRoadmapKeysAsync(string userId);

    /// <summary>
    /// Gets roadmap for a specific selected career domain.
    /// </summary>
    Task<RoadmapDto> GetRoadmapByDomainAsync(string userId, string domainId);

    /// <summary>
    /// Marks a specific roadmap step (Skill/Module) as Complete, updating UserSkill and UserProgress.
    /// </summary>
    Task<bool> MarkStepCompleteAsync(string userId, string stepId);

    /// <summary>
    /// Gets the quiz associated with a roadmap module (without exposing correct answers).
    /// </summary>
    Task<RoadmapModuleQuizDto> GetModuleQuizAsync(string moduleId);

    /// <summary>
    /// Submits and evaluates answers for a module quiz. On passing, unlocks completion.
    /// </summary>
    Task<RoadmapQuizResultDto> SubmitModuleQuizAsync(string userId, string moduleId, List<int> userAnswers);

    /// <summary>
    /// Forces fresh AI generation of a roadmap for a career key.
    /// </summary>
    Task<RoadmapDto> RegenerateRoadmapAsync(string userId, string careerKey);

    /// <summary>
    /// Returns user's saved roadmaps list with progress %.
    /// </summary>
    Task<List<UserSavedRoadmapDto>> GetUserSavedRoadmapsAsync(string userId);

    /// <summary>
    /// Saves a roadmap into user's "My Roadmaps" list.
    /// </summary>
    Task<bool> SaveUserRoadmapAsync(string userId, string careerKey, string? careerTitle = null);

    /// <summary>
    /// Accepts a career recommendation and makes its comprehensive roadmap the user's active roadmap.
    /// </summary>
    Task<RoadmapDto> AcceptCareerRoadmapAsync(string userId, string careerKey, string? careerTitle = null);

    /// <summary>
    /// Removes a roadmap link from user's "My Roadmaps" list.
    /// </summary>
    Task<bool> RemoveUserRoadmapAsync(string userId, string careerKey);

    /// <summary>
    /// Sets a saved roadmap as the user's default on login.
    /// </summary>
    Task<bool> SetDefaultUserRoadmapAsync(string userId, string careerKey);
}
