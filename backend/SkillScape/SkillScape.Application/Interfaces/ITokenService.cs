namespace SkillScape.Application.Interfaces;

/// <summary>
/// JWT token service interface
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(string userId, string email, string role);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
}
