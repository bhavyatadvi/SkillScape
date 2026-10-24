namespace SkillScape.Application.DTOs;

/// <summary>
/// Career domain DTO
/// </summary>
public class CareerDomainDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string Color { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Career domain with skills
/// </summary>
public class CareerDomainDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string Color { get; set; } = string.Empty;
    public List<SkillDto> Skills { get; set; } = new();
}

/// <summary>
/// Skill DTO
/// </summary>
public class SkillDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = string.Empty;
    public int XPReward { get; set; }
    public string? ResourceUrl { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// User skill DTO
/// </summary>
public class UserSkillDto
{
    public string Id { get; set; } = string.Empty;
    public string SkillId { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ProgressPercentage { get; set; }
    public DateTime StartedAt { get; set; }
}

/// <summary>
/// Complete skill request
/// </summary>
public class CompleteSkillRequest
{
    public string SkillId { get; set; } = string.Empty;
}
