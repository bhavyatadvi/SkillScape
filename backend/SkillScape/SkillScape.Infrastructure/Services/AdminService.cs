using Microsoft.EntityFrameworkCore;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;
using System.Text.Json;

namespace SkillScape.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly IQuestionValidator _questionValidator;

    public AdminService(ApplicationDbContext context, IQuestionValidator questionValidator)
    {
        _context = context;
        _questionValidator = questionValidator;
    }

    public async Task<AdminDashboardStatsDto> GetDashboardAsync()
    {
        var totalUsers = await _context.Users.CountAsync();
        var activeStudents = await _context.Users.CountAsync(u => u.Role == "Student" && !u.IsBlocked);

        return new AdminDashboardStatsDto
        {
            TotalUsers = totalUsers,
            ActiveStudents = activeStudents,
        };
    }

    public async Task<List<AdminUserDto>> GetUsersAsync(string? role, bool? isBlocked)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);

        if (isBlocked.HasValue)
            query = query.Where(u => u.IsBlocked == isBlocked.Value);

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role,
                IsBlocked = u.IsBlocked,
                BlockedReason = u.BlockedReason,
                ProfileCompleted = u.ProfileCompleted,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task BlockOrUnblockUserAsync(string adminId, BlockUserDto request)
    {
        var user = await _context.Users.FindAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found");

        if (user.Role == "Admin" && request.IsBlocked)
            throw new InvalidOperationException("Admin account cannot be blocked");

        user.IsBlocked = request.IsBlocked;
        user.BlockedReason = request.IsBlocked ? request.Reason : null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await AddAuditLog(adminId, request.IsBlocked ? "BlockUser" : "UnblockUser", "User", user.Id);
    }

    public async Task UpdateUserRoleAsync(string adminId, UpdateUserRoleDto request)
    {
        var user = await _context.Users.FindAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found");

        var normalizedRole = request.Role?.Trim();
        if (normalizedRole is not ("Student" or "Admin"))
            throw new InvalidOperationException("Role must be Student or Admin");

        user.Role = normalizedRole;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await AddAuditLog(adminId, "UpdateUserRole", "User", user.Id);
    }

    public async Task<PlatformAnalyticsDto> GetAnalyticsAsync()
    {
        // Materialize the compact quiz-result projection before grouping. This avoids
        // provider-specific translation failures in the admin dashboard's aggregate queries.
        var quizResultRows = await _context.QuizResults
            .AsNoTracking()
            .Select(result => new { result.RecommendedDomainId, result.ScoresJson })
            .ToListAsync();

        var legacyDomainCounts = quizResultRows
            .Where(result => !string.IsNullOrWhiteSpace(result.RecommendedDomainId))
            .GroupBy(result => result.RecommendedDomainId!)
            .Select(group => new { DomainId = group.Key, Count = group.Count() })
            .ToList();
        var selectedDomainIds = legacyDomainCounts.Select(item => item.DomainId).ToList();
        var selectedDomainNames = await _context.CareerDomains
            .Where(domain => selectedDomainIds.Contains(domain.Id))
            .ToDictionaryAsync(domain => domain.Id, domain => domain.Name);
        var adaptivePathCounts = await _context.QuizResultsV2
            .AsNoTracking()
            .Where(result => result.PrimaryTaxonomyNodeId != null && result.PrimaryTaxonomyNodeId != "")
            .GroupBy(result => result.PrimaryTaxonomyNodeId)
            .Select(group => new { NodeId = group.Key, Count = group.Count() })
            .ToListAsync();
        var adaptiveNodeIds = adaptivePathCounts.Select(item => item.NodeId).ToList();
        var adaptiveNodeNames = await _context.CareerTaxonomyV2
            .Where(node => adaptiveNodeIds.Contains(node.Id))
            .ToDictionaryAsync(node => node.Id, node => node.Name);
        var pathCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in legacyDomainCounts)
        {
            var label = selectedDomainNames.GetValueOrDefault(item.DomainId, item.DomainId);
            pathCounts[label] = pathCounts.GetValueOrDefault(label) + item.Count;
        }
        foreach (var item in adaptivePathCounts)
        {
            var label = adaptiveNodeNames.GetValueOrDefault(item.NodeId, item.NodeId);
            pathCounts[label] = pathCounts.GetValueOrDefault(label) + item.Count;
        }
        var careerDomainSelectionCounts = pathCounts
            .Select(item => new { Label = item.Key, Count = item.Value })
            .OrderByDescending(item => item.Count)
            .ToList();
        var mostSelectedPathName = careerDomainSelectionCounts.FirstOrDefault()?.Label ?? "N/A";

        var answeredQuestionCategories = await _context.QuizQuestions
            .AsNoTracking()
            .Where(question => question.Category != null)
            .Select(question => new { question.Category, AnswerCount = question.Responses.Count() })
            .ToListAsync();
        var leastAnsweredCategory = answeredQuestionCategories
            .Where(question => question.AnswerCount > 0)
            .GroupBy(question => question.Category)
            .Select(group => new { Category = group.Key, Answers = group.Sum(question => question.AnswerCount) })
            .OrderBy(group => group.Answers)
            .Select(group => group.Category)
            .FirstOrDefault();

        var parsedScores = new List<double>();
        foreach (var json in quizResultRows.Select(result => result.ScoresJson).Where(json => !string.IsNullOrWhiteSpace(json)))
        {
            try
            {
                var scoreMap = JsonSerializer.Deserialize<Dictionary<string, double>>(json!);
                if (scoreMap != null && scoreMap.Count > 0)
                {
                    parsedScores.Add(scoreMap.Values.Average());
                }
            }
            catch
            {
                // Ignore malformed score json entries
            }
        }

        var averageQuizScore = parsedScores.Count > 0
            ? Math.Round(Math.Clamp(parsedScores.Average(), 0, 100), 1)
            : 0.0;

        var today = DateTime.UtcNow.Date;
        var thirtyDaysAgo = today.AddDays(-30);

        var dailySignups = await _context.Users.CountAsync(u => u.CreatedAt >= today);
        var activeUsersLast30Days = await _context.Users.CountAsync(u => u.UpdatedAt >= thirtyDaysAgo);
        
        // Quiz attempts are persisted by two engines. Count completed results from
        // both stores so the admin totals agree with student quiz history.
        var legacyQuizCount = await _context.QuizResults.CountAsync();
        var adaptiveQuizCount = await _context.QuizResultsV2.CountAsync();
        var totalQuizzesTaken = legacyQuizCount + adaptiveQuizCount;
        var totalActiveDomains = await _context.CareerDomains.CountAsync(domain => domain.IsActive);
        var totalQuestionsInBank =
            await _context.QuizQuestions.CountAsync(question => question.IsActive) +
            await _context.QuizQuestionsV2.CountAsync(question => question.IsActive);

        return new PlatformAnalyticsDto
        {
            MostSelectedCareerPath = mostSelectedPathName,
            WeakestQuizCategory = leastAnsweredCategory ?? "N/A",
            AverageQuizScore = averageQuizScore,
            DailySignups = dailySignups,
            ActiveUsersLast30Days = activeUsersLast30Days,
            TotalQuizzesTaken = totalQuizzesTaken,
            TotalActiveDomains = totalActiveDomains,
            TotalQuestionsInBank = totalQuestionsInBank,
            CareerDomainSelections = careerDomainSelectionCounts.Select(item => new AdminAnalyticsSliceDto
            {
                Label = item.Label,
                Value = item.Count
            }).ToList(),
        };
    }

    public async Task<List<AdminUserProgressSnapshotDto>> GetProgressSnapshotsAsync()
    {
        var users = await _context.Users
            .AsNoTracking()
            .Select(user => new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.UpdatedAt,
                CompletedSkills = user.UserSkills.Count(skill => skill.IsCompleted)
            })
            .ToListAsync();

        var legacyCounts = await _context.QuizResults
            .AsNoTracking()
            .GroupBy(result => result.UserId)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.UserId, item => item.Count);
        var adaptiveCounts = await _context.QuizResultsV2
            .AsNoTracking()
            .GroupBy(result => result.UserId)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.UserId, item => item.Count);

        return users.Select(user => new AdminUserProgressSnapshotDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                QuizzesTaken = legacyCounts.GetValueOrDefault(user.Id) + adaptiveCounts.GetValueOrDefault(user.Id),
                CompletedSkills = user.CompletedSkills,
                AverageDomainProgress = 0, // No per-domain progress source is currently persisted.
                UpdatedAt = user.UpdatedAt
            })
            .OrderByDescending(user => user.QuizzesTaken)
            .ThenByDescending(user => user.CompletedSkills)
            .ToList();
    }

    public async Task<List<AdminQuizQuestionCrudDto>> GetQuizQuestionsAsync()
    {
        var questions = await _context.QuizQuestions
            .AsNoTracking()
            .Include(q => q.CareerDomain)
            .Include(q => q.Options)
            .OrderBy(q => q.DisplayOrder)
            .ToListAsync();

        return questions.Select(MapQuizQuestion).ToList();
    }

    public async Task<AdminQuizQuestionCrudDto> CreateQuizQuestionAsync(string adminId, UpsertAdminQuizQuestionDto request)
    {
        ValidateQuizQuestionRequest(request);

        var domain = await _context.CareerDomains.FindAsync(request.CareerDomainId)
            ?? throw new InvalidOperationException("Career domain not found");

        var question = new QuizQuestion
        {
            Id = Guid.NewGuid().ToString(),
            CareerDomainId = request.CareerDomainId,
            Text = request.Text,
            Category = request.Category,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            Options = request.Options.Select(option => new QuizOption
            {
                Id = Guid.NewGuid().ToString(),
                Text = option.Text,
                DisplayOrder = option.DisplayOrder,
                DomainWeightJson = JsonSerializer.Serialize(option.DomainWeights ?? new Dictionary<string, int>())
            }).ToList()
        };

        var validationResult = await _questionValidator.ValidateQuestionAsync(question);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Question validation failed: {validationResult.Reason}");
        }

        _context.QuizQuestions.Add(question);
        await _context.SaveChangesAsync();
        await AddAuditLog(adminId, "CreateQuizQuestion", "QuizQuestion", question.Id);

        question.CareerDomain = domain;
        return MapQuizQuestion(question);
    }

    public async Task<AdminQuizQuestionCrudDto> UpdateQuizQuestionAsync(string adminId, string questionId, UpsertAdminQuizQuestionDto request)
    {
        ValidateQuizQuestionRequest(request);

        var question = await _context.QuizQuestions
            .Include(q => q.CareerDomain)
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId)
            ?? throw new InvalidOperationException("Quiz question not found");

        var domain = await _context.CareerDomains.FindAsync(request.CareerDomainId)
            ?? throw new InvalidOperationException("Career domain not found");

        var hasResponses = await _context.QuizResponses.AnyAsync(response => response.QuizQuestionId == question.Id);
        if (hasResponses)
        {
            var optionsChanged = request.Options.Count != question.Options.Count || request.Options.Any(requestedOption =>
            {
                var existingOption = question.Options.FirstOrDefault(option => option.Id == requestedOption.Id);
                if (existingOption == null || existingOption.Text != requestedOption.Text || existingOption.DisplayOrder != requestedOption.DisplayOrder)
                    return true;

                var existingWeights = ParseWeights(existingOption.DomainWeightJson).OrderBy(pair => pair.Key).ToArray();
                var requestedWeights = (requestedOption.DomainWeights ?? new Dictionary<string, int>()).OrderBy(pair => pair.Key).ToArray();
                return !existingWeights.SequenceEqual(requestedWeights);
            });
            if (optionsChanged)
                throw new InvalidOperationException("This question has saved responses. Its options are locked to preserve quiz history; you can still edit the question text, category, domain, and active status.");
        }

        question.CareerDomainId = request.CareerDomainId;
        question.Text = request.Text;
        question.Category = request.Category;
        question.DisplayOrder = request.DisplayOrder;
        question.IsActive = request.IsActive;

        if (!hasResponses)
        {
            var existingOptions = question.Options.ToDictionary(option => option.Id);
            var retainedIds = new HashSet<string>();
            var updatedOptions = new List<QuizOption>();
            foreach (var optionRequest in request.Options)
            {
                if (!string.IsNullOrWhiteSpace(optionRequest.Id) && existingOptions.TryGetValue(optionRequest.Id, out var existingOption))
                {
                    existingOption.Text = optionRequest.Text;
                    existingOption.DisplayOrder = optionRequest.DisplayOrder;
                    existingOption.DomainWeightJson = JsonSerializer.Serialize(optionRequest.DomainWeights ?? new Dictionary<string, int>());
                    retainedIds.Add(existingOption.Id);
                    updatedOptions.Add(existingOption);
                }
                else
                {
                    var newOption = new QuizOption
                    {
                        Id = Guid.NewGuid().ToString(),
                        QuizQuestionId = question.Id,
                        Text = optionRequest.Text,
                        DisplayOrder = optionRequest.DisplayOrder,
                        DomainWeightJson = JsonSerializer.Serialize(optionRequest.DomainWeights ?? new Dictionary<string, int>())
                    };
                    _context.QuizOptions.Add(newOption);
                    updatedOptions.Add(newOption);
                }
            }

            _context.QuizOptions.RemoveRange(existingOptions.Values.Where(option => !retainedIds.Contains(option.Id)));
            question.Options = updatedOptions;
        }

        var validationResult = await _questionValidator.ValidateQuestionAsync(question);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Question validation failed: {validationResult.Reason}");
        }

        await _context.SaveChangesAsync();
        await AddAuditLog(adminId, "UpdateQuizQuestion", "QuizQuestion", question.Id);

        question.CareerDomain = domain;
        return MapQuizQuestion(question);
    }

    public async Task DeleteQuizQuestionAsync(string adminId, string questionId)
    {
        var question = await _context.QuizQuestions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId)
            ?? throw new InvalidOperationException("Quiz question not found");

        if (await _context.QuizResponses.AnyAsync(response => response.QuizQuestionId == questionId))
            throw new InvalidOperationException("This question has saved responses and cannot be deleted. Set it inactive to remove it from quizzes while preserving history.");

        _context.QuizQuestions.Remove(question);
        await _context.SaveChangesAsync();
        await AddAuditLog(adminId, "DeleteQuizQuestion", "QuizQuestion", questionId);
    }

    public async Task<List<AdminRoadmapModuleCrudDto>> GetRoadmapModulesAsync(string? domainId)
    {
        var query = _context.RoadmapSteps
            .Include(r => r.CareerDomain)
            .Include(r => r.Skill)
            .Include(r => r.Topics)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(domainId))
            query = query.Where(r => r.CareerDomainId == domainId);

        var modules = await query
            .OrderBy(r => r.CareerDomain!.DisplayOrder)
            .ThenBy(r => r.Skill!.DisplayOrder)
            .ThenBy(r => r.StepNumber)
            .ToListAsync();

        return modules.Select(MapRoadmapModule).ToList();
    }

    public async Task<AdminRoadmapModuleCrudDto> CreateRoadmapModuleAsync(string adminId, UpsertAdminRoadmapModuleDto request)
    {
        var domain = await _context.CareerDomains.FindAsync(request.CareerDomainId)
            ?? throw new InvalidOperationException("Career domain not found");

        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == request.SkillId && s.CareerDomainId == request.CareerDomainId)
            ?? throw new InvalidOperationException("Skill not found for selected domain");

        var module = new RoadmapStep
        {
            Id = Guid.NewGuid().ToString(),
            CareerDomainId = request.CareerDomainId,
            SkillId = request.SkillId,
            Title = request.Title,
            Description = request.Description,
            StepNumber = request.StepNumber,
            EstimatedHours = request.EstimatedHours,
            IsActive = request.IsActive,
            Topics = request.Topics.Select(topic => new RoadmapTopic
            {
                Id = Guid.NewGuid().ToString(),
                Title = topic.Title,
                ResourceUrl = topic.ResourceUrl,
                DisplayOrder = topic.DisplayOrder
            }).ToList()
        };

        _context.RoadmapSteps.Add(module);
        await _context.SaveChangesAsync();
        await AddAuditLog(adminId, "CreateRoadmapModule", "RoadmapStep", module.Id);

        module.CareerDomain = domain;
        module.Skill = skill;
        return MapRoadmapModule(module);
    }

    public async Task<AdminRoadmapModuleCrudDto> UpdateRoadmapModuleAsync(string adminId, string moduleId, UpsertAdminRoadmapModuleDto request)
    {
        var module = await _context.RoadmapSteps
            .Include(r => r.CareerDomain)
            .Include(r => r.Skill)
            .Include(r => r.Topics)
            .FirstOrDefaultAsync(r => r.Id == moduleId)
            ?? throw new InvalidOperationException("Roadmap module not found");

        var domain = await _context.CareerDomains.FindAsync(request.CareerDomainId)
            ?? throw new InvalidOperationException("Career domain not found");

        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == request.SkillId && s.CareerDomainId == request.CareerDomainId)
            ?? throw new InvalidOperationException("Skill not found for selected domain");

        module.CareerDomainId = request.CareerDomainId;
        module.SkillId = request.SkillId;
        module.Title = request.Title;
        module.Description = request.Description;
        module.StepNumber = request.StepNumber;
        module.EstimatedHours = request.EstimatedHours;
        module.IsActive = request.IsActive;

        var existingTopics = module.Topics.ToDictionary(topic => topic.Id);
        var retainedTopicIds = new HashSet<string>();
        var updatedTopics = new List<RoadmapTopic>();
        foreach (var topicRequest in request.Topics)
        {
            if (!string.IsNullOrWhiteSpace(topicRequest.Id) && existingTopics.TryGetValue(topicRequest.Id, out var existingTopic))
            {
                existingTopic.Title = topicRequest.Title;
                existingTopic.ResourceUrl = topicRequest.ResourceUrl;
                existingTopic.DisplayOrder = topicRequest.DisplayOrder;
                retainedTopicIds.Add(existingTopic.Id);
                updatedTopics.Add(existingTopic);
            }
            else
            {
                var newTopic = new RoadmapTopic
                {
                    Id = Guid.NewGuid().ToString(),
                    ModuleId = module.Id,
                    Title = topicRequest.Title,
                    ResourceUrl = topicRequest.ResourceUrl,
                    DisplayOrder = topicRequest.DisplayOrder
                };
                _context.RoadmapTopics.Add(newTopic);
                updatedTopics.Add(newTopic);
            }
        }

        _context.RoadmapTopics.RemoveRange(existingTopics.Values.Where(topic => !retainedTopicIds.Contains(topic.Id)));
        module.Topics = updatedTopics;

        await _context.SaveChangesAsync();
        await AddAuditLog(adminId, "UpdateRoadmapModule", "RoadmapStep", module.Id);

        module.CareerDomain = domain;
        module.Skill = skill;
        return MapRoadmapModule(module);
    }

    public async Task DeleteRoadmapModuleAsync(string adminId, string moduleId)
    {
        var module = await _context.RoadmapSteps.FindAsync(moduleId)
            ?? throw new InvalidOperationException("Roadmap module not found");

        var hasLearnerProgress = await _context.UserModuleProgressions.AnyAsync(progress => progress.ModuleId == moduleId);
        var hasDependentModules = await _context.RoadmapSteps.AnyAsync(step => step.PrerequisiteStepId == moduleId);
        if (hasLearnerProgress || hasDependentModules)
            throw new InvalidOperationException("This module has learner progress or prerequisite links and cannot be deleted. Set it inactive instead to preserve roadmap history.");

        _context.RoadmapSteps.Remove(module);
        await _context.SaveChangesAsync();
        await AddAuditLog(adminId, "DeleteRoadmapModule", "RoadmapStep", moduleId);
    }

    // --- Hierarchical Questions ---
    
    public async Task<List<HierarchicalQuizQuestionDto>> GetHierarchicalQuestionsAsync()
    {
        var questions = await _context.HierarchicalQuizQuestions
            .AsNoTracking()
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
            
        return questions.Select(q => new HierarchicalQuizQuestionDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            TargetCareerTags = q.TargetCareerTags,
            HierarchyLevel = q.HierarchyLevel,
            StreamType = q.StreamType,
            AptitudeType = q.AptitudeType,
            Options = string.IsNullOrEmpty(q.OptionsJson) ? new List<HierarchicalQuizOptionDto>() : System.Text.Json.JsonSerializer.Deserialize<List<HierarchicalQuizOptionDto>>(q.OptionsJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!
        }).ToList();
    }

    public async Task<HierarchicalQuizQuestionDto> CreateHierarchicalQuestionAsync(string adminId, HierarchicalQuizQuestionDto request)
    {
        ValidateHierarchicalQuestionRequest(request);

        var entity = new HierarchicalQuizQuestion
        {
            Id = Guid.NewGuid().ToString(),
            QuestionText = request.QuestionText.Trim(),
            TargetCareerTags = request.TargetCareerTags ?? "",
            HierarchyLevel = request.HierarchyLevel,
            StreamType = request.StreamType ?? "",
            AptitudeType = request.AptitudeType ?? "",
            OptionsJson = System.Text.Json.JsonSerializer.Serialize(request.Options),
            CreatedAt = DateTime.UtcNow
        };
        
        _context.HierarchicalQuizQuestions.Add(entity);
        await _context.SaveChangesAsync();
        await AddAuditLog(adminId, "CreateHierarchicalQuestion", "HierarchicalQuizQuestion", entity.Id);
        
        request.Id = entity.Id;
        return request;
    }

    public async Task<HierarchicalQuizQuestionDto> UpdateHierarchicalQuestionAsync(string adminId, string questionId, HierarchicalQuizQuestionDto request)
    {
        ValidateHierarchicalQuestionRequest(request);

        var entity = await _context.HierarchicalQuizQuestions.FirstOrDefaultAsync(q => q.Id == questionId);
        if (entity == null) throw new InvalidOperationException("Question not found.");
        
        entity.QuestionText = request.QuestionText.Trim();
        entity.TargetCareerTags = request.TargetCareerTags ?? "";
        entity.HierarchyLevel = request.HierarchyLevel;
        entity.StreamType = request.StreamType ?? "";
        entity.AptitudeType = request.AptitudeType ?? "";
        entity.OptionsJson = System.Text.Json.JsonSerializer.Serialize(request.Options);
        
        _context.HierarchicalQuizQuestions.Update(entity);
        await _context.SaveChangesAsync();
        await AddAuditLog(adminId, "UpdateHierarchicalQuestion", "HierarchicalQuizQuestion", questionId);
        
        return request;
    }

    public async Task DeleteHierarchicalQuestionAsync(string adminId, string questionId)
    {
        var entity = await _context.HierarchicalQuizQuestions.FirstOrDefaultAsync(q => q.Id == questionId);
        if (entity == null) throw new InvalidOperationException("Question not found.");
        
        _context.HierarchicalQuizQuestions.Remove(entity);
        await _context.SaveChangesAsync();
        await AddAuditLog(adminId, "DeleteHierarchicalQuestion", "HierarchicalQuizQuestion", questionId);
    }

    private AdminQuizQuestionCrudDto MapQuizQuestion(QuizQuestion question)
    {
        return new AdminQuizQuestionCrudDto
        {
            Id = question.Id,
            CareerDomainId = question.CareerDomainId,
            CareerDomainName = question.CareerDomain?.Name ?? string.Empty,
            Text = question.Text,
            Category = question.Category,
            DisplayOrder = question.DisplayOrder,
            IsActive = question.IsActive,
            Options = question.Options
                .OrderBy(option => option.DisplayOrder)
                .Select(option => new AdminQuizOptionCrudDto
                {
                    Id = option.Id,
                    Text = option.Text,
                    DisplayOrder = option.DisplayOrder,
                    DomainWeights = ParseWeights(option.DomainWeightJson)
                })
                .ToList()
        };
    }

    private static void ValidateQuizQuestionRequest(UpsertAdminQuizQuestionDto request)
    {
        if (request == null)
            throw new InvalidOperationException("Question details are required.");

        request.Text = request.Text?.Trim() ?? string.Empty;
        request.Category = request.Category?.Trim() ?? string.Empty;
        if (request.Text.Length == 0 || request.Text.Length > 2000)
            throw new InvalidOperationException("Question text must be between 1 and 2000 characters.");

        var options = request.Options?
            .OfType<AdminQuizOptionCrudDto>()
            .Where(option => !string.IsNullOrWhiteSpace(option.Text))
            .Select(option =>
            {
                option.Text = option.Text.Trim();
                return option;
            })
            .ToList() ?? new List<AdminQuizOptionCrudDto>();
        if (options.Count < 2)
            throw new InvalidOperationException("At least two non-empty quiz options are required.");
        if (options.Any(option => option.Text.Length > 500))
            throw new InvalidOperationException("Quiz options cannot exceed 500 characters.");

        request.Options = options;
    }

    private static void ValidateHierarchicalQuestionRequest(HierarchicalQuizQuestionDto request)
    {
        if (request == null)
            throw new InvalidOperationException("Question details are required.");

        request.QuestionText = request.QuestionText?.Trim() ?? string.Empty;
        if (request.QuestionText.Length == 0 || request.QuestionText.Length > 2000)
            throw new InvalidOperationException("Question text must be between 1 and 2000 characters.");
        if (request.HierarchyLevel < 1 || request.HierarchyLevel > 20)
            throw new InvalidOperationException("Hierarchy level must be between 1 and 20.");

        var options = request.Options?
            .OfType<HierarchicalQuizOptionDto>()
            .Where(option => !string.IsNullOrWhiteSpace(option.Text))
            .Select(option =>
            {
                option.Text = option.Text.Trim();
                return option;
            })
            .ToList() ?? new List<HierarchicalQuizOptionDto>();
        if (options.Count < 2)
            throw new InvalidOperationException("At least two non-empty answer options are required.");
        if (options.Any(option => option.Text.Length > 500))
            throw new InvalidOperationException("Answer options cannot exceed 500 characters.");

        request.Options = options;
    }

    private AdminRoadmapModuleCrudDto MapRoadmapModule(RoadmapStep module)
    {
        return new AdminRoadmapModuleCrudDto
        {
            Id = module.Id,
            CareerDomainId = module.CareerDomainId,
            CareerDomainName = module.CareerDomain?.Name ?? string.Empty,
            SkillId = module.SkillId,
            SkillName = module.Skill?.Name ?? string.Empty,
            Title = module.Title,
            Description = module.Description,
            StepNumber = module.StepNumber,
            EstimatedHours = module.EstimatedHours,
            IsActive = module.IsActive,
            Topics = module.Topics
                .OrderBy(topic => topic.DisplayOrder)
                .Select(topic => new AdminRoadmapTopicCrudDto
                {
                    Id = topic.Id,
                    Title = topic.Title,
                    ResourceUrl = topic.ResourceUrl,
                    DisplayOrder = topic.DisplayOrder
                })
                .ToList()
        };
    }

    private Dictionary<string, int> ParseWeights(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, int>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
        }
        catch
        {
            return new Dictionary<string, int>();
        }
    }

    private async Task AddAuditLog(string adminId, string action, string targetEntity, string? targetEntityId)
    {
        _context.AdminAuditLogs.Add(new AdminAuditLog
        {
            Id = Guid.NewGuid().ToString(),
            AdminId = adminId,
            Action = action,
            TargetEntity = targetEntity,
            TargetEntityId = targetEntityId,
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task<List<object>> GetPeerRoomsAsync()
    {
        var circles = await _context.LearningCircles
            .Include(c => c.Members)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
            
        return circles.Select(c => new 
        {
            id = c.Id,
            name = c.Name,
            description = c.Description,
            careerDomain = c.CareerDomain,
            memberCount = c.Members.Count,
            isPublic = c.IsPublic,
            isDisabledByAdmin = c.IsDisabledByAdmin,
            adminDisabledReason = c.AdminDisabledReason,
            createdAt = c.CreatedAt,
            isMeetingActive = c.IsMeetingActive
        }).ToList<object>();
    }

    public async Task<object> GetPeerRoomDetailsAsync(string roomId)
    {
        var circle = await _context.LearningCircles
            .Include(c => c.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.Id == roomId);
            
        if (circle == null) return null;
        
        return new 
        {
            id = circle.Id,
            name = circle.Name,
            description = circle.Description,
            careerDomain = circle.CareerDomain,
            circleType = circle.CircleType,
            memberCount = circle.Members.Count,
            isPublic = circle.IsPublic,
            isDisabledByAdmin = circle.IsDisabledByAdmin,
            adminDisabledReason = circle.AdminDisabledReason,
            createdAt = circle.CreatedAt,
            isMeetingActive = circle.IsMeetingActive,
            members = circle.Members.Select(m => new {
                userId = m.UserId,
                name = m.User?.FullName ?? "Unknown",
                role = m.Role,
                joinedAt = m.JoinedAt
            }).ToList()
        };
    }

    public async Task SetPeerRoomDisabledAsync(string roomId, bool isDisabled, string? reason)
    {
        var circle = await _context.LearningCircles.FirstOrDefaultAsync(c => c.Id == roomId);
        if (circle == null) throw new InvalidOperationException("Peer group not found.");
        circle.IsDisabledByAdmin = isDisabled;
        circle.AdminDisabledReason = isDisabled ? (string.IsNullOrWhiteSpace(reason) ? "Disabled by admin" : reason.Trim()) : null;
        circle.IsMeetingActive = isDisabled ? false : circle.IsMeetingActive;
        circle.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
