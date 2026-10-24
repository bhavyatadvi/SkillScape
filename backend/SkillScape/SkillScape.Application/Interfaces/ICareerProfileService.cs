using System.IO;
using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface ICareerProfileService
{
    Task<CareerProfileDto?> GetProfileAsync(string userId);
    Task<CareerProfileDto> CreateOrUpdateProfileAsync(string userId, SaveCareerProfileRequest request);
    Task<bool> DeleteProfileAsync(string userId);
    Task<ResumeParseResultDto> ParseResumeAsync(Stream stream, string fileName);
}
