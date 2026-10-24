using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Service for integrating with OpenAI API
/// </summary>
public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAIService> _logger;

    private const string OpenAiApiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAIService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Send a message to OpenAI and get a response
    /// </summary>
    public async Task<string> GetResponseAsync(string systemPrompt, string userMessage, List<ConversationContext>? conversationHistory = null)
    {
        try
        {
            // Try to get API key from environment variable first, then from configuration
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
                ?? _configuration["OpenAI:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("OpenAI API key is not configured. Set OPENAI_API_KEY environment variable or configure in appsettings.json");
                throw new InvalidOperationException("OpenAI API key is not configured");
            }

            var messages = new List<Dictionary<string, string>>();

            // Add system prompt
            messages.Add(new Dictionary<string, string>
            {
                { "role", "system" },
                { "content", systemPrompt }
            });

            // Add conversation history
            if (conversationHistory != null && conversationHistory.Any())
            {
                foreach (var context in conversationHistory.TakeLast(10)) // Limit to last 10 messages for context
                {
                    messages.Add(new Dictionary<string, string>
                    {
                        { "role", context.Role },
                        { "content", context.Content }
                    });
                }
            }

            // Add current user message
            messages.Add(new Dictionary<string, string>
            {
                { "role", "user" },
                { "content", userMessage }
            });

            var request = new
            {
                model = "gpt-4o-mini",
                messages = messages,
                temperature = 0.7,
                max_tokens = 2000,
                top_p = 0.9
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(OpenAiApiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonDocument.Parse(responseContent);
            var messageContent = jsonResponse.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return messageContent ?? "Unable to generate response";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            throw;
        }
    }

    /// <summary>
    /// Extract structured data from text using OpenAI
    /// </summary>
    public async Task<string> ExtractStructuredDataAsync(string text, string extractionSchema)
    {
        try
        {
            var systemPrompt = $@"You are a JSON extraction expert. Extract structured data from the provided text according to the following schema:
{extractionSchema}

Return ONLY valid JSON that matches the schema. Do not include any explanations or markdown formatting.";

            return await GetResponseAsync(systemPrompt, text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting structured data");
            throw;
        }
    }
}
