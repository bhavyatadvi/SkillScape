using Microsoft.AspNetCore.Mvc;
using SkillScape.Application.Interfaces;
using SkillScape.Application.DTOs;

namespace SkillScape.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestEmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public TestEmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <summary>
    /// Test email sending functionality
    /// </summary>
    [HttpPost("send-test")]
    public async Task<ActionResult<ApiResponse<string>>> SendTestEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            var htmlBody = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; padding: 20px; }
        .container { max-width: 600px; margin: 0 auto; background: #f9f9f9; padding: 30px; border-radius: 10px; }
        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; border-radius: 10px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Email Test Successful!</h1>
        </div>
        <p>Hello!</p>
        <p>This is a test email from SkillScape. If you're reading this, your email configuration is working correctly! 🎉</p>
        <p><strong>Test details:</strong></p>
        <ul>
            <li>Sent at: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @" UTC</li>
            <li>From: SkillScape Platform</li>
        </ul>
        <p>Best regards,<br>The SkillScape Team</p>
    </div>
</body>
</html>";

            await _emailService.SendEmailAsync(request.ToEmail, "✅ SkillScape Email Test", htmlBody);
            
            return Ok(ApiResponse<string>.SuccessResponse(
                "Email sent successfully! Check your inbox (and spam folder).",
                "Test email sent"
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse(
                $"Failed to send email: {ex.Message}. " +
                $"Details: {ex.InnerException?.Message ?? "No additional details"}"
            ));
        }
    }
}

public class TestEmailRequest
{
    public string ToEmail { get; set; } = string.Empty;
}
