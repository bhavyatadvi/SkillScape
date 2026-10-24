using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillScape.Domain.Entities
{
    public class CourseProvider
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        
        [MaxLength(100)]
        public string ProviderType { get; set; } // Government, University, Commercial
        
        [MaxLength(500)]
        public string LogoUrl { get; set; }
        
        [MaxLength(500)]
        public string WebsiteUrl { get; set; }
        
        public double AuthorityScore { get; set; } = 1.0; // Higher means more official/trusted
        
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }

    public class Course
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string ProviderId { get; set; }
        [ForeignKey(nameof(ProviderId))]
        public CourseProvider Provider { get; set; }
        
        [MaxLength(500)]
        public string Url { get; set; }
        
        [MaxLength(500)]
        public string ImageUrl { get; set; }
        
        [MaxLength(100)]
        public string Domain { get; set; }
        
        [MaxLength(100)]
        public string Subdomain { get; set; }
        
        [MaxLength(50)]
        public string Level { get; set; } // Beginner, Intermediate, Advanced, All levels
        
        public int DurationValue { get; set; }
        
        [MaxLength(50)]
        public string DurationUnit { get; set; } // Hours, Days, Weeks, Months, Years
        
        [MaxLength(50)]
        public string LearningMode { get; set; } // Online, Self-paced, Instructor-led, Hybrid
        
        [MaxLength(50)]
        public string Language { get; set; } = "English";
        
        public bool CertificateAvailable { get; set; }
        
        [MaxLength(100)]
        public string CertificateType { get; set; } // Course certificate, Professional certificate, etc.
        
        public bool IsFree { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [MaxLength(10)]
        public string Currency { get; set; } = "INR";
        
        public bool FinancialAidAvailable { get; set; }
        
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        
        public double TrendingScore { get; set; }
        public string TrendDirection { get; set; } // "+18% ↑", "Stable", "-4% ↓"
        
        public double RelevanceScore { get; set; }
        
        [MaxLength(100)]
        public string Source { get; set; }
        
        public DateTime? LastVerified { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public ICollection<CourseSkill> CourseSkills { get; set; } = new List<CourseSkill>();
        public ICollection<CourseCareer> CourseCareers { get; set; } = new List<CourseCareer>();
        public ICollection<CourseEducationEligibility> EducationEligibilities { get; set; } = new List<CourseEducationEligibility>();
        public ICollection<SavedCourse> SavedByUsers { get; set; } = new List<SavedCourse>();
        public ICollection<LearningProgress> LearningProgresses { get; set; } = new List<LearningProgress>();
    }

    public class CourseSkill
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string CourseId { get; set; }
        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string SkillName { get; set; }
    }

    public class CourseCareer
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string CourseId { get; set; }
        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string CareerTitle { get; set; }
    }

    public class CourseEducationEligibility
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string CourseId { get; set; }
        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string EducationLevel { get; set; } // 10th, 12th, Diploma, Undergraduate, Graduate, Postgraduate, Doctoral
    }

    public class TrendingSkill
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(100)]
        public string SkillName { get; set; }
        
        public double TrendingScore { get; set; }
        
        [MaxLength(50)]
        public string TrendDirection { get; set; }
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class SavedCourse
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }
        
        [Required]
        public string CourseId { get; set; }
        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }
        
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }

    public class LearningProgress
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }
        
        [Required]
        public string CourseId { get; set; }
        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } // NotStarted, InProgress, Completed, Dropped
        
        public int ProgressPercentage { get; set; }
        
        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    }
}
