using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillScape.Domain.Entities;

// PART 34: VERSIONING
public interface IVersionedEntity
{
    string EntityId { get; set; }
    string Version { get; set; }
    bool IsActive { get; set; }
}

public abstract class VersionedEntityBase : IVersionedEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Version { get; set; } = "1.0";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ──────────────────────────────────────────────────────────────────────────
// PART 33: TAXONOMY ENTITIES
// ──────────────────────────────────────────────────────────────────────────

public class NormCareerDomain : VersionedEntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<NormCareerField> Fields { get; set; } = new List<NormCareerField>();
}

public class NormCareerField : VersionedEntityBase
{
    public int DomainId { get; set; }
    public NormCareerDomain Domain { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<NormCareerSubField> SubFields { get; set; } = new List<NormCareerSubField>();
}

public class NormCareerSubField : VersionedEntityBase
{
    public int FieldId { get; set; }
    public NormCareerField Field { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<NormCareerSpecialization> Specializations { get; set; } = new List<NormCareerSpecialization>();
}

public class NormCareerSpecialization : VersionedEntityBase
{
    public int SubFieldId { get; set; }
    public NormCareerSubField SubField { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class NormCareerFamily : VersionedEntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<NormOccupation> Occupations { get; set; } = new List<NormOccupation>();
}

public class NormOccupation : VersionedEntityBase
{
    public int FamilyId { get; set; }
    public NormCareerFamily Family { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OnetCode { get; set; } = string.Empty;
    public string EscoCode { get; set; } = string.Empty;
}

// ──────────────────────────────────────────────────────────────────────────
// EDUCATIONAL ENTITIES
// ──────────────────────────────────────────────────────────────────────────

public class NormEducationStage : VersionedEntityBase
{
    public string StageName { get; set; } = string.Empty; // e.g. "After 10th", "After 12th"
    public int OrderIndex { get; set; }
}

public class NormCourse : VersionedEntityBase
{
    public string Title { get; set; } = string.Empty;
    public string DegreeType { get; set; } = string.Empty; // B.Tech, B.Sc
    public int DurationYears { get; set; }
}

public class NormSubject : VersionedEntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class NormCertification : VersionedEntityBase
{
    public string Name { get; set; } = string.Empty;
    public string IssuingAuthority { get; set; } = string.Empty;
}

public class NormGovernmentExam : VersionedEntityBase
{
    public string Name { get; set; } = string.Empty;
    public string OrganizingBody { get; set; } = string.Empty;
}

public class NormEntranceExam : VersionedEntityBase
{
    public string Name { get; set; } = string.Empty;
    public string OrganizingBody { get; set; } = string.Empty;
    public string ForDegreeType { get; set; } = string.Empty;
}

// ──────────────────────────────────────────────────────────────────────────
// ASSESSMENT ENTITIES
// ──────────────────────────────────────────────────────────────────────────

public class NormSkill : VersionedEntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Hard, Soft
}

public class NormAbility : VersionedEntityBase
{
    public string Name { get; set; } = string.Empty;
    public string MeasurementType { get; set; } = string.Empty;
}

public class NormInterest : VersionedEntityBase
{
    public string Name { get; set; } = string.Empty;
    public string HollandCode { get; set; } = string.Empty; // RIASEC mapping
}

// ──────────────────────────────────────────────────────────────────────────
// MAPPINGS
// ──────────────────────────────────────────────────────────────────────────

public class NormCareerSkillMapping : VersionedEntityBase
{
    public int OccupationId { get; set; }
    public NormOccupation Occupation { get; set; } = null!;
    public int SkillId { get; set; }
    public NormSkill Skill { get; set; } = null!;
    public string ProficiencyLevel { get; set; } = string.Empty;
}

public class NormCareerEducationMapping : VersionedEntityBase
{
    public int OccupationId { get; set; }
    public NormOccupation Occupation { get; set; } = null!;
    public int CourseId { get; set; }
    public NormCourse Course { get; set; } = null!;
    public bool IsMandatory { get; set; }
}

public class NormCareerOccupationMapping : VersionedEntityBase
{
    public int SpecializationId { get; set; }
    public NormCareerSpecialization Specialization { get; set; } = null!;
    public int OccupationId { get; set; }
    public NormOccupation Occupation { get; set; } = null!;
}

// ──────────────────────────────────────────────────────────────────────────
// QUIZ ENGINE ENTITIES
// ──────────────────────────────────────────────────────────────────────────

public class NormQuestionBank : VersionedEntityBase
{
    public string QuestionText { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Difficulty { get; set; } = 1;
    public int SourceId { get; set; }
    public NormQuestionSource Source { get; set; } = null!;
    
    public ICollection<NormQuestionOption> Options { get; set; } = new List<NormQuestionOption>();
    public ICollection<NormQuestionFieldMapping> TargetFields { get; set; } = new List<NormQuestionFieldMapping>();
}

public class NormQuestionOption : VersionedEntityBase
{
    public int QuestionId { get; set; }
    public NormQuestionBank Question { get; set; } = null!;
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrectAnswer { get; set; } // If it's an ability question
    public ICollection<NormQuestionSignal> Signals { get; set; } = new List<NormQuestionSignal>();
}

public class NormQuestionSignal : VersionedEntityBase
{
    public int OptionId { get; set; }
    public NormQuestionOption Option { get; set; } = null!;
    public string DimensionType { get; set; } = string.Empty; // Skill, Interest, Field
    public string DimensionId { get; set; } = string.Empty; // E.g., 'field_cs'
    public double Weight { get; set; }
}

public class NormQuestionSource : VersionedEntityBase
{
    public string SourceName { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ReferenceUrl { get; set; } = string.Empty;
}

public class NormQuestionFieldMapping : VersionedEntityBase
{
    public int QuestionId { get; set; }
    public NormQuestionBank Question { get; set; } = null!;
    public int FieldId { get; set; }
    public NormCareerField Field { get; set; } = null!;
    public double DiscriminatorWeight { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────
// SYSTEM & ML ENTITIES
// ──────────────────────────────────────────────────────────────────────────

public class NormQuizSession : VersionedEntityBase
{
    public string UserId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = "InProgress";
    
    public ICollection<NormQuizResponse> Responses { get; set; } = new List<NormQuizResponse>();
    public ICollection<NormPredictionResult> Predictions { get; set; } = new List<NormPredictionResult>();
}

public class NormQuizResponse : VersionedEntityBase
{
    public int SessionId { get; set; }
    public NormQuizSession Session { get; set; } = null!;
    public int QuestionId { get; set; }
    public NormQuestionBank Question { get; set; } = null!;
    public int OptionId { get; set; }
    public NormQuestionOption Option { get; set; } = null!;
    public double TimeTakenSeconds { get; set; }
}

public class NormPredictionResult : VersionedEntityBase
{
    public int SessionId { get; set; }
    public NormQuizSession Session { get; set; } = null!;
    public int ModelVersionId { get; set; }
    public NormModelVersion ModelVersion { get; set; } = null!;
    public string PredictionJson { get; set; } = string.Empty;
}

public class NormModelVersion : VersionedEntityBase
{
    public string ModelName { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public double ValidationAccuracy { get; set; }
    public string BlobStoragePath { get; set; } = string.Empty;
}

public class NormDataSource : VersionedEntityBase
{
    public string SourceName { get; set; } = string.Empty; // e.g. "O*NET 28.0", "ESCO 1.1"
    public string Description { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
}

public class NormDataIngestionLog : VersionedEntityBase
{
    public int DataSourceId { get; set; }
    public NormDataSource DataSource { get; set; } = null!;
    public string RunStatus { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public string ErrorLogs { get; set; } = string.Empty;
}
