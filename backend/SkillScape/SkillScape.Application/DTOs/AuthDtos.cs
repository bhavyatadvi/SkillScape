using System.ComponentModel.DataAnnotations;

namespace SkillScape.Application.DTOs;

/// <summary>
/// Login request
/// </summary>
public class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Register request
/// </summary>
public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^Student$")]
    [StringLength(20)]
    public string Role { get; set; } = "Student";
}

/// <summary>
/// Auth response with token
/// </summary>
public class AuthResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool ProfileCompleted { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// User profile DTO
/// </summary>
public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool ProfileCompleted { get; set; }
    public int Level { get; set; }
    public long TotalXP { get; set; }
    public int CurrentStreak { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Update profile request
/// </summary>
public class UpdateProfileRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Bio { get; set; }

    [StringLength(2048)]
    public string? ProfileImageUrl { get; set; }
}

/// <summary>
/// Change password request
/// </summary>
public class ChangePasswordRequest
{
    [Required]
    [StringLength(128)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Refresh token request
/// </summary>
public class RefreshTokenRequest
{
    [Required]
    [StringLength(4096)]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Forgot password request
/// </summary>
public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Reset password request
/// </summary>
public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(4096)]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
