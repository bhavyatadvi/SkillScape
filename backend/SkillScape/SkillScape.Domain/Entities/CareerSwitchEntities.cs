using System;
using System.Collections.Generic;

namespace SkillScape.Domain.Entities
{
    public class CareerSwitchNode
    {
        public int Id { get; set; }
        public string NodeId { get; set; } // e.g. "sci.eng.civil"
        public string Title { get; set; }
        public string Description { get; set; }
        public string Industry { get; set; } // e.g. "Engineering"
        public string Domain { get; set; } // e.g. "Civil"
        
        // Requirements
        public string MinimumEducation { get; set; } // e.g. "Bachelors"
        public bool IsRegulated { get; set; } // True if requires license/specific degree

        // Relationships
        public ICollection<CareerTransition> OutgoingTransitions { get; set; } = new List<CareerTransition>();
        public ICollection<CareerTransition> IncomingTransitions { get; set; } = new List<CareerTransition>();
    }

    public class TransferableSkill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; } // e.g. "Analytical", "Communication"
    }

    public class CareerTransition
    {
        public int Id { get; set; }

        public int FromCareerId { get; set; }
        public CareerSwitchNode FromCareer { get; set; }

        public int ToCareerId { get; set; }
        public CareerSwitchNode ToCareer { get; set; }

        public string TransitionDifficulty { get; set; } // "Easy", "Moderate", "Difficult", "Major Reskilling"
        public string TransitionType { get; set; } // "Adjacent", "Cross-Functional", "Cross-Industry", "Regulated"
        
        public double CompatibilityScore { get; set; } // Base compatibility 0-100

        public string WhyItFits { get; set; }
        public string PotentialChallenges { get; set; }

        // JSON string of skill gaps, required certifications, entry roles
        public string ExistingSkillsUsed { get; set; }
        public string NewSkillsRequired { get; set; }
        public string EntryRoles { get; set; }
    }

    public class CareerSwitchQuestion
    {
        public string Id { get; set; } // e.g. "Q_SWITCH_001"
        public string Text { get; set; }
        public string QuestionCategory { get; set; } // "Motivations", "WorkStyle", "NegativePreferences", "DomainSpecific"
        public string DomainDiscriminator { get; set; } // If specific to a domain, e.g. "Engineering"
        
        public ICollection<CareerSwitchQuestionOption> Options { get; set; } = new List<CareerSwitchQuestionOption>();
    }

    public class CareerSwitchQuestionOption
    {
        public int Id { get; set; }
        public string CareerSwitchQuestionId { get; set; }
        public CareerSwitchQuestion Question { get; set; }

        public int Index { get; set; }
        public string Text { get; set; }

        public string AffectedCareerNodeId { get; set; } // The target career this affects
        public double Weight { get; set; } // +1.0 for positive interest, -1.0 for negative preference
    }
}
