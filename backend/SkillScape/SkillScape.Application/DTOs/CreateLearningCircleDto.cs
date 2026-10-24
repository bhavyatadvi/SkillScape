using System.ComponentModel.DataAnnotations;

namespace SkillScape.Application.DTOs;

public class CreateLearningCircleDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string CareerDomain { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string CircleType { get; set; } = "Domain Circle";
    
    [MaxLength(500)]
    public string Tags { get; set; } = string.Empty;
}
