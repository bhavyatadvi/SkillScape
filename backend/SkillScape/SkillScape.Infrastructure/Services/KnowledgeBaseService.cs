using System.Text.Json;
using Microsoft.Extensions.Logging;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Service for managing the knowledge base
/// </summary>
public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly ILogger<KnowledgeBaseService> _logger;
    private readonly string _knowledgeBasePath;

    public KnowledgeBaseService(ILogger<KnowledgeBaseService> logger, string knowledgeBasePath)
    {
        _logger = logger;
        _knowledgeBasePath = knowledgeBasePath;
    }

    /// <summary>
    /// Get relevant careers based on user interests
    /// </summary>
    public async Task<List<CareerSuggestion>> GetRelevantCareersAsync(List<string> interests, int limit = 5)
    {
        try
        {
            var careers = await LoadCareersAsync();
            var suggestions = new List<CareerSuggestion>();

            foreach (var career in careers)
            {
                var matchScore = CalculateMatchScore(interests, career);
                if (matchScore > 0)
                {
                    suggestions.Add(new CareerSuggestion
                    {
                        Title = career.GetProperty("title").GetString() ?? "",
                        Description = career.GetProperty("description").GetString(),
                        MatchScore = matchScore,
                        KeyResponsibilities = career.GetProperty("keyResponsibilities")
                            .EnumerateArray()
                            .Select(e => e.GetString() ?? "")
                            .ToList(),
                        AverageSalary = career.GetProperty("averageSalary").GetString(),
                        JobOutlook = career.GetProperty("jobOutlook").GetString()
                    });
                }
            }

            return suggestions
                .OrderByDescending(s => s.MatchScore)
                .Take(limit)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relevant careers");
            return new List<CareerSuggestion>();
        }
    }

    /// <summary>
    /// Get skills required for a specific career
    /// </summary>
    public async Task<List<SkillRequirement>> GetCareerSkillsAsync(string careerPath, string skillLevel = "Beginner")
    {
        try
        {
            var careers = await LoadCareersAsync();
            var career = careers.FirstOrDefault(c =>
                c.GetProperty("title").GetString()?.Equals(careerPath, StringComparison.OrdinalIgnoreCase) ?? false);

            if (career.ValueKind == JsonValueKind.Undefined)
                return new List<SkillRequirement>();

            var skillIds = career.GetProperty("requiredSkillsIds")
                .EnumerateArray()
                .Select(s => s.GetString() ?? "")
                .ToList();

            var skills = await LoadSkillsAsync();
            var requirements = new List<SkillRequirement>();

            foreach (var skillId in skillIds)
            {
                var skill = skills.FirstOrDefault(s =>
                    s.GetProperty("id").GetString()?.Equals(skillId, StringComparison.OrdinalIgnoreCase) ?? false);

                if (skill.ValueKind != JsonValueKind.Undefined)
                {
                    requirements.Add(new SkillRequirement
                    {
                        Name = skill.GetProperty("name").GetString() ?? "",
                        Description = skill.GetProperty("description").GetString(),
                        Level = skill.GetProperty("level").GetString() ?? "Intermediate",
                        EstimatedMonths = skill.GetProperty("estimatedMonths").GetInt32(),
                        Resources = skill.GetProperty("resources")
                            .EnumerateArray()
                            .Select(r => r.GetProperty("title").GetString() ?? "")
                            .ToList()
                    });
                }
            }

            return requirements;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting career skills");
            return new List<SkillRequirement>();
        }
    }

    /// <summary>
    /// Get a learning roadmap for a career path
    /// </summary>
    public async Task<Roadmap?> GetCareerRoadmapAsync(string careerPath, string skillLevel = "Beginner")
    {
        try
        {
            var careers = await LoadCareersAsync();
            var career = careers.FirstOrDefault(c =>
                c.GetProperty("title").GetString()?.Equals(careerPath, StringComparison.OrdinalIgnoreCase) ?? false);

            if (career.ValueKind == JsonValueKind.Undefined)
                return null;

            var roadmapId = career.GetProperty("roadmapId").GetString();
            var roadmaps = await LoadRoadmapsAsync();
            var roadmapElement = roadmaps.FirstOrDefault(r =>
                r.GetProperty("id").GetString()?.Equals(roadmapId, StringComparison.OrdinalIgnoreCase) ?? false);

            if (roadmapElement.ValueKind == JsonValueKind.Undefined)
                return null;

            return ParseRoadmap(roadmapElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting career roadmap");
            return null;
        }
    }

    /// <summary>
    /// Search the knowledge base
    /// </summary>
    public async Task<List<string>> SearchAsync(string query)
    {
        try
        {
            var results = new List<string>();
            var lowerQuery = query.ToLower();

            var careers = await LoadCareersAsync();
            var matchingCareers = careers
                .Where(c => (c.GetProperty("title").GetString()?.ToLower().Contains(lowerQuery) ?? false) ||
                           (c.GetProperty("description").GetString()?.ToLower().Contains(lowerQuery) ?? false))
                .Select(c => c.GetProperty("title").GetString() ?? "");

            results.AddRange(matchingCareers);

            return results.Take(10).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching knowledge base");
            return new List<string>();
        }
    }

    // Private helper methods

    private async Task<List<JsonElement>> LoadCareersAsync()
    {
        try
        {
            var filePath = Path.Combine(_knowledgeBasePath, "careers.json");
            if (!File.Exists(filePath))
            {
                _logger.LogError($"Careers file not found at: {filePath}");
                return new List<JsonElement>();
            }
            var json = await File.ReadAllTextAsync(filePath);
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("careers").EnumerateArray().ToList();
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, $"Careers file not found in knowledge base path: {_knowledgeBasePath}");
            return new List<JsonElement>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing careers.json - invalid JSON format");
            return new List<JsonElement>();
        }
    }

    private async Task<List<JsonElement>> LoadSkillsAsync()
    {
        try
        {
            var filePath = Path.Combine(_knowledgeBasePath, "skills.json");
            if (!File.Exists(filePath))
            {
                _logger.LogError($"Skills file not found at: {filePath}");
                return new List<JsonElement>();
            }
            var json = await File.ReadAllTextAsync(filePath);
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("skills").EnumerateArray().ToList();
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, $"Skills file not found in knowledge base path: {_knowledgeBasePath}");
            return new List<JsonElement>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing skills.json - invalid JSON format");
            return new List<JsonElement>();
        }
    }

    private async Task<List<JsonElement>> LoadRoadmapsAsync()
    {
        try
        {
            var filePath = Path.Combine(_knowledgeBasePath, "roadmaps.json");
            if (!File.Exists(filePath))
            {
                _logger.LogError($"Roadmaps file not found at: {filePath}");
                return new List<JsonElement>();
            }
            var json = await File.ReadAllTextAsync(filePath);
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("roadmaps").EnumerateArray().ToList();
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, $"Roadmaps file not found in knowledge base path: {_knowledgeBasePath}");
            return new List<JsonElement>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing roadmaps.json - invalid JSON format");
            return new List<JsonElement>();
        }
    }

    private double CalculateMatchScore(List<string> interests, JsonElement career)
    {
        if (!interests.Any())
            return 0.5; // Default score if no interests

        var title = career.GetProperty("title").GetString()?.ToLower() ?? "";
        var category = career.GetProperty("category").GetString()?.ToLower() ?? "";

        var matches = interests.Count(i =>
            title.Contains(i.ToLower()) ||
            category.Contains(i.ToLower())
        );

        return (double)matches / interests.Count;
    }

    private Roadmap ParseRoadmap(JsonElement roadmapElement)
    {
        var phases = roadmapElement.GetProperty("phases")
            .EnumerateArray()
            .Select(p => new RoadmapPhase
            {
                Name = p.GetProperty("name").GetString() ?? "",
                DurationMonths = p.GetProperty("durationMonths").GetInt32(),
                Topics = p.GetProperty("topics").EnumerateArray().Select(t => t.GetString() ?? "").ToList(),
                Projects = p.GetProperty("projects").EnumerateArray().Select(pr => pr.GetString() ?? "").ToList(),
                Milestones = p.GetProperty("milestones").EnumerateArray().Select(m => m.GetString() ?? "").ToList()
            })
            .ToList();

        return new Roadmap
        {
            Title = roadmapElement.GetProperty("title").GetString() ?? "",
            Description = roadmapElement.GetProperty("description").GetString(),
            TotalDurationMonths = roadmapElement.GetProperty("totalDurationMonths").GetInt32(),
            Phases = phases
        };
    }
}
