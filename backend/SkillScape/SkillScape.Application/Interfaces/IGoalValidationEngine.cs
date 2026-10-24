using System.Threading.Tasks;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Interfaces;

public interface IGoalValidationEngine
{
    /// <summary>
    /// Validates user's primary goal against their current profile across academic, skill, and readiness axes.
    /// Returns alignment rating (STRONG_FIT, MODERATE_FIT, WEAK_FIT, CURRENTLY_NOT_ALIGNED), skill gaps,
    /// and recommended alternative career paths if alignment is weak.
    /// </summary>
    Task<GoalValidationResultDto> ValidateGoalAsync(ParsedProfileDto profile);
}
