namespace SkillScape.Domain.Entities;

/// <summary>
/// User's skill progress
/// </summary>
public class UserSkill
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; } = string.Empty;
    
    public string SkillId { get; set; } = string.Empty;
    
    public bool IsCompleted { get; set; } = false;
    
    public bool IsCleared { get; set; } = false;
    
    public DateTime? CompletedAt { get; set; }
    
    public int ProgressPercentage { get; set; } = 0;
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
    public Skill? Skill { get; set; }
}
