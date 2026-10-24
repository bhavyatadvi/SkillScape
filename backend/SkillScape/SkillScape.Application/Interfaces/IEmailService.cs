namespace SkillScape.Application.Interfaces;

/// <summary>
/// Email service interface
/// </summary>
public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken);
    Task SendWelcomeEmailAsync(string toEmail, string toName);
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
