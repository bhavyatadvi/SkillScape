using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.API.Controllers;

/// <summary>
/// Career Readiness and Skill Gap Analyzer API.
/// All user-specific endpoints extract userId from JWT claims only.
/// </summary>
[ApiController]
[Route("api/skill-analyzer")]
[Authorize]
public class SkillAnalyzerController : ControllerBase
{
    private readonly ICareerIntelligenceService _intelligence;
    private readonly ISkillGapAnalyzerService _analyzerService;

    public SkillAnalyzerController(
        ICareerIntelligenceService intelligence,
        ISkillGapAnalyzerService analyzerService)
    {
        _intelligence = intelligence;
        _analyzerService = analyzerService;
    }

    private string? GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? User.FindFirst("id")?.Value;

    // ─── Profile Status ─────────────────────────────────────────────────────

    /// <summary>
    /// GET /api/skill-analyzer/profile-status
    /// Returns whether the authenticated user has a Career Profile and its completeness.
    /// </summary>
    [HttpGet("profile-status")]
    public async Task<IActionResult> GetProfileStatus()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized(new { error = "User not authenticated." });

        try
        {
            var status = await _analyzerService.GetProfileStatusAsync(userId);
            return Ok(new { success = true, data = status });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ─── Career Intelligence Lookup ─────────────────────────────────────────

    /// <summary>
    /// GET /api/skill-analyzer/domains
    /// Returns all available career domains. Open for use in preparation form.
    /// </summary>
    [HttpGet("domains")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDomains()
    {
        try
        {
            var domains = await _intelligence.GetDomainsAsync();
            return Ok(new { success = true, data = domains });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/skill-analyzer/subdomains?domainId=
    /// Returns sub-domains for a domain, or all sub-domains.
    /// </summary>
    [HttpGet("subdomains")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSubDomains([FromQuery] string? domainId)
    {
        try
        {
            var subdomains = await _intelligence.GetSubDomainsAsync(domainId);
            return Ok(new { success = true, data = subdomains });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/skill-analyzer/careers?domainId=&subDomainId=
    /// Returns careers filtered by domainId and/or subDomainId.
    /// </summary>
    [HttpGet("careers")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCareers([FromQuery] string? domainId, [FromQuery] string? subDomainId)
    {
        try
        {
            var careers = !string.IsNullOrWhiteSpace(subDomainId)
                ? await _intelligence.GetCareersBySubDomainAsync(subDomainId)
                : (!string.IsNullOrWhiteSpace(domainId)
                    ? await _intelligence.GetCareersByDomainAsync(domainId)
                    : await _intelligence.GetAllCareersAsync());

            // A few legacy records contain display-only specialization labels. The
            // intelligence service resolves those labels, then this final canonical
            // field filter prevents similarly named specializations in other fields
            // (for example Corporate Accounting vs Corporate Law) leaking into UI.
            if (!string.IsNullOrWhiteSpace(domainId))
            {
                careers = careers
                    .Where(c => c.DomainId.Equals(domainId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            return Ok(new { success = true, data = careers });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/skill-analyzer/careers/search?q=&domainId=&subDomainId=
    /// Searches careers by title, alternate names, or alias matching.
    /// </summary>
    [HttpGet("careers/search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchCareers([FromQuery] string q, [FromQuery] string? domainId, [FromQuery] string? subDomainId)
    {
        try
        {
            var careers = await _intelligence.SearchCareersAsync(q ?? string.Empty, domainId, subDomainId);
            return Ok(new { success = true, data = careers });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }


    /// <summary>
    /// GET /api/skill-analyzer/careers/{careerId}
    /// Returns full career detail including skill requirements and interest areas.
    /// </summary>
    [HttpGet("careers/{careerId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCareerDetail(string careerId)
    {
        try
        {
            var career = await _intelligence.GetCareerByIdAsync(careerId);
            if (career == null)
                return NotFound(new { error = $"Career '{careerId}' not found." });
            return Ok(new { success = true, data = career });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ─── Analysis ───────────────────────────────────────────────────────────

    /// <summary>
    /// POST /api/skill-analyzer/analyze
    /// Runs the full career readiness and skill gap analysis for the authenticated user.
    /// userId is always taken from JWT claims — the frontend cannot override it.
    /// </summary>
    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] SkillAnalyzerRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized(new { error = "User not authenticated." });

        if (string.IsNullOrWhiteSpace(request.CareerId) && string.IsNullOrWhiteSpace(request.CustomCareerName))
            return BadRequest(new { error = "Either CareerId or CustomCareerName is required." });

        // A canonical selection must retain its field → specialization → role relationship.
        // Custom roles remain supported as an explicit escape hatch.
        if (!string.IsNullOrWhiteSpace(request.CareerId) &&
            !await _intelligence.IsCareerInHierarchyAsync(request.CareerId, request.DomainId, request.SubDomainId))
        {
            return BadRequest(new { error = "The selected target role does not belong to the selected field and specialization." });
        }

        try
        {
            var result = await _analyzerService.AnalyzeAsync(userId, request);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
