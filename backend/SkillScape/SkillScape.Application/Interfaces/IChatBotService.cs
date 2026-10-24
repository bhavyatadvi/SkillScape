using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

/// <summary>
/// Interface for the AI chatbot service
/// </summary>
public interface IChatBotService
{
    /// <summary>
    /// Process a user message and generate an AI response
    /// </summary>
    /// <param name="chatRequest">The user's chat request</param>
    /// <param name="userId">The ID of the user sending the message</param>
    /// <returns>The AI's structured response</returns>
    Task<ChatResponse> ProcessMessageAsync(ChatRequest chatRequest, string userId);

    /// <summary>
    /// Get conversation history for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="limit">Maximum number of conversation turns to retrieve</param>
    /// <returns>List of conversation turns</returns>
    Task<List<ConversationContext>> GetConversationHistoryAsync(string userId, int limit = 10);

    /// <summary>
    /// Clear conversation history for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    Task ClearConversationHistoryAsync(string userId);
}

/// <summary>
/// Interface for OpenAI integration
/// </summary>
public interface IOpenAIService
{
    /// <summary>
    /// Send a message to OpenAI and get a structured response
    /// </summary>
    /// <param name="systemPrompt">The system context/role</param>
    /// <param name="userMessage">The user's message</param>
    /// <param name="conversationHistory">Previous conversation context</param>
    /// <returns>The AI's text response</returns>
    Task<string> GetResponseAsync(string systemPrompt, string userMessage, List<ConversationContext>? conversationHistory = null);

    /// <summary>
    /// Extract structured information from text
    /// </summary>
    /// <param name="text">The text to parse</param>
    /// <param name="extractionSchema">JSON schema for extraction</param>
    /// <returns>Extracted structured data</returns>
    Task<string> ExtractStructuredDataAsync(string text, string extractionSchema);
}

/// <summary>
/// Interface for knowledge base management
/// </summary>
public interface IKnowledgeBaseService
{
    /// <summary>
    /// Get relevant careers based on user interests
    /// </summary>
    /// <param name="interests">User interests</param>
    /// <param name="limit">Maximum number of results</param>
    /// <returns>List of relevant careers</returns>
    Task<List<CareerSuggestion>> GetRelevantCareersAsync(List<string> interests, int limit = 5);

    /// <summary>
    /// Get skills required for a specific career
    /// </summary>
    /// <param name="careerPath">The career title</param>
    /// <param name="skillLevel">The user's current skill level</param>
    /// <returns>List of required skills</returns>
    Task<List<SkillRequirement>> GetCareerSkillsAsync(string careerPath, string skillLevel = "Beginner");

    /// <summary>
    /// Get a learning roadmap for a career path
    /// </summary>
    /// <param name="careerPath">The career title</param>
    /// <param name="skillLevel">The user's current skill level</param>
    /// <returns>The roadmap</returns>
    Task<Roadmap?> GetCareerRoadmapAsync(string careerPath, string skillLevel = "Beginner");

    /// <summary>
    /// Search the knowledge base
    /// </summary>
    /// <param name="query">Search query</param>
    /// <returns>Relevant results</returns>
    Task<List<string>> SearchAsync(string query);
}
