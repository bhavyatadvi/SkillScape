using System.Collections.Generic;

namespace SkillScape.Domain.Entities
{
    public class AssessmentContext
    {
        public string EducationLevel { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        
        public List<string> Interests { get; set; } = new();
        public List<string> TargetCareers { get; set; } = new();
        
        public string CareerGoalMode { get; set; } = string.Empty;
        public bool CareerChange { get; set; } = false;
        
        // Output from mapping
        public List<string> EligibleDomains { get; set; } = new();
        public List<string> EligibleSubdomains { get; set; } = new();
        public List<string> EligibleSpecializations { get; set; } = new();
        public List<string> CandidateCareers { get; set; } = new();
        public List<string> InterestSignals { get; set; } = new();
        
        // After Graduation Specific
        public string PreferenceMode { get; set; } = string.Empty;
        public List<string> SelectedFields { get; set; } = new();
        public List<string> SelectedCareers { get; set; } = new();
        
        // Engineering Specific
        public string DegreeIntent { get; set; } = string.Empty;
        public string EngineeringPreferenceMode { get; set; } = string.Empty;
        public List<string> PreferredEngineeringFields { get; set; } = new();
        public string LockedDomain { get; set; } = string.Empty;

        // After Post-Graduation Specific
        public string PreviousDegree { get; set; } = string.Empty;
        public string PostGraduationDegree { get; set; } = string.Empty;
        public string PostGraduationMajor { get; set; } = string.Empty;
        public string PostGraduationSpecialization { get; set; } = string.Empty;
        public string WorkExperience { get; set; } = string.Empty;

        // ── Domain-Aware Assessment Engine Fields (v2) ───────────────────────────

        /// <summary>
        /// Canonical primary domain from DomainTaxonomyEngine: "Healthcare", "Technology", "Commerce", etc.
        /// Used for question pool domain filtering — replaces EligibleDomains for AfterPostGraduation.
        /// </summary>
        public string DomainTaxonomyTag { get; set; } = string.Empty;

        /// <summary>
        /// Dimensions the quiz must NOT re-ask because they are already known from the profile.
        /// Example: "broad_field_preference", "pg_major_field", "specialization_area"
        /// </summary>
        public List<string> KnownFacts { get; set; } = new();

        /// <summary>
        /// Career families inferred from DomainTaxonomyEngine for the user's PG field.
        /// Used as discriminators in question selection and LLM prompt generation.
        /// Example: ["Clinical Practice", "Medical Research", "Healthcare Administration"]
        /// </summary>
        public List<string> InferredCareerFamilies { get; set; } = new();

        /// <summary>
        /// Dimensions already tested during the assessment (populated as quiz progresses).
        /// Used to prevent asking semantically duplicate questions about the same dimension.
        /// Example: ["clinical_vs_research", "solo_vs_team", "leadership_interest"]
        /// </summary>
        public List<string> TestedDimensions { get; set; } = new();
    }
}
