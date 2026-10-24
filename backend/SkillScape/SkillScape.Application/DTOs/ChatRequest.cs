namespace SkillScape.Application.DTOs;

/// <summary>
/// Represents a user message sent to the AI chatbot
/// </summary>
public class ChatRequest
{
    /// <summary>
    /// The user's message or query
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// User's current skill level (Beginner, Intermediate, Advanced)
    /// </summary>
    public string? SkillLevel { get; set; }

    /// <summary>
    /// User's areas of interest (e.g., "Web Development", "Data Science")
    /// </summary>
    public List<string> Interests { get; set; } = new();

    /// <summary>
    /// Previous conversation context for personalization
    /// </summary>
    public List<ConversationContext> ConversationHistory { get; set; } = new();
}

/// <summary>
/// Represents the context of a conversation turn
/// </summary>
public class ConversationContext
{
    public string Role { get; set; } = "user"; // "user" or "assistant"
    public required string Content { get; set; }
}
