using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.Interfaces;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class EligibilityEvaluatorService : IEligibilityService
{
    private readonly ApplicationDbContext _context;

    public EligibilityEvaluatorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EligibilityResult> EvaluateEligibilityAsync(string profileJson, string targetNodeId)
    {
        // For demonstration of Part 30 (Eligibility Separation)
        // In a real scenario, this would parse profileJson to get actual grades/streams
        // and match against EligibilityConstraintsV2 for the targetNodeId.

        var constraints = await _context.EligibilityConstraintsV2
            .Where(c => c.TaxonomyNodeId == targetNodeId)
            .ToListAsync();

        if (constraints.Any())
        {
            try
            {
                var profile = JsonSerializer.Deserialize<Dictionary<string, object>>(profileJson) ?? new();
                
                foreach (var constraint in constraints)
                {
                    // Mock check
                    if (profile.TryGetValue("Subjects", out var subjectsObj))
                    {
                        var subjects = subjectsObj.ToString()?.ToUpperInvariant() ?? "";
                        if (!subjects.Contains(constraint.RequiredSubjectCode.ToUpperInvariant()))
                        {
                            return new EligibilityResult 
                            { 
                                IsEligible = false, 
                                Reason = $"Your interest profile aligns strongly, but your current academic qualifications (missing {constraint.RequiredSubjectCode}) do not satisfy the required entry conditions."
                            };
                        }
                    }
                }
            }
            catch
            {
                // Ignore parse errors for mock
            }
        }

        return new EligibilityResult { IsEligible = true, Reason = "Eligible" };
    }
}
