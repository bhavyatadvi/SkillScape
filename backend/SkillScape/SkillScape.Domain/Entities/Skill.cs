namespace SkillScape.Domain.Entities;

/// <summary>
/// Skill - represents a learnable skill within a career domain
/// </summary>
public class Skill
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string CareerDomainId { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string DifficultyLevel { get; set; } = "Beginner";
    
    public int XPReward { get; set; } = 10;
    
    public string? ResourceUrl { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public CareerDomain? CareerDomain { get; set; }
    public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    public ICollection<RoadmapStep> RoadmapSteps { get; set; } = new List<RoadmapStep>();
}
