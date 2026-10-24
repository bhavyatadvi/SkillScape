using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SkillScape.Application.Interfaces;
using SkillScape.Application.Configuration;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Email service implementation using MailKit
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken)
    {
        // var encodedEmail = Uri.EscapeDataString(toEmail);
        // var encodedToken = Uri.EscapeDataString(resetToken);
        // var resetUrl = $"{_emailSettings.FrontendUrl}/reset-password?email={encodedEmail}&token={encodedToken}";
        
        var resetUrl = $"{_emailSettings.FrontendUrl}/reset-password?email={Uri.EscapeDataString(toEmail)}&token={Uri.EscapeDataString(resetToken)}";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #777; font-size: 12px; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🔐 Password Reset Request</h1>
        </div>
        <div class=""content"">
            <p>Hello {toName},</p>
            
            <p>We received a request to reset your password for your SkillScape account. If you didn't make this request, you can safely ignore this email.</p>
            
            <p>To reset your password, click the button below:</p>
            
            <center>
                <a href=""{resetUrl}"" class=""button"" style=""color: #ffffff; text-decoration: none;"">Reset Password</a>
            </center>
            
            <p>Or copy and paste this link into your browser:</p>
            <p style=""word-break: break-all; color: #667eea;"">{resetUrl}</p>
            
            <div class=""warning"">
                <strong>⚠️ Important:</strong> This link will expire in 15 minutes for security reasons.
            </div>
            
            <p>If you have any questions, feel free to contact our support team.</p>
            
            <p>Best regards,<br>The SkillScape Team</p>
        </div>
        <div class=""footer"">
            <p>This is an automated message, please do not reply to this email.</p>
            <p>&copy; 2026 SkillScape. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, "Reset Your Password - SkillScape", htmlBody);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string toName)
    {
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 15px 30px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #777; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎉 Welcome to SkillScape!</h1>
        </div>
        <div class=""content"">
            <p>Hello {toName},</p>
            
            <p>Welcome to SkillScape! We're excited to have you on board.</p>
            
            <p>Start your learning journey by exploring our personalized roadmaps, taking quizzes, and connecting with mentors.</p>
            
            <center>
                <a href=""{_emailSettings.FrontendUrl}/dashboard"" class=""button"" style=""color: #ffffff; text-decoration: none;"">Go to Dashboard</a>
            </center>
            
            <p>If you have any questions, our support team is here to help.</p>
            
            <p>Happy learning!<br>The SkillScape Team</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2026 SkillScape. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, "Welcome to SkillScape! 🚀", htmlBody);
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Connect to SMTP server
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, 
                _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            // Authenticate
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);

            // Send email
            await client.SendAsync(message);

            // Disconnect
            await client.DisconnectAsync(true);

            _logger.LogInformation($"Email sent successfully to {toEmail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {toEmail}: {ex.Message}");
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }
}
