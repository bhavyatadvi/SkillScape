using System;
using System.Collections.Generic;

namespace SkillScape.Domain.Entities;

public class RoadmapBlueprint
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CareerKey { get; set; } = string.Empty;
    public string CareerTitle { get; set; } = string.Empty;
    public string GeneratedBy { get; set; } = "ai"; // "ai" | "deterministic" | "ai-freeform"
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastWebCheckedAt { get; set; } = DateTime.UtcNow;
    public string SourceModel { get; set; } = "groq/llama-3.3-70b";
    public string? RawPromptHash { get; set; }

    public ICollection<RoadmapPhase> Phases { get; set; } = new List<RoadmapPhase>();
}
