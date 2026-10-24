using System.ComponentModel.DataAnnotations;

namespace SkillScape.Application.DTOs;

public class AdminDashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveStudents { get; set; }
}

public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public string? BlockedReason { get; set; }
    public bool ProfileCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BlockUserDto
{
    public string UserId { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public string? Reason { get; set; }
}

public class UpdateUserRoleDto
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class PlatformAnalyticsDto
{
    public string MostSelectedCareerPath { get; set; } = string.Empty;
    public string WeakestQuizCategory { get; set; } = string.Empty;
    public double AverageQuizScore { get; set; }
    public int DailySignups { get; set; }
    public int ActiveUsersLast30Days { get; set; }
    public int TotalQuizzesTaken { get; set; }
    public int TotalActiveDomains { get; set; }
    public int TotalQuestionsInBank { get; set; }
    public List<AdminAnalyticsSliceDto> CareerDomainSelections { get; set; } = new();
}

public class AdminAnalyticsSliceDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class AdminQuizOptionCrudDto
{
    public string? Id { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Text { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }
    public Dictionary<string, int> DomainWeights { get; set; } = new();
}

public class AdminQuizQuestionCrudDto
{
    public string Id { get; set; } = string.Empty;
    public string CareerDomainId { get; set; } = string.Empty;
    public string CareerDomainName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public List<AdminQuizOptionCrudDto> Options { get; set; } = new();
}

public class UpsertAdminQuizQuestionDto
{
    [Required]
    [StringLength(100)]
    public string CareerDomainId { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Text { get; set; } = string.Empty;

    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    [Required]
    [MinLength(2, ErrorMessage = "At least two non-empty options are required.")]
    public List<AdminQuizOptionCrudDto> Options { get; set; } = new();
}

public class AdminRoadmapTopicCrudDto
{
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ResourceUrl { get; set; }
    public int DisplayOrder { get; set; }
}

public class AdminRoadmapModuleCrudDto
{
    public string Id { get; set; } = string.Empty;
    public string CareerDomainId { get; set; } = string.Empty;
    public string CareerDomainName { get; set; } = string.Empty;
    public string SkillId { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StepNumber { get; set; }
    public int EstimatedHours { get; set; }
    public bool IsActive { get; set; }
    public List<AdminRoadmapTopicCrudDto> Topics { get; set; } = new();
}

public class UpsertAdminRoadmapModuleDto
{
    public string CareerDomainId { get; set; } = string.Empty;
    public string SkillId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StepNumber { get; set; }
    public int EstimatedHours { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public List<AdminRoadmapTopicCrudDto> Topics { get; set; } = new();
}

public class AdminResumeSummaryDto
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int CompletedSkills { get; set; }
    public int Certifications { get; set; }
    public long TotalXP { get; set; }
    public int Level { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AdminUserProgressSnapshotDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int QuizzesTaken { get; set; }
    public int CompletedSkills { get; set; }
    public double AverageDomainProgress { get; set; }
    public DateTime UpdatedAt { get; set; }
}
