using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class QuestionValidator : IQuestionValidator
{
    private readonly ApplicationDbContext _context;

    public QuestionValidator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool IsValid, string Reason)> ValidateQuestionAsync(QuizQuestion question)
    {
        // 1. Check for internal metadata leakage in the Question Text
        if (question.Text.Contains("[") && question.Text.Contains("]"))
        {
             return (false, "Question text contains internal metadata or tags.");
        }

        // 2. No generic "personality" duplicates (e.g. checking if it has terms like "Variation" or "Strategy")
        if (question.Text.Contains("Variation", System.StringComparison.OrdinalIgnoreCase) || 
            question.Text.Contains("Strategy", System.StringComparison.OrdinalIgnoreCase))
        {
             return (false, "Question text contains generic placeholders like 'Variation' or 'Strategy'.");
        }

        // 3. Semantic duplicate check (simple exact text match for now)
        bool exists = await _context.QuizQuestions.AnyAsync(q => q.Text.ToLower() == question.Text.ToLower() && q.Id != question.Id);
        if (exists)
        {
             return (false, "A question with the exact same text already exists.");
        }

        // 4. Validate Options format
        if (question.Options == null || question.Options.Count < 2)
        {
             return (false, "Question must have at least 2 options.");
        }
        
        foreach (var option in question.Options)
        {
            if (option.Text.Contains("{") || option.Text.Contains("}"))
            {
                 return (false, "Option text cannot contain JSON or internal scoring metadata.");
            }
        }

        return (true, "Valid");
    }
}
