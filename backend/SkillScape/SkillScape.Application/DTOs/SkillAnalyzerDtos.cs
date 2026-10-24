using System;
using System.Collections.Generic;

namespace SkillScape.Application.DTOs;

// ─── Career Intelligence DTOs ──────────────────────────────────────────────

public class DomainDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> AlternateNames { get; set; } = new();
}

public class SubDomainDto
{
    public string Id { get; set; } = string.Empty;
    public string DomainId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class CareerSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DomainId { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string SubDomainId { get; set; } = string.Empty;
    public string SubDomainName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string SubDomain { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> AlternateNames { get; set; } = new();
    public List<string> InterestAreas { get; set; } = new();
    public List<string> CareerLevels { get; set; } = new();
}

public class CareerDetailDto : CareerSummaryDto
{
    public List<string> RequiredEducation { get; set; } = new();
    public CareerExperienceRequirementsDto ExperienceRequirements { get; set; } = new();
    public List<string> Certifications { get; set; } = new();
    public List<CareerSkillRequirementDto> SkillRequirements { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public string LastUpdated { get; set; } = string.Empty;
}

public class CareerExperienceRequirementsDto
{
    public int EntryLevel { get; set; }
    public int MidLevel { get; set; }
    public int Senior { get; set; }
}

public class CareerSkillRequirementDto
{
    public string SkillId { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public int RequiredLevel { get; set; }
    public string Importance { get; set; } = string.Empty; // Core | Supporting | Advanced
}

public class SkillDefinitionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> AlternateNames { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// ─── Preparation Profile ───────────────────────────────────────────────────

public class SkillAnalyzerRequest
{
    /// <summary>Preparation type e.g. "Job / Career", "Internship", etc.</summary>
    public string PreparationType { get; set; } = string.Empty;

    /// <summary>Domain id from domains.json e.g. "cs"</summary>
    public string DomainId { get; set; } = string.Empty;
    public string FieldId { get; set; } = string.Empty;

    /// <summary>SubDomain id from subdomains.json e.g. "cs_software_dev"</summary>
    public string SubDomainId { get; set; } = string.Empty;

    /// <summary>Career id from careers.json e.g. "career_fullstack_dev"</summary>
    public string CareerId { get; set; } = string.Empty;

    /// <summary>Custom career name when not found in catalog</summary>
    public string? CustomCareerName { get; set; }

    public string TargetLevel { get; set; } = string.Empty;
    public string TargetWorkType { get; set; } = string.Empty;

    /// <summary>"Student" | "Fresher" | "Beginner" | "Junior" | "Mid-Level" | "Experienced"</summary>
    public string CurrentLevel { get; set; } = string.Empty;

    /// <summary>e.g. "3–6 months"</summary>
    public string Timeline { get; set; } = string.Empty;

    /// <summary>Selected interest areas</summary>
    public List<string> InterestAreas { get; set; } = new();

    public string? Notes { get; set; }
}

// ─── Skill Analyzer Result ─────────────────────────────────────────────────

public class SkillAnalyzerResultDto
{
    // Target
    public string CareerId { get; set; } = string.Empty;
    public string CareerName { get; set; } = string.Empty;
    public string DomainId { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string SubDomainId { get; set; } = string.Empty;
    public string SubDomainName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string TargetCareerTitle { get; set; } = string.Empty;
    public string TargetLevel { get; set; } = string.Empty;
    public string TargetWorkType { get; set; } = string.Empty;
    public string PreparationType { get; set; } = string.Empty;
    public string CurrentLevel { get; set; } = string.Empty;
    public string Timeline { get; set; } = string.Empty;

    // Overall readiness
    public int OverallReadinessScore { get; set; }
    public string ReadinessLabel { get; set; } = string.Empty;
    public int SkillAlignmentScore { get; set; }
    public int ExperienceFitScore { get; set; }
    public int EducationFitScore { get; set; }
    public ResumeAtsScoreDto? ResumeAtsScore { get; set; }

    // Baseline Summary
    public BaselineSummaryDto BaselineSummary { get; set; } = new();

    // Fit breakdown
    public CareerFitDto CareerFit { get; set; } = new();

    // Skill breakdown
    public List<SkillStrengthDto> Strengths { get; set; } = new();
    public List<SkillGapItemDto> SkillGaps { get; set; } = new();
    public List<SkillGapItemDto> MatchedSkills { get; set; } = new();
    public List<SkillGapItemDto> MissingSkills { get; set; } = new();

    // AI narrative & strategic advice
    public AIRecommendationDto Recommendations { get; set; } = new();
    public string StrategicSummary { get; set; } = string.Empty;
    public List<ProjectIdeaDto> RecommendedProjects { get; set; } = new();
    public List<string> RecommendedCertifications { get; set; } = new();

    // Roadmap
    public List<LearningPhaseDto> LearningRoadmap { get; set; } = new();
    public List<LearningPhaseDto> SkillGapRoadmap { get; set; } = new();

    // Meta
    public bool IsLowDataProfile { get; set; }
    public string LowDataMessage { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class BaselineSummaryDto
{
    public bool HasProfile { get; set; }
    public int CompletenessScore { get; set; }
    public int ExtractedSkillsCount { get; set; }
    public int ProjectsCount { get; set; }
    public int YearsOfExperience { get; set; }
    public string Degree { get; set; } = string.Empty;
    public string BroadField { get; set; } = string.Empty;
}

/// <summary>
/// Transparent ATS-style match estimate. It is generated only from the structured
/// profile extracted from an uploaded resume; it is not a score from an external ATS.
/// </summary>
public class ResumeAtsScoreDto
{
    public int Score { get; set; }
    public string Label { get; set; } = string.Empty;
    public string ResumeFileName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> MatchedKeywords { get; set; } = new();
    public List<string> MissingKeywords { get; set; } = new();
}

public class ProjectIdeaDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public List<string> TargetSkills { get; set; } = new();
}

public class CareerFitDto
{
    public string EducationFit { get; set; } = string.Empty;
    public string EducationFitDetail { get; set; } = string.Empty;

    public string ExperienceFit { get; set; } = string.Empty;
    public string ExperienceFitDetail { get; set; } = string.Empty;

    public string SkillsFit { get; set; } = string.Empty;
}

public class SkillStrengthDto
{
    public string SkillId { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public string Importance { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
}

public class SkillGapItemDto
{
    public string SkillId { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int CurrentLevel { get; set; }
    public int UserProficiencyLevel { get; set; }
    public int RequiredLevel { get; set; }
    public int RequiredProficiencyLevel { get; set; }
    public int Gap { get; set; }
    public string Importance { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string PriorityRank { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
    public string EvidenceSource { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class AIRecommendationDto
{
    public string ReadinessExplanation { get; set; } = string.Empty;
    public List<string> WhatToLearnFirst { get; set; } = new();
    public List<SkillAdviceDto> SkillAdvice { get; set; } = new();
    public List<string> StudyTopics { get; set; } = new();
    public List<string> ProjectSuggestions { get; set; } = new();
    public List<string> CareerAdvice { get; set; } = new();
    public bool IsAIGenerated { get; set; } = true;
    public bool AIFailed { get; set; }
    public string AIProvider { get; set; } = string.Empty;
    public string AIFailureMessage { get; set; } = string.Empty;
}

public class SkillAdviceDto
{
    public string Skill { get; set; } = string.Empty;
    public string WhyItMatters { get; set; } = string.Empty;
    public string HowToLearn { get; set; } = string.Empty;
}

public class LearningPhaseDto
{
    public int Phase { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
    public List<string> TargetSkills { get; set; } = new();
    public List<string> Subtopics { get; set; } = new();
    public List<string> ActionItems { get; set; } = new();
    public string EstimatedTime { get; set; } = string.Empty;
    public string EstimatedDuration { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

// ─── Profile Status ────────────────────────────────────────────────────────

public class ProfileStatusDto
{
    public bool HasProfile { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ProfileCompletionPercent { get; set; }
    public int CompletenessScore { get; set; }
    public int ExtractedSkillsCount { get; set; }
    public int YearsOfExperience { get; set; }
    public string Degree { get; set; } = string.Empty;
    public string BroadField { get; set; } = string.Empty;
    public bool HasSkills { get; set; }
    public bool HasEducation { get; set; }
    public bool HasExperience { get; set; }
}
