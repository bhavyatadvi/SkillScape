using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Service for integrating with Groq API - Primary AI Provider
/// Groq provides faster inference than traditional APIs
/// </summary>
public class GroqService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GroqService> _logger;

    private const string GroqApiUrl = "https://api.groq.com/openai/v1/chat/completions";

    public GroqService(HttpClient httpClient, IConfiguration configuration, ILogger<GroqService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Send a message to Groq API and get a response
    /// Groq specializes in fast inference with multiple model options
    /// </summary>
    public async Task<string> GetResponseAsync(string systemPrompt, string userMessage, List<ConversationContext>? conversationHistory = null)
        => await GetResponseInternalAsync(systemPrompt, userMessage, conversationHistory, useSkillAnalyzerKey: false);

    /// <summary>
    /// Generates a Skill Analyzer action plan with its dedicated Groq project/key.
    /// This is intentionally separate from the shared chat provider key.
    /// </summary>
    public async Task<string> GetSkillAnalyzerResponseAsync(string systemPrompt, string userMessage)
        => await GetResponseInternalAsync(systemPrompt, userMessage, conversationHistory: null, useSkillAnalyzerKey: true);

    private async Task<string> GetResponseInternalAsync(
        string systemPrompt,
        string userMessage,
        List<ConversationContext>? conversationHistory,
        bool useSkillAnalyzerKey)
    {
        try
        {
            // The Skill Analyzer has an intentionally independent Groq key and
            // model, so its usage/billing and permissions are isolated from the
            // shared chatbot provider.
            var apiKey = useSkillAnalyzerKey
                ? Environment.GetEnvironmentVariable("GROQ_SKILL_ANALYZER_API_KEY") ?? _configuration["GroqSkillAnalyzer:ApiKey"]
                : Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? _configuration["Groq:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                var keyName = useSkillAnalyzerKey ? "GROQ_SKILL_ANALYZER_API_KEY" : "GROQ_API_KEY";
                _logger.LogWarning("Groq API key {KeyName} is not configured.", keyName);
                throw new InvalidOperationException($"{keyName} is not configured");
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
                foreach (var context in conversationHistory.TakeLast(10)) // Limit to last 10 messages
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

            // The chatbot renders ordinary provider output as user-facing text.
            // Only the assessment/extraction workflows require a JSON response.
            // Requesting json_object for every chat caused regular answers to be
            // displayed as raw JSON (and some models reject it altogether).
            object? responseFormat = null;

            if (systemPrompt.Contains("Question Generator") || userMessage.Contains("TARGET CAREERS TO DISCRIMINATE:"))
            {
                responseFormat = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = "career_question",
                        strict = true,
                        schema = new
                        {
                            type = "object",
                            additionalProperties = false,
                            properties = new Dictionary<string, object>
                            {
                                { "questionId", new { type = "string" } },
                                { "conceptId", new { type = "string" } },
                                { "competencyId", new { type = "string" } },
                                { "careerIds", new { type = "array", items = new { type = "string" } } },
                                { "subdomainIds", new { type = "array", items = new { type = "string" } } },
                                { "questionType", new { type = "string" } },
                                { "difficulty", new { type = "string" } },
                                { "question", new { type = "string" } },
                                { "answerMappings", new { 
                                    type = "array", 
                                    items = new { 
                                        type = "object", 
                                        additionalProperties = false,
                                        properties = new Dictionary<string, object>
                                        {
                                            { "text", new { type = "string" } },
                                            { "signals", new {
                                                type = "object",
                                                additionalProperties = new { type = "number" }
                                            } }
                                        },
                                        required = new[] { "text", "signals" }
                                    }
                                } }
                            },
                            required = new[] { "questionId", "conceptId", "competencyId", "careerIds", "subdomainIds", "questionType", "difficulty", "question", "answerMappings" }
                        }
                    }
                };
            }
            else if (systemPrompt.Contains("Career Assessment Analyzer"))
            {
                responseFormat = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = "blueprint_analysis",
                        strict = true,
                        schema = new
                        {
                            type = "object",
                            additionalProperties = false,
                            properties = new Dictionary<string, object>
                            {
                                { "candidate_profiles", new {
                                    type = "array",
                                    items = new {
                                        type = "object",
                                        additionalProperties = false,
                                        properties = new Dictionary<string, object>
                                        {
                                            { "career", new { type = "string" } },
                                            { "domain", new { type = "string" } },
                                            { "subdomain", new { type = "string" } },
                                            { "specialization", new { type = "string" } },
                                            { "competencies", new { type = "array", items = new { type = "string" } } }
                                        },
                                        required = new[] { "career", "domain", "subdomain", "specialization", "competencies" }
                                    }
                                } }
                            },
                            required = new[] { "candidate_profiles" }
                        }
                    }
                };
            }

            // A configured model is tried first, followed by compatible Groq
            // models. This avoids a single retired model turning a personalized
            // action plan into a generic fallback.
            var configuredModel = useSkillAnalyzerKey
                ? Environment.GetEnvironmentVariable("GROQ_SKILL_ANALYZER_MODEL")?.Trim() ?? _configuration["GroqSkillAnalyzer:Model"]?.Trim()
                : Environment.GetEnvironmentVariable("GROQ_MODEL")?.Trim() ?? _configuration["Groq:Model"]?.Trim();
            var modelsToTry = new[] { configuredModel, "openai/gpt-oss-120b", "openai/gpt-oss-20b", "qwen/qwen3.6-27b" }
                .Where(model => !string.IsNullOrWhiteSpace(model))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            Exception? lastEx = null;

            foreach (var modelName in modelsToTry)
            {
                try
                {
                    var request = new Dictionary<string, object?>
                    {
                        ["model"] = modelName,
                        ["messages"] = messages,
                        ["temperature"] = 0.3,
                        ["max_tokens"] = 3072,
                        ["top_p"] = 0.9
                    };

                    if (responseFormat is not null)
                    {
                        request["response_format"] = responseFormat;
                    }

                    var content = new StringContent(
                        JsonSerializer.Serialize(request),
                        System.Text.Encoding.UTF8,
                        "application/json"
                    );

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                    _logger.LogInformation("Sending request to Groq API with model: {Model}", modelName);

                    var response = await _httpClient.PostAsync(GroqApiUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Groq API model {Model} returned status {Code}: {Error}", modelName, response.StatusCode, errorContent);
                        lastEx = new HttpRequestException($"Groq API model {modelName} returned status code: {response.StatusCode} - {errorContent}");
                        continue;
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonDocument.Parse(responseContent);

                    var messageContent = jsonResponse.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    _logger.LogInformation("Successfully received response from Groq API using model {Model}", modelName);
                    return messageContent ?? "Unable to generate response";
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    _logger.LogWarning(ex, "Failed attempt with Groq model {Model}", modelName);
                }
            }

            throw lastEx ?? new InvalidOperationException("All Groq models failed.");
        }
        catch (Exception ex)
        
        {
            _logger.LogError(ex, "Error calling Groq API.");
            throw;
        }
    }

    /// <summary>
    /// Extract structured data from text using Groq
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
            _logger.LogError(ex, "Error extracting structured data with Groq");
            throw;
        }
    }
}
