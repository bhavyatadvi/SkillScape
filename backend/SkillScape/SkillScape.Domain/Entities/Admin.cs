namespace SkillScape.Domain.Entities;

public class AdminAuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AdminId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string? TargetEntityId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApplicationUser? Admin { get; set; }
}
