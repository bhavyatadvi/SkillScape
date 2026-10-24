using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SkillScape.API.Controllers;

/// <summary>
/// V2 Adaptive Career Quiz Engine — context-aware, hierarchical, evidence-based career prediction.
/// </summary>
[ApiController]
[Route("api/quiz")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly IAdaptiveQuizService _quizService;
    private readonly IProfileParserService _profileParser;
    private readonly IGoalValidationEngine _goalValidator;
    private readonly IDynamicRoadmapEngine _roadmapEngine;
    private readonly IRoadmapService _roadmapService;

    public QuizController(
        IAdaptiveQuizService quizService,
        IProfileParserService profileParser,
        IGoalValidationEngine goalValidator,
        IDynamicRoadmapEngine roadmapEngine,
        IRoadmapService roadmapService)
    {
        _quizService = quizService;
        _profileParser = profileParser;
        _goalValidator = goalValidator;
        _roadmapEngine = roadmapEngine;
        _roadmapService = roadmapService;
    }

    /// <summary>Returns all available quiz phases with metadata.</summary>
    [HttpGet("phases")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPhases()
    {
        var phases = await _quizService.GetPhasesAsync();
        return Ok(phases);
    }

    /// <summary>
    /// Parse natural language Current Status & Primary Goal text into structured profile data.
    /// </summary>
    [HttpPost("profile/parse")]
    [AllowAnonymous]
    public IActionResult ParseProfile([FromBody] ParseProfileRequestDto request)
    {
        var parsed = _profileParser.ParseProfile(
            request.PhaseKey,
            request.CurrentStatusRaw,
            request.PrimaryGoalRaw,
            request.RawFields);
        return Ok(parsed);
    }

    /// <summary>
    /// Validate primary goal fit and calculate alternative career paths if fit is low.
    /// </summary>
    [HttpPost("goal/validate")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateGoal([FromBody] ParsedProfileDto profile)
    {
        var result = await _goalValidator.ValidateGoalAsync(profile);
        return Ok(result);
    }

    /// <summary>
    /// Generate step-by-step career transition roadmap from starting point to target role.
    /// </summary>
    [HttpPost("{sessionId}/roadmap")]
    public async Task<IActionResult> GenerateRoadmap(string sessionId, [FromBody] GenerateRoadmapRequestDto request)
    {
        var parsedProfile = _profileParser.ParseProfile(request.PhaseKey, request.CurrentStatus, request.TargetRole);
        var roadmap = await _roadmapEngine.GenerateRoadmapAsync(parsedProfile, request.TargetNodeId, request.TargetRole);
        return Ok(roadmap);
    }

    /// <summary>
    /// Start a new quiz session.
    /// Accepts the phase key and initial profile data from the background form.
    /// Returns the session ID and the first question.
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> StartSession([FromBody] AdaptiveQuizStartRequest request)
    {
        // Accept the phase supplied in the profile or assessment type as a
        // compatibility fallback for older quiz clients. The current client
        // sends `phase`, but this prevents a valid start request becoming a
        // 400 when one of the equivalent fields is used.
        if (string.IsNullOrWhiteSpace(request.PhaseKey))
        {
            request.PhaseKey = request.Profile?.PhaseKey
                ?? request.AssessmentType
                ?? string.Empty;
        }
        Console.WriteLine($"\n\n[StartSession] Received PhaseKey: '{request.PhaseKey}', Profile.PhaseKey: '{request.Profile?.PhaseKey}', Profile.SubPhaseKey: '{request.Profile?.SubPhaseKey}'\n\n");
        if (string.IsNullOrWhiteSpace(request.PhaseKey))
            return BadRequest("PhaseKey is required.");

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue("id");

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User identity not found.");

        try
        {
            var (session, firstQuestion) = await _quizService.StartSessionAsync(userId, request);
            return Ok(new { session, firstQuestion });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StartSession Error] {ex}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Submit an answer for the current question and receive the next question.
    /// If IsComplete = true in the response, call POST /submit/{sessionId}.
    /// </summary>
    [HttpPost("{sessionId}/answer")]
    public async Task<IActionResult> AnswerAndGetNext(string sessionId, [FromBody] AdaptiveQuizAnswerRequest request)
    {
        request.SessionId = sessionId; // Bind from route
        if (string.IsNullOrWhiteSpace(request.SessionId) || string.IsNullOrWhiteSpace(request.QuestionId))
            return BadRequest("SessionId and QuestionId are required.");

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var response = await _quizService.AnswerAndGetNextAsync(userId, request);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Finalize the session and generate the full prediction result.
    /// Returns the primary recommendation with supporting evidence.
    /// </summary>
    [HttpPost("{sessionId}/complete")]
    public async Task<IActionResult> SubmitSession(string sessionId)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var result = await _quizService.SubmitSessionAsync(userId, sessionId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get the most recent completed quiz result for the current user.
    /// </summary>
    [HttpGet("result")]
    public async Task<IActionResult> GetLatestResult()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _quizService.GetLatestResultAsync(userId);
        if (result == null) return NotFound(new { error = "No quiz results found for this user." });

        return Ok(result);
    }

    /// <summary>
    /// Get all completed quiz results for the current user.
    /// </summary>
    [HttpGet("results")]
    public async Task<IActionResult> GetResults()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var results = await _quizService.GetResultsAsync(userId);
        return Ok(results);
    }

    /// <summary>
    /// Get a specific completed quiz result by ID for the current user.
    /// </summary>
    [HttpGet("results/{resultId}")]
    public async Task<IActionResult> GetResultById(string resultId)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _quizService.GetResultByIdAsync(userId, resultId);
        if (result == null) return NotFound(new { error = "Quiz result not found." });

        return Ok(result);
    }

    /// <summary>
    /// Delete a specific quiz result by ID for the current user.
    /// </summary>
    [HttpDelete("results/{resultId}")]
    public async Task<IActionResult> DeleteResult(string resultId)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _quizService.DeleteResultAsync(userId, resultId);
        if (!success) return NotFound(new { error = "Quiz result not found." });

        return Ok(new { success = true });
    }

    /// <summary>
    /// Finalize the user's decision when a career pivot is suggested.
    /// </summary>
    [HttpPost("finalize-decision")]
    public async Task<IActionResult> FinalizeDecision([FromBody] FinalizeDecisionRequestDto request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            if (string.IsNullOrWhiteSpace(request.SessionId) || string.IsNullOrWhiteSpace(request.ChosenCareerId))
                return BadRequest(new { error = "SessionId and ChosenCareerId are required." });

            var roadmap = await _roadmapService.AcceptCareerRoadmapAsync(userId, request.ChosenCareerId, request.CareerTitle);
            return Ok(new
            {
                success = true,
                chosenCareerId = roadmap.DomainId,
                careerTitle = roadmap.DomainName,
                roadmap
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class FinalizeDecisionRequestDto
{
    public string SessionId { get; set; } = string.Empty;
    public string ChosenCareerId { get; set; } = string.Empty;
    public string? CareerTitle { get; set; }
}

public class ParseProfileRequestDto
{
    public string PhaseKey { get; set; } = string.Empty;
    public string? CurrentStatusRaw { get; set; }
    public string? PrimaryGoalRaw { get; set; }
    public Dictionary<string, string>? RawFields { get; set; }
}

public class GenerateRoadmapRequestDto
{
    public string PhaseKey { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
}
