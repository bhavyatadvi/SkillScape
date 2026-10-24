using System.Threading.Tasks;
using SkillScape.Domain.Entities;

namespace SkillScape.Application.Interfaces;

public interface IQuestionValidator
{
    Task<(bool IsValid, string Reason)> ValidateQuestionAsync(QuizQuestion question);
}
