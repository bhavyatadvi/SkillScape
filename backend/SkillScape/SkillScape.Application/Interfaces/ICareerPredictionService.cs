using System.Collections.Generic;
using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface ICareerPredictionService
{
    Task<PredictionResultDto> PredictCareerAsync(string userId, List<QuizResponseRequest> responses, string phaseKey);
}
