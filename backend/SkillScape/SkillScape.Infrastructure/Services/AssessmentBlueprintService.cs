using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkillScape.Domain.Entities;

using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services
{

    public interface IAssessmentBlueprintService
    {
        Task<List<AssessmentSlot>> GenerateBlueprintAsync(AdaptiveQuizSession session, List<CareerTaxonomyV2> targetCareers, ParsedProfileDto profile);
        Task<AssessmentSlot?> GetNextAvailableSlotAsync(AdaptiveQuizSession session);
        Task MarkSlotCompletedAsync(AdaptiveQuizSession session, int slotIndex, string questionId);
    }

    public class AssessmentBlueprintService : IAssessmentBlueprintService
    {
        private readonly IOpenAIService _llmService;
        private readonly Microsoft.Extensions.Logging.ILogger<AssessmentBlueprintService> _logger;

        public AssessmentBlueprintService(IOpenAIService llmService, Microsoft.Extensions.Logging.ILogger<AssessmentBlueprintService> logger)
        {
            _llmService = llmService;
            _logger = logger;
        }

        public async Task<List<AssessmentSlot>> GenerateBlueprintAsync(AdaptiveQuizSession session, List<CareerTaxonomyV2> targetCareers, ParsedProfileDto profile)
        {
            var plan = new List<AssessmentSlot>();
            int slotCounter = 0;
            var subdomains = GetSubdomains(targetCareers);
            var careerIds = GetIds(targetCareers);

            var competencies = new List<string> { "problem_solving", "analytical_reasoning", "decision_making", "crisis_management", "hands_on", "specialization_interest", "advanced_tooling", "team_vs_solo", "structured_vs_dynamic", "domain_curiosity", "edge_case_handling", "advanced_reasoning", "tedious_vs_creative", "specialization_depth", "stakeholder_management", "industry_trends", "cross_functional" };
            
            try
            {
                var prompt = $@"You are an expert Career Assessment Analyzer.
Analyze the following target careers for a user with the given academic background.
Education Level: {session.PhaseKey}
Degree: {profile.Education?.Degree}
Major: {profile.Education?.Specialization}
Target Careers: {string.Join(", ", targetCareers.Select(c => c.Name))}

Return ONLY valid JSON matching this exact structure:
{{
  ""candidate_profiles"": [
    {{
      ""career"": ""Career Name"",
      ""domain"": ""Domain"",
      ""subdomain"": ""Subdomain"",
      ""specialization"": ""Specialization"",
      ""competencies"": [
        ""competency1"",
        ""competency2"",
        ""competency3"",
        ""competency4""
      ]
    }}
  ]
}}";
                var responseJson = await _llmService.GetResponseAsync(prompt, "Please analyze these careers and return the JSON.");
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = JsonSerializer.Deserialize<CandidateProfileResponse>(responseJson, options);
                
                if (response?.CandidateProfiles != null && response.CandidateProfiles.Any())
                {
                    var dynamicCompetencies = new HashSet<string>();
                    foreach (var p in response.CandidateProfiles)
                    {
                        if (p.Competencies != null)
                        {
                            foreach (var c in p.Competencies)
                            {
                                dynamicCompetencies.Add(c.ToLower().Replace(" ", "_"));
                            }
                        }
                    }
                    if (dynamicCompetencies.Any())
                    {
                        competencies = dynamicCompetencies.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch dynamic competencies from LLM. Falling back to default comprehensive blueprint.");
            }

            // Generate all unique pairs of careers for pairwise discrimination
            var careerPairs = new List<List<string>>();
            for (int i = 0; i < careerIds.Count; i++)
            {
                for (int j = i + 1; j < careerIds.Count; j++)
                {
                    careerPairs.Add(new List<string> { careerIds[i], careerIds[j] });
                }
            }

            // Distribute the competencies across different slot types to maintain variety
            var slotTypes = new[] { "Competency", "Scenario", "TaskPreference", "Subdomain", "WorkStyle", "Knowledge" };
            for (int i = 0; i < competencies.Count; i++)
            {
                var slotType = slotTypes[i % slotTypes.Length];
                
                // For the first few questions, use pairwise discrimination. After that, discriminate between all.
                var discriminateList = (i < careerPairs.Count) ? careerPairs[i] : careerIds;

                var slot = new AssessmentSlot 
                { 
                    SlotIndex = slotCounter++, 
                    SlotType = slotType, 
                    TargetCompetency = competencies[i], 
                    DiscriminateBetween = discriminateList 
                };
                
                if (slotType == "Subdomain")
                {
                    slot.TargetSubdomains = subdomains;
                }
                plan.Add(slot);
            }

            session.QuestionPlanJson = JsonSerializer.Serialize(plan);
            
            return await Task.FromResult(plan);
        }

        public async Task<AssessmentSlot?> GetNextAvailableSlotAsync(AdaptiveQuizSession session)
        {
            if (string.IsNullOrEmpty(session.QuestionPlanJson) || session.QuestionPlanJson == "[]")
                return null;

            var plan = JsonSerializer.Deserialize<List<AssessmentSlot>>(session.QuestionPlanJson);
            if (plan == null) return null;

            foreach (var slot in plan)
            {
                if (!slot.IsCompleted)
                    return await Task.FromResult(slot);
            }
            return null; // All slots complete
        }

        public async Task MarkSlotCompletedAsync(AdaptiveQuizSession session, int slotIndex, string questionId)
        {
            if (string.IsNullOrEmpty(session.QuestionPlanJson)) return;

            var plan = JsonSerializer.Deserialize<List<AssessmentSlot>>(session.QuestionPlanJson);
            if (plan == null) return;

            var slot = plan.Find(s => s.SlotIndex == slotIndex);
            if (slot != null)
            {
                slot.IsCompleted = true;
                slot.AskedQuestionId = questionId;
                session.QuestionPlanJson = JsonSerializer.Serialize(plan);
            }
            await Task.CompletedTask;
        }

        private List<string> GetIds(List<CareerTaxonomyV2> careers)
        {
            var ids = new List<string>();
            foreach (var c in careers) ids.Add(c.Id);
            return ids;
        }

        private List<string> GetSubdomains(List<CareerTaxonomyV2> careers)
        {
            var subdomains = new HashSet<string>();
            foreach (var c in careers)
            {
                if (!string.IsNullOrEmpty(c.SubdomainsJson) && c.SubdomainsJson != "[]")
                {
                    try
                    {
                        var subs = JsonSerializer.Deserialize<List<string>>(c.SubdomainsJson);
                        if (subs != null)
                        {
                            foreach (var s in subs) subdomains.Add(s);
                        }
                    }
                    catch { }
                }
            }
            return new List<string>(subdomains);
        }
    }

    public class CandidateProfileResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("candidate_profiles")]
        public List<CandidateProfile> CandidateProfiles { get; set; } = new();
    }

    public class CandidateProfile
    {
        [System.Text.Json.Serialization.JsonPropertyName("career")]
        public string Career { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonPropertyName("subdomain")]
        public string Subdomain { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonPropertyName("specialization")]
        public string Specialization { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonPropertyName("competencies")]
        public List<string> Competencies { get; set; } = new();
    }
}
