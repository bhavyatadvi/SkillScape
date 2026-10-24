using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using System.Text;

namespace SkillScape.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Register new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Registration successful"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<UserProfileDto>.ErrorResponse("User not authenticated"));

            var result = await _authService.GetCurrentUserAsync(userId);
            return Ok(ApiResponse<UserProfileDto>.SuccessResponse(result, "User profile retrieved"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserProfileDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPatch("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<UserProfileDto>.ErrorResponse("User not authenticated"));

            var result = await _authService.UpdateProfileAsync(userId, request);
            return Ok(ApiResponse<UserProfileDto>.SuccessResponse(result, "Profile updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserProfileDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Change password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("User not authenticated"));

            await _authService.ChangePasswordAsync(userId, request);
            return Ok(ApiResponse<string>.SuccessResponse("", "Password changed successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Upload avatar image
    /// </summary>
    [HttpPost("upload-avatar")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> UploadAvatar(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<string>.ErrorResponse("No file uploaded"));

            const long maxAvatarSize = 5 * 1024 * 1024;
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (file.Length > maxAvatarSize ||
                string.IsNullOrEmpty(extension) ||
                !allowedExtensions.Contains(extension) ||
                !allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    "Avatar must be a JPG, PNG, or WebP image no larger than 5 MB."));
            }
            if (!await HasValidImageSignatureAsync(file, extension))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("The uploaded avatar is not a valid image."));
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            // Simple local upload for now (could connect to S3/Cloudinary later)
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/avatars/{fileName}";
            
            // Also update the user's profile automatically
            var updateRequest = new UpdateProfileRequest 
            {
                ProfileImageUrl = fileUrl
            };
            
            var user = await _authService.GetCurrentUserAsync(currentUserId);
            updateRequest.FullName = user.FullName; // Required property
            updateRequest.Bio = user.Bio;
            
            await _authService.UpdateProfileAsync(currentUserId, updateRequest);

            return Ok(ApiResponse<string>.SuccessResponse(fileUrl, "Avatar uploaded successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Avatar upload failed."));
        }
    }

    /// <summary>
    /// Delete current user account
    /// </summary>
    [HttpDelete("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> DeleteAccount()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("User not authenticated"));

            await _authService.DeleteAccountAsync(userId);
            return Ok(ApiResponse<string>.SuccessResponse("", "Account deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var result = await _authService.ForgotPasswordAsync(request);
            return Ok(ApiResponse<string>.SuccessResponse(result, "Password reset link sent"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request);
            return Ok(ApiResponse<string>.SuccessResponse("", "Password reset successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    private static async Task<bool> HasValidImageSignatureAsync(IFormFile file, string? extension)
    {
        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = await stream.ReadAsync(header);
        return extension switch
        {
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => read >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => read >= 12 && Encoding.ASCII.GetString(header, 0, 4) == "RIFF" &&
                       Encoding.ASCII.GetString(header, 8, 4) == "WEBP",
            _ => false
        };
    }
}
