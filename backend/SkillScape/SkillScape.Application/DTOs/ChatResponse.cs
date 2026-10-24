namespace SkillScape.Application.DTOs;

/// <summary>
/// Represents the AI chatbot's response to a user query
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// The AI's response message
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Structured career suggestions from the knowledge base
    /// </summary>
    public List<CareerSuggestion> Careers { get; set; } = new();

    /// <summary>
    /// Required skills for the suggested path
    /// </summary>
    public List<SkillRequirement> Skills { get; set; } = new();

    /// <summary>
    /// Step-by-step learning roadmap
    /// </summary>
    public Roadmap? Roadmap { get; set; }

    /// <summary>
    /// References to external resources
    /// </summary>
    public List<Resource> Resources { get; set; } = new();

    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Confidence score of the response (0.0 to 1.0)
    /// </summary>
    public double ConfidenceScore { get; set; }
}

/// <summary>
/// Represents a career suggestion
/// </summary>
public class CareerSuggestion
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public double MatchScore { get; set; } // 0.0 to 1.0
    public List<string> KeyResponsibilities { get; set; } = new();
    public string? AverageSalary { get; set; }
    public string? JobOutlook { get; set; }
}

/// <summary>
/// Represents a skill requirement
/// </summary>
public class SkillRequirement
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string Level { get; set; } = "Intermediate"; // Beginner, Intermediate, Advanced, Expert
    public int EstimatedMonths { get; set; }
    public List<string> Resources { get; set; } = new();
}

/// <summary>
/// Represents a learning roadmap
/// </summary>
public class Roadmap
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int TotalDurationMonths { get; set; }
    public List<RoadmapPhase> Phases { get; set; } = new();
}

/// <summary>
/// Represents a phase in the learning roadmap
/// </summary>
public class RoadmapPhase
{
    public required string Name { get; set; }
    public int DurationMonths { get; set; }
    public List<string> Topics { get; set; } = new();
    public List<string> Projects { get; set; } = new();
    public List<string> Milestones { get; set; } = new();
}

/// <summary>
/// Represents an external learning resource
/// </summary>
public class Resource
{
    public required string Title { get; set; }
    public required string Url { get; set; }
    public string Type { get; set; } = "Course"; // Course, Article, Book, Video
    public string? Provider { get; set; }
    public int? DurationHours { get; set; }
    public double? Rating { get; set; }
}
