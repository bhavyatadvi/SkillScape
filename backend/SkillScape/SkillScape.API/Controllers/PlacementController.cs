using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.API.Controllers;

/// <summary>
/// Placement Readiness & Skill Gap Analysis API
/// 
/// Endpoints:
///   GET  /api/placement/roles              → list available target roles
///   GET  /api/placement/assessment/{role}  → get role-specific questions
///   POST /api/placement/assessment/submit  → submit answers, get ML result
///   GET  /api/placement/history            → user's assessment history
///   GET  /api/placement/assessment/{id}    → get specific assessment result
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlacementController : ControllerBase
{
    private readonly IPlacementService _placementService;
    private readonly ILogger<PlacementController> _logger;

    public PlacementController(IPlacementService placementService, ILogger<PlacementController> logger)
    {
        _placementService = placementService;
        _logger = logger;
    }

    /// <summary>
    /// Returns all available target job roles with metadata
    /// </summary>
    [HttpGet("roles")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<PlacementRoleDto>>>> GetRoles()
    {
        try
        {
            var roles = await _placementService.GetAvailableRolesAsync();
            return Ok(ApiResponse<List<PlacementRoleDto>>.SuccessResponse(roles, "Placement roles retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching placement roles");
            return StatusCode(500, ApiResponse<List<PlacementRoleDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Returns role-specific assessment questions (correct answers excluded from response)
    /// </summary>
    [HttpGet("assessment/{role}")]
    public async Task<ActionResult<ApiResponse<List<PlacementQuestionDto>>>> GetAssessmentQuestions(string role)
    {
        try
        {
            var decodedRole = Uri.UnescapeDataString(role);
            var questions = await _placementService.GetAssessmentQuestionsAsync(decodedRole);

            if (!questions.Any())
                return NotFound(ApiResponse<List<PlacementQuestionDto>>.ErrorResponse(
                    $"No assessment questions found for role: {decodedRole}"));

            return Ok(ApiResponse<List<PlacementQuestionDto>>.SuccessResponse(
                questions, $"Assessment questions for {decodedRole} retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching assessment questions for role {Role}", role);
            return StatusCode(500, ApiResponse<List<PlacementQuestionDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Submits assessment answers. Scores them server-side, runs ML model, returns full result.
    /// </summary>
    [HttpPost("assessment/submit")]
    public async Task<ActionResult<ApiResponse<PlacementResultDto>>> SubmitAssessment(
        [FromBody] SubmitPlacementRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<PlacementResultDto>.ErrorResponse("User not authenticated"));

            if (string.IsNullOrEmpty(request.TargetRole))
                return BadRequest(ApiResponse<PlacementResultDto>.ErrorResponse("Target role is required"));

            if (!request.Answers.Any())
                return BadRequest(ApiResponse<PlacementResultDto>.ErrorResponse("At least one answer is required"));

            var result = await _placementService.SubmitAssessmentAsync(userId, request);
            return Ok(ApiResponse<PlacementResultDto>.SuccessResponse(result, "Assessment completed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<PlacementResultDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting placement assessment");
            return StatusCode(500, ApiResponse<PlacementResultDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Returns the logged-in user's past assessment history
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<List<PlacementHistoryDto>>>> GetMyHistory()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<List<PlacementHistoryDto>>.ErrorResponse("User not authenticated"));

            var history = await _placementService.GetMyAssessmentHistoryAsync(userId);
            return Ok(ApiResponse<List<PlacementHistoryDto>>.SuccessResponse(
                history, $"Found {history.Count} past assessments"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching assessment history");
            return StatusCode(500, ApiResponse<List<PlacementHistoryDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Returns a specific past assessment result by ID
    /// </summary>
    [HttpGet("assessment/result/{assessmentId}")]
    public async Task<ActionResult<ApiResponse<PlacementResultDto>>> GetAssessmentResult(string assessmentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<PlacementResultDto>.ErrorResponse("User not authenticated"));

            var result = await _placementService.GetAssessmentByIdAsync(userId, assessmentId);
            return Ok(ApiResponse<PlacementResultDto>.SuccessResponse(result, "Assessment result retrieved"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<PlacementResultDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching assessment result {Id}", assessmentId);
            return StatusCode(500, ApiResponse<PlacementResultDto>.ErrorResponse(ex.Message));
        }
    }
}
