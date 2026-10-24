namespace SkillScape.Application.DTOs;

/// <summary>
/// User progress DTO
/// </summary>
public class UserProgressDto
{
    public string Id { get; set; } = string.Empty;
    public string DomainId { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public long XPInDomain { get; set; }
    public int SkillsCompleted { get; set; }
    public int TotalSkills { get; set; }
    public double ProgressPercentage { get; set; }
}

/// <summary>
/// User dashboard stats
/// </summary>
public class UserStatsDto
{
    public int Level { get; set; }
    public long TotalXP { get; set; }
    public long XPToNextLevel { get; set; }
    public int CurrentStreak { get; set; }
    public int CompletedSkills { get; set; }
    public long XpEarnedToday { get; set; }
    public int SkillsCompletedThisWeek { get; set; }
    public List<UserProgressDto> DomainProgressions { get; set; } = new();
}

/// <summary>
/// Badge DTO
/// </summary>
public class BadgeDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public bool Earned { get; set; }
    public DateTime? EarnedAt { get; set; }
}

/// <summary>
/// Leaderboard User DTO
/// </summary>
public class LeaderboardUserDto
{
    public int Rank { get; set; }
    public string Name { get; set; } = string.Empty;
    public long Xp { get; set; }
    public int Level { get; set; }
    public bool IsUser { get; set; }
    public string Avatar { get; set; } = string.Empty;
}
