using System.ComponentModel.DataAnnotations;

namespace SkillScape.Application.DTOs;

/// <summary>
/// Quiz question DTO
/// </summary>
public class QuizQuestionDto
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<QuizOptionDto> Options { get; set; } = new();
}

/// <summary>
/// Quiz option DTO
/// </summary>
public class QuizOptionDto
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Quiz response request (user answer)
/// </summary>
public class QuizResponseRequest
{
    [Required]
    [StringLength(100)]
    public string QuestionId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string OptionId { get; set; } = string.Empty;
}

/// <summary>
/// Submit quiz request (batch of answers)
/// </summary>
public class SubmitQuizRequest
{
    public const int MinimumRequiredQuestions = 7;

    [Required]
    [StringLength(100)]
    public string PhaseKey { get; set; } = string.Empty;

    [Required]
    [MinLength(MinimumRequiredQuestions, ErrorMessage = "At least 7 quiz responses are required.")]
    public List<QuizResponseRequest> Responses { get; set; } = new();
}

/// <summary>
/// Confirm quiz result request
/// </summary>
public class ConfirmQuizRequest
{
    public string RecommendedDomainId { get; set; } = string.Empty;
}

/// <summary>
/// Quiz result DTO
/// </summary>
public class QuizResultDto
{
    public string Id { get; set; } = string.Empty;
    public string RecommendedDomainId { get; set; } = string.Empty;
    public string RecommendedDomainName { get; set; } = string.Empty;
    public Dictionary<string, int> Scores { get; set; } = new();
    public string RecommendationReason { get; set; } = string.Empty;
    public List<string> RecommendedSkills { get; set; } = new();
    public string SalaryRange { get; set; } = string.Empty;
    public Dictionary<int, int> FiveYearTrend { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}
