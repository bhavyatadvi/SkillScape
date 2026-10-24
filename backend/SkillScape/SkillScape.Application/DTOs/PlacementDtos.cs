namespace SkillScape.Application.DTOs;

// =============================================
// Placement Readiness & Skill Gap Analysis DTOs
// =============================================

/// <summary>
/// Available job role for placement assessment
/// </summary>
public class PlacementRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<string> KeySkills { get; set; } = new();
}

/// <summary>
/// A single placement assessment question returned to the client.
/// CorrectAnswer is NOT included — only sent server-side for scoring.
/// </summary>
public class PlacementQuestionDto
{
    public string Id { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string SkillMapped { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Medium";
    public int DisplayOrder { get; set; }
}

/// <summary>
/// A user's answer to a single placement question
/// </summary>
public class PlacementAnswerRequest
{
    public string QuestionId { get; set; } = string.Empty;
    public string SelectedAnswer { get; set; } = string.Empty;
}

/// <summary>
/// Payload sent when submitting a completed placement assessment
/// </summary>
public class SubmitPlacementRequest
{
    public string TargetRole { get; set; } = string.Empty;
    public List<PlacementAnswerRequest> Answers { get; set; } = new();
}

/// <summary>
/// Skill-wise score breakdown
/// </summary>
public class SkillScoreDto
{
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double UserScore { get; set; }
    public double RequiredLevel { get; set; }
    public double GapPercentage { get; set; }
    public bool IsMet { get; set; }
    public double Weightage { get; set; }
}

/// <summary>
/// A week in the improvement roadmap
/// </summary>
public class RoadmapWeekDto
{
    public string Week { get; set; } = string.Empty;
    public string Focus { get; set; } = string.Empty;
    public List<string> Tasks { get; set; } = new();
}

/// <summary>
/// Full placement assessment result returned to the client
/// </summary>
public class PlacementResultDto
{
    public string AssessmentId { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;

    /// <summary>ML-predicted readiness score 0-100</summary>
    public double ReadinessScore { get; set; }

    /// <summary>Label based on score: "Placement Ready", "Almost Ready", "Needs Improvement"</summary>
    public string ReadinessLabel { get; set; } = string.Empty;

    public List<SkillScoreDto> SkillScores { get; set; } = new();
    public List<string> StrongSkills { get; set; } = new();
    public List<string> WeakSkills { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public List<RoadmapWeekDto> Roadmap { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Summary of a past assessment for history listing
/// </summary>
public class PlacementHistoryDto
{
    public string AssessmentId { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
    public double ReadinessScore { get; set; }
    public string ReadinessLabel { get; set; } = string.Empty;
    public int StrongSkillCount { get; set; }
    public int WeakSkillCount { get; set; }
    public DateTime CompletedAt { get; set; }
}
