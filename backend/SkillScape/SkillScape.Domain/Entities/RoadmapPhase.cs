using System;
using System.Collections.Generic;

namespace SkillScape.Domain.Entities;

public class RoadmapPhase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BlueprintId { get; set; } = string.Empty;
    public int PhaseNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Level { get; set; } = "Beginner"; // Beginner | Intermediate | Advanced | ProLevel
    public string Summary { get; set; } = string.Empty;
    public string EstimatedDuration { get; set; } = "2-4 Weeks";

    public RoadmapBlueprint? Blueprint { get; set; }
    public ICollection<RoadmapModule> Modules { get; set; } = new List<RoadmapModule>();
}
