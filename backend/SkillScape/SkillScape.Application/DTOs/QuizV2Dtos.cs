using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkillScape.Application.DTOs;

// ─────────────────────────────────────────────────────────────────────────────
// QUIZ ENGINE V2 — Application Layer DTOs
// ─────────────────────────────────────────────────────────────────────────────

// ── Phase Info ───────────────────────────────────────────────────────────────

public class PhaseInfoDto
{
    public string PhaseKey { get; set; } = string.Empty;         // "After10th"
    public string Title { get; set; } = string.Empty;            // "After 10th Grade"
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string[] SubPhases { get; set; } = Array.Empty<string>(); // for After12th: pcm, pcb, etc.
    public bool RequiresBackgroundForm { get; set; } = true;
}

// ── Profile Collection DTOs (phase-specific input forms) ─────────────────────

/// <summary>
/// Flexible DTO for phase-specific profile collection.
/// The Fields dictionary carries phase-specific keys validated server-side.
/// </summary>
public class PhaseProfileInputDto
{
    public string PhaseKey { get; set; } = string.Empty;
    public string SubPhaseKey { get; set; } = string.Empty; // e.g. "pcm", "pcb", "commerce"

    /// <summary>
    /// Flexible key-value map of profile data collected from the form.
    /// Keys are phase-specific (e.g. "board", "mathPerformance", "degree", "currentRole").
    /// Values are strings (dropdowns, text inputs). Numeric values sent as strings.
    /// </summary>
    public Dictionary<string, string> Fields { get; set; } = new();
}

// ── Session Management ────────────────────────────────────────────────────────

public class AdaptiveQuizStartRequest
{
    [JsonPropertyName("assessmentType")]
    public string AssessmentType { get; set; } = string.Empty; // "AFTER_GRADUATION", "GOVERNMENT_JOBS", "CAREER_SWITCH", etc.

    [JsonPropertyName("phase")]
    public string PhaseKey { get; set; } = string.Empty;

    [JsonPropertyName("educationProfile")]
    public PhaseProfileInputDto Profile { get; set; } = new();
}

public class AdaptiveQuizSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string PhaseKey { get; set; } = string.Empty;
    public string SubPhaseKey { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTime StartedAt { get; set; }
}

// ── Adaptive Question Flow ────────────────────────────────────────────────────

public class AdaptiveQuizAnswerRequest
{
    [JsonIgnore]
    public string SessionId { get; set; } = string.Empty; // Filled from route

    [JsonPropertyName("questionId")]
    public string QuestionId { get; set; } = string.Empty;

    [JsonPropertyName("answerId")]
    public string AnswerId { get; set; } = string.Empty; // User spec says string

    [JsonIgnore]
    public int OptionIndex => int.TryParse(AnswerId, out int idx) ? idx : 0;
}

public class QuizOptionV2Dto
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    // signals are NOT sent to the client
}

public class QuizQuestionV2Dto
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public string Dimension { get; set; } = string.Empty;
    public string FieldId { get; set; } = string.Empty;
    public string DomainId { get; set; } = string.Empty;
    public List<QuizOptionV2Dto> Options { get; set; } = new();
}

public class AdaptiveQuizNextResponse
{
    [JsonPropertyName("nextQuestion")]
    public QuizQuestionV2Dto? Question { get; set; }

    [JsonPropertyName("progress")]
    public double Progress { get; set; }

    [JsonPropertyName("isComplete")]
    public bool IsComplete { get; set; } = false;
    
    [JsonPropertyName("questionNumber")]
    public int QuestionNumber { get; set; }
    
    [JsonPropertyName("estimatedRemaining")]
    public int EstimatedRemaining { get; set; }

    [JsonPropertyName("currentLeadingDomainNames")]
    public List<string> CurrentLeadingDomainNames { get; set; } = new();
}

// ── Result DTOs ───────────────────────────────────────────────────────────────

public class TaxonomyMatchDto
{
    public string NodeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HierarchyPath { get; set; } = string.Empty; // "Science / Engineering / Civil"
    public double MatchPercent { get; set; }
    public double Probability { get; set; }
    public bool IsRecommended { get; set; }
}

