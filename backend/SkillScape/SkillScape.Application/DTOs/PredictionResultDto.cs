using System.Collections.Generic;

namespace SkillScape.Application.DTOs;

public class PredictionResultDto
{
    public string RecommendedDomainId { get; set; } = string.Empty;
    public Dictionary<string, int> Scores { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
}
