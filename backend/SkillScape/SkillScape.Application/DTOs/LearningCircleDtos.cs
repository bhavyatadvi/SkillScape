using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkillScape.Application.DTOs;

public class LearningCircleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CareerDomain { get; set; } = string.Empty;
    public string CircleType { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public bool IsPublic { get; set; }
    public string CreatorId { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public bool IsMeetingActive { get; set; }
    public DateTime? ScheduledMeetingTime { get; set; }
    public bool IsMember { get; set; }
    public bool IsCreator { get; set; }
    public bool IsDisabledByAdmin { get; set; }
    public string? AdminDisabledReason { get; set; }
}

public class SetPeerRoomModerationDto
{
    public bool IsDisabled { get; set; }
    [MaxLength(1000)]
    public string? Reason { get; set; }
}

public class CircleRecommendationDto
{
    public LearningCircleDto Circle { get; set; } = new LearningCircleDto();
    public double CompatibilityScore { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class LearningCircleMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string CircleId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class LearningCircleJoinRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string CircleId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}

public class DiscussionDto
{
    public string Id { get; set; } = string.Empty;
    public string CircleId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateDiscussionDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

public class ResourceDto
{
    public string Id { get; set; } = string.Empty;
    public string CircleId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string AddedByUserId { get; set; } = string.Empty;
    public string AddedByUserName { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}

public class CreateResourceDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(5000)]
    public string Url { get; set; } = string.Empty;
}

public class LearningCircleTaskDto
{
    public string Id { get; set; } = string.Empty;
    public string CircleId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string CreatedByUserName { get; set; } = string.Empty;
    public string AssignedToUserId { get; set; } = string.Empty;
    public string AssignedToUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateLearningCircleTaskDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime? DueDate { get; set; }
    
    public string AssignedToUserId { get; set; } = string.Empty;
}

public class UpdateLearningCircleTaskDto
{
    [Required]
    [RegularExpression("Pending|Completed", ErrorMessage = "Status must be 'Pending' or 'Completed'")]
    public string Status { get; set; } = string.Empty;
}

public class LearningCircleNoteDto
{
    public string Id { get; set; } = string.Empty;
    public string CircleId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string UpdatedByUserId { get; set; } = string.Empty;
    public string UpdatedByUserName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class UpdateLearningCircleNoteDto
{
    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}

public class AiMentorRequestDto
{
    [Required]
    [MaxLength(1000)]
    public string Query { get; set; } = string.Empty;
}

public class CircleProgressDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TasksCompleted { get; set; }
    public int ResourcesShared { get; set; }
    public int DiscussionsStarted { get; set; }
    public int Score { get; set; }
}

public class CircleOverviewDto
{
    public int DiscussionsCount { get; set; }
    public int ResourcesCount { get; set; }
    public int TasksCompletedCount { get; set; }
    public int MembersCount { get; set; }
    public double AvgPlacementReadiness { get; set; } // e.g. 78.5
    public List<string> PopularSkills { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty; // "discussion", "resource", "task"
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