public class EvidenceItemDto
{
    public string QuestionText { get; set; } = string.Empty;
    public string SelectedOptionText { get; set; } = string.Empty;
    public List<SignalContributionDto> SignalsContributed { get; set; } = new();
}

public class SignalContributionDto
{
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public double Weight { get; set; }
}

public class EducationPathDto
{
    public string Stage { get; set; } = string.Empty;        // "After 12th"
    public string DegreeType { get; set; } = string.Empty;   // "B.Tech / B.E."
    public string Duration { get; set; } = string.Empty;     // "4 years"
    public List<string> EntranceExams { get; set; } = new();
    public List<string> TopInstitutionTypes { get; set; } = new();
    public List<string> TypicalSubjects { get; set; } = new();
    public string NextStep { get; set; } = string.Empty;     // After degree: M.Tech, Industry, etc.
}

public class QuizV2ResultDto
{
    [JsonPropertyName("resultId")]
    public string ResultId { get; set; } = string.Empty;

    [JsonPropertyName("recommendations")]
    public List<TaxonomyMatchDto> Recommendations { get; set; } = new();

    [JsonIgnore]
    public string PhaseKey { get; set; } = string.Empty;

    [JsonIgnore]
    public TaxonomyMatchDto PrimaryRecommendation { get; set; } = new();
    
    [JsonIgnore]
    public List<TaxonomyMatchDto> AlternativeRecommendations { get; set; } = new();
    
    [JsonIgnore]
    public double ConfidenceScore { get; set; }

    /// <summary>Traceable evidence — every recommendation point explained.</summary>
    public List<EvidenceItemDto> EvidenceItems { get; set; } = new();

    /// <summary>Generated from evidence, NOT a static template.</summary>
    public string WhyRecommended { get; set; } = string.Empty;

    // Phase 8 Exact Result Mapping
    public string BestMatchName { get; set; } = string.Empty;
    public string CompatibilityScoreText { get; set; } = string.Empty;
    public string ConfidenceText { get; set; } = string.Empty;
    public List<ScoredCareerDto> TargetCareerComparison { get; set; } = new();
    public List<string> StrongestCompetencies { get; set; } = new();
    public string SpecializationText { get; set; } = string.Empty;
    public List<string> DevelopmentAreas { get; set; } = new();
    public List<string> RecommendedNextSteps { get; set; } = new();
    public string CloseMatchWarning { get; set; } = string.Empty;
    [Obsolete("Legacy field. Use CanonicalForecast instead.")]
    public string ExpectedSalaryRange { get; set; } = string.Empty;
    [Obsolete("Legacy field. Use CanonicalEducation instead.")]
    public string ExpectedTimeToJobReady { get; set; } = string.Empty;

    public List<string> SkillGap { get; set; } = new();
    public EducationPathDto? EducationPath { get; set; }
    public ScoredNodeDto PrimaryDomain { get; set; } = new();
    public List<ScoredNodeDto> Fields { get; set; } = new();
    public List<ScoredNodeDto> Specializations { get; set; } = new();
    public List<ScoredNodeDto> CareerFamilies { get; set; } = new();
    
    public ConfidenceDto Confidence { get; set; } = new();
    
    public List<string> Evidence { get; set; } = new();
    
    // Original fields kept for backward compatibility during transition
    public List<string> RecommendedCourses { get; set; } = new();
    public List<string> TypicalCareerRoles { get; set; } = new();
    
    // Degree and Professional Qualification Extensions (After 12th)
    public ScoredNodeDto? RecommendedDegree { get; set; }
    public List<ScoredNodeDto> AlternativeDegrees { get; set; } = new();
    public ScoredNodeDto? ProfessionalQualification { get; set; }
    public ScoredNodeDto? RecommendedCareer { get; set; }
    public List<ScoredNodeDto> AlternativeCareers { get; set; } = new();
    
    // Eligibility Separation (Part 30)
    public EligibilityStatusDto Eligibility { get; set; } = new();
    
