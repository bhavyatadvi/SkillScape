using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.API.Controllers;

[ApiController]
[Route("api/career-guidance")]
[Authorize]
public class CareerGuidanceController : ControllerBase
{
    private readonly ICareerGuidanceService _guidanceService;

    public CareerGuidanceController(ICareerGuidanceService guidanceService)
    {
        _guidanceService = guidanceService;
    }

    /// <summary>
    /// Get career paths hierarchal tree by stream type
    /// </summary>
    [HttpGet("paths")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<CareerPathDto>>>> GetPathsTree([FromQuery] string streamType)
    {
        try
        {
            if (string.IsNullOrEmpty(streamType))
            {
                return BadRequest(ApiResponse<List<CareerPathDto>>.ErrorResponse("streamType is required"));
            }

            var result = await _guidanceService.GetCareerPathsTreeAsync(streamType);
            return Ok(ApiResponse<List<CareerPathDto>>.SuccessResponse(result, "Career path hierarchy retrieved"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<CareerPathDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get details of a single career path
    /// </summary>
    [HttpGet("paths/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CareerPathDto>>> GetPathById(string id)
    {
        try
        {
            var result = await _guidanceService.GetCareerPathByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<CareerPathDto>.ErrorResponse("Career path not found"));
            }

            return Ok(ApiResponse<CareerPathDto>.SuccessResponse(result, "Career path details retrieved"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CareerPathDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get adaptive quiz questions filtered by academic stage (stream)
    /// </summary>
    [HttpGet("quiz/questions")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<HierarchicalQuizQuestionDto>>>> GetQuizQuestions([FromQuery] string streamType)
    {
        try
        {
            if (string.IsNullOrEmpty(streamType))
            {
                return BadRequest(ApiResponse<List<HierarchicalQuizQuestionDto>>.ErrorResponse("streamType is required"));
            }

            var result = await _guidanceService.GetQuizQuestionsAsync(streamType);
            return Ok(ApiResponse<List<HierarchicalQuizQuestionDto>>.SuccessResponse(result, "Quiz questions retrieved"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<HierarchicalQuizQuestionDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Submit answers and get career recommendation profile
    /// </summary>
    [HttpPost("quiz/submit")]
    public async Task<ActionResult<ApiResponse<UserCareerProfileDto>>> SubmitQuiz([FromBody] QuizSubmissionRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<UserCareerProfileDto>.ErrorResponse("User not authenticated"));
            }

            if (request == null || string.IsNullOrEmpty(request.SelectedStream) || request.Answers == null)
            {
                return BadRequest(ApiResponse<UserCareerProfileDto>.ErrorResponse("Invalid quiz submission request"));
            }

            var result = await _guidanceService.SubmitQuizAnswersAsync(userId, request);
            return Ok(ApiResponse<UserCareerProfileDto>.SuccessResponse(result, "Quiz recommendation generated successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<UserCareerProfileDto>.ErrorResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<UserCareerProfileDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return StatusCode(500, ApiResponse<UserCareerProfileDto>.ErrorResponse($"{ex.Message} Inner: {innerMsg}"));
        }
    }

    /// <summary>
    /// Get current user's career profile recommendation
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<UserCareerProfileDto>>> GetProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<UserCareerProfileDto>.ErrorResponse("User not authenticated"));
            }

            var result = await _guidanceService.GetUserCareerProfileAsync(userId);
            if (result == null)
            {
                return NotFound(ApiResponse<UserCareerProfileDto>.ErrorResponse("No career profile found for user. Complete a career quiz first."));
            }

            return Ok(ApiResponse<UserCareerProfileDto>.SuccessResponse(result, "User career profile retrieved"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<UserCareerProfileDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserCareerProfileDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Toggle bookmarking a career path
    /// </summary>
    [HttpPost("bookmarks/{pathId}")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleBookmark(string pathId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
            }

            var bookmarked = await _guidanceService.ToggleBookmarkPathAsync(userId, pathId);
            string msg = bookmarked ? "Path bookmarked successfully" : "Path removed from bookmarks";
            return Ok(ApiResponse<bool>.SuccessResponse(bookmarked, msg));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Accept a career path and copy its roadmap as the active profile roadmap
    /// </summary>
    [HttpPost("accept/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> AcceptCareer(string id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
            }

            var result = await _guidanceService.AcceptCareerPathAsync(userId, id);
            if (!result)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Career path not found"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Career path accepted and roadmap initialized"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Retrieve bookmarked career paths
    /// </summary>
    [HttpGet("bookmarks")]
    public async Task<ActionResult<ApiResponse<List<CareerPathDto>>>> GetBookmarks()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<CareerPathDto>>.ErrorResponse("User not authenticated"));
            }

            var result = await _guidanceService.GetBookmarkedPathsAsync(userId);
            return Ok(ApiResponse<List<CareerPathDto>>.SuccessResponse(result, "Bookmarked career paths retrieved"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<List<CareerPathDto>>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<CareerPathDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Predict salary compounding based on sliders
    /// </summary>
    [HttpPost("forecast")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<SalaryForecastDto>>> ForecastSalary([FromBody] SalaryForecastRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.CareerPathId))
            {
                return BadRequest(ApiResponse<SalaryForecastDto>.ErrorResponse("CareerPathId is required"));
            }

            var result = await _guidanceService.ForecastSalaryAsync(request);
            return Ok(ApiResponse<SalaryForecastDto>.SuccessResponse(result, "Salary forecast generated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaryForecastDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SalaryForecastDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Retrieve metrics and aggregated charts data for the dashboard
    /// </summary>
    [HttpGet("analytics")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<GuidanceAnalyticsDto>>> GetAnalytics()
    {
        try
        {
            var result = await _guidanceService.GetGuidanceAnalyticsAsync();
            return Ok(ApiResponse<GuidanceAnalyticsDto>.SuccessResponse(result, "Guidance analytics compiled successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<GuidanceAnalyticsDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Compare two career paths side-by-side
    /// </summary>
    [HttpGet("compare")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<CareerCompareDto>>> CompareCareers([FromQuery] string id1, [FromQuery] string id2)
    {
        try
        {
            if (string.IsNullOrEmpty(id1) || string.IsNullOrEmpty(id2))
            {
                return BadRequest(ApiResponse<CareerCompareDto>.ErrorResponse("Both id1 and id2 parameters are required"));
            }

            var result = await _guidanceService.CompareCareersAsync(id1, id2);
            return Ok(ApiResponse<CareerCompareDto>.SuccessResponse(result, "Career path comparison retrieved"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CareerCompareDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CareerCompareDto>.ErrorResponse(ex.Message));
        }
    }
}
