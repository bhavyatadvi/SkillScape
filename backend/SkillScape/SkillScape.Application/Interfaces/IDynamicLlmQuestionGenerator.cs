using System.Collections.Generic;
using System.Threading.Tasks;
using SkillScape.Application.DTOs;
using SkillScape.Domain.Entities;

namespace SkillScape.Application.Interfaces;

public interface IDynamicLlmQuestionGenerator
{
    Task<QuizQuestionV2?> GenerateNextQuestionAsync(
        ParsedProfileDto profile,
        Dictionary<string, double> prior,
        List<EvidenceItemDto> evidenceItems,
        AssessmentSlot slot,
        List<string> answeredQuestionTexts,
        List<string> askedConceptIds,
        string phaseKey);
}
