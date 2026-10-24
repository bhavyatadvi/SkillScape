using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkillScape.Application.DTOs;

/// <summary>
/// Represents a user's self-reported skill for the central Career Profile.
/// </summary>
public class CareerSkillDto
{
    public string Name { get; set; } = string.Empty;
    public string Proficiency { get; set; } = "Beginner";
}

public class WorkPreferencesDto
{
    public List<string> Environment { get; set; } = new();
    public List<string> Style { get; set; } = new();
}

public class CareerProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    // Education
    public string EducationLevel { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string BroadField { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    [Range(1960, 2100)]
    public int? GraduationYear { get; set; }
    public string AdditionalQualifications { get; set; } = string.Empty;

    // Current Career
    public string EducationStatus { get; set; } = string.Empty;
    public string CurrentEmploymentStatus { get; set; } = string.Empty;
    public string CurrentOccupation { get; set; } = string.Empty;
    public string CurrentRole { get; set; } = string.Empty;
    public string CurrentIndustry { get; set; } = string.Empty;
    public string OrganizationType { get; set; } = string.Empty;
    [Range(0, 60)]
    public double? ExperienceYears { get; set; }
    public string RelevantExperience { get; set; } = string.Empty;
    public string PreviousRoles { get; set; } = string.Empty;
    public string PreviousIndustries { get; set; } = string.Empty;

    // Skills
    public List<CareerSkillDto> CurrentSkills { get; set; } = new();
    public List<string> Certifications { get; set; } = new();

    // Preferences
    public List<string> CareerLikes { get; set; } = new();
    public List<string> CareerDislikes { get; set; } = new();
    public WorkPreferencesDto WorkPreferences { get; set; } = new();
    public List<string> Motivations { get; set; } = new();
    public List<string> Constraints { get; set; } = new();

    // Goals
    public string CareerChangePreference { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PreferredWorkMode { get; set; } = string.Empty;
    public string SalaryRange { get; set; } = string.Empty;
    public string CareerGoals { get; set; } = string.Empty;

    // Resume metadata
    public string ProfileSource { get; set; } = "manual";
    public string ResumeFileName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveCareerProfileRequest
{
    // Education
    public string EducationLevel { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string BroadField { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    [Range(1960, 2100)]
    public int? GraduationYear { get; set; }
    public string AdditionalQualifications { get; set; } = string.Empty;

    // Current Career
    public string EducationStatus { get; set; } = string.Empty;
    public string CurrentEmploymentStatus { get; set; } = string.Empty;
    public string CurrentOccupation { get; set; } = string.Empty;
    public string CurrentRole { get; set; } = string.Empty;
    public string CurrentIndustry { get; set; } = string.Empty;
    public string OrganizationType { get; set; } = string.Empty;
    [Range(0, 60)]
    public double? ExperienceYears { get; set; }
    public string RelevantExperience { get; set; } = string.Empty;
    public string PreviousRoles { get; set; } = string.Empty;
    public string PreviousIndustries { get; set; } = string.Empty;

    // Skills
    public List<CareerSkillDto> CurrentSkills { get; set; } = new();
    public List<string> Certifications { get; set; } = new();

    // Preferences
    public List<string> CareerLikes { get; set; } = new();
    public List<string> CareerDislikes { get; set; } = new();
    public WorkPreferencesDto WorkPreferences { get; set; } = new();
    public List<string> Motivations { get; set; } = new();
    public List<string> Constraints { get; set; } = new();

    // Goals
    public string CareerChangePreference { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PreferredWorkMode { get; set; } = string.Empty;
    public string SalaryRange { get; set; } = string.Empty;
    public string CareerGoals { get; set; } = string.Empty;

    // Resume metadata
    public string ProfileSource { get; set; } = "manual";
    public string ResumeFileName { get; set; } = string.Empty;
}

public class ResumeParseResultDto : SaveCareerProfileRequest
{
    public string ExtractedTextSummary { get; set; } = string.Empty;

    /// <summary>
    /// Comprehensive flat list of all skills extracted from every section of the resume,
    /// including skills inferred from project/job/internship descriptions.
    /// Populated during resume parsing; merged into CurrentSkills before returning to the client.
    /// </summary>
    public List<string> ParsedSkills { get; set; } = new();
}
