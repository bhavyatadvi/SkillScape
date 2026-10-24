using System;
using System.Collections.Generic;

namespace SkillScape.Domain.Entities;

public class RoadmapModuleQuiz
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ModuleId { get; set; } = string.Empty;
    public int PassScorePercentage { get; set; } = 70;

    public RoadmapModule? Module { get; set; }
    public ICollection<RoadmapQuizQuestion> Questions { get; set; } = new List<RoadmapQuizQuestion>();
}
