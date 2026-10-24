namespace SkillScape.Domain.Entities;

/// <summary>
/// Career Domain - Main category like Frontend, Backend, Data Science, etc.
/// </summary>
public class CareerDomain
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string? IconUrl { get; set; }
    
    public string Color { get; set; } = string.Empty;
    
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    public ICollection<RoadmapStep> RoadmapSteps { get; set; } = new List<RoadmapStep>();
    public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
}
