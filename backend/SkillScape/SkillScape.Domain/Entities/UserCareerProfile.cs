using System;

namespace SkillScape.Domain.Entities;

/// <summary>
/// Represents a user's final recommended career profile and saved bookmark paths.
/// </summary>
public class UserCareerProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; } = string.Empty;
    
    public string SelectedHierarchy { get; set; } = string.Empty; // After10th, After12th, etc.
    
    public string QuizAnswersJson { get; set; } = "{}"; // JSON map: questionId -> answerText
    
    public string RecommendedCareerId { get; set; } = string.Empty;
    
    public double ConfidenceScore { get; set; } // e.g. 85.5%
    
    public double PredictedSalary { get; set; } // predicted base salary (in INR)
    
    public string RoadmapJson { get; set; } = "[]"; // JSON serialized learning path milestones
    
    public string SkillGapJson { get; set; } = "[]"; // JSON array of missing skills
    
    public string BookmarkedPaths { get; set; } = "[]"; // JSON array of career path IDs bookmarked by user
    
    public string TopMatchesJson { get; set; } = "[]"; // JSON array of top 10 recommended careers
    
    public string WhyRecommended { get; set; } = string.Empty;
    
    public string WhyNotRecommended { get; set; } = string.Empty;
    
    public string StrengthsWeaknessesJson { get; set; } = "{}"; // JSON mapping strengths and weaknesses
    
    public string CareerGrowthGraphJson { get; set; } = "[]"; // JSON array of projected salary growth trend points
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser? User { get; set; }
    public CareerPath? RecommendedCareer { get; set; }
}
