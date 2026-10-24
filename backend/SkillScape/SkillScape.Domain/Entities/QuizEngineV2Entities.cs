using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SkillScape.Domain.Entities;

// ─────────────────────────────────────────────────────────────────────────────
// ADAPTIVE QUIZ ENGINE V2 — Domain Entities
//
// Architecture:
//   Phase → ProfileCollection → AdaptiveQuizSession → QuizQuestionV2 (with signals)
//   → BayesianEngine → EligibilityConstraintV2 → QuizResultV2
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Represents the complete career taxonomy tree.
/// Each node belongs to exactly one level of the hierarchy:
///   Level 0: Grand Domain   (e.g. Science, Commerce, Arts)
///   Level 1: Domain         (e.g. Engineering, Medical, Law)
///   Level 2: Field          (e.g. Civil Engineering, MBBS, LLB)
///   Level 3: Subfield       (e.g. Structural Engineering, Cardiology)
///   Level 4: Specialization (e.g. Bridge Engineer, Interventional Cardiologist)
/// </summary>
public class CareerTaxonomyV2
{
    public string Id { get; set; } = string.Empty;          // e.g. "sci.eng.civil.struct"
    public string NodeType { get; set; } = "career";        // "career", "degree", "prof_qual"
    public string ParentId { get; set; } = string.Empty;    // e.g. "sci.eng.civil"
    public int Level { get; set; }                          // 0..4
    public string Name { get; set; } = string.Empty;        // e.g. "Structural Engineering"
    public string ShortCode { get; set; } = string.Empty;   // e.g. "STRUCT_ENG"
    public string Description { get; set; } = string.Empty;
    public string PhasesApplicableJson { get; set; } = "[]"; // JSON: ["After12th","AfterGraduation"]
    public string KeySubjectsRequired { get; set; } = string.Empty; // comma-separated subject codes
    public string TypicalRolesJson { get; set; } = "[]";    // JSON array of role titles
    public string RequiredDegreesJson { get; set; } = "[]";
    public string RecommendedCoursesJson { get; set; } = "[]";
    public string InstitutionTypesJson { get; set; } = "[]"; // IIT, NIT, Private, etc.
    public string EntranceExamsJson { get; set; } = "[]";   // JEE, NEET, CLAT, etc.
    public string SalaryRangeJson { get; set; } = "{}";     // {"entry": 400000, "senior": 1500000}
    public string TimeToCompleteQualification { get; set; } = string.Empty;
    public string TimeToJobReady { get; set; } = string.Empty;
    public string EntryLevelSalaryRange { get; set; } = string.Empty;
    public string ExperiencedSalaryRange { get; set; } = string.Empty;
    public string GovernmentExamsApplicableJson { get; set; } = "[]";
    public string RequiredExperience { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public DateTime? LastVerifiedDate { get; set; }
    public double IndustryGrowthRate { get; set; }           // CAGR %
    public double DemandIndex { get; set; }                  // 0–100
    public string Icon { get; set; } = "briefcase";
    public string ColorHex { get; set; } = "#4F46E5";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Deterministic Career Knowledge Mapping
    public string Domain { get; set; } = string.Empty;              // e.g. "Healthcare", "Technology", "Commerce"
    public string Subdomain { get; set; } = string.Empty;           // e.g. "Clinical Medicine", "Cybersecurity"
    public string Specialization { get; set; } = string.Empty;      // e.g. "Internal Medicine", "Offensive Security"
    public string EligibleEducationLevelsJson { get; set; } = "[]"; // e.g. ["After10th", "After12th", "Graduation", "Postgraduation", "CareerSwitch"]
    public string TraitWeightsJson { get; set; } = "{}";            // e.g. {"diagnostic_reasoning": 0.95, "patient_communication": 0.85}
    public string AptitudeDimensionsJson { get; set; } = "{}";      // e.g. {"analytical_reasoning": 0.9, "spatial_reasoning": 0.4}
    public string WorkStyleDimensionsJson { get; set; } = "{}";     // e.g. {"high_pressure": 0.9, "hands_on": 0.8}
    public string CompetencyDimensionsJson { get; set; } = "[]";    // e.g. ["threat_detection", "incident_response"]
    public string InterestDimensionsJson { get; set; } = "[]";   
    public string WorkTaskDimensionsJson { get; set; } = "[]";
    public string KnowledgeAreasJson { get; set; } = "[]";
    public string QuestionTagsJson { get; set; } = "[]";
    public string SubdomainsJson { get; set; } = "[]";
    public string ScoringWeightsJson { get; set; } = "{}";
}

/// <summary>
/// A question in the V2 question bank.
/// Every question has explicit semantic metadata, target discriminators, and dimension weights per option.
/// </summary>
public class QuizQuestionV2
{
    public string Id { get; set; } = string.Empty;          // e.g. "Q_MED_CLIN_001", "Q_CYBER_SOC_001"
    public string PhaseKey { get; set; } = string.Empty;    // After10th, After12th, AfterGraduation, AfterPG, CareerSwitch
    public string SubPhaseKey { get; set; } = string.Empty; // empty or pcm/pcb/commerce/arts/vocational/medical/tech
    public string EducationLevelsJson { get; set; } = "[]"; // JSON array of applicable levels: ["10th", "12th", "Graduation", "Postgraduation"]
    public string Domain { get; set; } = string.Empty;      // e.g. "Healthcare", "Technology"
    public string Subdomain { get; set; } = string.Empty;   // e.g. "Clinical Medicine", "Cybersecurity"
    public string Specialization { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty; // Situational, ProblemSolving, WorkPreference, DomainScenario
    public string LanguageComplexity { get; set; } = "Simple"; // Simple, Intermediate, Advanced
    public string Text { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public string Dimension { get; set; } = string.Empty;
    public string FieldId { get; set; } = string.Empty;
    public string DomainId { get; set; } = string.Empty;
    
    // Granular Question Metadata & Discrimination Targets
    public string ConceptId { get; set; } = string.Empty;
    public string CompetencyId { get; set; } = string.Empty;
    public string SubdomainIdsJson { get; set; } = "[]";
    public string TargetDiscriminatesJson { get; set; } = "[]"; // JSON array of career IDs this question differentiates
    public string DimensionWeightsJson { get; set; } = "{}";    // JSON map of dimensions tested
    public string CareerWeightsJson { get; set; } = "{}";       // JSON map of career weights

    /// <summary>
    /// JSON array of Option objects with full human-readable text, dimension scores, and signals.
    /// Format: [{"index": 0, "text": "...", "signals": [{"nodeId": "...", "weight": 1.0}], "dimensionScores": {"...": 5}}]
    /// </summary>
    public string OptionsJson { get; set; } = "[]";

    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Per-question-option signal weight toward a specific taxonomy node.
/// Normalized weights are used by the Bayesian engine.
/// </summary>
public class QuizOptionSignalV2
{
    public int Id { get; set; }
    public string QuestionId { get; set; } = string.Empty;
    public int OptionIndex { get; set; }                    // 0-based
    public string TaxonomyNodeId { get; set; } = string.Empty; // e.g. "sci.eng.civil"
    public double SignalWeight { get; set; }                // 0.0 to 1.0
}

/// <summary>
/// Hard or soft constraint preventing a taxonomy node from being recommended
/// when a student's profile does not meet the prerequisite.
/// </summary>
public class EligibilityConstraintV2
{
    public int Id { get; set; }
    public string TaxonomyNodeId { get; set; } = string.Empty;
    public string PhaseKey { get; set; } = string.Empty;
    public string RequiredSubjectCode { get; set; } = string.Empty; // e.g. "PHYSICS", "BIOLOGY", "MATHEMATICS"
    public string RequiredMinPerformance { get; set; } = "any"; // any, average, good, excellent
    public bool IsHardConstraint { get; set; } = true; // hard = exclude, soft = penalize
    public double SoftPenaltyFactor { get; set; } = 0.3; // multiplier if soft and violated
}

/// <summary>
/// A live adaptive quiz session tracking the student's progress and probability state.
/// </summary>
public class AdaptiveQuizSession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = string.Empty;

    public string AssessmentType { get; set; } = string.Empty; // Determines the module (e.g. GOVERNMENT_JOBS, CAREER_SWITCH)
    public string PhaseKey { get; set; } = string.Empty;
    public string SubPhaseKey { get; set; } = string.Empty;

    /// <summary>JSON map of profile fields collected from BackgroundForm.</summary>
    public string BackgroundProfileJson { get; set; } = "{}";

    /// <summary>Structured contextual intelligence output from ProfileMapper.</summary>
    public string AssessmentContextJson { get; set; } = "{}";

    /// <summary>JSON map: taxonomyNodeId → probability (normalized, sums to 1.0)</summary>
    public string ProbabilityDistributionJson { get; set; } = "{}";

    /// <summary>JSON array of question IDs already asked, in order.</summary>
    public string AnsweredQuestionIdsJson { get; set; } = "[]";

    /// <summary>JSON array of {questionId, optionIndex} objects.</summary>
    public string AnswersJson { get; set; } = "[]";

    /// <summary>JSON object holding arbitrary session state (e.g. asked intents, current stage).</summary>
    public string StateJson { get; set; } = "{}";

    public string Status { get; set; } = "Active"; // Active, Completed, Abandoned
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    // Phase 2 Additions: Target Mode and Multi-Dimensional State
    public string Mode { get; set; } = "Discovery"; // Target vs Discovery
    public string TargetCareersJson { get; set; } = "[]"; // Careers specified by user
    public string QuestionPlanJson { get; set; } = "[]"; // The blueprint sequence
    public string CompetencyScoresJson { get; set; } = "{}";
    public string CareerScoresJson { get; set; } = "{}";
    public string SubdomainScoresJson { get; set; } = "{}";
    public double ConfidenceScore { get; set; } = 0.0;
}

/// <summary>
/// Historical record of questions asked, to prevent semantic repetition.
/// </summary>
public class QuizQuestionRecordV2
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public string ConceptId { get; set; } = string.Empty;
    public string CompetencyTested { get; set; } = string.Empty;
    public string DiscriminatorTargetsJson { get; set; } = "[]";
    public DateTime AskedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// The final result of a completed V2 quiz session.
/// Every field is derived from the actual answers — never static default text.
/// </summary>
public class QuizResultV2
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string PhaseKey { get; set; } = string.Empty;

    /// <summary>The taxonomy node ID of the primary recommendation.</summary>
    public string PrimaryTaxonomyNodeId { get; set; } = string.Empty;

    /// <summary>JSON array of {nodeId, probability, matchPercent} objects, top 5.</summary>
    public string TopPredictionsJson { get; set; } = "[]";

    public string TopSubdomainsJson { get; set; } = "[]";
    public string SubdomainEvidenceJson { get; set; } = "[]";

    /// <summary>Final confidence score 0–100, reflecting distribution entropy.</summary>
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// JSON array of evidence items — each item explains which question+option
    /// contributed how much to the final recommendation.
    /// [{questionText, selectedOptionText, signalsContributed: [{nodeId, weight}]}]
    /// </summary>
    public string EvidenceLogJson { get; set; } = "[]";

    /// <summary>JSON array of skills to develop for the recommended path.</summary>
    public string SkillGapJson { get; set; } = "[]";

    /// <summary>JSON object describing the recommended education path.</summary>
    public string EducationPathJson { get; set; } = "{}";

    /// <summary>JSON array of recommended courses/certifications.</summary>
    public string RecommendedCoursesJson { get; set; } = "[]";

    /// <summary>JSON array of eligibility warnings (soft constraint violations).</summary>
    public string EligibilityWarningsJson { get; set; } = "[]";

    /// <summary>Name of the ML model used (or "RuleBased" if fallback).</summary>
    public string ModelUsed { get; set; } = "RuleBased";

    /// <summary>JSON map of raw Bayesian scores per taxonomy node.</summary>
    public string RuleBasedScoresJson { get; set; } = "{}";

    /// <summary>Human-readable explanation generated from evidence (NOT a static template).</summary>
    public string WhyRecommended { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
