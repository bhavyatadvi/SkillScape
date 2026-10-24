namespace SkillScape.Domain.Entities;

/// <summary>
/// Defines the required skill levels for a specific target job role (e.g., Software Developer).
/// Seed data is loaded via DatabaseSeeder.
/// </summary>
public class RoleSkillProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Target job role name (e.g., "Software Developer")</summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>JSON-serialized list of SkillRequirement objects</summary>
    public string SkillsJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Individual skill requirement within a RoleSkillProfile (not stored separately, embedded as JSON).
/// </summary>
public class SkillRequirement
{
    public string SkillName { get; set; } = string.Empty;

    /// <summary>Technical | Aptitude | SoftSkill</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Required proficiency level 0-100</summary>
    public int RequiredLevel { get; set; }

    /// <summary>Weight of this skill in overall readiness score calculation (sum across role = 100)</summary>
    public double Weightage { get; set; }
}

/// <summary>
/// A single assessment question mapped to a specific skill category for placement evaluation.
/// Questions are role-tagged so the same bank can serve multiple roles.
/// </summary>
public class PlacementAssessmentQuestion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Comma-separated role names this question applies to (e.g., "Software Developer,Full Stack Developer")</summary>
    public string ApplicableRoles { get; set; } = string.Empty;

    public string QuestionText { get; set; } = string.Empty;

    /// <summary>JSON array of option strings</summary>
    public string OptionsJson { get; set; } = "[]";

    /// <summary>The exact text of the correct answer option</summary>
    public string CorrectAnswer { get; set; } = string.Empty;

    /// <summary>The skill this question evaluates (e.g., "Data Structures")</summary>
    public string SkillMapped { get; set; } = string.Empty;

    /// <summary>Technical | Aptitude | SoftSkill</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Easy | Medium | Hard</summary>
    public string Difficulty { get; set; } = "Medium";

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }
}

/// <summary>
/// Stores a complete placement assessment result for a user.
/// </summary>
public class PlacementAssessment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = string.Empty;

    public string TargetRole { get; set; } = string.Empty;

    /// <summary>JSON: { "Data Structures": 70, "Java": 80, ... }</summary>
    public string SkillScoresJson { get; set; } = "{}";

    /// <summary>ML model predicted readiness percentage (0-100)</summary>
    public double ReadinessScore { get; set; }

    /// <summary>JSON array of skill names where user meets or exceeds requirement</summary>
    public string StrongSkillsJson { get; set; } = "[]";

    /// <summary>JSON array of skill names where user is below requirement</summary>
    public string WeakSkillsJson { get; set; } = "[]";

    /// <summary>JSON array of recommendation strings</summary>
    public string RecommendationsJson { get; set; } = "[]";

    /// <summary>JSON array of roadmap week objects { week, tasks[] }</summary>
    public string RoadmapJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser? User { get; set; }
}
