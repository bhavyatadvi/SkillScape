using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services
{
    public class ReverseCareerFinderDto
    {
        public string CurrentCareerNodeId { get; set; } = string.Empty;
        public List<string> NegativePreferences { get; set; } = new(); // e.g. ["coding", "sales"]
        public List<string> PositivePreferences { get; set; } = new();
    }

    public class CareerAlternativeDto
    {
        public string TargetCareerNodeId { get; set; } = string.Empty;
        public string TargetCareerTitle { get; set; } = string.Empty;
        public string TransitionDifficulty { get; set; } = string.Empty;
        public double MatchScore { get; set; }
        public string WhyItFits { get; set; } = string.Empty;
        public string NewSkillsRequired { get; set; } = string.Empty;
        public string ExistingSkillsUsed { get; set; } = string.Empty;
    }

    public interface IReverseCareerFinderService
    {
        Task<List<CareerAlternativeDto>> GetAlternativesAsync(ReverseCareerFinderDto request);
    }

    public class ReverseCareerFinderService : IReverseCareerFinderService
    {
        private readonly ApplicationDbContext _context;

        public ReverseCareerFinderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CareerAlternativeDto>> GetAlternativesAsync(ReverseCareerFinderDto request)
        {
            var profile = await _context.CareerSwitchNodes
                .Include(c => c.OutgoingTransitions)
                    .ThenInclude(t => t.ToCareer)
                .FirstOrDefaultAsync(c => c.NodeId == request.CurrentCareerNodeId);

            if (profile == null)
                return new List<CareerAlternativeDto>();

            var results = new List<CareerAlternativeDto>();

            foreach (var transition in profile.OutgoingTransitions)
            {
                // Basic filtering based on negative preferences
                bool exclude = false;
                if (request.NegativePreferences.Contains("coding") && transition.ToCareer.NodeId.Contains("cs.software"))
                    exclude = true;
                
                if (exclude) continue;

                // Adjust score based on positive preferences
                double score = transition.CompatibilityScore;
                if (request.PositivePreferences.Contains("project management") && transition.ToCareer.NodeId.Contains("mgmt.project"))
                {
                    score += 5;
                }

                results.Add(new CareerAlternativeDto
                {
                    TargetCareerNodeId = transition.ToCareer.NodeId,
                    TargetCareerTitle = transition.ToCareer.Title,
                    TransitionDifficulty = transition.TransitionDifficulty,
                    MatchScore = Math.Min(100, score),
                    WhyItFits = transition.WhyItFits,
                    NewSkillsRequired = transition.NewSkillsRequired,
                    ExistingSkillsUsed = transition.ExistingSkillsUsed
                });
            }

            return results.OrderByDescending(r => r.MatchScore).ToList();
        }
    }
}
