using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services;

public class ResumeGroqService : IResumeGroqService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResumeGroqService> _logger;

    private const string GroqApiUrl = "https://api.groq.com/openai/v1/chat/completions";
    // Fallback chain of valid Groq models (primary → fallbacks)
    private static readonly string[] ModelFallbackChain =
    {
        "llama-3.3-70b-versatile",
        "llama-3.1-8b-instant",
        "meta-llama/llama-4-scout-17b-16e"
    };

    public ResumeGroqService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ResumeGroqService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetResumeProfileAsync(string resumeText, CancellationToken cancellationToken = default)
    {
        // 1. Get API Key specifically from GROQ_API_RESUME environment variable or GroqResume:ApiKey config
        var apiKey = Environment.GetEnvironmentVariable("GROQ_API_RESUME")
            ?? _configuration["GroqResume:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("GROQ_API_RESUME environment variable or GroqResume:ApiKey configuration is missing.");
            throw new InvalidOperationException("Resume AI service is not configured.");
        }

        // Allow override via env var / config; otherwise use the fallback chain
        var configuredModel = Environment.GetEnvironmentVariable("GROQ_RESUME_MODEL")
            ?? _configuration["GroqResume:Model"];

        var modelsToTry = configuredModel != null
            ? new[] { configuredModel }
            : ModelFallbackChain;
        var userMessage = $"Please parse this resume and return JSON:\n\n{resumeText}";

        var messages = new List<object>
        {
            new { role = "system", content = @"You are a strict resume parsing system.
Extract career profile information from the supplied resume text.
Return ONLY valid JSON matching the supplied schema.
Never invent information.
Do NOT include markdown formatting, code block backticks (```json or ```).
If information is not available in the resume, return:
- empty string """" for text fields
- empty array [] for array fields
- null for numeric fields

Do not guess career preferences, salary, motivations, or constraints unless explicitly stated in the resume text.

SKILL EXTRACTION (parsedSkills field):
Scan EVERY section of the resume for skills:
  Skills, Technical Skills, Core Competencies, Technologies, Tools,
  Programming Languages, Frameworks, Libraries, Databases, Cloud, Platforms,
  Certifications, Professional Experience, Projects, Internships, Education.

Also INFER skills when explicitly demonstrated through project/job/internship descriptions:
  - ""Built REST APIs using ASP.NET Core"" → [""ASP.NET Core"", ""REST APIs""]
  - ""Worked with MongoDB and SQL Server"" → [""MongoDB"", ""SQL Server""]
  - ""Developed React applications"" → [""React""]
  - ""Implemented ML models using Python and scikit-learn"" → [""Python"", ""Machine Learning"", ""scikit-learn""]
  - ""Used Git and GitHub for version control"" → [""Git"", ""GitHub""]
  - ""Deployed on AWS EC2"" → [""AWS"", ""EC2""]
  - ""Wrote unit tests with Jest"" → [""Jest"", ""Unit Testing""]

Include every distinct technology, tool, language, framework, library, platform, methodology,
or domain skill supported by the resume text.
Do NOT duplicate entries (case-insensitive deduplication).
Do NOT invent skills not found or clearly demonstrated in the resume.
Return parsedSkills as a flat JSON array of strings.

For currentSkills, include the same skills with your best proficiency estimate:
  - Use context clues (years of experience, seniority, description depth) to assign proficiency.
  - If no context clues exist, use ""Intermediate"".

Schema:
{
  ""educationLevel"": ""High School | Diploma | Bachelors | Masters | Doctorate | empty string"",
  ""degree"": ""string"",
  ""broadField"": ""string"",
  ""specialization"": ""string"",
  ""institution"": ""string"",
  ""graduationYear"": number or null,
  ""additionalQualifications"": ""string"",
  ""educationStatus"": ""string"",
  ""currentEmploymentStatus"": ""Student | Fresher | Working | Self-employed | Freelancer | Entrepreneur | Unemployed | Career break | Other | empty string"",
  ""currentOccupation"": ""string"",
  ""currentRole"": ""string"",
  ""currentIndustry"": ""string"",
  ""organizationType"": ""string"",
  ""experienceYears"": number or null,
  ""relevantExperience"": ""string"",
  ""previousRoles"": ""string"",
  ""previousIndustries"": ""string"",
  ""parsedSkills"": [""string""],
  ""currentSkills"": [
    {
      ""name"": ""string"",
      ""proficiency"": ""Beginner | Intermediate | Advanced | Expert""
    }
  ],
  ""certifications"": [""string""],
  ""careerLikes"": [],
  ""careerDislikes"": [],
  ""workPreferences"": {
    ""environment"": [],
    ""style"": []
  },
  ""motivations"": [],
  ""constraints"": [],
  ""careerChangePreference"": ""string"",
  ""location"": ""string"",
  ""preferredWorkMode"": ""string"",
  ""salaryRange"": ""string"",
  ""careerGoals"": ""string""
}" },
            new { role = "user", content = userMessage }
        };

        Exception? lastException = null;

        foreach (var modelName in modelsToTry)
        {
            _logger.LogInformation("Resume AI request started using model {Model}.", modelName);

            var requestBody = new
            {
                model = modelName,
                messages = messages,
                temperature = 0.2,
                max_tokens = 4096,
                top_p = 0.9,
                response_format = new { type = "json_object" }
            };

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, GroqApiUrl);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            requestMessage.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Resume AI HTTP call failed to connect using model {Model}. Trying next.", modelName);
                lastException = ex;
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Resume AI request failed. StatusCode: {StatusCode}, Model: {Model}, Reason: {Reason} — trying next model.",
                    (int)response.StatusCode, modelName, errorContent);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new InvalidOperationException("Resume AI authentication failed. Check your GROQ_API_RESUME key.");

                // Rate limit or bad model → try next
                lastException = new InvalidOperationException(
                    $"Resume AI returned {(int)response.StatusCode}: {errorContent}");
                continue;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            // Parse the response and check for empty content ("model output error")
            string rawJson;
            try
            {
                using var doc = JsonDocument.Parse(responseContent);
                rawJson = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not parse Groq response JSON for model {Model}. Trying next.", modelName);
                lastException = ex;
                continue;
            }

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                _logger.LogWarning("Model {Model} returned empty content (model output error). Trying next model.", modelName);
                lastException = new InvalidOperationException($"Model {modelName} returned empty output.");
                continue;
            }

            var cleanedJson = CleanJsonResponse(rawJson);
            _logger.LogInformation("Resume profile JSON received from model {Model} successfully.", modelName);
            return cleanedJson;
        }

        // All models failed
        throw new InvalidOperationException(
            "All Resume AI models failed to produce a result. Please try again later.",
            lastException);
    }

    private static string CleanJsonResponse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "{}";
        var trimmed = raw.Trim();
        var match = Regex.Match(trimmed, @"```(?:json)?\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }
        return trimmed;
    }
}
