using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillScape.Domain.Entities;

public class LearningCircle
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string CareerDomain { get; set; } = string.Empty;
    
    // Type of Circle: "Domain Circle", "Career Goal Circle", "Skill Development Circle"
    [Required]
    [MaxLength(50)]
    public string CircleType { get; set; } = "Domain Circle";
    
    [MaxLength(500)]
    public string Tags { get; set; } = string.Empty;
    
    public int MemberCount { get; set; } = 0;
    
    public bool IsPublic { get; set; } = true;
    public bool IsDisabledByAdmin { get; set; }
    [MaxLength(1000)]
    public string? AdminDisabledReason { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string CreatorId { get; set; } = string.Empty;
    public bool IsMeetingActive { get; set; } = false;
    public DateTime? ScheduledMeetingTime { get; set; }

    public ICollection<LearningCircleMember> Members { get; set; } = new List<LearningCircleMember>();
    public ICollection<LearningCircleDiscussion> Discussions { get; set; } = new List<LearningCircleDiscussion>();
    public ICollection<LearningCircleResource> Resources { get; set; } = new List<LearningCircleResource>();
    public ICollection<LearningCircleJoinRequest> JoinRequests { get; set; } = new List<LearningCircleJoinRequest>();
    public ICollection<LearningCircleTask> Tasks { get; set; } = new List<LearningCircleTask>();
    public ICollection<LearningCircleNote> Notes { get; set; } = new List<LearningCircleNote>();
}

public class LearningCircleMember
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string CircleId { get; set; } = string.Empty;
    [ForeignKey(nameof(CircleId))]
    public LearningCircle? Circle { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    // E.g., "Member", "Facilitator"
    [MaxLength(50)]
    public string Role { get; set; } = "Member";
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class LearningCircleDiscussion
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string CircleId { get; set; } = string.Empty;
    [ForeignKey(nameof(CircleId))]
    public LearningCircle? Circle { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LearningCircleResource
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string CircleId { get; set; } = string.Empty;
    [ForeignKey(nameof(CircleId))]
    public LearningCircle? Circle { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Url { get; set; } = string.Empty;
    
    [Required]
    public string AddedByUserId { get; set; } = string.Empty;
    [ForeignKey(nameof(AddedByUserId))]
    public ApplicationUser? AddedByUser { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

public class LearningCircleJoinRequest
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string CircleId { get; set; } = string.Empty;
    [ForeignKey(nameof(CircleId))]
    public LearningCircle? Circle { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}

public class LearningCircleTask
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string CircleId { get; set; } = string.Empty;
    [ForeignKey(nameof(CircleId))]
    public LearningCircle? Circle { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // "Pending", "InProgress", "Completed"

    public DateTime? DueDate { get; set; }
    
    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;
    [ForeignKey(nameof(CreatedByUserId))]
    public ApplicationUser? CreatedByUser { get; set; }
    
    public string AssignedToUserId { get; set; } = string.Empty;
    [ForeignKey(nameof(AssignedToUserId))]
    public ApplicationUser? AssignedToUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LearningCircleNote
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string CircleId { get; set; } = string.Empty;
    [ForeignKey(nameof(CircleId))]
    public LearningCircle? Circle { get; set; }

    public string Content { get; set; } = string.Empty;

    [Required]
    public string UpdatedByUserId { get; set; } = string.Empty;
    [ForeignKey(nameof(UpdatedByUserId))]
    public ApplicationUser? UpdatedByUser { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
