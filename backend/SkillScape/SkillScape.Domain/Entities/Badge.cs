namespace SkillScape.Domain.Entities;

/// <summary>
/// Badge - achievement/reward
/// </summary>
public class Badge
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string IconUrl { get; set; } = string.Empty;
    
    public string Rarity { get; set; } = "Common"; // Common, Rare, Epic, Legendary
    
    public long XPRequired { get; set; } = 0; // XP threshold to unlock
    
    public int? SkillsCompletedRequired { get; set; }
    
    public int? DomainLevelRequired { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}

/// <summary>
/// User's earned badges
/// </summary>
public class UserBadge
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; } = string.Empty;
    
    public string BadgeId { get; set; } = string.Empty;
    
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
    public Badge? Badge { get; set; }
}
