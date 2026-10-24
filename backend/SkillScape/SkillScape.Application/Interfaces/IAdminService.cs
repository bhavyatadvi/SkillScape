using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardStatsDto> GetDashboardAsync();
    Task<List<AdminUserDto>> GetUsersAsync(string? role, bool? isBlocked);
    Task BlockOrUnblockUserAsync(string adminId, BlockUserDto request);
    Task UpdateUserRoleAsync(string adminId, UpdateUserRoleDto request);
    Task<PlatformAnalyticsDto> GetAnalyticsAsync();
    Task<List<AdminUserProgressSnapshotDto>> GetProgressSnapshotsAsync();
    Task<List<AdminQuizQuestionCrudDto>> GetQuizQuestionsAsync();
    Task<AdminQuizQuestionCrudDto> CreateQuizQuestionAsync(string adminId, UpsertAdminQuizQuestionDto request);
    Task<AdminQuizQuestionCrudDto> UpdateQuizQuestionAsync(string adminId, string questionId, UpsertAdminQuizQuestionDto request);
    Task DeleteQuizQuestionAsync(string adminId, string questionId);
    
    // Hierarchical Questions
    Task<List<HierarchicalQuizQuestionDto>> GetHierarchicalQuestionsAsync();
    Task<HierarchicalQuizQuestionDto> CreateHierarchicalQuestionAsync(string adminId, HierarchicalQuizQuestionDto request);
    Task<HierarchicalQuizQuestionDto> UpdateHierarchicalQuestionAsync(string adminId, string questionId, HierarchicalQuizQuestionDto request);
    Task DeleteHierarchicalQuestionAsync(string adminId, string questionId);
    Task<List<AdminRoadmapModuleCrudDto>> GetRoadmapModulesAsync(string? domainId);
    Task<AdminRoadmapModuleCrudDto> CreateRoadmapModuleAsync(string adminId, UpsertAdminRoadmapModuleDto request);
    Task<AdminRoadmapModuleCrudDto> UpdateRoadmapModuleAsync(string adminId, string moduleId, UpsertAdminRoadmapModuleDto request);
    Task DeleteRoadmapModuleAsync(string adminId, string moduleId);
    Task<List<object>> GetPeerRoomsAsync();
    Task<object> GetPeerRoomDetailsAsync(string roomId);
    Task SetPeerRoomDisabledAsync(string roomId, bool isDisabled, string? reason);
}
