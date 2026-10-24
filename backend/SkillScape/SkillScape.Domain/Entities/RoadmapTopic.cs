using System;

namespace SkillScape.Domain.Entities;

public enum RoadmapTopicType
{
    Concept,
    HandsOnLab,
    Project,
    Reading
}

/// <summary>
/// Defines a specific learning topic or path within a broader Roadmap Module (Step)
/// </summary>
public class RoadmapTopic
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string? ModuleId { get; set; } // FK to RoadmapStep (Legacy)
    
    public string? RoadmapModuleId { get; set; } // FK to RoadmapModule (Comprehensive)
    
    public string Title { get; set; } = string.Empty;
    
    public string? ResourceUrl { get; set; }
    
    public int DisplayOrder { get; set; }
    
    public RoadmapTopicType Type { get; set; } = RoadmapTopicType.Concept;

    public bool IsOptional { get; set; } = false;

    // Navigation property
    public RoadmapStep? Module { get; set; }
    public RoadmapModule? RoadmapModule { get; set; }
}
