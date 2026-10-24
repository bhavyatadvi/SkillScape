using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SkillScape.API.Hubs;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RoadmapController : ControllerBase
{
    private readonly IRoadmapService _roadmapService;
    private readonly IFreeformRoadmapService _freeformRoadmapService;
    private readonly IHubContext<ChatHub> _hubContext;

    public RoadmapController(
        IRoadmapService roadmapService,
        IFreeformRoadmapService freeformRoadmapService,
        IHubContext<ChatHub> hubContext)
    {
        _roadmapService = roadmapService;
        _freeformRoadmapService = freeformRoadmapService;
        _hubContext = hubContext;
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("id")?.Value;
    }

    [HttpGet("my-interest")]
    public async Task<ActionResult<ApiResponse<RoadmapDto>>> GetMyRoadmap()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<RoadmapDto>.ErrorResponse("User not authenticated"));

            var result = await _roadmapService.GetMyRoadmapAsync(userId);
            return Ok(ApiResponse<RoadmapDto>.SuccessResponse(result!, "Roadmap retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoadmapDto>.ErrorResponse($"Error retrieving roadmap: {ex.Message}"));
        }
    }

    [HttpGet("options")]
    public async Task<ActionResult<ApiResponse<List<RoadmapOptionDto>>>> GetRoadmapOptions()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<List<RoadmapOptionDto>>.ErrorResponse("User not authenticated"));

            var result = await _roadmapService.GetRoadmapOptionsAsync(userId);
            return Ok(ApiResponse<List<RoadmapOptionDto>>.SuccessResponse(result, "Roadmap options retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<RoadmapOptionDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("dismissed")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetDismissedRoadmaps()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<List<string>>.ErrorResponse("User not authenticated"));

            var result = await _roadmapService.GetDismissedRoadmapKeysAsync(userId);
            return Ok(ApiResponse<List<string>>.SuccessResponse(result, "Dismissed roadmaps retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<string>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("domain/{domainId}")]
    public async Task<ActionResult<ApiResponse<RoadmapDto>>> GetRoadmapByDomain(string domainId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<RoadmapDto>.ErrorResponse("User not authenticated"));

            var result = await _roadmapService.GetRoadmapByDomainAsync(userId, domainId);
            return Ok(ApiResponse<RoadmapDto>.SuccessResponse(result, "Roadmap retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoadmapDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("mark-done/{stepId}")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkStepComplete(string stepId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));

            var result = await _roadmapService.MarkStepCompleteAsync(userId, stepId);

            if (result)
            {
                await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new
                {
                    Title = "Step Completed!",
                    Message = "You successfully completed a roadmap module. XP added!",
                    Type = "achievement"
                });
            }

            return Ok(ApiResponse<bool>.SuccessResponse(result, "Step marked complete successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("module/{moduleId}/quiz")]
    public async Task<ActionResult<ApiResponse<RoadmapModuleQuizDto>>> GetModuleQuiz(string moduleId)
    {
        try
        {
            var quiz = await _roadmapService.GetModuleQuizAsync(moduleId);
            return Ok(ApiResponse<RoadmapModuleQuizDto>.SuccessResponse(quiz, "Module quiz retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoadmapModuleQuizDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("module/{moduleId}/quiz/submit")]
    public async Task<ActionResult<ApiResponse<RoadmapQuizResultDto>>> SubmitModuleQuiz(string moduleId, [FromBody] QuizSubmissionDto submission)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<RoadmapQuizResultDto>.ErrorResponse("User not authenticated"));

            var result = await _roadmapService.SubmitModuleQuizAsync(userId, moduleId, submission.Answers);

            if (result.Passed)
            {
                await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new
                {
                    Title = "Quiz Passed!",
                    Message = $"Awesome job! You scored {result.ScorePercentage}% and passed the quiz.",
                    Type = "achievement"
                });
            }

            return Ok(ApiResponse<RoadmapQuizResultDto>.SuccessResponse(result, "Quiz evaluated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoadmapQuizResultDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("regenerate/{careerKey}")]
    public async Task<ActionResult<ApiResponse<RoadmapDto>>> RegenerateRoadmap(string careerKey)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<RoadmapDto>.ErrorResponse("User not authenticated"));

            var result = await _roadmapService.RegenerateRoadmapAsync(userId, careerKey);
            return Ok(ApiResponse<RoadmapDto>.SuccessResponse(result, "Roadmap regenerated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoadmapDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("my-roadmaps")]
    public async Task<ActionResult<ApiResponse<List<UserSavedRoadmapDto>>>> GetMyRoadmaps()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<List<UserSavedRoadmapDto>>.ErrorResponse("User not authenticated"));

            var result = await _roadmapService.GetUserSavedRoadmapsAsync(userId);
            return Ok(ApiResponse<List<UserSavedRoadmapDto>>.SuccessResponse(result, "Saved roadmaps retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<UserSavedRoadmapDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("my-roadmaps/{careerKey}")]
    public async Task<ActionResult<ApiResponse<bool>>> SaveMyRoadmap(string careerKey, [FromQuery] string? title = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));

            var result = await _roadmapService.SaveUserRoadmapAsync(userId, careerKey, title);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Roadmap saved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("accept")]
    public async Task<ActionResult<ApiResponse<RoadmapDto>>> AcceptCareerRoadmap([FromBody] AcceptCareerRoadmapRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<RoadmapDto>.ErrorResponse("User not authenticated"));

            var result = await _roadmapService.AcceptCareerRoadmapAsync(userId, request.CareerKey, request.CareerTitle);
            return Ok(ApiResponse<RoadmapDto>.SuccessResponse(result, "Career roadmap accepted and activated"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoadmapDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("my-roadmaps/{careerKey}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveMyRoadmap(string careerKey)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));

            var result = await _roadmapService.RemoveUserRoadmapAsync(userId, careerKey);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Roadmap removed successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpPatch("my-roadmaps/{careerKey}/default")]
    public async Task<ActionResult<ApiResponse<bool>>> SetDefaultMyRoadmap(string careerKey)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));

            var result = await _roadmapService.SetDefaultUserRoadmapAsync(userId, careerKey);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Default roadmap updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("ask-ai")]
    public async Task<ActionResult<ApiResponse<RoadmapDto>>> AskAIRoadmap([FromBody] AskAIRoadmapRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<RoadmapDto>.ErrorResponse("User not authenticated"));

            var result = await _freeformRoadmapService.GenerateFreeformRoadmapAsync(request.GoalText, request.SituationText, userId);
            return Ok(ApiResponse<RoadmapDto>.SuccessResponse(result, "Freeform AI roadmap generated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RoadmapDto>.ErrorResponse(ex.Message));
        }
    }
}

public class AcceptCareerRoadmapRequestDto
{
    public string CareerKey { get; set; } = string.Empty;
    public string? CareerTitle { get; set; }
}
