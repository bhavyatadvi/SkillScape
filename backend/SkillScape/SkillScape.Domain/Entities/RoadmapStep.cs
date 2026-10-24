namespace SkillScape.Domain.Entities;

/// <summary>
/// Roadmap step - learning path for a career domain
/// </summary>
public class RoadmapStep
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string CareerDomainId { get; set; } = string.Empty;
    
    public string SkillId { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public int StepNumber { get; set; }
    
    public string? ResourceUrl { get; set; }
    
    public int EstimatedHours { get; set; } = 10;
    
    public bool IsPrerequisiteActive { get; set; } = false;
    
    public string? PrerequisiteStepId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public CareerDomain? CareerDomain { get; set; }
    public Skill? Skill { get; set; }
    public RoadmapStep? PrerequisiteStep { get; set; }
    public ICollection<RoadmapStep> DependentSteps { get; set; } = new List<RoadmapStep>();
    public ICollection<RoadmapTopic> Topics { get; set; } = new List<RoadmapTopic>();
    public ICollection<UserModuleProgress> UserProgressions { get; set; } = new List<UserModuleProgress>();
}
