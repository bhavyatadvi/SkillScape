using System;
using System.ComponentModel.DataAnnotations;

namespace SkillScape.Domain.Entities;

public class QuizAssessmentRecord
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string EducationPhase { get; set; } = string.Empty;
    
    public string QuestionIdsJson { get; set; } = "[]";
    
    public string AnswerIdsJson { get; set; } = "[]";
    
    public string FeatureVectorJson { get; set; } = "{}";
    
    public string PredictedStream { get; set; } = string.Empty;
    
    public string PredictedSubStream { get; set; } = string.Empty;
    
    public double Confidence { get; set; }
    
    public string? StudentSelectedPathId { get; set; }
    
    public string? EventualConfirmedPathId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
