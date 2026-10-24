using System.Collections.Generic;
using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface ILearningCircleService
{
    Task<List<CircleRecommendationDto>> GetRecommendedCirclesAsync(string userId);
    Task<List<LearningCircleDto>> GetAllPublicCirclesAsync(string userId = "");
    Task<List<LearningCircleDto>> GetMyCirclesAsync(string userId);
    Task<bool> HasCompletedCareerQuizAsync(string userId);
    Task<bool> JoinCircleAsync(string userId, string circleId);
    Task<string> RequestJoinCircleAsync(string userId, string circleId); // returns Status
    Task<List<LearningCircleJoinRequestDto>> GetPendingRequestsAsync(string circleId, string userId);
    Task<(bool Success, string RequesterId, string CircleName)> ProcessJoinRequestAsync(string requestId, string userId, bool approve);
    Task<bool> ToggleMeetingStatusAsync(string circleId, string userId, bool isActive);
    
    Task<LearningCircleDto> GetCircleByIdAsync(string circleId, string userId = "");
    Task<LearningCircleDto> CreateCircleAsync(CreateLearningCircleDto dto, string userId);
    Task<List<string>> ScheduleMeetingAsync(string circleId, string userId, DateTime meetingTime);
    Task<List<DiscussionDto>> GetDiscussionsAsync(string circleId, string userId);
    Task<DiscussionDto> CreateDiscussionAsync(string circleId, string userId, CreateDiscussionDto dto);
    Task<bool> ClearDiscussionsAsync(string circleId, string userId);
    Task<List<ResourceDto>> GetResourcesAsync(string circleId, string userId);
    Task<ResourceDto> CreateResourceAsync(string circleId, string userId, CreateResourceDto dto);
    Task<bool> DeleteResourceAsync(string circleId, string resourceId, string userId);
    Task<List<LearningCircleMemberDto>> GetCircleMembersAsync(string circleId);
    Task<bool> RemoveMemberAsync(string circleId, string memberId, string userId);
    Task<bool> LeaveCircleAsync(string circleId, string userId);


    // Tasks
    Task<List<LearningCircleTaskDto>> GetTasksAsync(string circleId, string userId);
    Task<LearningCircleTaskDto> CreateTaskAsync(string circleId, string userId, CreateLearningCircleTaskDto dto);
    Task<bool> UpdateTaskStatusAsync(string circleId, string taskId, string userId, string status);

    // Notes
    Task<LearningCircleNoteDto?> GetNoteAsync(string circleId, string userId);
    Task<LearningCircleNoteDto> UpdateNoteAsync(string circleId, string userId, string content);

    // AI Mentor
    Task<string> GetAiMentorResponseAsync(string circleId, string userId, string query);

    // Progress Board
    Task<List<CircleProgressDto>> GetProgressBoardAsync(string circleId);

    // Overview
    Task<CircleOverviewDto> GetCircleOverviewAsync(string circleId);
}
