using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

namespace SkillScape.Application.DTOs;

public class CareerPathDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Level { get; set; }
    public string? ParentCareerId { get; set; }
    public string StreamType { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public List<string> SkillsRequired { get; set; } = new();
    public string FutureScope { get; set; } = string.Empty;
    public Dictionary<string, double> SalaryData { get; set; } = new();
    public double IndustryGrowth { get; set; }
    public List<RoadmapMilestoneDto> Roadmap { get; set; } = new();
    public List<string> Certifications { get; set; } = new();
    public List<string> Colleges { get; set; } = new();
    public double DemandIndex { get; set; }
    public string Icon { get; set; } = "briefcase";
    public string Color { get; set; } = "blue";
    public string WhatYouWillStudy { get; set; } = string.Empty;
    public List<string> RecommendedSubjects { get; set; } = new();
    public List<string> TopCompanies { get; set; } = new();
    public List<string> RequiredDegrees { get; set; } = new();
    public string WorkEnvironment { get; set; } = "Office";
    public double RemoteWorkPossibility { get; set; }
    public double EntrepreneurshipPotential { get; set; }
    public double CareerFlexibility { get; set; }
    public List<string> LearningResources { get; set; } = new();
    public List<string> YoutubeResources { get; set; } = new();
    public string RoadmapTimeline { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = "Medium";
    public string PersonalityMatch { get; set; } = string.Empty;
    public string RelatedCareerIds { get; set; } = string.Empty;
    public double AutomationRisk { get; set; }
    public List<CareerPathDto> SubCareers { get; set; } = new();
    public double MatchScore { get; set; }
}

public class RoadmapMilestoneDto
{
    public string Phase { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public List<string> Topics { get; set; } = new();
    public string ProjectSuggestion { get; set; } = string.Empty;
}

public class HierarchicalQuizOptionDto
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Text { get; set; } = string.Empty;

    public Dictionary<string, double> Weights { get; set; } = new();

    [StringLength(500)]
    public string NextLevelTags { get; set; } = string.Empty;
}

public class HierarchicalQuizQuestionDto
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string QuestionText { get; set; } = string.Empty;

    [StringLength(1000)]
    public string TargetCareerTags { get; set; } = string.Empty;

    [Range(1, 20)]
    public int HierarchyLevel { get; set; }

    [Required]
    [StringLength(100)]
    public string StreamType { get; set; } = string.Empty;

    [StringLength(100)]
    public string AptitudeType { get; set; } = string.Empty;

    [Required]
    [MinLength(2, ErrorMessage = "At least two answer options are required.")]
    public List<HierarchicalQuizOptionDto> Options { get; set; } = new();
}

public class QuizAnswerInputDto
{
    [Required]
    [StringLength(100)]
    public string QuestionId { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int OptionIndex { get; set; }
}

public class QuizSubmissionRequest
{
    [Required]
    [StringLength(100)]
    public string SelectedStream { get; set; } = string.Empty; // After10th, etc.

    public Dictionary<string, string> BackgroundContext { get; set; } = new();

    [Required]
    [MinLength(1, ErrorMessage = "At least one quiz answer is required.")]
    public List<QuizAnswerInputDto> Answers { get; set; } = new();

    public Dictionary<string, double>? ClientWeights { get; set; }
}

public class UserCareerProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string SelectedHierarchy { get; set; } = string.Empty;
    public string EducationStage { get; set; } = string.Empty;
    public string PredictionLevel { get; set; } = string.Empty;
    public Dictionary<string, string> QuizAnswers { get; set; } = new();
    public CareerPathDto? RecommendedCareer { get; set; }
    public double ConfidenceScore { get; set; }
    public double PredictedSalary { get; set; }
    public List<RoadmapMilestoneDto> Roadmap { get; set; } = new();
    public List<string> SkillGap { get; set; } = new();
    public List<string> BookmarkedPaths { get; set; } = new();
    public List<CareerPathDto> TopMatches { get; set; } = new();
    public string WhyRecommended { get; set; } = string.Empty;
    public string WhyNotRecommended { get; set; } = string.Empty;
    public Dictionary<string, List<string>> StrengthsWeaknesses { get; set; } = new();
    public List<SalaryTrendPointDto> CareerGrowthGraph { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string ResultStatus { get; set; } = "Success"; // Can be Success, InsufficientConfidence
}

public class SalaryForecastRequest
{
    public string CareerPathId { get; set; } = string.Empty;
    public int TargetYear { get; set; }
    public int ExpectedGraduationYear { get; set; }
    public string Location { get; set; } = "Tier 1"; // Tier 1, Tier 2, Tier 3, Global
    public string SkillLevel { get; set; } = "Intermediate"; // Beginner, Intermediate, Expert
}

public class SalaryForecastDto
{
    public string CareerTitle { get; set; } = string.Empty;
    public int TargetYear { get; set; }
    public double PredictedSalary { get; set; }
    public double PredictedSalaryMin { get; set; }
    public double PredictedSalaryMax { get; set; }
    public double IndiaSalaryAvg { get; set; }
    public double GlobalSalaryAvg { get; set; }
    public List<SalaryTrendPointDto> TrendData { get; set; } = new();
}

public class SalaryTrendPointDto
{
    public int Year { get; set; }
    public double SalaryIndia { get; set; }
    public double SalaryGlobal { get; set; }
}

public class CareerCompareDto
{
    public CareerPathDto Career1 { get; set; } = null!;
    public CareerPathDto Career2 { get; set; } = null!;
}

public class GuidanceAnalyticsDto
{
    public List<AnalyticsItemDto> MostChosenCareers { get; set; } = new();
    public List<AnalyticsItemDto> PopularStreams { get; set; } = new();
    public List<AnalyticsTrendPointDto> UserInterestTrends { get; set; } = new();
    public double AveragePredictionConfidence { get; set; }
    public int TotalQuizCompletions { get; set; }
    public List<AnalyticsItemDto> SkillGapTrends { get; set; } = new();
}

public class AnalyticsItemDto
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class AnalyticsTrendPointDto
{
    public string Month { get; set; } = string.Empty;
    public int TechCount { get; set; }
    public int BusinessCount { get; set; }
    public int ArtsCount { get; set; }
}
