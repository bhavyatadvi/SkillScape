using System;

namespace SkillScape.Domain.Entities;

public class UserSavedRoadmap
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string CareerKey { get; set; } = string.Empty;
    public string CareerTitle { get; set; } = string.Empty;
    public string BlueprintId { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastOpenedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
    public RoadmapBlueprint? Blueprint { get; set; }
}
