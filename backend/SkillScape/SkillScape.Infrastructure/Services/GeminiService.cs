using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Service for integrating with Google Gemini API
/// </summary>
public class GeminiService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetResponseAsync(string systemPrompt, string userMessage, List<ConversationContext>? conversationHistory = null)
    {
        // Keep the model configurable, but always try currently supported
        // Gemini model IDs as fallbacks. The former 3.5/"flash-latest" aliases
        // are not available to many Google AI Studio projects; when they fail,
        // the caller silently falls back to generic saved guidance.
        var configuredModel = _configuration["Gemini:Model"]?.Trim();
        var modelsToTry = string.IsNullOrWhiteSpace(configuredModel)
            ? new[] { "gemini-2.5-flash", "gemini-2.5-flash-lite", "gemini-2.0-flash" }
            : new[] { configuredModel, "gemini-2.5-flash", "gemini-2.5-flash-lite", "gemini-2.0-flash" }
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        var lastError = "";

        foreach (var modelId in modelsToTry)
        {
            try
            {
                var apiKey = (Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                    ?? _configuration["Gemini:ApiKey"])?.Trim();

                if (string.IsNullOrEmpty(apiKey) || IsPlaceholderApiKey(apiKey))
                {
                    throw new InvalidOperationException("Gemini API key is missing or still set to a placeholder. Set Gemini:ApiKey or GEMINI_API_KEY to a valid Google AI Studio key.");
                }

                var messages = new List<object>();

                // Add conversation history
                if (conversationHistory != null && conversationHistory.Any())
                {
                    foreach (var context in conversationHistory.TakeLast(10))
                    {
                        messages.Add(new
                        {
                            role = context.Role == "assistant" ? "model" : "user",
                            parts = new[] { new { text = context.Content } }
                        });
                    }
                }

                var userContent = userMessage;

                messages.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = userContent } }
                });

                var requestBody = new
                {
                    systemInstruction = string.IsNullOrWhiteSpace(systemPrompt)
                        ? null
                        : new { parts = new[] { new { text = systemPrompt } } },
                    contents = messages,
                    generationConfig = new {
                        temperature = 0.35,
                        maxOutputTokens = 2048
                    }
                };

                // Use v1 endpoint as it's more stable for these models
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelId}:generateContent?key={apiKey}";
                var content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation($"Trying Gemini model: {modelId}");
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var jsonDoc = JsonDocument.Parse(responseContent);
                    return jsonDoc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? "Empty response";
                }

                lastError = $"Model {modelId} failed: {response.StatusCode} - {responseContent}";
                _logger.LogWarning(lastError);
            }
            catch (Exception ex)
            {
                lastError = $"Error with {modelId}: {ex.Message}";
                _logger.LogWarning(ex, lastError);
            }
        }

        throw new HttpRequestException($"All Gemini models failed. Last error: {lastError}");
    }

    private static bool IsPlaceholderApiKey(string apiKey) =>
        apiKey.Contains("your_gemini_api_key", StringComparison.OrdinalIgnoreCase)
        || apiKey.Contains("replace_with", StringComparison.OrdinalIgnoreCase)
        || apiKey.Contains("your_api_key", StringComparison.OrdinalIgnoreCase);

    public async Task<string> ExtractStructuredDataAsync(string text, string extractionSchema)
    {
        var prompt = $"Extract JSON for schema: {extractionSchema}. Text: {text}";
        return await GetResponseAsync("You are a JSON extractor.", prompt);
    }
}
