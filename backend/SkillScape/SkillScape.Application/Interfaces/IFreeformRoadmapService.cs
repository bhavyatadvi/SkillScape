using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface IFreeformRoadmapService
{
    Task<RoadmapDto> GenerateFreeformRoadmapAsync(string userGoalText, string? situationText, string userId);
}
