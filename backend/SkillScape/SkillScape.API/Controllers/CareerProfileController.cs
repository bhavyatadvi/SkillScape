using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.API.Controllers;

[ApiController]
[Route("api/career-profile")]
[Authorize]
public class CareerProfileController : ControllerBase
{
    private readonly ICareerProfileService _profileService;

    public CareerProfileController(ICareerProfileService profileService)
    {
        _profileService = profileService;
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value 
            ?? User.FindFirst("id")?.Value;
    }

    /// <summary>
    /// GET /api/career-profile
    /// Get currently authenticated user's central Career Profile.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<CareerProfileDto>>> GetProfile()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<CareerProfileDto>.ErrorResponse("User not authenticated"));
        }

        try
        {
            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null)
            {
                return Ok(ApiResponse<CareerProfileDto>.SuccessResponse(null, "Career profile not found for this user"));
            }

            return Ok(ApiResponse<CareerProfileDto>.SuccessResponse(profile, "Career profile retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CareerProfileDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// POST /api/career-profile
    /// Create new central Career Profile for authenticated user.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CareerProfileDto>>> CreateProfile([FromBody] SaveCareerProfileRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<CareerProfileDto>.ErrorResponse("User not authenticated"));
        }

        try
        {
            var existing = await _profileService.GetProfileAsync(userId);
            if (existing != null)
            {
                // Update existing if already present instead of erroring out
                var updated = await _profileService.CreateOrUpdateProfileAsync(userId, request);
                return Ok(ApiResponse<CareerProfileDto>.SuccessResponse(updated, "Career profile updated successfully"));
            }

            var created = await _profileService.CreateOrUpdateProfileAsync(userId, request);
            return CreatedAtAction(nameof(GetProfile), ApiResponse<CareerProfileDto>.SuccessResponse(created, "Career profile created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<CareerProfileDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CareerProfileDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// PUT /api/career-profile
    /// Update existing central Career Profile for authenticated user.
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<CareerProfileDto>>> UpdateProfile([FromBody] SaveCareerProfileRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<CareerProfileDto>.ErrorResponse("User not authenticated"));
        }

        try
        {
            var updated = await _profileService.CreateOrUpdateProfileAsync(userId, request);
            return Ok(ApiResponse<CareerProfileDto>.SuccessResponse(updated, "Career profile updated successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<CareerProfileDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CareerProfileDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// DELETE /api/career-profile
    /// Delete authenticated user's Career Profile.
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProfile()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
        }

        try
        {
            var deleted = await _profileService.DeleteProfileAsync(userId);
            if (!deleted)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("No career profile found to delete"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Career profile deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// POST /api/career-profile/resume
    /// Upload resume (PDF or DOCX), extract text, parse with AI into structured Career Profile data.
    /// DOES NOT persist data to database; returns draft for frontend prefilling.
    /// </summary>
    [HttpPost("resume")]
    public async Task<ActionResult<ApiResponse<ResumeParseResultDto>>> UploadResume(IFormFile file)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<ResumeParseResultDto>.ErrorResponse("User not authenticated"));
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<ResumeParseResultDto>.ErrorResponse("Please select a valid resume file."));
        }

        var allowedExtensions = new[] { ".pdf", ".docx" };
        var fileExt = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExt) || Array.IndexOf(allowedExtensions, fileExt) < 0)
        {
            return BadRequest(ApiResponse<ResumeParseResultDto>.ErrorResponse("Only PDF (.pdf) and Word (.docx) files are supported."));
        }

        // Limit file size to 10MB
        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(ApiResponse<ResumeParseResultDto>.ErrorResponse("File size exceeds 10 MB limit. Please upload a smaller file."));
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _profileService.ParseResumeAsync(stream, file.FileName);
            return Ok(ApiResponse<ResumeParseResultDto>.SuccessResponse(result, "Resume parsed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ResumeParseResultDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ResumeParseResultDto>.ErrorResponse(ex.Message));
        }
    }
}
