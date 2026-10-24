using System;
using System.Collections.Generic;

namespace SkillScape.Domain.Entities;

public class RoadmapModule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PhaseId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StepNumber { get; set; }
    public int EstimatedHours { get; set; } = 10;
    public bool IsActive { get; set; } = true;

    public RoadmapPhase? Phase { get; set; }
    public ICollection<RoadmapTopic> Topics { get; set; } = new List<RoadmapTopic>();
    public RoadmapModuleQuiz? Quiz { get; set; }
}
