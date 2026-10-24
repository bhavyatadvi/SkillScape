using System;

namespace SkillScape.Domain.Entities;

/// <summary>
/// Represents a quiz question used by the adaptive hierarchical career quiz system.
/// </summary>
public class HierarchicalQuizQuestion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string QuestionText { get; set; } = string.Empty;
    
    // JSON: [{"text": "Option Text", "weights": {"tech": 3, "science": 1}, "nextLevelTags": "pcm"}]
    public string OptionsJson { get; set; } = "[]"; 
    
    public string TargetCareerTags { get; set; } = string.Empty; // Tags this question filters/addresses
    
    public int HierarchyLevel { get; set; } // level of hierarchy (1 to 5)
    
    public string StreamType { get; set; } = string.Empty; // After10th, After12th, AfterGraduation, CareerSwitch, SkillEnhancement
    
    public string AptitudeType { get; set; } = string.Empty; // Interest, Aptitude, SubjectLiking, WorkStyle, RiskTaking
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
