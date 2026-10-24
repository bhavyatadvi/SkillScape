using System.Collections.Generic;

namespace SkillScape.Application.DTOs
{
    public class AssessmentSlot
    {
        public int SlotIndex { get; set; }
        public string SlotType { get; set; } = string.Empty; // e.g. "Scenario", "Competency", "TaskPreference", "Subdomain", "WorkStyle"
        public string TargetCompetency { get; set; } = string.Empty; // e.g. "analytical_reasoning"
        public List<string> DiscriminateBetween { get; set; } = new(); // e.g. ["sci.eng.mech", "sci.eng.cs"]
        public List<string> TargetSubdomains { get; set; } = new(); // e.g. ["cyber.network", "cyber.soc"]
        public bool IsCompleted { get; set; } = false;
        public string AskedQuestionId { get; set; } = string.Empty;
    }
}
