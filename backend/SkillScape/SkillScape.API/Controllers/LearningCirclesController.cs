using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SkillScape.Application.Interfaces;
using SkillScape.Application.DTOs;
using Microsoft.AspNetCore.SignalR;
using SkillScape.API.Hubs;

namespace SkillScape.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LearningCirclesController : ControllerBase
{
    private readonly ILearningCircleService _learningCircleService;
    private readonly IHubContext<ChatHub> _hubContext;

    public LearningCirclesController(ILearningCircleService learningCircleService, IHubContext<ChatHub> hubContext)
    {
        _learningCircleService = learningCircleService;
        _hubContext = hubContext;
    }

    [HttpGet("explore")]
    public async Task<IActionResult> ExploreCircles()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var circles = await _learningCircleService.GetAllPublicCirclesAsync(userId);
        return Ok(circles);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyCircles()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        return Ok(await _learningCircleService.GetMyCirclesAsync(userId));
    }

    [HttpGet("recommended")]
    public async Task<IActionResult> GetRecommendedCircles()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var hasCompletedQuiz = await _learningCircleService.HasCompletedCareerQuizAsync(userId);
        if (!hasCompletedQuiz)
        {
            return Ok(new { RequiresQuiz = true, Recommendations = Array.Empty<object>() });
        }

        var recommendations = await _learningCircleService.GetRecommendedCirclesAsync(userId);
        return Ok(new { RequiresQuiz = false, Recommendations = recommendations });
    }

    [HttpGet("{circleId}")]
    public async Task<IActionResult> GetCircle(string circleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var circle = await _learningCircleService.GetCircleByIdAsync(circleId, userId);
        if (circle == null) return NotFound("Learning circle not found.");
        return Ok(circle);
    }

    [HttpPost("{circleId}/join")]
    public async Task<IActionResult> JoinCircle(string circleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var status = await _learningCircleService.RequestJoinCircleAsync(userId, circleId);
        if (status == "NotFound") return NotFound("Circle not found.");
        if (status == "DisabledByAdmin") return StatusCode(403, "This group has been disabled by an administrator.");
        if (status == "AlreadyMember") return Ok(new { Message = "You are already a member.", Status = "AlreadyMember" });
        
        if (status == "Pending")
        {
            var circle = await _learningCircleService.GetCircleByIdAsync(circleId);
            if (circle != null)
            {
                await _hubContext.Clients.Group(circle.CreatorId).SendAsync("ReceiveNotification", new 
                {
                    Title = "New Join Request",
                    Message = $"Someone wants to join '{circle.Name}'",
                    Type = "join_request",
                    CircleId = circle.Id
                });
            }
        }

        return Ok(new { Message = "Join request sent to creator. Pending approval.", Status = status });
    }

    [HttpGet("{circleId}/requests")]
    public async Task<IActionResult> GetJoinRequests(string circleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var requests = await _learningCircleService.GetPendingRequestsAsync(circleId, userId);
        return Ok(requests);
    }

    [HttpPost("requests/{requestId}/process")]
    public async Task<IActionResult> ProcessJoinRequest(string requestId, [FromQuery] bool approve)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _learningCircleService.ProcessJoinRequestAsync(requestId, userId, approve);
        if (!result.Success) return BadRequest("Unable to process request. You may not have permission.");

        // Broadcast to requester
        await _hubContext.Clients.Group(result.RequesterId).SendAsync("ReceiveNotification", new 
        {
            Title = approve ? "Join Request Approved" : "Join Request Rejected",
            Message = approve ? $"Your request to join '{result.CircleName}' was approved!" : $"Your request to join '{result.CircleName}' was declined.",
            Type = "join_request_processed"
        });

        return Ok(new { Message = approve ? "Request approved" : "Request rejected" });
    }

    [HttpPost("{circleId}/meeting-status")]
    public async Task<IActionResult> ToggleMeetingStatus(string circleId, [FromQuery] bool isActive)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _learningCircleService.ToggleMeetingStatusAsync(circleId, userId, isActive);
        if (!success) return BadRequest("Unable to change meeting status. Only the creator can do this.");

        if (!isActive)
        {
            await _hubContext.Clients.Group($"video_{circleId}").SendAsync("SessionEnded");
            await _hubContext.Clients.Group(circleId).SendAsync("SessionEnded");
        }

        return Ok(new { Message = "Meeting status updated", IsActive = isActive });
    }

    [HttpDelete("{circleId}/discussions")]
    public async Task<IActionResult> ClearDiscussions(string circleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _learningCircleService.ClearDiscussionsAsync(circleId, userId);
        if (!success) return BadRequest("Unable to clear discussions. Only the creator can do this.");
        return Ok(new { Message = "Discussions cleared" });
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateCircle([FromBody] CreateLearningCircleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var circle = await _learningCircleService.CreateCircleAsync(dto, userId);
            return Ok(circle);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    public class ScheduleMeetingDto
    {
        public DateTime MeetingTime { get; set; }
    }

    [HttpPost("{circleId}/schedule")]
    public async Task<IActionResult> ScheduleMeeting(string circleId, [FromBody] ScheduleMeetingDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var memberIds = await _learningCircleService.ScheduleMeetingAsync(circleId, userId, dto.MeetingTime);
        if (memberIds == null || memberIds.Count == 0) return BadRequest("Unable to schedule meeting. You must be the creator, or there are no members.");
        
        var circle = await _learningCircleService.GetCircleByIdAsync(circleId);

        // Broadcast via SignalR to all members
        foreach (var memberId in memberIds)
        {
            await _hubContext.Clients.Group(memberId).SendAsync("ReceiveNotification", new 
            {
                Title = "Meeting Scheduled",
                Message = $"A meeting has been scheduled for '{circle.Name}' at {dto.MeetingTime:f}",
                Type = "meeting_scheduled",
                CircleId = circle.Id
            });
        }

        return Ok(new { Message = "Meeting scheduled and notifications sent." });
    }

    [HttpGet("{circleId}/discussions")]
    public async Task<IActionResult> GetDiscussions(string circleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var discussions = await _learningCircleService.GetDiscussionsAsync(circleId, userId);
        return Ok(discussions);
    }

    [HttpPost("{circleId}/discussions")]
    public async Task<IActionResult> CreateDiscussion(string circleId, [FromBody] CreateDiscussionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var discussion = await _learningCircleService.CreateDiscussionAsync(circleId, userId, dto);
        
        // Broadcast new message via SignalR
        await _hubContext.Clients.Group(circleId).SendAsync("ReceiveMessage", discussion);

        return Ok(discussion);
    }

    [HttpGet("{circleId}/resources")]
    public async Task<IActionResult> GetResources(string circleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var resources = await _learningCircleService.GetResourcesAsync(circleId, userId);
        return Ok(resources);
    }

    [HttpPost("{circleId}/resources")]
    public async Task<IActionResult> CreateResource(string circleId, [FromBody] CreateResourceDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var resource = await _learningCircleService.CreateResourceAsync(circleId, userId, dto);
        
        // Broadcast via SignalR
        await _hubContext.Clients.Group(circleId).SendAsync("ReceiveResource", resource);

        return Ok(resource);
    }

    [HttpDelete("{circleId}/resources/{resourceId}")]
    public async Task<IActionResult> DeleteResource(string circleId, string resourceId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _learningCircleService.DeleteResourceAsync(circleId, resourceId, userId);
        if (!success) return BadRequest("Unable to delete resource. Only the creator can do this.");
        return Ok(new { Message = "Resource deleted" });
    }

    [HttpGet("{circleId}/members")]
    public async Task<IActionResult> GetMembers(string circleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var members = await _learningCircleService.GetCircleMembersAsync(circleId);
        return Ok(members);
    }

    [HttpDelete("{circleId}/members/{memberId}")]
    public async Task<IActionResult> RemoveMember(string circleId, string memberId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _learningCircleService.RemoveMemberAsync(circleId, memberId, userId);
        if (!success) return BadRequest("Unable to remove member. You must be the creator and cannot remove yourself.");
        
        return Ok(new { Message = "Member removed successfully" });
    }

    [HttpPost("{circleId}/leave")]
    public async Task<IActionResult> LeaveCircle(string circleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _learningCircleService.LeaveCircleAsync(circleId, userId);
        if (!success) return BadRequest("Unable to leave circle. The creator cannot leave their own circle.");
        
        return Ok(new { Message = "You have left the circle successfully" });
    }


    [HttpGet("{circleId}/tasks")]
    public async Task<IActionResult> GetTasks(string circleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var tasks = await _learningCircleService.GetTasksAsync(circleId, userId);
        return Ok(tasks);
    }

    [HttpPost("{circleId}/tasks")]
    public async Task<IActionResult> CreateTask(string circleId, [FromBody] CreateLearningCircleTaskDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var task = await _learningCircleService.CreateTaskAsync(circleId, userId, dto);
        
        await _hubContext.Clients.Group(circleId).SendAsync("ReceiveTask", task);

        return Ok(task);
    }

    [HttpPut("{circleId}/tasks/{taskId}")]
    public async Task<IActionResult> UpdateTaskStatus(string circleId, string taskId, [FromBody] UpdateLearningCircleTaskDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _learningCircleService.UpdateTaskStatusAsync(circleId, taskId, userId, dto.Status);
        if (!success) return BadRequest("Unable to update task.");

        await _hubContext.Clients.Group(circleId).SendAsync("TaskUpdated", new { TaskId = taskId, Status = dto.Status });
        return Ok(new { Message = "Task updated successfully." });
    }

    [HttpGet("{circleId}/notes")]
    public async Task<IActionResult> GetNote(string circleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var note = await _learningCircleService.GetNoteAsync(circleId, userId);
        if (note == null) return Ok(new { Content = "" });
        return Ok(note);
    }

    [HttpPut("{circleId}/notes")]
    public async Task<IActionResult> UpdateNote(string circleId, [FromBody] UpdateLearningCircleNoteDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var note = await _learningCircleService.UpdateNoteAsync(circleId, userId, dto.Content);
        
        // Notify others that notes were updated
        await _hubContext.Clients.Group(circleId).SendAsync("NotesUpdated", note);
        return Ok(note);
    }

    [HttpPost("{circleId}/ai-mentor")]
    public async Task<IActionResult> GetAiMentorResponse(string circleId, [FromBody] AiMentorRequestDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var response = await _learningCircleService.GetAiMentorResponseAsync(circleId, userId, dto.Query);
        return Ok(new { Response = response });
    }

    [HttpGet("{circleId}/progress")]
    public async Task<IActionResult> GetProgressBoard(string circleId)
    {
        var progress = await _learningCircleService.GetProgressBoardAsync(circleId);
        return Ok(progress);
    }

    [HttpGet("{circleId}/overview")]
    public async Task<IActionResult> GetOverview(string circleId)
    {
        var overview = await _learningCircleService.GetCircleOverviewAsync(circleId);
        return Ok(overview);
    }
}
