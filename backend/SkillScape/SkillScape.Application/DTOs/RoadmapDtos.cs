using System;
using System.Collections.Generic;

namespace SkillScape.Application.DTOs;

public class RoadmapDto
{
    public string DomainId { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public double ProgressPercentage { get; set; } // Overall domain progress
    public DateTime CurriculumCreatedAt { get; set; }
    public DateTime LastWebCheckedAt { get; set; }
    public string CurriculumSource { get; set; } = string.Empty;
    public bool HasAdaptiveUpdates { get; set; }
    public List<SkillRoadmapDto> Skills { get; set; } = new List<SkillRoadmapDto>();
    public List<RoadmapPhaseDto> Phases { get; set; } = new List<RoadmapPhaseDto>();
}

public class RoadmapPhaseDto
{
    public string PhaseId { get; set; } = string.Empty;
    public int PhaseNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Level { get; set; } = "Beginner"; // Beginner | Intermediate | Advanced | ProLevel
    public string Summary { get; set; } = string.Empty;
    public string EstimatedDuration { get; set; } = string.Empty;
    public List<RoadmapModuleDto> Modules { get; set; } = new List<RoadmapModuleDto>();
}

public class RoadmapOptionDto
{
    public string DomainId { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public double ProgressPercentage { get; set; }
    public bool IsRecommended { get; set; }
}

public class SkillRoadmapDto
{
    public string SkillId { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public string SkillDescription { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public List<RoadmapModuleDto> Modules { get; set; } = new List<RoadmapModuleDto>();
}

public class RoadmapModuleDto
{
    public string ModuleId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StepNumber { get; set; }
    public int EstimatedHours { get; set; }
    public bool IsCompleted { get; set; }
    public List<RoadmapTopicDto> Topics { get; set; } = new List<RoadmapTopicDto>();
    public RoadmapModuleQuizDto? Quiz { get; set; }
}

public class RoadmapTopicDto
{
    public string TopicId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ResourceUrl { get; set; }
    public string? Type { get; set; } = "Concept";
    public bool IsOptional { get; set; } = false;
}

public class RoadmapModuleQuizDto
{
    public string QuizId { get; set; } = string.Empty;
    public string ModuleId { get; set; } = string.Empty;
    public int PassScorePercentage { get; set; } = 70;
    public List<RoadmapQuizQuestionDto> Questions { get; set; } = new List<RoadmapQuizQuestionDto>();
}

public class RoadmapQuizQuestionDto
{
    public string QuestionId { get; set; } = string.Empty;
    public string QuizId { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new List<string>();
    public string DifficultyTag { get; set; } = "Medium";
    // Correct answer is hidden during question fetch, provided on submit
    public int? CorrectOptionIndex { get; set; }
    public string? Explanation { get; set; }
}

public class QuizSubmissionDto
{
    public List<int> Answers { get; set; } = new List<int>(); // Selected option index for each question in order
}

public class RoadmapQuizResultDto
{
    public string ModuleId { get; set; } = string.Empty;
    public double ScorePercentage { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public bool Passed { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<RoadmapQuizQuestionDto> EvaluatedQuestions { get; set; } = new List<RoadmapQuizQuestionDto>();
}

public class UserSavedRoadmapDto
{
    public string Id { get; set; } = string.Empty;
    public string CareerKey { get; set; } = string.Empty;
    public string CareerTitle { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastOpenedAt { get; set; }
}

public class AskAIRoadmapRequestDto
{
    public string GoalText { get; set; } = string.Empty;
    public string? SituationText { get; set; }
}
