using System;

namespace SkillScape.Domain.Entities;

/// <summary>
/// Tracks user progress on a custom career path module that is not persisted as a RoadmapStep.
/// </summary>
public class UserCustomModuleProgress
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = string.Empty;

    public string ModuleId { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedAt { get; set; }

    public int EstimatedHours { get; set; } = 0;

    public ApplicationUser? User { get; set; }
}
