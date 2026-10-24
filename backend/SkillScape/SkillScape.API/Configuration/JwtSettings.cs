namespace SkillScape.API.Configuration;

/// <summary>
/// JWT configuration settings
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public string Issuer { get; set; } = "SkillScape";
    public string Audience { get; set; } = "SkillScapeClient";
}