    public string ModelUsed { get; set; } = "ML.NET Ensemble";
    public string ResultStatus { get; set; } = "Success"; 
    public DateTime GeneratedAt { get; set; }
    
    public ExplanationDto Explanation { get; set; } = new();

    // Canonical Phase 1-6 Structure
    public CanonicalCareerDto? Career { get; set; }
    public CanonicalCompatibilityDto? Compatibility { get; set; }
    public CanonicalEducationDto? Education { get; set; }
    public CanonicalSalaryDto? Salary { get; set; }
    public CanonicalForecastDto? Forecast { get; set; }
}

public class CanonicalCareerDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CanonicalCompatibilityDto
{
    public double Score { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}

public class CanonicalEducationDto
{
    public string QualificationId { get; set; } = string.Empty;
    public string QualificationName { get; set; } = string.Empty;
    public string QualificationLevel { get; set; } = string.Empty;
    public int? Duration { get; set; }
    public string DurationUnit { get; set; } = "years";
    public string CurrentStatus { get; set; } = string.Empty;
    public int? CurrentYear { get; set; }
    public double? RemainingDuration { get; set; }
    public DateTime? ExpectedGraduationDate { get; set; }
    public int? ExpectedGraduationYear { get; set; }
    public string DurationStatus { get; set; } = string.Empty;
    public string GraduationStatus { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}

public class CanonicalSalaryDto
{
    public string Status { get; set; } = "Unavailable";
    public string SalaryRoleId { get; set; } = string.Empty;
    public string SalaryRoleName { get; set; } = string.Empty;
    public string Country { get; set; } = "India";
    public string Currency { get; set; } = "INR";
    public int? ExperienceMinYears { get; set; }
    public int? ExperienceMaxYears { get; set; }
    public double? MinSalary { get; set; }
    public double? MaxSalary { get; set; }
    public double? MedianSalary { get; set; }
    public string SalaryUnit { get; set; } = "LPA";
    public string Source { get; set; } = string.Empty;
    public string DataPeriod { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime? LastVerifiedAt { get; set; }
}

public class CanonicalForecastDto
{
    public string Status { get; set; } = "Unavailable";
    public string ScenarioType { get; set; } = "UndergraduateCareer";
    public string AnchorType { get; set; } = "Unknown";
    public DateTime? AnchorDate { get; set; }
    public int? AnchorYear { get; set; }
    public string Methodology { get; set; } = "Unavailable";
    public string Version { get; set; } = string.Empty;
    public List<CanonicalForecastYearDto> Years { get; set; } = new();
}

public class CanonicalForecastYearDto
{
    public int YearNumber { get; set; }
    public int? CalendarYear { get; set; }
    public int? ExperienceMinYears { get; set; }
    public int? ExperienceMaxYears { get; set; }
    public double? MinSalary { get; set; }
    public double? MaxSalary { get; set; }
    public double? MedianSalary { get; set; }
    public string Currency { get; set; } = "INR";
    public string SalaryUnit { get; set; } = "LPA";
    public string ValueType { get; set; } = "Unavailable";
    public string Source { get; set; } = string.Empty;
    public string DataPeriod { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public class ScoredNodeDto
{
    public string Name { get; set; } = string.Empty;
    public double Score { get; set; }
}

public class ConfidenceDto
{
    public string Level { get; set; } = string.Empty;
    public double Score { get; set; }
}

public class EligibilityStatusDto
{
    public bool IsEligible { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ExplanationDto
{
    public string WhyRecommended { get; set; } = string.Empty;
    public string ClosestAlternative { get; set; } = string.Empty;
    public string WhyWonOverAlternative { get; set; } = string.Empty;
    public List<string> ContributingAnswers { get; set; } = new();
}

public class HierarchicalNodeDto
{
    public string NodeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Probability { get; set; }
    public double MatchPercent { get; set; }
    public List<HierarchicalNodeDto> Children { get; set; } = new();
}

// ── Taxonomy Browse DTOs ──────────────────────────────────────────────────────

public class TaxonomyNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string ParentId { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public List<string> PhasesApplicable { get; set; } = new();
    public List<string> TypicalRoles { get; set; } = new();
    public List<string> RecommendedCourses { get; set; } = new();
    public List<string> EntranceExams { get; set; } = new();
    public Dictionary<string, double> SalaryRange { get; set; } = new();
    public double IndustryGrowthRate { get; set; }
    public double DemandIndex { get; set; }
    public List<TaxonomyNodeDto> Children { get; set; } = new();
}

// ── Profile Parser & Goal Validation DTOs ────────────────────────────────────

public class ParsedProfileDto
{
    public ParsedEducationDto Education { get; set; } = new();
    public ParsedCurrentStatusDto CurrentStatus { get; set; } = new();
    public ParsedPrimaryGoalDto PrimaryGoal { get; set; } = new();
    public Dictionary<string, string> ExtractedFields { get; set; } = new();
}

public class ParsedEducationDto
{
    public string Level { get; set; } = string.Empty; // e.g. "AfterGraduation", "Class10"
    public string Degree { get; set; } = string.Empty; // e.g. "B.Tech", "B.Com", "M.Sc"
    public string Specialization { get; set; } = string.Empty; // e.g. "Computer Engineering", "Finance", "Chemistry"
}

public class ParsedCurrentStatusDto
{
    public string Role { get; set; } = string.Empty; // e.g. "Graduate", "Accounts Assistant", "Unemployed"
    public string EmploymentStatus { get; set; } = string.Empty; // "employed", "unemployed", "student"
    public string Experience { get; set; } = string.Empty; // "internship", "fresher", "1-2 years"
    public List<string> Skills { get; set; } = new(); // e.g. ["PHP", "React", "Accounting"]
}

public class ParsedPrimaryGoalDto
{
    public string Goal { get; set; } = string.Empty; // e.g. "software development job"
    public string GoalCategory { get; set; } = string.Empty; // "employment", "higher_education", "switch", "upskilling"
    public string TargetDomain { get; set; } = string.Empty; // e.g. "software_engineering", "financial_analysis"
}

public class GoalValidationResultDto
{
    public string Goal { get; set; } = string.Empty;
    public string AlignmentRating { get; set; } = "STRONG_FIT"; // STRONG_FIT, MODERATE_FIT, WEAK_FIT, CURRENTLY_NOT_ALIGNED
    public double OverallScore { get; set; } // 0.0 to 1.0
    public bool IsGoodFit { get; set; } = true;
    public List<string> Reasons { get; set; } = new();
    public List<string> SkillGap { get; set; } = new();
    public List<AlternativeCareerOptionDto> RecommendedAlternatives { get; set; } = new();
}

public class AlternativeCareerOptionDto
{
    public string NodeId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public double MatchPercent { get; set; }
    public string WhyMatches { get; set; } = string.Empty;
    public int RequiredTransitionSkillCount { get; set; }
    public bool IsEasierTransition { get; set; }
}

// ── Dynamic Roadmap DTOs ──────────────────────────────────────────────────────

public class DynamicRoadmapDto
{
    public string TargetRole { get; set; } = string.Empty;
    public string StartingPoint { get; set; } = string.Empty;
    public List<RoadmapStepDto> Steps { get; set; } = new();
}

public class RoadmapStepDto
{
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // SkillGap, Education, Project, Certification, Application
    public string Description { get; set; } = string.Empty;
    public string EstimatedDuration { get; set; } = string.Empty;
}

public class ScoredCareerDto
{
    public string NodeId { get; set; } = string.Empty;
    public string CareerName { get; set; } = string.Empty;
    public int Score { get; set; }
    
    [Obsolete("Legacy field. Use CanonicalSalary instead.")]
    public string ExpectedSalaryRange { get; set; } = string.Empty;
    
    [Obsolete("Legacy field. Use CanonicalEducation instead.")]
    public string ExpectedTimeToJobReady { get; set; } = string.Empty;
    
    public CanonicalEducationDto? Education { get; set; }
    public CanonicalSalaryDto? Salary { get; set; }
    public CanonicalForecastDto? Forecast { get; set; }
}
