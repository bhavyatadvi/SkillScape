using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface IDynamicRoadmapEngine
{
    /// <summary>
    /// Generates a customized, step-by-step career transition roadmap
    /// based on user's actual starting point (Current Status, Skills) and Target Role.
    /// </summary>
    Task<DynamicRoadmapDto> GenerateRoadmapAsync(ParsedProfileDto profile, string targetNodeId, string targetRoleTitle);
}
