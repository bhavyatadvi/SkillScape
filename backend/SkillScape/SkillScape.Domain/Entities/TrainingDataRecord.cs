using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillScape.Domain.Entities;

[Table("TrainingDataRecords")]
public class TrainingDataRecord
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Dataset Versioning
    public string DatasetVersion { get; set; } = "v1.0";
    
    // Features (Input)
    public string EducationStage { get; set; } = string.Empty;
    public string Stream { get; set; } = string.Empty;
    public float MathematicsScore { get; set; } // Numerical proxy for "strong/medium/low"
    public float PhysicsScore { get; set; }
    public float ProgrammingInterest { get; set; }
    public float SpatialReasoning { get; set; }
    public float HandsOnInterest { get; set; }
    public string QuestionResponsesJson { get; set; } = "[]"; // Serialized raw answers for specific embeddings/features
    public string SkillSignalsJson { get; set; } = "{}"; // Aggregated signal weights
    
    // Labels (Targets for the distinct models)
    public string DomainLabel { get; set; } = string.Empty; // For Model A
    public string FieldLabel { get; set; } = string.Empty; // For Model B
    public string SubFieldLabel { get; set; } = string.Empty; // For Model C
    public string SpecializationLabel { get; set; } = string.Empty; // For Model D
    public string CareerFamilyLabel { get; set; } = string.Empty; // For Model E
    public string OccupationLabel { get; set; } = string.Empty; // Fine-grained Model
}
