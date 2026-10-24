using System;

namespace SkillScape.Domain.Entities;

/// <summary>
/// Records a roadmap that a user intentionally removed so automatic
/// recommendation logic does not recreate it on the next page load.
/// </summary>
public class UserRoadmapDismissal
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string CareerKey { get; set; } = string.Empty;
    public DateTime DismissedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}