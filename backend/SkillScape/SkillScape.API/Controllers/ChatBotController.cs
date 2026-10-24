using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.API.Controllers;

/// <summary>
/// API endpoints for the AI career chatbot
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatBotController : ControllerBase
{
    private readonly IChatBotService _chatBotService;
    private readonly ILogger<ChatBotController> _logger;

    public ChatBotController(IChatBotService chatBotService, ILogger<ChatBotController> logger)
    {
        _chatBotService = chatBotService;
        _logger = logger;
    }

    /// <summary>
    /// Send a message to the AI chatbot
    /// </summary>
    /// <param name="chatRequest">The user's chat request</param>
    /// <returns>The AI's structured response</returns>
    [HttpPost("message")]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest chatRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(chatRequest.Message))
        {
            return BadRequest("Message cannot be empty");
        }

        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var response = await _chatBotService.ProcessMessageAsync(chatRequest, userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chatbot message");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Get conversation history for the current user
    /// </summary>
    /// <param name="limit">Maximum number of conversation turns to retrieve (default: 10)</param>
    /// <returns>List of conversation turns</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<ConversationContext>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 10)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var history = await _chatBotService.GetConversationHistoryAsync(userId, limit);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation history");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving conversation history" });
        }
    }

    /// <summary>
    /// Clear conversation history for the current user
    /// </summary>
    /// <returns>Success message</returns>
    [HttpDelete("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClearHistory()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            await _chatBotService.ClearConversationHistoryAsync(userId);
            return Ok(new { message = "Conversation history cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing conversation history");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while clearing conversation history" });
        }
    }

    /// <summary>
    /// Get quick career suggestions based on interests
    /// </summary>
    /// <param name="interests">Comma-separated list of interests</param>
    /// <returns>List of relevant careers</returns>
    [HttpGet("careers")]
    [ProducesResponseType(typeof(List<CareerSuggestion>), StatusCodes.Status200OK)]
    public IActionResult GetCareers([FromQuery] string? interests)
    {
        try
        {
            var interestList = string.IsNullOrEmpty(interests)
                ? new List<string> { "Technology" }
                : interests.Split(',').Select(i => i.Trim()).ToList();

            // Get knowledge base service through dependency injection
            // This is a simplified endpoint - in production, you'd inject IKnowledgeBaseService
            return Ok(new { message = "Use POST /chatbot/message with career guidance queries for AI-powered recommendations" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting careers");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving careers" });
        }
    }
}
