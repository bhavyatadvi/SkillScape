using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkillScape.Domain.Entities;

/// <summary>
/// User entity - extends ASP.NET Identity User
/// </summary>
[BsonIgnoreExtraElements]
public class ApplicationUser
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Email { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
    
    public string? Bio { get; set; }
    
    public string? ProfileImageUrl { get; set; }
    
    public string Role { get; set; } = "Student";

    public string? PasswordHash { get; set; }

    public bool ProfileCompleted { get; set; } = false;
    
    public int Level { get; set; } = 1;
    
    public long TotalXP { get; set; } = 0;
    
    public int CurrentStreak { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;

    public bool IsBlocked { get; set; } = false;

    public string? BlockedReason { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiry { get; set; }

    // Peer Matching Engine Attributes
    public string? LearningDomain { get; set; }
    public string? LearningGoals { get; set; }
    public string? Availability { get; set; } = "Flexible";
    
    // Navigation properties
    [BsonIgnore]
    public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();

    [BsonIgnore]
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    [BsonIgnore]
    public ICollection<QuizResponse> QuizResponses { get; set; } = new List<QuizResponse>();
    [BsonIgnore]
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    [BsonIgnore]    public ICollection<UserCustomModuleProgress> UserCustomModuleProgresses { get; set; } = new List<UserCustomModuleProgress>();
}
