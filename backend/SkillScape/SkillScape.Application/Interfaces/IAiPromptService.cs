using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces
{
    public interface IAiPromptService
    {
        List<AiQuizQuestionDto> GenerateQuizQuestions(string category);
        AiCareerPredictionDto PredictCareer(string category, List<string> answers, Dictionary<string, string>? extraContext = null);
    }
}
