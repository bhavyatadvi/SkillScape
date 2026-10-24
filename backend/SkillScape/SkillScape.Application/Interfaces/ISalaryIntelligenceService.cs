using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface ISalaryIntelligenceService
{
    Task<SalaryForecastResponseV2> ForecastSalaryAsync(SalaryForecastRequestV2 request);
    Task<BatchSalaryForecastResponse> CompareCareersAsync(BatchSalaryForecastRequest request);
}
