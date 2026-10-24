using System;

namespace SkillScape.Domain.Entities;

public class CareerProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = string.Empty;

    // Education
    public string EducationLevel { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string BroadField { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public int? GraduationYear { get; set; }
    public string AdditionalQualifications { get; set; } = string.Empty;

    // Current Career
    public string EducationStatus { get; set; } = string.Empty;
    public string CurrentEmploymentStatus { get; set; } = string.Empty;
    public string CurrentOccupation { get; set; } = string.Empty;
    public string CurrentRole { get; set; } = string.Empty;
    public string CurrentIndustry { get; set; } = string.Empty;
    public string OrganizationType { get; set; } = string.Empty;
    public double? ExperienceYears { get; set; }
    public string RelevantExperience { get; set; } = string.Empty;
    public string PreviousRoles { get; set; } = string.Empty;
    public string PreviousIndustries { get; set; } = string.Empty;

    // Skills (Stored as JSON)
    public string CurrentSkillsJson { get; set; } = "[]";
    public string CertificationsJson { get; set; } = "[]";

    // Preferences (Stored as JSON)
    public string CareerLikesJson { get; set; } = "[]";
    public string CareerDislikesJson { get; set; } = "[]";
    public string WorkPreferencesJson { get; set; } = "{}";
    public string MotivationsJson { get; set; } = "[]";
    public string ConstraintsJson { get; set; } = "[]";

    // Goals
    public string CareerChangePreference { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PreferredWorkMode { get; set; } = string.Empty;
    public string SalaryRange { get; set; } = string.Empty;
    public string CareerGoals { get; set; } = string.Empty;

    // Resume metadata
    public string ProfileSource { get; set; } = "manual";
    public string ResumeFileName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}
