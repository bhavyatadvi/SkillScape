using System.Collections.Generic;
using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface ICareerGuidanceService
{
    Task<List<CareerPathDto>> GetCareerPathsTreeAsync(string streamType);
    
    Task<CareerPathDto?> GetCareerPathByIdAsync(string id);
    
    Task<List<HierarchicalQuizQuestionDto>> GetQuizQuestionsAsync(string streamType);
    
    Task<UserCareerProfileDto> SubmitQuizAnswersAsync(string userId, QuizSubmissionRequest request);
    
    Task<UserCareerProfileDto?> GetUserCareerProfileAsync(string userId);
    
    Task<bool> ToggleBookmarkPathAsync(string userId, string pathId);
    
    Task<bool> AcceptCareerPathAsync(string userId, string pathId);
    
    Task<List<CareerPathDto>> GetBookmarkedPathsAsync(string userId);
    
    Task<SalaryForecastDto> ForecastSalaryAsync(SalaryForecastRequest request);
    
    Task<GuidanceAnalyticsDto> GetGuidanceAnalyticsAsync();
    
    Task<CareerCompareDto> CompareCareersAsync(string pathId1, string pathId2);
}
