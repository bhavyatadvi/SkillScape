using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Generates the next adaptive quiz question via the Groq LLM.
/// Phase 3 Architecture: Domain-specific, Blueprint-driven, No generic personality questions.
/// </summary>
public class LlmQuestionGeneratorService : IDynamicLlmQuestionGenerator
{
    private readonly IOpenAIService _llmService;
    private readonly ILogger<LlmQuestionGeneratorService> _logger;
    private readonly JsonSerializerOptions _jsonOpts;

    public LlmQuestionGeneratorService(
        IOpenAIService llmService,
        ILogger<LlmQuestionGeneratorService> logger)
    {
        _llmService = llmService;
        _logger = logger;
        _jsonOpts = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
    }

    public async Task<QuizQuestionV2?> GenerateNextQuestionAsync(
        ParsedProfileDto profile,
        Dictionary<string, double> prior,
        List<EvidenceItemDto> evidenceItems,
        AssessmentSlot slot,
        List<string> answeredQuestionTexts,
        List<string> askedConceptIds,
        string phaseKey)
    {
        try
        {
            // Build fully-contextualized prompts
            string systemPrompt = BuildSystemPrompt(slot, profile);
            string userMessage  = BuildUserPrompt(profile, prior, evidenceItems, answeredQuestionTexts, askedConceptIds, slot);

            _logger.LogInformation(
                "[AdaptiveQuiz] Generating LLM question. Type={Type} Competency={Competency} Field={Field}",
                slot.SlotType, slot.TargetCompetency, profile.Education?.Specialization);

            string jsonResponse = await _llmService.GetResponseAsync(systemPrompt, userMessage);

            // Strip any markdown wrapping
            jsonResponse = Regex.Replace(jsonResponse, @"```json\s*|```\s*", "").Trim();

            // Parse Phase 3 structure
            var llmOutput = JsonSerializer.Deserialize<LlmQuestionResponse>(jsonResponse, _jsonOpts);
            if (llmOutput == null || string.IsNullOrWhiteSpace(llmOutput.Question))
            {
                _logger.LogWarning("[AdaptiveQuiz] LLM returned invalid/empty response structure.");
                return null;
            }

            // Older prompts/providers return the same answer data under `options`.
            // Normalize that shape before applying the normal validation rules.
            if (llmOutput.AnswerMappings.Count == 0 && llmOutput.Options.Count > 0)
                llmOutput.AnswerMappings = llmOutput.Options;

            if (llmOutput.AnswerMappings.Count < 2)
            {
                _logger.LogWarning("[AdaptiveQuiz] LLM returned fewer than two answer options.");
                return null;
            }

            // Validate: question must not be semantically identical to a previous question
            if (IsSemanticDuplicate(llmOutput.Question, answeredQuestionTexts))
            {
                _logger.LogWarning("[AdaptiveQuiz] LLM generated a semantically duplicate question. Rejecting.");
                return null;
            }

            var entity = MapToEntity(llmOutput, phaseKey);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AdaptiveQuiz] Phase 3 LLM Generation FAILED.");
            throw; // Phase 3 explicitly bans silent fallbacks to generic questions
        }
    }

    private string BuildSystemPrompt(AssessmentSlot slot, ParsedProfileDto profile)
    {
        string field = profile.Education?.Specialization ?? profile.Education?.Degree ?? "General";
        
        return $@"You are the Career Assessment Engine for a professional career guidance platform.

Your job is NOT to give generic career advice.

Your job is to generate a scientifically structured, domain-specific career assessment that determines which of the user's TARGET CAREERS is the strongest compatibility match based on the user's answers.

==================================================
CORE PRINCIPLE
==================================================

The user's TARGET CAREERS are the candidate set.

You MUST NOT introduce unrelated careers unless explicitly instructed.

You MUST NOT ask generic questions about broad professional domains when the user's academic field and target careers are already known.

Every question must contribute measurable evidence that helps distinguish between the candidate careers.

==================================================
QUESTION GENERATION RULES
==================================================

Every question must satisfy ALL of the following:

1. It must be relevant to the user's education level ({profile.Education?.Level ?? "Not specified"}).
2. It must be relevant to the user's degree/major ({field}).
3. It must be relevant to at least TWO candidate careers OR provide strong evidence for one candidate career.
4. It must help discriminate between target careers.
5. It must test a meaningful career dimension.
6. It must NOT be a generic personality question unless that personality dimension is directly relevant to the candidate careers.
7. It must NOT repeat an existing question.
8. It must NOT be semantically equivalent to an existing question.
9. It must NOT repeat the same scenario with slightly different wording.
10. It must NOT ask the user to choose a career directly.
11. It must NOT reveal which answer corresponds to which career.
12. It must not contain leading language.
13. It must use realistic situations whenever possible.
14. It must test the actual subdomains of the candidate careers.

==================================================
TARGET CAREER CONSTRAINT
==================================================

If the target list contains specific careers, you MUST NOT suddenly ask questions about completely unrelated fields unless they are in the list.

==================================================
CURRENT BLUEPRINT SLOT
==================================================
You are generating a question for this specific slot:
- Slot Type: {slot.SlotType}
- Target Competency: {slot.TargetCompetency}
- Purpose: You must distinguish between the specific target careers provided by the user.

==================================================
ANSWER DESIGN
==================================================

Use exactly 4 options.
Each option must represent a different measurable preference.
Do NOT make:
A = obviously correct
B = obviously wrong
C = neutral
D = unrelated
All options must be plausible. Do not use 'All of the above' or 'None of the above'.

==================================================
REQUIRED JSON FORMAT
==================================================
Return ONLY valid JSON matching this exact structure:
{{
  ""questionId"": ""unique_id"",
  ""conceptId"": ""{slot.TargetCompetency}"",
  ""competencyId"": ""{slot.SlotType}"",
  ""careerIds"": [""list of career IDs this discriminates""],
  ""subdomainIds"": [""relevant subdomains""],
  ""questionType"": ""{slot.SlotType}"",
  ""difficulty"": ""Intermediate"",
  ""question"": ""Specific scenario-based question text"",
  ""answerMappings"": [
    {{
      ""text"": ""Option A text"",
      ""signals"": {{""career_node_id"": 0.9, ""career_node_id2"": 0.3}}
    }},
    {{
      ""text"": ""Option B text"",
      ""signals"": {{""career_node_id"": 0.9}}
    }},
    {{
      ""text"": ""Option C text"",
      ""signals"": {{}}
    }},
    {{
      ""text"": ""Option D text"",
      ""signals"": {{}}
    }}
  ]
}}
Do NOT generate all possible subdomains if they are irrelevant to the selected target career.
Generate exactly 4 answerMappings. Return NO explanatory text outside JSON.";
    }

    private string BuildUserPrompt(
        ParsedProfileDto profile,
        Dictionary<string, double> prior,
        List<EvidenceItemDto> evidenceItems,
        List<string> answeredTexts,
        List<string> askedConceptIds,
        AssessmentSlot slot)
    {
        var targetCareers = slot.DiscriminateBetween.Any() ? slot.DiscriminateBetween : prior.Keys.Take(5).ToList();
        
        var validNodeIds = targetCareers
            .Select(k => $"  {k}  → {CareerDisplayNameService.GetDisplayName(k)}")
            .ToList();

        var prevQs = answeredTexts.Select((q, i) => $"  {i + 1}. {q}").ToList();

        string targetsSection = $@"TARGET CAREERS TO DISCRIMINATE:
{string.Join("\n", validNodeIds)}";

        if (slot.TargetSubdomains != null && slot.TargetSubdomains.Any())
        {
            targetsSection += $"\n\nTARGET SUBDOMAINS TO DISCRIMINATE:\n{string.Join("\n", slot.TargetSubdomains.Select(s => $"  {s}"))}";
            targetsSection += "\n\nCRITICAL INSTRUCTION: For this question, use ONLY the Subdomain IDs listed above in your 'signals' dictionary. DO NOT use the Career IDs.";
        }
        else
        {
            targetsSection += "\n\nCRITICAL INSTRUCTION: Use ONLY the target career IDs listed above in your 'signals' dictionary. DO NOT invent IDs.";
        }

        return $@"STUDENT ACADEMIC PROFILE:
  Degree: {profile.Education?.Degree}
  Broad Field: {profile.Education?.Specialization}
  Major: {profile.Education?.Specialization}
  Assessment Mode: Target Career Comparison Mode

{targetsSection}

PREVIOUSLY ASKED QUESTIONS (DO NOT REPEAT):
{(prevQs.Any() ? string.Join("\n", prevQs) : "  None yet")}

TASK:
Generate the single most informative domain-specific question to test '{slot.TargetCompetency}'.";
    }

    private static bool IsSemanticDuplicate(string newQuestion, List<string> previousTexts)
    {
        if (!previousTexts.Any()) return false;

        string Normalize(string s) =>
            Regex.Replace(s.ToLowerInvariant(), @"[^a-z0-9\s]", "").Trim();

        var normNew = Normalize(newQuestion);
        var newWords = new HashSet<string>(normNew.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        foreach (var prev in previousTexts)
        {
            var normPrev = Normalize(prev);
            var prevWords = new HashSet<string>(normPrev.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            if (normNew == normPrev) return true;

            var intersection = newWords.Intersect(prevWords).Count();
            var union = newWords.Union(prevWords).Count();
            double jaccard = union > 0 ? (double)intersection / union : 0;
            if (jaccard > 0.65) return true;
        }
        return false;
    }

    private static QuizQuestionV2 MapToEntity(LlmQuestionResponse r, string phaseKey)
    {
        var options = r.AnswerMappings.Select((opt, i) =>
        {
            var signals = (opt.Signals ?? new()).Select(kv =>
                new { nodeId = kv.Key, weight = kv.Value }).ToList();
            return new { index = i, text = opt.Text, signals };
        }).ToList();

        return new QuizQuestionV2
        {
            Id                    = string.IsNullOrEmpty(r.QuestionId) ? $"LLM_{Guid.NewGuid():N}"[..20] : r.QuestionId,
            PhaseKey              = phaseKey,
            SubPhaseKey           = r.ConceptId,
            Text                  = r.Question,
            QuestionType          = r.QuestionType,
            Intent                = r.ConceptId,
            Dimension             = r.CompetencyId,
            ConceptId             = r.ConceptId,
            CompetencyId          = r.CompetencyId,
            TargetDiscriminatesJson = JsonSerializer.Serialize(r.CareerIds),
            SubdomainIdsJson      = JsonSerializer.Serialize(r.SubdomainIds),
            OptionsJson           = JsonSerializer.Serialize(options),
            LanguageComplexity    = r.Difficulty,
            DisplayOrder          = 999,
            IsActive              = false 
        };
    }

    private class LlmQuestionResponse
    {
        public string QuestionId { get; set; } = string.Empty;
        public string ConceptId { get; set; } = string.Empty;
        public string CompetencyId { get; set; } = string.Empty;
        public List<string> CareerIds { get; set; } = new();
        public List<string> SubdomainIds { get; set; } = new();
        public string QuestionType { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public List<LlmOption> AnswerMappings { get; set; } = new();
        public List<LlmOption> Options { get; set; } = new();
    }

    private class LlmOption
    {
        public string Text { get; set; } = string.Empty;
        public Dictionary<string, double>? Signals { get; set; }
    }
}
