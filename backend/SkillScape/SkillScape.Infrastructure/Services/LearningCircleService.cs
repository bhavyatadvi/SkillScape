using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class LearningCircleService : ILearningCircleService
{
    private readonly ApplicationDbContext _context;
    private readonly IOpenAIService _aiService;
    public LearningCircleService(ApplicationDbContext context, IOpenAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<bool> HasCompletedCareerQuizAsync(string userId)
    {
        var profile = await _context.UserCareerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile != null &&
            (!string.IsNullOrWhiteSpace(profile.RecommendedCareerId) || HasMeaningfulQuizAnswers(profile.QuizAnswersJson)))
            return true;

        var hasCareerProfile = await _context.CareerProfiles.AnyAsync(p => p.UserId == userId &&
            (p.BroadField != "" || p.Specialization != "" || p.CareerGoals != "" || p.CurrentRole != "" || p.CurrentIndustry != ""));
        if (hasCareerProfile) return true;
            
        var hasSavedRoadmap = await _context.UserSavedRoadmaps.AnyAsync(r => r.UserId == userId);
        return hasSavedRoadmap;
    }

    public async Task<List<CircleRecommendationDto>> GetRecommendedCirclesAsync(string userId)
    {
        var profile = await _context.UserCareerProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        var hasSavedRoadmap = await _context.UserSavedRoadmaps.AnyAsync(r => r.UserId == userId);

        var hasCareerProfile = await _context.CareerProfiles.AnyAsync(p => p.UserId == userId &&
            (p.BroadField != "" || p.Specialization != "" || p.CareerGoals != "" || p.CurrentRole != "" || p.CurrentIndustry != ""));
        if (!hasSavedRoadmap && !hasCareerProfile && (profile == null ||
            (string.IsNullOrWhiteSpace(profile.RecommendedCareerId) && !HasMeaningfulQuizAnswers(profile.QuizAnswersJson))))
        {
            // If the user hasn't completed the quiz, return empty for recommendations
            return new List<CircleRecommendationDto>();
        }

        var user = await _context.Users.FindAsync(userId);
        
        var latestRoadmap = await _context.UserSavedRoadmaps
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        // Prefer populated data from every profile source. Roadmaps and profile columns can
        // exist with empty strings, so null-coalescing alone can hide valid fallback values.
        var careerProfile = await _context.CareerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        var careerPath = !string.IsNullOrWhiteSpace(profile?.RecommendedCareerId)
            ? await _context.CareerPaths.FirstOrDefaultAsync(p => p.Id == profile.RecommendedCareerId)
            : null;
        var profileSignals = string.Join(" ", new[]
        {
            latestRoadmap?.CareerTitle,
            careerPath?.Title,
            careerPath?.Description,
            careerPath?.Tags,
            careerPath?.TaxonomyPath,
            careerProfile?.BroadField,
            careerProfile?.Specialization,
            careerProfile?.CurrentOccupation,
            careerProfile?.CurrentRole,
            careerProfile?.CurrentIndustry,
            careerProfile?.CareerGoals,
            careerProfile?.CareerLikesJson,
            careerProfile?.MotivationsJson,
            careerProfile?.CurrentSkillsJson,
            user?.LearningDomain,
            user?.LearningGoals,
            profile?.WhyRecommended
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var learningDomain = FirstNonEmpty(latestRoadmap?.CareerTitle, careerPath?.Title,
            careerProfile?.Specialization, careerProfile?.BroadField, user?.LearningDomain);

        var allCircles = await _context.LearningCircles
            .Where(c => c.IsPublic && !c.IsDisabledByAdmin && _context.Users.Any(user => user.Id == c.CreatorId))
            .ToListAsync();
        var recommendations = new List<CircleRecommendationDto>();

        if (allCircles.Count == 0) return recommendations;

        // Recommendations must still work when an optional AI provider is unavailable.
        // Rank the public circles locally using the user's target domain and stated goals.
        var userTerms = GetCircleMatchTerms(profileSignals);
        var targetDomain = NormalizeCircleText(learningDomain);
        var rankedCircles = allCircles
            .Select(circle =>
            {
                var domainText = NormalizeCircleText(circle.CareerDomain);
                var nameAndTags = GetCircleMatchTerms($"{circle.Name} {circle.Tags} {circle.CircleType}");
                var descriptionTerms = GetCircleMatchTerms(circle.Description);
                var domainTerms = GetCircleMatchTerms(circle.CareerDomain);

                var domainOverlap = domainTerms.Intersect(userTerms).Count();
                var nameOverlap = nameAndTags.Intersect(userTerms).Count();
                var descriptionOverlap = descriptionTerms.Intersect(userTerms).Count();
                var exactDomainMatch = !string.IsNullOrWhiteSpace(targetDomain) && !string.IsNullOrWhiteSpace(domainText) &&
                    (domainText.Contains(targetDomain) || targetDomain.Contains(domainText));

                var score = (exactDomainMatch ? 55 : 0) +
                    Math.Min(domainOverlap * 12, 36) +
                    Math.Min(nameOverlap * 10, 30) +
                    Math.Min(descriptionOverlap * 3, 12);

                return new { Circle = circle, Score = Math.Min(score, 98), DomainOverlap = domainOverlap };
            })
            .Where(match => match.Score >= 18)
            .OrderByDescending(match => match.Score)
            .ThenByDescending(match => match.Circle.MemberCount)
            .Take(3)
            .ToList();

        foreach (var match in rankedCircles)
        {
            var targetLabel = string.IsNullOrWhiteSpace(learningDomain) ? "learning goals" : $"{learningDomain} career goal";
            var reason = match.DomainOverlap > 0 || match.Score >= 55
                ? $"Related to your {targetLabel}."
                : "Matches skills or interests in your learning goals.";

            recommendations.Add(new CircleRecommendationDto
            {
                Circle = await MapToDtoAsync(match.Circle, userId),
                CompatibilityScore = match.Score,
                Reason = reason
            });
        }

        return recommendations;
    }

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static bool HasMeaningfulQuizAnswers(string? answersJson)
    {
        if (string.IsNullOrWhiteSpace(answersJson)) return false;
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(answersJson);
            return document.RootElement.ValueKind switch
            {
                System.Text.Json.JsonValueKind.Object => document.RootElement.EnumerateObject().Any(),
                System.Text.Json.JsonValueKind.Array => document.RootElement.GetArrayLength() > 0,
                System.Text.Json.JsonValueKind.String => !string.IsNullOrWhiteSpace(document.RootElement.GetString()),
                _ => false
            };
        }
        catch (System.Text.Json.JsonException)
        {
            // Legacy non-JSON answer payloads should still count when they contain data.
            return answersJson.Trim().Length > 2;
        }
    }

    private static HashSet<string> GetCircleMatchTerms(string value)
    {
        var terms = Regex.Matches(NormalizeCircleText(value), "[a-z0-9]+")
            .Select(match => match.Value)
            .Where(term => term.Length > 2 && !CircleMatchStopWords.Contains(term))
            .ToHashSet();

        var normalized = NormalizeCircleText(value);
        foreach (var (phrase, relatedTerms) in CircleRelatedTerms)
        {
            if ($" {normalized} ".Contains($" {phrase} ")) terms.UnionWith(relatedTerms);
        }

        return terms;
    }

    private static string NormalizeCircleText(string value) =>
        Regex.Replace(value ?? string.Empty, "[^a-zA-Z0-9]+", " ").Trim().ToLowerInvariant();

    private static readonly HashSet<string> CircleMatchStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "and", "the", "for", "with", "from", "your", "you", "career", "goal", "goals", "circle", "circles", "learning", "group"
    };

    private static readonly Dictionary<string, string[]> CircleRelatedTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ai"] = new[] { "artificial", "intelligence", "machine", "learning", "data", "science", "technology", "software" },
        ["ml"] = new[] { "machine", "learning", "artificial", "intelligence", "data", "science", "technology", "python" },
        ["artificial intelligence"] = new[] { "ai", "machine", "learning", "data", "science", "technology", "software", "engineering" },
        ["machine learning"] = new[] { "machine", "learning", "ai", "artificial", "intelligence", "data", "science", "technology", "python" },
        ["computer science"] = new[] { "computer", "science", "software", "technology", "programming", "coding", "data", "engineering" },
        ["data science"] = new[] { "data", "science", "analytics", "machine", "learning", "ai", "statistics", "technology" },
        ["engineering"] = new[] { "engineering", "science", "technology", "design", "technical", "stem" },
        ["commerce"] = new[] { "commerce", "business", "finance", "accounting", "economics", "marketing", "management" },
        ["civil administration"] = new[] { "civil", "administration", "government", "public", "service", "upsc", "policy" },
        ["government job"] = new[] { "government", "public", "service", "upsc", "civil", "administration", "exam" }
    };

    public async Task<List<LearningCircleDto>> GetAllPublicCirclesAsync(string userId = "")
    {
        var circles = await _context.LearningCircles
            .Where(c => c.IsPublic && !c.IsDisabledByAdmin && _context.Users.Any(user => user.Id == c.CreatorId))
            .OrderByDescending(c => c.MemberCount)
            .ToListAsync();
            
        var dtos = new List<LearningCircleDto>();
        foreach (var circle in circles)
        {
            dtos.Add(await MapToDtoAsync(circle, userId));
        }
        return dtos;
    }

    public async Task<List<LearningCircleDto>> GetMyCirclesAsync(string userId)
    {
        var circles = await _context.LearningCircles
            .Where(c => c.CreatorId == userId || _context.LearningCircleMembers.Any(m => m.CircleId == c.Id && m.UserId == userId))
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
        var dtos = new List<LearningCircleDto>();
        foreach (var circle in circles) dtos.Add(await MapToDtoAsync(circle, userId));
        return dtos;
    }

    public async Task<string> RequestJoinCircleAsync(string userId, string circleId)
    {
        var circle = await _context.LearningCircles.FindAsync(circleId);
        if (circle == null) return "NotFound";
        if (circle.IsDisabledByAdmin) return "DisabledByAdmin";

        // Check if already a member
        var exists = await _context.LearningCircleMembers.AnyAsync(m => m.UserId == userId && m.CircleId == circleId);
        if (exists) return "AlreadyMember";

        // Check if a request already exists
        var existingRequest = await _context.LearningCircleJoinRequests
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CircleId == circleId && r.Status == "Pending");
        
        if (existingRequest != null) return "Pending";

        _context.LearningCircleJoinRequests.Add(new LearningCircleJoinRequest
        {
            UserId = userId,
            CircleId = circleId,
            Status = "Pending",
            RequestedAt = DateTime.UtcNow
        });
        
        var user = await _context.Users.FindAsync(userId);
        var creatorExists = await _context.Users.AnyAsync(u => u.Id == circle.CreatorId);
        if (creatorExists)
        {
            var dbNotification = new Notification
            {
                UserId = circle.CreatorId,
                Type = "JoinRequest",
                Title = "New Join Request",
                Message = $"{user?.FullName ?? "Someone"} wants to join '{circle.Name}'",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(dbNotification);
        }

        await _context.SaveChangesAsync();
        return "Pending";
    }

    public async Task<List<LearningCircleJoinRequestDto>> GetPendingRequestsAsync(string circleId, string userId)
    {
        var circle = await _context.LearningCircles.FindAsync(circleId);
        if (circle == null || circle.CreatorId != userId) return new List<LearningCircleJoinRequestDto>();

        var requests = await _context.LearningCircleJoinRequests
            .Include(r => r.User)
            .Where(r => r.CircleId == circleId && r.Status == "Pending")
            .ToListAsync();

        return requests.Select(r => new LearningCircleJoinRequestDto
        {
            Id = r.Id,
            CircleId = r.CircleId,
            UserId = r.UserId,
            UserFullName = r.User?.FullName ?? "Unknown User",
            Status = r.Status,
            RequestedAt = r.RequestedAt
        }).ToList();
    }

    public async Task<(bool Success, string RequesterId, string CircleName)> ProcessJoinRequestAsync(string requestId, string userId, bool approve)
    {
        var request = await _context.LearningCircleJoinRequests
            .Include(r => r.Circle)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null || request.Circle?.CreatorId != userId || request.Status != "Pending") 
            return (false, string.Empty, string.Empty);

        request.Status = approve ? "Approved" : "Rejected";
        request.ProcessedAt = DateTime.UtcNow;

        if (approve)
        {
            _context.LearningCircleMembers.Add(new LearningCircleMember
            {
                UserId = request.UserId,
                CircleId = request.CircleId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow
            });
            request.Circle.MemberCount += 1;
            var dbNotification = new Notification
            {
                UserId = request.UserId,
                Type = "JoinRequestApproved",
                Title = "Join Request Approved",
                Message = $"Your request to join '{request.Circle.Name}' was approved!",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(dbNotification);
        }
        else 
        {
            var dbNotification = new Notification
            {
                UserId = request.UserId,
                Type = "JoinRequestRejected",
                Title = "Join Request Rejected",
                Message = $"Your request to join '{request.Circle.Name}' was declined.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(dbNotification);
        }

        await _context.SaveChangesAsync();
        return (true, request.UserId, request.Circle.Name);
    }

    public async Task<bool> ToggleMeetingStatusAsync(string circleId, string userId, bool isActive)
    {
        var circle = await _context.LearningCircles.FindAsync(circleId);
        if (circle == null || circle.IsDisabledByAdmin) return false;
        
        bool isCreator = circle.CreatorId == userId;
        bool isMember = await _context.LearningCircleMembers.AnyAsync(m => m.CircleId == circleId && m.UserId == userId);

        if (!isCreator && !isMember) return false;

        circle.IsMeetingActive = isActive;
        circle.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> JoinCircleAsync(string userId, string circleId)
    {
        // Keep this for backwards compatibility or admin force join if needed, but normally use RequestJoinCircleAsync now
        var exists = await _context.LearningCircleMembers.AnyAsync(m => m.UserId == userId && m.CircleId == circleId);
        if (exists) return true;

        var circle = await _context.LearningCircles.FindAsync(circleId);
        if (circle == null || circle.IsDisabledByAdmin) return false;

        _context.LearningCircleMembers.Add(new LearningCircleMember
        {
            UserId = userId,
            CircleId = circleId,
            Role = "Member",
            JoinedAt = DateTime.UtcNow
        });
        
        circle.MemberCount += 1;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<LearningCircleDto> GetCircleByIdAsync(string circleId, string userId = "")
    {
        var circle = await _context.LearningCircles
            .FirstOrDefaultAsync(c => c.Id == circleId && _context.Users.Any(user => user.Id == c.CreatorId) &&
                (!c.IsDisabledByAdmin && c.IsPublic || c.IsDisabledByAdmin &&
                    (c.CreatorId == userId || _context.LearningCircleMembers.Any(m => m.CircleId == c.Id && m.UserId == userId))));
        if (circle == null) return null;
        return await MapToDtoAsync(circle, userId);
    }

    public async Task<LearningCircleDto> CreateCircleAsync(CreateLearningCircleDto dto, string userId)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var name = dto.Name?.Trim() ?? string.Empty;
        if (name.Length < 3 || name.Length > 100)
            throw new ArgumentException("Circle name must be between 3 and 100 characters.", nameof(dto));

        var creator = await _context.Users.FindAsync(userId);
        if (creator == null)
            throw new UnauthorizedAccessException("A registered platform account is required to create a learning circle.");

        var circle = new LearningCircle
        {
            Name = name,
            Description = dto.Description,
            CareerDomain = dto.CareerDomain,
            CircleType = dto.CircleType,
            Tags = dto.Tags,
            MemberCount = 1,
            IsPublic = true,
            CreatorId = userId,
            IsMeetingActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.LearningCircles.Add(circle);
        
        var member = new LearningCircleMember
        {
            CircleId = circle.Id,
            UserId = userId,
            Role = "Facilitator",
            JoinedAt = DateTime.UtcNow
        };

        _context.LearningCircleMembers.Add(member);
        await _context.SaveChangesAsync();

        return await MapToDtoAsync(circle, userId);
    }

    public async Task<List<string>> ScheduleMeetingAsync(string circleId, string userId, DateTime meetingTime)
    {
        var circle = await _context.LearningCircles.FindAsync(circleId);
        if (circle == null || circle.IsDisabledByAdmin) return new List<string>();
        
        bool isMember = await _context.LearningCircleMembers.AnyAsync(m => m.CircleId == circleId && m.UserId == userId);
        if (!isMember && circle.CreatorId != userId) return new List<string>();

        circle.ScheduledMeetingTime = meetingTime;

        var memberIds = await _context.LearningCircleMembers
            .Where(m => m.CircleId == circleId)
            .Select(m => m.UserId)
            .ToListAsync();

        foreach (var memberId in memberIds)
        {
            var dbNotification = new Notification
            {
                UserId = memberId,
                Type = "MeetingScheduled",
                Title = "Live Session Scheduled",
                Message = $"A live study session has been scheduled for '{circle.Name}' at {meetingTime:f}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(dbNotification);
        }
        
        await _context.SaveChangesAsync();
        return memberIds;
    }

    private async Task<LearningCircleDto> MapToDtoAsync(LearningCircle circle, string userId = "")
    {
        bool isMember = false;
        bool isCreator = circle.CreatorId == userId;

        if (!string.IsNullOrEmpty(userId))
        {
            isMember = await _context.LearningCircleMembers
                .AnyAsync(m => m.CircleId == circle.Id && m.UserId == userId);
        }

        var creator = await _context.Users.FindAsync(circle.CreatorId);
        var creatorName = !string.IsNullOrWhiteSpace(creator?.FullName)
            ? creator.FullName
            : creator?.Email ?? "Registered member";

        return new LearningCircleDto
        {
            Id = circle.Id,
            Name = circle.Name,
            Description = circle.Description,
            CareerDomain = circle.CareerDomain,
            CircleType = circle.CircleType,
            Tags = circle.Tags,
            MemberCount = circle.MemberCount,
            IsPublic = circle.IsPublic,
            CreatorId = circle.CreatorId,
            CreatorName = creatorName,
            IsMeetingActive = circle.IsMeetingActive,
            ScheduledMeetingTime = circle.ScheduledMeetingTime.HasValue ? DateTime.SpecifyKind(circle.ScheduledMeetingTime.Value, DateTimeKind.Utc) : null,
            IsMember = isMember,
            IsCreator = isCreator,
            IsDisabledByAdmin = circle.IsDisabledByAdmin,
            AdminDisabledReason = circle.AdminDisabledReason
        };
    }
    public async Task<List<DiscussionDto>> GetDiscussionsAsync(string circleId, string userId)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        
        var oldDiscussions = await _context.LearningCircleDiscussions
            .Where(d => d.CircleId == circleId && d.CreatedAt < sevenDaysAgo)
            .ToListAsync();
            
        if (oldDiscussions.Any())
        {
            _context.LearningCircleDiscussions.RemoveRange(oldDiscussions);
            await _context.SaveChangesAsync();
        }

        var discussions = await _context.LearningCircleDiscussions
            .Include(d => d.User)
            .Where(d => d.CircleId == circleId)
            .OrderBy(d => d.CreatedAt)
            .Select(d => new DiscussionDto
            {
                Id = d.Id,
                CircleId = d.CircleId,
                UserId = d.UserId,
                UserName = d.User != null ? d.User.FullName : "Unknown",
                Content = d.Content,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();

        return discussions;
    }

    public async Task<DiscussionDto> CreateDiscussionAsync(string circleId, string userId, CreateDiscussionDto dto)
    {
        if (await _context.LearningCircles.AnyAsync(c => c.Id == circleId && c.IsDisabledByAdmin))
            throw new InvalidOperationException("This group has been disabled by an administrator.");
        var discussion = new LearningCircleDiscussion
        {
            CircleId = circleId,
            UserId = userId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.LearningCircleDiscussions.Add(discussion);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        return new DiscussionDto
        {
            Id = discussion.Id,
            CircleId = discussion.CircleId,
            UserId = discussion.UserId,
            UserName = user?.FullName ?? "Unknown",
            Content = discussion.Content,
            CreatedAt = discussion.CreatedAt
        };
    }

    public async Task<bool> ClearDiscussionsAsync(string circleId, string userId)
    {
        var circle = await _context.LearningCircles.FindAsync(circleId);
        if (circle == null || circle.CreatorId != userId) return false;

        var discussions = await _context.LearningCircleDiscussions.Where(d => d.CircleId == circleId).ToListAsync();
        _context.LearningCircleDiscussions.RemoveRange(discussions);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ResourceDto>> GetResourcesAsync(string circleId, string userId)
    {
        var resources = await _context.LearningCircleResources
            .Include(r => r.AddedByUser)
            .Where(r => r.CircleId == circleId)
            .OrderByDescending(r => r.AddedAt)
            .Select(r => new ResourceDto
            {
                Id = r.Id,
                CircleId = r.CircleId,
                Title = r.Title,
                Url = r.Url,
                AddedByUserId = r.AddedByUserId,
                AddedByUserName = r.AddedByUser != null ? r.AddedByUser.FullName : "Unknown",
                AddedAt = r.AddedAt
            })
            .ToListAsync();

        return resources;
    }

    public async Task<ResourceDto> CreateResourceAsync(string circleId, string userId, CreateResourceDto dto)
    {
        if (await _context.LearningCircles.AnyAsync(c => c.Id == circleId && c.IsDisabledByAdmin))
            throw new InvalidOperationException("This group has been disabled by an administrator.");
        var resource = new LearningCircleResource
        {
            CircleId = circleId,
            Title = dto.Title,
            Url = dto.Url,
            AddedByUserId = userId,
            AddedAt = DateTime.UtcNow
        };

        _context.LearningCircleResources.Add(resource);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        return new ResourceDto
        {
            Id = resource.Id,
            CircleId = resource.CircleId,
            Title = resource.Title,
            Url = resource.Url,
            AddedByUserId = resource.AddedByUserId,
            AddedByUserName = user?.FullName ?? "Unknown",
            AddedAt = resource.AddedAt
        };
    }

    public async Task<bool> DeleteResourceAsync(string circleId, string resourceId, string userId)
    {
        var circle = await _context.LearningCircles.FindAsync(circleId);
        if (circle == null || circle.IsDisabledByAdmin || circle.CreatorId != userId) return false;

        var resource = await _context.LearningCircleResources.FindAsync(resourceId);
        if (resource == null || resource.CircleId != circleId) return false;

        _context.LearningCircleResources.Remove(resource);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<LearningCircleMemberDto>> GetCircleMembersAsync(string circleId)
    {
        var members = await _context.LearningCircleMembers
            .Include(m => m.User)
            .Where(m => m.CircleId == circleId)
            .Select(m => new LearningCircleMemberDto
            {
                Id = m.Id,
                CircleId = m.CircleId,
                UserId = m.UserId,
                FullName = m.User != null ? m.User.FullName : "Unknown",
                Role = m.Role,
                JoinedAt = m.JoinedAt
            })
            .ToListAsync();
            
        return members;
    }

    public async Task<bool> RemoveMemberAsync(string circleId, string memberId, string userId)
    {
        var circle = await _context.LearningCircles.FindAsync(circleId);
        // Only the creator can remove members
        if (circle == null || circle.CreatorId != userId) return false;

        var member = await _context.LearningCircleMembers.FirstOrDefaultAsync(m => m.Id == memberId && m.CircleId == circleId);
        if (member == null) return false;

        // Cannot remove the creator
        if (member.UserId == circle.CreatorId) return false;

        _context.LearningCircleMembers.Remove(member);
        circle.MemberCount = Math.Max(0, circle.MemberCount - 1);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LeaveCircleAsync(string circleId, string userId)
    {
        var circle = await _context.LearningCircles.FindAsync(circleId);
        if (circle == null) return false;
        
        // Creator cannot leave the circle (they must delete it or transfer ownership - for now just prevent it)
        if (circle.CreatorId == userId) return false;

        var member = await _context.LearningCircleMembers.FirstOrDefaultAsync(m => m.CircleId == circleId && m.UserId == userId);
        if (member == null) return false;

        _context.LearningCircleMembers.Remove(member);
        circle.MemberCount = Math.Max(0, circle.MemberCount - 1);
        await _context.SaveChangesAsync();
        return true;
    }

    // Tasks
    public async Task<List<LearningCircleTaskDto>> GetTasksAsync(string circleId, string userId)
    {
        var tasks = await _context.LearningCircleTasks
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Where(t => t.CircleId == circleId && t.AssignedToUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new LearningCircleTaskDto
            {
                Id = t.Id,
                CircleId = t.CircleId,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                DueDate = t.DueDate,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUserName = t.CreatedByUser != null ? t.CreatedByUser.FullName : "Unknown",
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUser != null ? t.AssignedToUser.FullName : "Unknown",
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return tasks;
    }

    public async Task<LearningCircleTaskDto> CreateTaskAsync(string circleId, string userId, CreateLearningCircleTaskDto dto)
    {
        if (await _context.LearningCircles.AnyAsync(c => c.Id == circleId && c.IsDisabledByAdmin))
            throw new InvalidOperationException("This group has been disabled by an administrator.");
        var members = await _context.LearningCircleMembers.Where(m => m.CircleId == circleId).ToListAsync();
        
        var mainTask = new LearningCircleTask
        {
            CircleId = circleId,
            Title = dto.Title,
            Description = dto.Description,
            DueDate = dto.DueDate,
            CreatedByUserId = userId,
            AssignedToUserId = userId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _context.LearningCircleTasks.Add(mainTask);

        foreach (var member in members)
        {
            if (member.UserId == userId) continue;
            
            var task = new LearningCircleTask
            {
                CircleId = circleId,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                CreatedByUserId = userId,
                AssignedToUserId = member.UserId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.LearningCircleTasks.Add(task);
        }

        await _context.SaveChangesAsync();

        var createdBy = await _context.Users.FindAsync(userId);

        return new LearningCircleTaskDto
        {
            Id = mainTask.Id,
            CircleId = mainTask.CircleId,
            Title = mainTask.Title,
            Description = mainTask.Description,
            Status = mainTask.Status,
            DueDate = mainTask.DueDate,
            CreatedByUserId = mainTask.CreatedByUserId,
            CreatedByUserName = createdBy?.FullName ?? "Unknown",
            AssignedToUserId = mainTask.AssignedToUserId,
            AssignedToUserName = createdBy?.FullName ?? "Unknown",
            CreatedAt = mainTask.CreatedAt
        };
    }

    public async Task<bool> UpdateTaskStatusAsync(string circleId, string taskId, string userId, string status)
    {
        var task = await _context.LearningCircleTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.CircleId == circleId);
        if (task == null || await _context.LearningCircles.AnyAsync(c => c.Id == circleId && c.IsDisabledByAdmin)) return false;

        task.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    // Notes
    public async Task<LearningCircleNoteDto?> GetNoteAsync(string circleId, string userId)
    {
        var note = await _context.LearningCircleNotes
            .Include(n => n.UpdatedByUser)
            .FirstOrDefaultAsync(n => n.CircleId == circleId);

        if (note == null) return null;

        return new LearningCircleNoteDto
        {
            Id = note.Id,
            CircleId = note.CircleId,
            Content = note.Content,
            UpdatedByUserId = note.UpdatedByUserId,
            UpdatedByUserName = note.UpdatedByUser?.FullName ?? "Unknown",
            UpdatedAt = note.UpdatedAt
        };
    }

    public async Task<LearningCircleNoteDto> UpdateNoteAsync(string circleId, string userId, string content)
    {
        if (await _context.LearningCircles.AnyAsync(c => c.Id == circleId && c.IsDisabledByAdmin))
            throw new InvalidOperationException("This group has been disabled by an administrator.");
        var note = await _context.LearningCircleNotes.FirstOrDefaultAsync(n => n.CircleId == circleId);
        
        if (note == null)
        {
            note = new LearningCircleNote
            {
                CircleId = circleId,
                Content = content,
                UpdatedByUserId = userId,
                UpdatedAt = DateTime.UtcNow
            };
            _context.LearningCircleNotes.Add(note);
        }
        else
        {
            note.Content = content;
            note.UpdatedByUserId = userId;
            note.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        var updater = await _context.Users.FindAsync(userId);

        return new LearningCircleNoteDto
        {
            Id = note.Id,
            CircleId = note.CircleId,
            Content = note.Content,
            UpdatedByUserId = note.UpdatedByUserId,
            UpdatedByUserName = updater?.FullName ?? "Unknown",
            UpdatedAt = note.UpdatedAt
        };
    }

    // AI Mentor
    public async Task<string> GetAiMentorResponseAsync(string circleId, string userId, string query)
    {
        var circle = await _context.LearningCircles.FindAsync(circleId);
        var domain = circle?.CareerDomain ?? "Learning";
        var systemPrompt = $"You are an AI Mentor for a Learning Circle focusing on {domain}. Be helpful, concise, and encourage collaborative learning. Respond to the user's query appropriately.";
        
        return await _aiService.GetResponseAsync(systemPrompt, query);
    }

    // Progress Board
    public async Task<List<CircleProgressDto>> GetProgressBoardAsync(string circleId)
    {
        var members = await _context.LearningCircleMembers
            .Include(m => m.User)
            .Where(m => m.CircleId == circleId)
            .ToListAsync();

        var progressList = new List<CircleProgressDto>();

        foreach (var member in members)
        {
            var tasksCompleted = await _context.LearningCircleTasks
                .CountAsync(t => t.CircleId == circleId && t.AssignedToUserId == member.UserId && t.Status == "Completed");
                
            var resourcesShared = await _context.LearningCircleResources
                .CountAsync(r => r.CircleId == circleId && r.AddedByUserId == member.UserId);
                
            var discussionsStarted = await _context.LearningCircleDiscussions
                .CountAsync(d => d.CircleId == circleId && d.UserId == member.UserId);

            var score = (tasksCompleted * 10) + (resourcesShared * 5) + (discussionsStarted * 2);

            progressList.Add(new CircleProgressDto
            {
                UserId = member.UserId,
                UserName = member.User.FullName ?? "Unknown",
                TasksCompleted = tasksCompleted,
                ResourcesShared = resourcesShared,
                DiscussionsStarted = discussionsStarted,
                Score = score
            });
        }

        return progressList.OrderByDescending(p => p.Score).ToList();
    }

    // Overview
    public async Task<CircleOverviewDto> GetCircleOverviewAsync(string circleId)
    {
        var membersCount = await _context.LearningCircleMembers.CountAsync(m => m.CircleId == circleId);
        var discussions = await _context.LearningCircleDiscussions.Include(d => d.User).Where(d => d.CircleId == circleId).ToListAsync();
        var resources = await _context.LearningCircleResources.Include(r => r.AddedByUser).Where(r => r.CircleId == circleId).ToListAsync();
        var tasks = await _context.LearningCircleTasks.Where(t => t.CircleId == circleId).ToListAsync();

        var recentActivities = new List<RecentActivityDto>();

        foreach (var d in discussions)
        {
            recentActivities.Add(new RecentActivityDto
            {
                Type = "discussion",
                Text = $"{d.User.FullName} posted a new question in Discussions",
                Timestamp = d.CreatedAt
            });
        }

        foreach (var r in resources)
        {
            recentActivities.Add(new RecentActivityDto
            {
                Type = "resource",
                Text = $"{r.AddedByUser.FullName} shared \"{r.Title}\"",
                Timestamp = r.AddedAt
            });
        }

        var completedTasks = tasks.Where(t => t.Status == "Completed").ToList();
        foreach (var t in completedTasks.GroupBy(x => x.Title))
        {
            recentActivities.Add(new RecentActivityDto
            {
                Type = "task",
                Text = $"Task \"{t.Key}\" was completed by {t.Count()} members",
                Timestamp = t.Max(x => x.DueDate) ?? DateTime.UtcNow
            });
        }

        recentActivities = recentActivities.OrderByDescending(x => x.Timestamp).Take(5).ToList();

        // Dummy stats for readiness and skills based on tasks completed
        var readiness = 20.0 + (completedTasks.Count * 2.5);
        if (readiness > 100) readiness = 100;

        return new CircleOverviewDto
        {
            DiscussionsCount = discussions.Count,
            ResourcesCount = resources.Count,
            TasksCompletedCount = completedTasks.Count,
            MembersCount = membersCount,
            AvgPlacementReadiness = Math.Round(readiness, 1),
            PopularSkills = new List<string> { "React", "Node.js", "System Design" },
            RecentActivities = recentActivities
        };
    }
}
