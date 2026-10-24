namespace SkillScape.Domain.Entities;

/// <summary>
/// Quiz question for career discovery
/// </summary>
public class QuizQuestion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string CareerDomainId { get; set; } = string.Empty;
    
    public string Text { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public CareerDomain? CareerDomain { get; set; }
    public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
    public ICollection<QuizResponse> Responses { get; set; } = new List<QuizResponse>();
}

/// <summary>
/// Quiz option with domain weight for scoring
/// </summary>
public class QuizOption
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string QuizQuestionId { get; set; } = string.Empty;
    
    public string Text { get; set; } = string.Empty;
    
    public string DomainWeightJson { get; set; } = "{}"; // JSON: {"frontend": 3, "backend": 1, ...}
    
    public int DisplayOrder { get; set; }
    
    // Navigation properties
    public QuizQuestion? QuizQuestion { get; set; }
    public ICollection<QuizResponse> Responses { get; set; } = new List<QuizResponse>();
    public ICollection<QuizQuestionSignal> Signals { get; set; } = new List<QuizQuestionSignal>();
}

/// <summary>
/// User's quiz response
/// </summary>
public class QuizResponse
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; } = string.Empty;
    
    public string QuizQuestionId { get; set; } = string.Empty;
    
    public string QuizOptionId { get; set; } = string.Empty;
    
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
    public QuizQuestion? QuizQuestion { get; set; }
    public QuizOption? QuizOption { get; set; }
    public QuizResult? QuizResult { get; set; }
}

/// <summary>
/// Quiz result - final recommendation based on answers
/// </summary>
public class QuizResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; } = string.Empty;
    
    public string RecommendedDomainId { get; set; } = string.Empty;
    
    public string ScoresJson { get; set; } = "{}"; // JSON: {"frontend": 15, "backend": 10, ...}
    
    public string RecommendationReason { get; set; } = string.Empty;
    
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
    public CareerDomain? RecommendedDomain { get; set; }
}
