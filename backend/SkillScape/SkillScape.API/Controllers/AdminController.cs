using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;
using System.Security.Claims;

namespace SkillScape.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ApplicationDbContext _context;

    public AdminController(IAdminService adminService, ApplicationDbContext context)
    {
        _adminService = adminService;
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<AdminDashboardStatsDto>>> GetDashboard()
    {
        var result = await _adminService.GetDashboardAsync();
        return Ok(ApiResponse<AdminDashboardStatsDto>.SuccessResponse(result, "Dashboard stats retrieved"));
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<AdminUserDto>>>> GetUsers([FromQuery] string? role, [FromQuery] bool? isBlocked)
    {
        var result = await _adminService.GetUsersAsync(role, isBlocked);
        return Ok(ApiResponse<List<AdminUserDto>>.SuccessResponse(result, "Users retrieved"));
    }

    [HttpPut("block-user")]
    public async Task<ActionResult<ApiResponse<bool>>> BlockUser([FromBody] BlockUserDto request)
    {
        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            await _adminService.BlockOrUnblockUserAsync(adminId, request);
            return Ok(ApiResponse<bool>.SuccessResponse(true, request.IsBlocked ? "User blocked" : "User unblocked"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("users/role")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateUserRole([FromBody] UpdateUserRoleDto request)
    {
        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            await _adminService.UpdateUserRoleAsync(adminId, request);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Role updated"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<ApiResponse<PlatformAnalyticsDto>>> GetAnalytics()
    {
        var result = await _adminService.GetAnalyticsAsync();
        return Ok(ApiResponse<PlatformAnalyticsDto>.SuccessResponse(result, "Analytics retrieved"));
    }

    [HttpGet("progress/snapshot")]
    public async Task<ActionResult<ApiResponse<List<AdminUserProgressSnapshotDto>>>> GetProgressSnapshot()
    {
        var result = await _adminService.GetProgressSnapshotsAsync();
        return Ok(ApiResponse<List<AdminUserProgressSnapshotDto>>.SuccessResponse(result, "Progress snapshot retrieved"));
    }

    [HttpGet("quiz/questions")]
    public async Task<ActionResult<ApiResponse<List<AdminQuizQuestionCrudDto>>>> GetQuizQuestions()
    {
        var result = await _adminService.GetQuizQuestionsAsync();
        return Ok(ApiResponse<List<AdminQuizQuestionCrudDto>>.SuccessResponse(result, "Quiz questions retrieved"));
    }

    [HttpPost("quiz/questions")]
    public async Task<ActionResult<ApiResponse<AdminQuizQuestionCrudDto>>> CreateQuizQuestion([FromBody] UpsertAdminQuizQuestionDto request)
    {
        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _adminService.CreateQuizQuestionAsync(adminId, request);
            return Ok(ApiResponse<AdminQuizQuestionCrudDto>.SuccessResponse(result, "Quiz question created"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AdminQuizQuestionCrudDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("quiz/questions/{questionId}")]
    public async Task<ActionResult<ApiResponse<AdminQuizQuestionCrudDto>>> UpdateQuizQuestion(string questionId, [FromBody] UpsertAdminQuizQuestionDto request)
    {
        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _adminService.UpdateQuizQuestionAsync(adminId, questionId, request);
            return Ok(ApiResponse<AdminQuizQuestionCrudDto>.SuccessResponse(result, "Quiz question updated"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AdminQuizQuestionCrudDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("quiz/questions/{questionId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteQuizQuestion(string questionId)
    {
        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            await _adminService.DeleteQuizQuestionAsync(adminId, questionId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Quiz question deleted"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("roadmap/modules")]
    public async Task<ActionResult<ApiResponse<List<AdminRoadmapModuleCrudDto>>>> GetRoadmapModules([FromQuery] string? domainId)
    {
        var result = await _adminService.GetRoadmapModulesAsync(domainId);
        return Ok(ApiResponse<List<AdminRoadmapModuleCrudDto>>.SuccessResponse(result, "Roadmap modules retrieved"));
    }

    [HttpPost("roadmap/modules")]
    public async Task<ActionResult<ApiResponse<AdminRoadmapModuleCrudDto>>> CreateRoadmapModule([FromBody] UpsertAdminRoadmapModuleDto request)
    {
        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _adminService.CreateRoadmapModuleAsync(adminId, request);
            return Ok(ApiResponse<AdminRoadmapModuleCrudDto>.SuccessResponse(result, "Roadmap module created"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AdminRoadmapModuleCrudDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("roadmap/modules/{moduleId}")]
    public async Task<ActionResult<ApiResponse<AdminRoadmapModuleCrudDto>>> UpdateRoadmapModule(string moduleId, [FromBody] UpsertAdminRoadmapModuleDto request)
    {
        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _adminService.UpdateRoadmapModuleAsync(adminId, moduleId, request);
            return Ok(ApiResponse<AdminRoadmapModuleCrudDto>.SuccessResponse(result, "Roadmap module updated"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AdminRoadmapModuleCrudDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("roadmap/modules/{moduleId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRoadmapModule(string moduleId)
    {
        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            await _adminService.DeleteRoadmapModuleAsync(adminId, moduleId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Roadmap module deleted"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("peer-rooms")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetPeerRooms()
    {
        var result = await _adminService.GetPeerRoomsAsync();
        return Ok(ApiResponse<List<object>>.SuccessResponse(result, "Peer rooms retrieved"));
    }

    [HttpGet("peer-rooms/{roomId}")]
    public async Task<ActionResult<ApiResponse<object>>> GetPeerRoomDetails(string roomId)
    {
        var result = await _adminService.GetPeerRoomDetailsAsync(roomId);
        if (result == null) return NotFound(ApiResponse<object>.ErrorResponse("Peer room not found"));
        return Ok(ApiResponse<object>.SuccessResponse(result, "Peer room details retrieved"));
    }

    [HttpPut("peer-rooms/{roomId}/moderation")]
    public async Task<ActionResult<ApiResponse<bool>>> SetPeerRoomModeration(string roomId, [FromBody] SetPeerRoomModerationDto request)
    {
        try
        {
            await _adminService.SetPeerRoomDisabledAsync(roomId, request.IsDisabled, request.Reason);
            return Ok(ApiResponse<bool>.SuccessResponse(true, request.IsDisabled ? "Peer group disabled" : "Peer group enabled"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("hierarchical-questions")]
    public async Task<ActionResult<ApiResponse<List<HierarchicalQuizQuestionDto>>>> GetHierarchicalQuestions()
    {
        var result = await _adminService.GetHierarchicalQuestionsAsync();
        return Ok(ApiResponse<List<HierarchicalQuizQuestionDto>>.SuccessResponse(result, "Questions retrieved"));
    }

    [HttpPost("hierarchical-questions")]
    public async Task<ActionResult<ApiResponse<HierarchicalQuizQuestionDto>>> CreateHierarchicalQuestion([FromBody] HierarchicalQuizQuestionDto request)
    {
        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _adminService.CreateHierarchicalQuestionAsync(adminId, request);
            return Ok(ApiResponse<HierarchicalQuizQuestionDto>.SuccessResponse(result, "Question created"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<HierarchicalQuizQuestionDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("hierarchical-questions/{questionId}")]
    public async Task<ActionResult<ApiResponse<HierarchicalQuizQuestionDto>>> UpdateHierarchicalQuestion(string questionId, [FromBody] HierarchicalQuizQuestionDto request)
    {
        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _adminService.UpdateHierarchicalQuestionAsync(adminId, questionId, request);
            return Ok(ApiResponse<HierarchicalQuizQuestionDto>.SuccessResponse(result, "Question updated"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<HierarchicalQuizQuestionDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("hierarchical-questions/{questionId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteHierarchicalQuestion(string questionId)
    {
        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            await _adminService.DeleteHierarchicalQuestionAsync(adminId, questionId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Question deleted"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }
}
