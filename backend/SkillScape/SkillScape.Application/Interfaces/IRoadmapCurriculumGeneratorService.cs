using System.Threading.Tasks;
using SkillScape.Domain.Entities;

namespace SkillScape.Application.Interfaces;

public interface IRoadmapCurriculumGeneratorService
{
    Task<RoadmapBlueprint> GetOrGenerateCurriculumAsync(
        string careerTitle, 
        string careerKey, 
        string? userLevel = null, 
        string? startingPointContext = null, 
        bool forceRegenerate = false);
}
