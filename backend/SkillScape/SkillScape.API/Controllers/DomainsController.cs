using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DomainsController : ControllerBase
{
    private readonly IDomainService _domainService;

    public DomainsController(IDomainService domainService)
    {
        _domainService = domainService;
    }

    /// <summary>
    /// Get all career domains
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<CareerDomainDto>>>> GetAllDomains()
    {
        try
        {
            var result = await _domainService.GetAllDomainsAsync();
            return Ok(ApiResponse<List<CareerDomainDto>>.SuccessResponse(result, "Domains retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<CareerDomainDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get domain with all skills
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CareerDomainDetailDto>>> GetDomainWithSkills(string id)
    {
        try
        {
            var result = await _domainService.GetDomainWithSkillsAsync(id);
            return Ok(ApiResponse<CareerDomainDetailDto>.SuccessResponse(result, "Domain details retrieved"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<CareerDomainDetailDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CareerDomainDetailDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get domain roadmap (learning path)
    /// </summary>
    [HttpGet("{id}/roadmap")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<RoadmapDto>>> GetDomainRoadmap(string id)
    {
        try
        {
            var result = await _domainService.GetDomainRoadmapAsync(id);
            return Ok(ApiResponse<RoadmapDto>.SuccessResponse(result, "Roadmap retrieved successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<RoadmapDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoadmapDto>.ErrorResponse(ex.Message));
        }
    }
}
