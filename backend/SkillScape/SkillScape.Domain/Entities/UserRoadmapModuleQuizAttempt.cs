using System;

namespace SkillScape.Domain.Entities;

public class UserRoadmapModuleQuizAttempt
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public double Score { get; set; }
    public int TotalQuestions { get; set; }
    public bool Passed { get; set; }
    public string AnsweredJson { get; set; } = "[]";
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}
