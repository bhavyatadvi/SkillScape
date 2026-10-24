using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class FreeformRoadmapService : IFreeformRoadmapService
{
    private readonly IRoadmapCurriculumGeneratorService _curriculumGenerator;
    private readonly ApplicationDbContext _context;
    private readonly GroqService _groqService;
    private readonly ILogger<FreeformRoadmapService> _logger;

    public FreeformRoadmapService(
        IRoadmapCurriculumGeneratorService curriculumGenerator,
        ApplicationDbContext context,
        GroqService groqService,
        ILogger<FreeformRoadmapService> logger)
    {
        _curriculumGenerator = curriculumGenerator;
        _context = context;
        _groqService = groqService;
        _logger = logger;
    }

    public async Task<RoadmapDto> GenerateFreeformRoadmapAsync(string userGoalText, string? situationText, string userId)
    {
        if (string.IsNullOrWhiteSpace(userGoalText))
            throw new ArgumentException("Goal text is required.");

        var combinedInput = $"Goal: {userGoalText}. Starting Situation: {situationText ?? "Not provided"}";

        // Step 1: Extract intent via AI
        string targetGoal = userGoalText.Trim();
        string startingPoint = situationText?.Trim() ?? "General learner";

        try
        {
            const string intentPrompt = @"Extract structured intent from the user's freeform roadmap request. Return ONLY valid JSON:
{
  ""targetGoal"": ""Short clear target career or goal (e.g. Entrepreneur, UX Designer)"",
  ""startingPoint"": ""Current background or constraints (e.g. 12th fail, non-tech BA graduate)"",
  ""regionContext"": ""India or Global""
}";
            var rawJson = await _groqService.GetSkillAnalyzerResponseAsync(intentPrompt, combinedInput);
            using var doc = JsonDocument.Parse(CleanJson(rawJson));
            var root = doc.RootElement;
            if (root.TryGetProperty("targetGoal", out var tg) && !string.IsNullOrWhiteSpace(tg.GetString()))
            {
                targetGoal = tg.GetString()!.Trim();
            }
            if (root.TryGetProperty("startingPoint", out var sp) && !string.IsNullOrWhiteSpace(sp.GetString()))
            {
                startingPoint = sp.GetString()!.Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract structured intent for freeform request. Using raw input.");
        }

        // Slugify goal key
        var careerKey = "custom-" + RegexReplace(targetGoal.ToLowerInvariant());

        // Step 2: Generate or reuse the canonical blueprint for this goal.
        // Re-generating on every request changed module IDs and made progress
        // disappear when a learner reopened the same custom roadmap.
        var blueprint = await _curriculumGenerator.GetOrGenerateCurriculumAsync(
            targetGoal,
            careerKey,
            userLevel: startingPoint,
            startingPointContext: $"Goal: {userGoalText}. Situation: {situationText ?? "Not provided"}",
            forceRegenerate: false
        );

        // Step 3: Link to UserSavedRoadmap
        if (!string.IsNullOrEmpty(userId))
        {
            var dismissal = await _context.UserRoadmapDismissals
                .FirstOrDefaultAsync(d => d.UserId == userId && d.CareerKey == careerKey);
            if (dismissal != null)
                _context.UserRoadmapDismissals.Remove(dismissal);

            var existingSaved = await _context.UserSavedRoadmaps
                .FirstOrDefaultAsync(u => u.UserId == userId && u.CareerKey == careerKey);

            if (existingSaved == null)
            {
                _context.UserSavedRoadmaps.Add(new UserSavedRoadmap
                {
                    UserId = userId,
                    CareerKey = careerKey,
                    CareerTitle = targetGoal,
                    BlueprintId = blueprint.Id,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    LastOpenedAt = DateTime.UtcNow
                });
                await _context.UserSavedRoadmaps
                    .Where(u => u.UserId == userId && u.CareerKey != careerKey)
                    .ExecuteUpdateAsync(updates => updates.SetProperty(u => u.IsDefault, false));
                await _context.SaveChangesAsync();
            }
            else
            {
                await _context.UserSavedRoadmaps
                    .Where(u => u.UserId == userId)
                    .ExecuteUpdateAsync(updates => updates.SetProperty(u => u.IsDefault, u => u.CareerKey == careerKey));
                existingSaved.LastOpenedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        return MapBlueprintToDto(blueprint, userId);
    }

    public static RoadmapDto MapBlueprintToDto(RoadmapBlueprint blueprint, string? userId)
    {
        var dto = new RoadmapDto
        {
            DomainId = blueprint.CareerKey,
            DomainName = blueprint.CareerTitle,
            ProgressPercentage = 0,
            CurriculumCreatedAt = blueprint.CreatedAt,
            LastWebCheckedAt = blueprint.LastWebCheckedAt,
            CurriculumSource = blueprint.GeneratedBy,
            HasAdaptiveUpdates = blueprint.LastWebCheckedAt > blueprint.CreatedAt,
            Phases = new List<RoadmapPhaseDto>()
        };

        foreach (var phase in blueprint.Phases.OrderBy(p => p.PhaseNumber))
        {
            var phaseDto = new RoadmapPhaseDto
            {
                PhaseId = phase.Id,
                PhaseNumber = phase.PhaseNumber,
                Title = phase.Title,
                Level = phase.Level,
                Summary = phase.Summary,
                EstimatedDuration = phase.EstimatedDuration,
                Modules = new List<RoadmapModuleDto>()
            };

            foreach (var mod in phase.Modules.OrderBy(m => m.StepNumber))
            {
                var modDto = new RoadmapModuleDto
                {
                    ModuleId = mod.Id,
                    Title = mod.Title,
                    Description = mod.Description,
                    StepNumber = mod.StepNumber,
                    EstimatedHours = mod.EstimatedHours,
                    IsCompleted = false,
                    Topics = mod.Topics.OrderBy(t => t.DisplayOrder).Select(t => new RoadmapTopicDto
                    {
                        TopicId = t.Id,
                        Title = t.Title,
                        ResourceUrl = t.ResourceUrl,
                        Type = t.Type.ToString(),
                        IsOptional = t.IsOptional
                    }).ToList()
                };

                if (mod.Quiz != null)
                {
                    modDto.Quiz = new RoadmapModuleQuizDto
                    {
                        QuizId = mod.Quiz.Id,
                        ModuleId = mod.Quiz.ModuleId,
                        PassScorePercentage = mod.Quiz.PassScorePercentage,
                        Questions = mod.Quiz.Questions.Select(q => new RoadmapQuizQuestionDto
                        {
                            QuestionId = q.Id,
                            QuizId = q.QuizId,
                            QuestionText = q.QuestionText,
                            Options = JsonSerializer.Deserialize<List<string>>(q.OptionsJson) ?? new(),
                            DifficultyTag = q.DifficultyTag
                        }).ToList()
                    };
                }

                phaseDto.Modules.Add(modDto);
            }

            dto.Phases.Add(phaseDto);
        }

        // Add legacy Skills mapping for backwards compatibility
        dto.Skills = dto.Phases.Select(p => new SkillRoadmapDto
        {
            SkillId = p.PhaseId,
            SkillName = p.Title,
            SkillDescription = p.Summary,
            IsCompleted = p.Modules.Count > 0 && p.Modules.All(m => m.IsCompleted),
            Modules = p.Modules
        }).ToList();

        return dto;
    }

    private static string CleanJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "{}";
        var match = System.Text.RegularExpressions.Regex.Match(raw.Trim(), @"```(?:json)?\s*([\s\S]*?)\s*```", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : raw.Trim();
    }

    private static string RegexReplace(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, @"[^a-z0-9]+", "-").Trim('-');
    }
}
