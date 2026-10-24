using System.Collections.Generic;
using SkillScape.Domain.Entities;

namespace SkillScape.Application.DTOs
{
    public class RecommendationResponseDto
    {
        public TargetCareerDto TargetCareer { get; set; } = new TargetCareerDto();
        public List<RecommendedSkillDto> RecommendedSkills { get; set; } = new List<RecommendedSkillDto>();
        public List<CourseRecommendationDto> Courses { get; set; } = new List<CourseRecommendationDto>();
        public RecommendationMetadataDto RecommendationMetadata { get; set; } = new RecommendationMetadataDto();
    }

    public class TargetCareerDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
    }

    public class RecommendedSkillDto
    {
        public string SkillId { get; set; } = string.Empty;
        public string SkillName { get; set; } = string.Empty;
        public int CurrentLevel { get; set; }
        public int RequiredLevel { get; set; }
        public int Gap { get; set; }
        public string Importance { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class CourseRecommendationDto : Course
    {
        public double RecommendationScore { get; set; }
        public string RecommendationReason { get; set; } = string.Empty;
        public List<string> MatchedSkills { get; set; } = new List<string>();
        public double SkillGapCoverage { get; set; }
        public bool CareerMatch { get; set; }
    }

    public class RecommendationMetadataDto
    {
        public string Strategy { get; set; } = string.Empty;
        public string ModelVersion { get; set; } = string.Empty;
        public string GeneratedAt { get; set; } = string.Empty;
    }
}
