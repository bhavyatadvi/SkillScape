using System.Collections.Generic;
using System.Threading.Tasks;
using SkillScape.Domain.Entities;

namespace SkillScape.Application.Interfaces;

public interface IQuestionGenerationService
{
    /// <summary>
    /// Generates and validates a batch of candidate questions dynamically based on the current state.
    /// </summary>
    Task<List<QuizQuestionV2>> GetValidatedCandidatesAsync(
        string phaseKey, 
        string subPhaseKey, 
        Dictionary<string, double> prior, 
        List<string> answeredQuestionIds);
}

public interface IQuestionQualityValidator
{
    bool IsValid(QuizQuestionV2 question);
}

public interface IQuestionRelevanceValidator
{
    bool IsRelevant(QuizQuestionV2 question, Dictionary<string, double> prior);
}

public interface IAgeLevelValidator
{
    bool IsAgeAppropriate(QuizQuestionV2 question, string phaseKey);
}

public interface IAnswerOptionValidator
{
    bool AreOptionsValid(QuizQuestionV2 question);
}

public interface IDuplicateDetector
{
    Task<bool> IsDuplicateAsync(QuizQuestionV2 candidate, List<string> answeredQuestionIds);
    Task<double> CalculateNoveltyScoreAsync(QuizQuestionV2 candidate, List<string> answeredQuestionIds);
}

public interface ISemanticSimilarityService
{
    Task<double> ComputeSimilarityAsync(string text1, string text2);
}
