using System;

namespace SkillScape.Domain.Entities;

/// <summary>
/// Tracks user progress on a specific module (RoadmapStep) within a skill
/// </summary>
public class UserModuleProgress
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; } = string.Empty;
    
    public string ModuleId { get; set; } = string.Empty; // FK to RoadmapStep
    
    public bool IsCompleted { get; set; } = false;
    
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
    public RoadmapStep? Module { get; set; }
}
