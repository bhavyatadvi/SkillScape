using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class CareerProfileService : ICareerProfileService
{
    private readonly ApplicationDbContext _context;
    private readonly IResumeTextExtractor _textExtractor;
    private readonly IResumeGroqService _resumeGroqService;
    private readonly ILogger<CareerProfileService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public CareerProfileService(
        ApplicationDbContext context,
        IResumeTextExtractor textExtractor,
        IResumeGroqService resumeGroqService,
        ILogger<CareerProfileService> logger)
    {
        _context = context;
        _textExtractor = textExtractor;
        _resumeGroqService = resumeGroqService;
        _logger = logger;
    }

    public async Task<CareerProfileDto?> GetProfileAsync(string userId)
    {
        var profile = await _context.CareerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null) return null;

        return MapEntityToDto(profile);
    }

    public async Task<CareerProfileDto> CreateOrUpdateProfileAsync(string userId, SaveCareerProfileRequest request)
    {
        if (request.GraduationYear.HasValue &&
            (request.GraduationYear.Value < 1960 || request.GraduationYear.Value > DateTime.UtcNow.Year + 6))
        {
            throw new ArgumentException(
                $"Graduation year must be between 1960 and {DateTime.UtcNow.Year + 6}.",
                nameof(request));
        }

        if (request.ExperienceYears.HasValue &&
            (!double.IsFinite(request.ExperienceYears.Value) ||
             request.ExperienceYears.Value < 0 ||
             request.ExperienceYears.Value > 60))
        {
            throw new ArgumentException("Years of experience must be between 0 and 60.", nameof(request));
        }

        var profile = await _context.CareerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        bool isNew = (profile == null);
        if (isNew)
        {
            profile = new CareerProfile
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };
        }

        // Education
        profile.EducationLevel = request.EducationLevel ?? string.Empty;
        profile.Degree = request.Degree ?? string.Empty;
        profile.BroadField = request.BroadField ?? string.Empty;
        profile.Specialization = request.Specialization ?? string.Empty;
        profile.Institution = request.Institution ?? string.Empty;
        profile.GraduationYear = request.GraduationYear;
        profile.AdditionalQualifications = request.AdditionalQualifications ?? string.Empty;

        // Current Career
        profile.EducationStatus = request.EducationStatus ?? string.Empty;
        profile.CurrentEmploymentStatus = request.CurrentEmploymentStatus ?? string.Empty;
        profile.CurrentOccupation = request.CurrentOccupation ?? string.Empty;
        profile.CurrentRole = request.CurrentRole ?? string.Empty;
        profile.CurrentIndustry = request.CurrentIndustry ?? string.Empty;
        profile.OrganizationType = request.OrganizationType ?? string.Empty;
        profile.ExperienceYears = request.ExperienceYears;
        profile.RelevantExperience = request.RelevantExperience ?? string.Empty;
        profile.PreviousRoles = request.PreviousRoles ?? string.Empty;
        profile.PreviousIndustries = request.PreviousIndustries ?? string.Empty;

        // JSON properties
        profile.CurrentSkillsJson = JsonSerializer.Serialize(request.CurrentSkills ?? new List<CareerSkillDto>(), JsonOptions);
        profile.CertificationsJson = JsonSerializer.Serialize(request.Certifications ?? new List<string>(), JsonOptions);
        profile.CareerLikesJson = JsonSerializer.Serialize(request.CareerLikes ?? new List<string>(), JsonOptions);
        profile.CareerDislikesJson = JsonSerializer.Serialize(request.CareerDislikes ?? new List<string>(), JsonOptions);
        profile.WorkPreferencesJson = JsonSerializer.Serialize(request.WorkPreferences ?? new WorkPreferencesDto(), JsonOptions);
        profile.MotivationsJson = JsonSerializer.Serialize(request.Motivations ?? new List<string>(), JsonOptions);
        profile.ConstraintsJson = JsonSerializer.Serialize(request.Constraints ?? new List<string>(), JsonOptions);

        // Goals
        profile.CareerChangePreference = request.CareerChangePreference ?? string.Empty;
        profile.Location = request.Location ?? string.Empty;
        profile.PreferredWorkMode = request.PreferredWorkMode ?? string.Empty;
        profile.SalaryRange = request.SalaryRange ?? string.Empty;
        profile.CareerGoals = request.CareerGoals ?? string.Empty;

        // Resume metadata
        profile.ProfileSource = string.IsNullOrWhiteSpace(request.ProfileSource) ? "manual" : request.ProfileSource;
        profile.ResumeFileName = request.ResumeFileName ?? string.Empty;
        profile.UpdatedAt = DateTime.UtcNow;

        if (isNew)
        {
            _context.CareerProfiles.Add(profile);
        }
        else
        {
            _context.CareerProfiles.Update(profile);
        }

        // Also update ApplicationUser's ProfileCompleted flag
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            user.ProfileCompleted = true;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return MapEntityToDto(profile);
    }

    public async Task<bool> DeleteProfileAsync(string userId)
    {
        var profile = await _context.CareerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return false;

        _context.CareerProfiles.Remove(profile);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ResumeParseResultDto> ParseResumeAsync(Stream stream, string fileName)
    {
        _logger.LogInformation("Resume parsing started for file {FileName}.", fileName);

        // 1. Extract text from PDF/DOCX
        var extractedText = await _textExtractor.ExtractTextAsync(stream, fileName);
        _logger.LogInformation("Resume text extracted successfully ({Length} characters).", extractedText.Length);

        // 2. Call dedicated Resume Groq Service (GROQ_API_RESUME + model openai/gpt-oss-120b)
        string cleanedJson;
        try
        {
            cleanedJson = await _resumeGroqService.GetResumeProfileAsync(extractedText);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Resume Groq AI service error during parsing.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI service failed during resume parsing.");
            throw new InvalidOperationException("Unable to extract profile information right now. Please try again or fill the profile manually.", ex);
        }

        ResumeParseResultDto result;
        try
        {
            result = JsonSerializer.Deserialize<ResumeParseResultDto>(cleanedJson, JsonOptions) ?? new ResumeParseResultDto();
            _logger.LogInformation("Resume profile JSON parsed into draft DTO successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize AI resume output. Raw response: {Raw}", cleanedJson);
            result = new ResumeParseResultDto();
        }

        result.ProfileSource = "resume";
        result.ResumeFileName = fileName;
        result.ExtractedTextSummary = extractedText.Length > 200 ? extractedText.Substring(0, 200) + "..." : extractedText;

        // Merge parsedSkills (flat string list) into currentSkills (objects with proficiency).
        // Any skill in parsedSkills not already in currentSkills is added with Intermediate proficiency.
        if (result.ParsedSkills.Count > 0)
        {
            var existingNames = new HashSet<string>(
                result.CurrentSkills.Select(s => s.Name),
                StringComparer.OrdinalIgnoreCase);

            foreach (var skillName in result.ParsedSkills)
            {
                if (!string.IsNullOrWhiteSpace(skillName) && !existingNames.Contains(skillName))
                {
                    result.CurrentSkills.Add(new CareerSkillDto { Name = skillName, Proficiency = "Intermediate" });
                    existingNames.Add(skillName);
                }
            }

            _logger.LogInformation(
                "Skills merged: {ParsedCount} from parsedSkills, {TotalCount} total in currentSkills.",
                result.ParsedSkills.Count, result.CurrentSkills.Count);
        }

        return result;
    }

    private static CareerProfileDto MapEntityToDto(CareerProfile entity)
    {
        return new CareerProfileDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            EducationLevel = entity.EducationLevel,
            Degree = entity.Degree,
            BroadField = entity.BroadField,
            Specialization = entity.Specialization,
            Institution = entity.Institution,
            GraduationYear = entity.GraduationYear,
            AdditionalQualifications = entity.AdditionalQualifications,
            EducationStatus = entity.EducationStatus,
            CurrentEmploymentStatus = entity.CurrentEmploymentStatus,
            CurrentOccupation = entity.CurrentOccupation,
            CurrentRole = entity.CurrentRole,
            CurrentIndustry = entity.CurrentIndustry,
            OrganizationType = entity.OrganizationType,
            ExperienceYears = entity.ExperienceYears,
            RelevantExperience = entity.RelevantExperience,
            PreviousRoles = entity.PreviousRoles,
            PreviousIndustries = entity.PreviousIndustries,
            CurrentSkills = SafeDeserializeList<CareerSkillDto>(entity.CurrentSkillsJson),
            Certifications = SafeDeserializeList<string>(entity.CertificationsJson),
            CareerLikes = SafeDeserializeList<string>(entity.CareerLikesJson),
            CareerDislikes = SafeDeserializeList<string>(entity.CareerDislikesJson),
            WorkPreferences = SafeDeserializeObject<WorkPreferencesDto>(entity.WorkPreferencesJson) ?? new WorkPreferencesDto(),
            Motivations = SafeDeserializeList<string>(entity.MotivationsJson),
            Constraints = SafeDeserializeList<string>(entity.ConstraintsJson),
            CareerChangePreference = entity.CareerChangePreference,
            Location = entity.Location,
            PreferredWorkMode = entity.PreferredWorkMode,
            SalaryRange = entity.SalaryRange,
            CareerGoals = entity.CareerGoals,
            ProfileSource = entity.ProfileSource,
            ResumeFileName = entity.ResumeFileName,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }

    private static List<T> SafeDeserializeList<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<T>();
        try
        {
            return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    private static T? SafeDeserializeObject<T>(string json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
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
