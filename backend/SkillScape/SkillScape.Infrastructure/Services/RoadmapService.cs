using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class RoadmapService : IRoadmapService
{
    private readonly ApplicationDbContext _context;
    private readonly IDynamicRoadmapEngine _dynamicRoadmapEngine;
    private readonly IRoadmapCurriculumGeneratorService _curriculumGenerator;
    private readonly ICareerIntelligenceService? _careerIntelligence;

    public RoadmapService(
        ApplicationDbContext context,
        IDynamicRoadmapEngine dynamicRoadmapEngine,
        IRoadmapCurriculumGeneratorService curriculumGenerator,
        ICareerIntelligenceService? careerIntelligence = null)
    {
        _context = context;
        _dynamicRoadmapEngine = dynamicRoadmapEngine;
        _curriculumGenerator = curriculumGenerator;
        _careerIntelligence = careerIntelligence;
    }

    private async Task<(string Key, string NormalizedKey, string Title)?> GetUserPredictedCareerAsync(string userId)
    {
        string? predictedKey = null;
        string? predictedTitle = null;

        // 1. Check for V2 Adaptive Assessment Quiz Results (Primary ML Engine)
        var latestV2Result = await _context.QuizResultsV2
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.GeneratedAt)
            .FirstOrDefaultAsync();

        if (latestV2Result != null && !string.IsNullOrEmpty(latestV2Result.PrimaryTaxonomyNodeId))
        {
            predictedKey = latestV2Result.PrimaryTaxonomyNodeId;
            var node = await _context.CareerTaxonomyV2.FindAsync(predictedKey);
            predictedTitle = node?.Name;
            if (string.IsNullOrWhiteSpace(predictedTitle))
            {
                var disp = CareerDisplayNameService.GetDisplayName(predictedKey);
                if (!string.IsNullOrWhiteSpace(disp) && disp != "Unknown Career")
                    predictedTitle = disp;
            }
        }

        // Fallback: Check TopPredictionsJson if PrimaryTaxonomyNodeId was not set
        if (latestV2Result != null && string.IsNullOrEmpty(predictedKey) && !string.IsNullOrEmpty(latestV2Result.TopPredictionsJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(latestV2Result.TopPredictionsJson);
                if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                {
                    var first = doc.RootElement[0];
                    if (first.TryGetProperty("nodeId", out var nid) && !string.IsNullOrEmpty(nid.GetString()))
                    {
                        predictedKey = nid.GetString();
                    }
                }
            }
            catch { }
        }

        // 2. Check for completed AdaptiveQuizSessions
        if (string.IsNullOrEmpty(predictedKey))
        {
            var latestSession = await _context.AdaptiveQuizSessions
                .Where(s => s.UserId == userId && s.Status == "Completed")
                .OrderByDescending(s => s.CompletedAt)
                .FirstOrDefaultAsync();

            if (latestSession != null && !string.IsNullOrEmpty(latestSession.TargetCareersJson))
            {
                try
                {
                    var targets = JsonSerializer.Deserialize<List<string>>(latestSession.TargetCareersJson);
                    var firstTarget = targets?.FirstOrDefault();
                    if (!string.IsNullOrEmpty(firstTarget))
                        predictedKey = firstTarget;
                }
                catch { }
            }
        }

        // 3. Check for UserCareerProfiles (accepted career switch / custom career profile)
        if (string.IsNullOrEmpty(predictedKey))
        {
            var profile = await _context.UserCareerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile != null && !string.IsNullOrEmpty(profile.RecommendedCareerId))
            {
                predictedKey = profile.RecommendedCareerId;
                var path = await _context.CareerPaths.FindAsync(predictedKey);
                predictedTitle = path?.Title;
            }
        }

        // 4. Check for Legacy Quiz Results
        if (string.IsNullOrEmpty(predictedKey))
        {
            var latestResult = await _context.QuizResults
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CompletedAt)
                .FirstOrDefaultAsync();
            if (latestResult != null && !string.IsNullOrEmpty(latestResult.RecommendedDomainId))
            {
                predictedKey = latestResult.RecommendedDomainId;
                var domain = await _context.CareerDomains
                    .FirstOrDefaultAsync(d => d.Id == predictedKey && d.IsActive);
                predictedTitle = domain?.Name;
            }
        }

        if (string.IsNullOrEmpty(predictedKey))
            return null;

        var normalizedKey = NormalizeCareerKey(predictedKey);
        if (string.IsNullOrEmpty(predictedTitle))
        {
            predictedTitle = await ResolveCareerTitleAsync(normalizedKey, null);
        }

        return (predictedKey, normalizedKey, predictedTitle);
    }

    private async Task EnsurePredictedCareerSavedAsync(string userId)
    {
        try
        {
            var predicted = await GetUserPredictedCareerAsync(userId);
            if (predicted == null) return;

            var (predictedKey, normalizedKey, predictedTitle) = predicted.Value;

            var existingSaved = await _context.UserSavedRoadmaps
                .Where(u => u.UserId == userId)
                .ToListAsync();

            var predictedSaved = existingSaved.FirstOrDefault(u => 
                u.CareerKey == normalizedKey || u.CareerKey == predictedKey);
            bool predictedDismissed = false;
            try
            {
                predictedDismissed = await _context.UserRoadmapDismissals
                    .AnyAsync(d => d.UserId == userId && (d.CareerKey == normalizedKey || d.CareerKey == predictedKey));
            }
            catch { }

            if (predictedSaved == null && !predictedDismissed)
            {
                // Only unset default on existing roadmaps if there isn't an active non-generic user-selected default roadmap
                bool hasExplicitCustomDefault = existingSaved.Any(s => s.IsDefault && s.CareerKey != "software-engineer" && s.CareerKey != "backend");
                if (!hasExplicitCustomDefault)
                {
                    foreach (var s in existingSaved) s.IsDefault = false;
                }

                // Ensure curriculum is generated and grab the valid blueprint
                var bp = await _curriculumGenerator.GetOrGenerateCurriculumAsync(predictedTitle, normalizedKey);

                _context.UserSavedRoadmaps.Add(new UserSavedRoadmap
                {
                    UserId = userId,
                    CareerKey = normalizedKey,
                    CareerTitle = predictedTitle,
                    BlueprintId = bp.Id,
                    IsDefault = !hasExplicitCustomDefault,
                    CreatedAt = DateTime.UtcNow,
                    LastOpenedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            else if (predictedSaved != null && !predictedSaved.IsDefault)
            {
                // Automatically promote ML predicted roadmap as default if current default is generic fallback
                var currentDefault = existingSaved.FirstOrDefault(u => u.IsDefault);
                if (currentDefault == null || currentDefault.CareerKey == "software-engineer" || currentDefault.CareerKey == "backend")
                {
                    foreach (var s in existingSaved) s.IsDefault = (s.Id == predictedSaved.Id);
                    predictedSaved.LastOpenedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error in EnsurePredictedCareerSavedAsync: {ex.Message}");
        }
    }

    public async Task<RoadmapDto?> GetMyRoadmapAsync(string userId)
    {
        try
        {
            await EnsurePredictedCareerSavedAsync(userId);

            // 1. Check if user has a default saved roadmap
            var defaultSaved = await _context.UserSavedRoadmaps
                .FirstOrDefaultAsync(u => u.UserId == userId && u.IsDefault);

            if (defaultSaved != null && !string.IsNullOrEmpty(defaultSaved.CareerKey))
            {
                return await GetRoadmapByDomainAsync(userId, defaultSaved.CareerKey);
            }

            // 2. Check for any saved roadmap
            var anySaved = await _context.UserSavedRoadmaps
                .OrderByDescending(u => u.LastOpenedAt)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (anySaved != null && !string.IsNullOrEmpty(anySaved.CareerKey))
            {
                anySaved.IsDefault = true;
                await _context.SaveChangesAsync();
                return await GetRoadmapByDomainAsync(userId, anySaved.CareerKey);
            }

            // Check if user has taken any quiz or has any predicted career or dismissals
            var predicted = await GetUserPredictedCareerAsync(userId);
            var hasDismissals = false;
            try
            {
                hasDismissals = await _context.UserRoadmapDismissals.AnyAsync(d => d.UserId == userId);
            }
            catch { }

            if (predicted != null || hasDismissals)
            {
                // User has an assessment or has deliberately dismissed roadmaps; do not inject software-engineer
                return null;
            }

            // Ultimate default fallback removed to force users to take the assessment
            return null;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error in GetMyRoadmapAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<List<RoadmapOptionDto>> GetRoadmapOptionsAsync(string userId)
    {
        await EnsurePredictedCareerSavedAsync(userId);

        var options = new List<RoadmapOptionDto>();
        var userSaved = await _context.UserSavedRoadmaps
            .Where(u => u.UserId == userId)
            .ToListAsync();

        var defaultSaved = userSaved.FirstOrDefault(u => u.IsDefault) ?? userSaved.FirstOrDefault();
        string? recommendedKey = defaultSaved?.CareerKey;

        foreach (var saved in userSaved)
        {
            var roadmap = await GetRoadmapByDomainAsync(userId, saved.CareerKey);
            options.Add(new RoadmapOptionDto
            {
                DomainId = saved.CareerKey,
                DomainName = saved.CareerTitle,
                ProgressPercentage = roadmap.ProgressPercentage,
                IsRecommended = saved.CareerKey == recommendedKey
            });
        }

        // Add standard domains if not already included
        var domains = await _context.CareerDomains
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();

        foreach (var d in domains)
        {
            if (options.Any(o => o.DomainId == d.Id)) continue;
            options.Add(new RoadmapOptionDto
            {
                DomainId = d.Id,
                DomainName = d.Name,
                ProgressPercentage = 0,
                IsRecommended = string.IsNullOrEmpty(recommendedKey) && options.Count == 0
            });
        }

        return options;
    }

    public async Task<List<string>> GetDismissedRoadmapKeysAsync(string userId)
    {
        try
        {
            return await _context.UserRoadmapDismissals
                .Where(d => d.UserId == userId)
                .Select(d => d.CareerKey)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetDismissedRoadmapKeysAsync: {ex.Message}");
            return new List<string>();
        }
    }

    public async Task<RoadmapDto> GetRoadmapByDomainAsync(string userId, string domainId)
    {
        if (string.IsNullOrWhiteSpace(domainId))
            throw new InvalidOperationException("Domain is required.");

        // Resolve title for domainId
        var normalizedDomainId = NormalizeCareerKey(domainId);
        var altDomainId = normalizedDomainId.Contains('-') ? normalizedDomainId.Replace('-', '_') : normalizedDomainId.Replace('_', '-');
        var careerPrefixed = "career-" + normalizedDomainId;
        var careerUnderline = "career_" + normalizedDomainId.Replace('-', '_');

        var savedRoadmap = await _context.UserSavedRoadmaps
            .FirstOrDefaultAsync(u => u.UserId == userId && (u.CareerKey == domainId || u.CareerKey == normalizedDomainId || u.CareerKey == altDomainId || u.CareerKey == careerPrefixed || u.CareerKey == careerUnderline));
        
        string careerTitle = !string.IsNullOrWhiteSpace(savedRoadmap?.CareerTitle)
            ? savedRoadmap.CareerTitle
            : await ResolveCareerTitleAsync(domainId, null);

        // Retrieve user background details for curriculum personalization
        string? userLevel = null;
        string? backgroundContext = null;
        var latestSession = await _context.AdaptiveQuizSessions
            .Where(s => s.UserId == userId && s.Status == "Completed")
            .OrderByDescending(s => s.CompletedAt)
            .FirstOrDefaultAsync();

        if (latestSession != null && !string.IsNullOrEmpty(latestSession.BackgroundProfileJson))
        {
            backgroundContext = latestSession.BackgroundProfileJson;
        }

        var blueprint = await _curriculumGenerator.GetOrGenerateCurriculumAsync(
            careerTitle,
            domainId,
            userLevel,
            backgroundContext
        );

        // Remove any dismissal for this domain since user explicitly requested/opened it
        bool wasDismissed = false;
        try
        {
            var dismissals = await _context.UserRoadmapDismissals
                .Where(d => d.UserId == userId && (d.CareerKey == domainId || d.CareerKey == normalizedDomainId || d.CareerKey == altDomainId))
                .ToListAsync();
            if (dismissals.Count > 0)
            {
                wasDismissed = true;
                _context.UserRoadmapDismissals.RemoveRange(dismissals);
                await _context.SaveChangesAsync();
            }
        }
        catch { }

        // If this career is not currently saved in UserSavedRoadmaps, check if it was dismissed OR matches predicted career
        // (e.g. user previously deleted it from My Roadmaps, then went to Home and clicked Continue Learning):
        // Automatically restore it into UserSavedRoadmaps with IsDefault = true!
        if (savedRoadmap == null)
        {
            var predicted = await GetUserPredictedCareerAsync(userId);
            bool isMatch = false;
            if (predicted != null)
            {
                var (predKey, predNormKey, predTitle) = predicted.Value;
                isMatch = string.Equals(domainId, predKey, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(domainId, predNormKey, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(normalizedDomainId, predNormKey, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(normalizedDomainId, predKey, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(altDomainId, predKey, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(altDomainId, predNormKey, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(careerTitle, predTitle, StringComparison.OrdinalIgnoreCase) ||
                          (!string.IsNullOrEmpty(predTitle) && careerTitle.Contains(predTitle, StringComparison.OrdinalIgnoreCase)) ||
                          (!string.IsNullOrEmpty(careerTitle) && predTitle.Contains(careerTitle, StringComparison.OrdinalIgnoreCase));
            }

            if (!isMatch)
            {
                var userProfile = await _context.UserCareerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (userProfile != null && !string.IsNullOrEmpty(userProfile.RecommendedCareerId))
                {
                    var profKey = NormalizeCareerKey(userProfile.RecommendedCareerId);
                    if (string.Equals(normalizedDomainId, profKey, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(domainId, profKey, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(altDomainId, profKey, StringComparison.OrdinalIgnoreCase))
                    {
                        isMatch = true;
                    }
                }
            }

            if (wasDismissed || isMatch)
            {
                var allSaved = await _context.UserSavedRoadmaps.Where(u => u.UserId == userId).ToListAsync();
                foreach (var s in allSaved) s.IsDefault = false;

                var restored = new UserSavedRoadmap
                {
                    UserId = userId,
                    CareerKey = normalizedDomainId,
                    CareerTitle = careerTitle,
                    BlueprintId = blueprint.Id,
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    LastOpenedAt = DateTime.UtcNow
                };
                _context.UserSavedRoadmaps.Add(restored);
                await _context.SaveChangesAsync();
                savedRoadmap = restored;
            }
        }
        else
        {
            savedRoadmap.LastOpenedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // Calculate progress set
        var completedModuleIds = await _context.UserModuleProgressions
            .Where(ump => ump.UserId == userId && ump.IsCompleted)
            .Select(ump => ump.ModuleId)
            .ToListAsync();

        var completedCustomIds = await _context.UserCustomModuleProgressions
            .Where(ump => ump.UserId == userId && ump.IsCompleted)
            .Select(ump => ump.ModuleId)
            .ToListAsync();

        var passedQuizModuleIds = await _context.UserRoadmapModuleQuizAttempts
            .Where(a => a.UserId == userId && a.Passed)
            .Select(a => a.ModuleId)
            .ToListAsync();

        var completedSet = completedModuleIds
            .Concat(completedCustomIds)
            .Concat(passedQuizModuleIds)
            .ToHashSet();

        var dto = FreeformRoadmapService.MapBlueprintToDto(blueprint, userId);

        int totalMods = 0;
        int completedMods = 0;

        foreach (var phase in dto.Phases)
        {
            foreach (var mod in phase.Modules)
            {
                totalMods++;
                if (completedSet.Contains(mod.ModuleId))
                {
                    mod.IsCompleted = true;
                    completedMods++;
                }
            }
        }

        foreach (var skill in dto.Skills)
        {
            int skillCompleted = 0;
            foreach (var mod in skill.Modules)
            {
                if (completedSet.Contains(mod.ModuleId))
                {
                    mod.IsCompleted = true;
                    skillCompleted++;
                }
            }
            skill.IsCompleted = skill.Modules.Count > 0 && skillCompleted == skill.Modules.Count;
        }

        if (totalMods > 0 && completedMods >= totalMods)
        {
            dto.ProgressPercentage = 100.0;
        }
        else
        {
            dto.ProgressPercentage = totalMods > 0 ? Math.Min(100.0, ((double)completedMods / totalMods) * 100.0) : 0;
        }
        return dto;
    }

    public async Task<RoadmapModuleQuizDto> GetModuleQuizAsync(string moduleId)
    {
        var quiz = await _context.RoadmapModuleQuizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.ModuleId == moduleId);

        if (quiz == null)
        {
            var mod = await _context.RoadmapModules
                .Include(m => m.Phase)
                .ThenInclude(p => p!.Blueprint)
                .FirstOrDefaultAsync(m => m.Id == moduleId);

            if (mod != null && mod.Phase?.Blueprint != null)
            {
                // Ensure blueprint is populated
                var bp = await _curriculumGenerator.GetOrGenerateCurriculumAsync(
                    mod.Phase.Blueprint.CareerTitle,
                    mod.Phase.Blueprint.CareerKey
                );
                quiz = await _context.RoadmapModuleQuizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.ModuleId == moduleId);
            }
        }

        if (quiz == null || quiz.Questions.Count == 0)
        {
            throw new InvalidOperationException($"Quiz not found for module {moduleId}");
        }

        return new RoadmapModuleQuizDto
        {
            QuizId = quiz.Id,
            ModuleId = quiz.ModuleId,
            PassScorePercentage = quiz.PassScorePercentage,
            Questions = quiz.Questions.Select(q => new RoadmapQuizQuestionDto
            {
                QuestionId = q.Id,
                QuizId = q.QuizId,
                QuestionText = q.QuestionText,
                Options = JsonSerializer.Deserialize<List<string>>(q.OptionsJson) ?? new(),
                DifficultyTag = q.DifficultyTag
                // CorrectOptionIndex and Explanation are omitted for secure test delivery
            }).ToList()
        };
    }

    public async Task<RoadmapQuizResultDto> SubmitModuleQuizAsync(string userId, string moduleId, List<int> userAnswers)
    {
        var quiz = await _context.RoadmapModuleQuizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.ModuleId == moduleId);

        if (quiz == null || quiz.Questions.Count == 0)
            throw new InvalidOperationException("Module quiz not found");

        var questions = quiz.Questions.ToList();
        int correctCount = 0;
        var evaluated = new List<RoadmapQuizQuestionDto>();

        for (int i = 0; i < questions.Count; i++)
        {
            var q = questions[i];
            int userChoice = i < userAnswers.Count ? userAnswers[i] : -1;
            bool isCorrect = userChoice == q.CorrectOptionIndex;
            if (isCorrect) correctCount++;

            evaluated.Add(new RoadmapQuizQuestionDto
            {
                QuestionId = q.Id,
                QuizId = q.QuizId,
                QuestionText = q.QuestionText,
                Options = JsonSerializer.Deserialize<List<string>>(q.OptionsJson) ?? new(),
                CorrectOptionIndex = q.CorrectOptionIndex,
                Explanation = q.Explanation,
                DifficultyTag = q.DifficultyTag
            });
        }

        double scorePct = ((double)correctCount / questions.Count) * 100;
        bool passed = scorePct >= quiz.PassScorePercentage;

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            // Attempts are historical and may be repeated, but completion rows
            // are unique per user/module. Keep the whole operation atomic so a
            // double click cannot leave an attempt without progress or vice versa.
            _context.UserRoadmapModuleQuizAttempts.Add(new UserRoadmapModuleQuizAttempt
            {
                UserId = userId,
                ModuleId = moduleId,
                Score = scorePct,
                TotalQuestions = questions.Count,
                Passed = passed,
                AnsweredJson = JsonSerializer.Serialize(userAnswers),
                AttemptedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            if (passed)
                await MarkStepCompleteAsync(userId, moduleId);

            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            var detail = ex.InnerException?.Message ?? ex.Message;
            throw new InvalidOperationException($"Unable to save quiz progress. Please try submitting once more. Database detail: {detail}", ex);
        }

        return new RoadmapQuizResultDto
        {
            ModuleId = moduleId,
            ScorePercentage = Math.Round(scorePct, 1),
            CorrectCount = correctCount,
            TotalQuestions = questions.Count,
            Passed = passed,
            Message = passed ? "Congratulations! You passed the module quiz." : $"You scored {Math.Round(scorePct)}%. Retake the quiz to achieve ≥{quiz.PassScorePercentage}% and pass.",
            EvaluatedQuestions = evaluated
        };
    }

    public async Task<RoadmapDto> RegenerateRoadmapAsync(string userId, string careerKey)
    {
        var existingBp = await _context.RoadmapBlueprints.FirstOrDefaultAsync(b => b.CareerKey == careerKey);
        string title = existingBp?.CareerTitle ?? careerKey;

        await _curriculumGenerator.GetOrGenerateCurriculumAsync(title, careerKey, forceRegenerate: true);
        return await GetRoadmapByDomainAsync(userId, careerKey);
    }

    public async Task<List<UserSavedRoadmapDto>> GetUserSavedRoadmapsAsync(string userId)
    {
        await EnsurePredictedCareerSavedAsync(userId);

        var savedList = await _context.UserSavedRoadmaps
            .Where(u => u.UserId == userId)
            .OrderByDescending(u => u.IsDefault)
            .ThenByDescending(u => u.LastOpenedAt)
            .ToListAsync();

        var results = new List<UserSavedRoadmapDto>();
        foreach (var s in savedList)
        {
            var roadmap = await GetRoadmapByDomainAsync(userId, s.CareerKey);
            results.Add(new UserSavedRoadmapDto
            {
                Id = s.Id,
                CareerKey = s.CareerKey,
                CareerTitle = s.CareerTitle,
                IsDefault = s.IsDefault,
                ProgressPercentage = roadmap.ProgressPercentage,
                CreatedAt = s.CreatedAt,
                LastOpenedAt = s.LastOpenedAt
            });
        }
        return results;
    }

    public async Task<bool> SaveUserRoadmapAsync(string userId, string careerKey, string? careerTitle = null)
    {
        var normalizedKey = NormalizeCareerKey(careerKey);
        var altKey = normalizedKey.Contains('-') ? normalizedKey.Replace('-', '_') : normalizedKey.Replace('_', '-');
        var careerPrefixed = "career-" + normalizedKey;
        var careerUnderline = "career_" + normalizedKey.Replace('-', '_');

        try
        {
            var dismissals = await _context.UserRoadmapDismissals
                .Where(d => d.UserId == userId && (d.CareerKey == normalizedKey || d.CareerKey == altKey || d.CareerKey == careerKey || d.CareerKey == careerPrefixed || d.CareerKey == careerUnderline))
                .ToListAsync();
            if (dismissals.Count > 0)
            {
                _context.UserRoadmapDismissals.RemoveRange(dismissals);
                await _context.SaveChangesAsync();
            }
        }
        catch { }

        var existing = await _context.UserSavedRoadmaps
            .FirstOrDefaultAsync(u => u.UserId == userId && (u.CareerKey == normalizedKey || u.CareerKey == altKey || u.CareerKey == careerKey || u.CareerKey == careerPrefixed || u.CareerKey == careerUnderline));

        if (existing != null)
        {
            existing.LastOpenedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(careerTitle) && existing.CareerTitle != careerTitle)
            {
                existing.CareerTitle = careerTitle;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        bool hasDefault = await _context.UserSavedRoadmaps.AnyAsync(u => u.UserId == userId && u.IsDefault);
        string resolvedTitle = !string.IsNullOrWhiteSpace(careerTitle) ? careerTitle : await ResolveCareerTitleAsync(normalizedKey, null);

        // Ensure blueprint exists
        var blueprint = await _curriculumGenerator.GetOrGenerateCurriculumAsync(resolvedTitle, normalizedKey);

        _context.UserSavedRoadmaps.Add(new UserSavedRoadmap
        {
            UserId = userId,
            CareerKey = normalizedKey,
            CareerTitle = resolvedTitle,
            BlueprintId = blueprint.Id,
            IsDefault = !hasDefault,
            CreatedAt = DateTime.UtcNow,
            LastOpenedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RoadmapDto> AcceptCareerRoadmapAsync(string userId, string careerKey, string? careerTitle = null)
    {
        if (string.IsNullOrWhiteSpace(careerKey))
            throw new ArgumentException("Career key is required.", nameof(careerKey));

        var normalizedKey = NormalizeCareerKey(careerKey);
        var resolvedTitle = await ResolveCareerTitleAsync(normalizedKey, careerTitle);

        await SaveUserRoadmapAsync(userId, normalizedKey, resolvedTitle);
        await SetDefaultUserRoadmapAsync(userId, normalizedKey);

        // Also update or record in UserCareerProfiles so reverse career switch is reflected across user's profile
        try
        {
            var profile = await _context.UserCareerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                _context.UserCareerProfiles.Add(new UserCareerProfile
                {
                    UserId = userId,
                    RecommendedCareerId = normalizedKey,
                    ConfidenceScore = 90.0,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                profile.RecommendedCareerId = normalizedKey;
            }
            await _context.SaveChangesAsync();
        }
        catch { }

        return await GetRoadmapByDomainAsync(userId, normalizedKey);
    }

    private async Task<string> ResolveCareerTitleAsync(string careerKey, string? suppliedTitle)
    {
        if (!string.IsNullOrWhiteSpace(suppliedTitle)) return suppliedTitle.Trim();

        var normalizedKey = NormalizeCareerKey(careerKey);

        var blueprint = await _context.RoadmapBlueprints.FirstOrDefaultAsync(b => b.CareerKey == normalizedKey || b.CareerKey == careerKey);
        if (blueprint != null && !string.IsNullOrWhiteSpace(blueprint.CareerTitle)) return blueprint.CareerTitle;

        if (_careerIntelligence != null)
        {
            var rawId = "career_" + normalizedKey.Replace('-', '_');
            var ciCareer = await _careerIntelligence.GetCareerByIdAsync(careerKey)
                        ?? await _careerIntelligence.GetCareerByIdAsync(rawId)
                        ?? await _careerIntelligence.GetCareerByIdAsync(normalizedKey);
            if (ciCareer != null && !string.IsNullOrWhiteSpace(ciCareer.Name))
                return ciCareer.Name;
        }

        var path = await _context.CareerPaths.FindAsync(normalizedKey) ?? await _context.CareerPaths.FindAsync(careerKey);
        if (path != null) return path.Title;
        var node = await _context.CareerTaxonomyV2.FindAsync(normalizedKey) ?? await _context.CareerTaxonomyV2.FindAsync(careerKey);
        if (node != null) return node.Name;
        var domain = await _context.CareerDomains.FindAsync(normalizedKey) ?? await _context.CareerDomains.FindAsync(careerKey);
        if (domain != null) return domain.Name;

        var disp = CareerDisplayNameService.GetDisplayName(normalizedKey);
        if (!string.IsNullOrWhiteSpace(disp) && disp != "Unknown Career") return disp;

        return System.Globalization.CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(normalizedKey.Replace('-', ' ').Replace('_', ' '));
    }

    public static string NormalizeCareerKey(string careerKey)
    {
        var cleaned = (careerKey ?? string.Empty).Trim().ToLowerInvariant()
            .Replace(" ", "-").Replace("/", "-").Replace("_", "-");
        if (cleaned.StartsWith("career-"))
        {
            cleaned = cleaned["career-".Length..];
        }
        return cleaned;
    }

    public async Task<bool> RemoveUserRoadmapAsync(string userId, string careerKey)
    {
        var normalizedKey = NormalizeCareerKey(careerKey);
        var altKey = normalizedKey.Contains('-') ? normalizedKey.Replace('-', '_') : normalizedKey.Replace('_', '-');
        var careerPrefixed = "career-" + normalizedKey;
        var careerUnderline = "career_" + normalizedKey.Replace('-', '_');

        var saved = await _context.UserSavedRoadmaps
            .FirstOrDefaultAsync(u => u.UserId == userId && (u.CareerKey == normalizedKey || u.CareerKey == altKey || u.CareerKey == careerKey || u.CareerKey == careerPrefixed || u.CareerKey == careerUnderline));

        if (saved == null) return false;

        bool wasDefault = saved.IsDefault;
        _context.UserSavedRoadmaps.Remove(saved);
        try
        {
            var existingDismissal = await _context.UserRoadmapDismissals
                .FirstOrDefaultAsync(d => d.UserId == userId && (d.CareerKey == normalizedKey || d.CareerKey == altKey || d.CareerKey == careerKey || d.CareerKey == careerPrefixed || d.CareerKey == careerUnderline));
            if (existingDismissal == null)
            {
                _context.UserRoadmapDismissals.Add(new UserRoadmapDismissal
                {
                    UserId = userId,
                    CareerKey = normalizedKey,
                    DismissedAt = DateTime.UtcNow
                });
            }
        }
        catch { }
        await _context.SaveChangesAsync();

        if (wasDefault)
        {
            var nextDefault = await _context.UserSavedRoadmaps
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.LastOpenedAt)
                .FirstOrDefaultAsync();

            if (nextDefault != null)
            {
                nextDefault.IsDefault = true;
                await _context.SaveChangesAsync();
            }
        }

        return true;
    }

    public async Task<bool> SetDefaultUserRoadmapAsync(string userId, string careerKey)
    {
        var normalizedKey = NormalizeCareerKey(careerKey);
        var altKey = normalizedKey.Contains('-') ? normalizedKey.Replace('-', '_') : normalizedKey.Replace('_', '-');
        var careerPrefixed = "career-" + normalizedKey;
        var careerUnderline = "career_" + normalizedKey.Replace('-', '_');

        var allUserSaved = await _context.UserSavedRoadmaps
            .Where(u => u.UserId == userId)
            .ToListAsync();

        var target = allUserSaved.FirstOrDefault(s => s.CareerKey == normalizedKey || s.CareerKey == altKey || s.CareerKey == careerKey || s.CareerKey == careerPrefixed || s.CareerKey == careerUnderline);
        if (target == null) return false;

        foreach (var s in allUserSaved)
        {
            s.IsDefault = (s.Id == target.Id);
            if (s.Id == target.Id) s.LastOpenedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkStepCompleteAsync(string userId, string stepId)
    {
        var isStandardRoadmapModule = await _context.RoadmapModules
            .AsNoTracking()
            .AnyAsync(module => module.Id == stepId);

        if (isStandardRoadmapModule)
        {
            // Standard roadmap module: write to UserCustomModuleProgressions
            // (UserModuleProgressions has a FK to RoadmapSteps which is a legacy table
            //  and does NOT contain the new RoadmapModules IDs — inserting there would violate the FK)
            var customProgress = await _context.UserCustomModuleProgressions
                .FirstOrDefaultAsync(ump => ump.UserId == userId && ump.ModuleId == stepId);

            if (customProgress == null)
            {
                _context.UserCustomModuleProgressions.Add(new UserCustomModuleProgress
                {
                    UserId = userId,
                    ModuleId = stepId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow,
                    EstimatedHours = 0
                });
            }
            else
            {
                customProgress.IsCompleted = true;
                customProgress.CompletedAt = DateTime.UtcNow;
            }
        }
        else
        {
            // Legacy / custom AI roadmap module: write to UserModuleProgressions
            // (these IDs do exist in RoadmapSteps so the FK is satisfied)
            var moduleProgress = await _context.UserModuleProgressions
                .FirstOrDefaultAsync(ump => ump.UserId == userId && ump.ModuleId == stepId);

            if (moduleProgress == null)
            {
                _context.UserModuleProgressions.Add(new UserModuleProgress
                {
                    UserId = userId,
                    ModuleId = stepId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                });
            }
            else
            {
                moduleProgress.IsCompleted = true;
                moduleProgress.CompletedAt = DateTime.UtcNow;
            }
        }

        // Award XP and Badge Progression
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            user.TotalXP += 100;
            int newLevel = (int)(user.TotalXP / 100) + 1;
            if (newLevel > user.Level) user.Level = newLevel;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
