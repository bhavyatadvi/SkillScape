using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.API.Controllers;

[ApiController]
[Route("api/salary-intelligence")]
public class SalaryIntelligenceController : ControllerBase
{
    private readonly ISalaryIntelligenceService _salaryIntelligenceService;

    public SalaryIntelligenceController(ISalaryIntelligenceService salaryIntelligenceService)
    {
        _salaryIntelligenceService = salaryIntelligenceService;
    }

    [HttpPost("forecast")]
    public async Task<IActionResult> ForecastSalary([FromBody] SalaryForecastRequestV2 request)
    {
        var result = await _salaryIntelligenceService.ForecastSalaryAsync(request);
        return Ok(new { data = result });
    }

    [HttpPost("compare")]
    public async Task<IActionResult> CompareCareers([FromBody] BatchSalaryForecastRequest request)
    {
        var result = await _salaryIntelligenceService.CompareCareersAsync(request);
        return Ok(new { data = result });
    }
}
