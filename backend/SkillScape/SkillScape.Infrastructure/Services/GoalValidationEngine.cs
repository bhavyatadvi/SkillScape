using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class GoalValidationEngine : IGoalValidationEngine
{
    private readonly ApplicationDbContext _context;

    public GoalValidationEngine(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GoalValidationResultDto> ValidateGoalAsync(ParsedProfileDto profile)
    {
        string goal = profile.PrimaryGoal.Goal;
        string degree = profile.Education.Degree;
        string spec = profile.Education.Specialization;
        string targetDomain = profile.PrimaryGoal.TargetDomain;

        var reasons = new List<string>();
        var skillGaps = new List<string>();
        double academicScore = CalculateAcademicAlignment(degree, spec, targetDomain, reasons);
        double skillScore = CalculateSkillAlignment(profile.CurrentStatus.Skills, targetDomain, skillGaps, reasons);
        double readinessScore = CalculateReadiness(profile.CurrentStatus.Experience, profile.CurrentStatus.EmploymentStatus, reasons);

        double overallScore = (academicScore * 0.45) + (skillScore * 0.35) + (readinessScore * 0.20);
        overallScore = Math.Min(Math.Max(overallScore, 0.0), 1.0);

        string rating;
        bool isGoodFit;

        if (overallScore >= 0.75)
        {
            rating = "STRONG_FIT";
            isGoodFit = true;
        }
        else if (overallScore >= 0.55)
        {
            rating = "MODERATE_FIT";
            isGoodFit = true;
        }
        else if (overallScore >= 0.35)
        {
            rating = "WEAK_FIT";
            isGoodFit = false;
        }
        else
        {
            rating = "CURRENTLY_NOT_ALIGNED";
            isGoodFit = false;
        }

        var result = new GoalValidationResultDto
        {
            Goal = goal,
            AlignmentRating = rating,
            OverallScore = Math.Round(overallScore, 2),
            IsGoodFit = isGoodFit,
            Reasons = reasons,
            SkillGap = skillGaps
        };

        // If goal fit is weak or not aligned, find nearby alternative career paths
        if (!isGoodFit)
        {
            result.RecommendedAlternatives = await FindAlternativeCareersAsync(profile);
        }

        return result;
    }

    private static double CalculateAcademicAlignment(string degree, string spec, string targetDomain, List<string> reasons)
    {
        string combined = $"{degree} {spec}".ToLowerInvariant();
        string target = targetDomain.ToLowerInvariant();

        if (target.Contains("cs") || target.Contains("software") || target.Contains("ai"))
        {
            if (combined.Contains("computer") || combined.Contains("b.tech") || combined.Contains("b.e") || combined.Contains("bca") || combined.Contains("mca"))
            {
                reasons.Add("Degree background aligns well with computing and engineering fundamentals.");
                return 0.90;
            }
            if (combined.Contains("b.a") || combined.Contains("arts") || combined.Contains("humanities"))
            {
                reasons.Add("Current degree (B.A. Arts/Humanities) lacks foundational programming and mathematical coursework.");
                return 0.25;
            }
            reasons.Add("Degree background has partial overlap with software requirements.");
            return 0.50;
        }

        if (target.Contains("civil") || target.Contains("struct"))
        {
            if (combined.Contains("civil"))
            {
                reasons.Add("Civil engineering degree directly satisfies academic prerequisites.");
                return 0.95;
            }
            reasons.Add("Target field requires formal Civil Engineering qualification.");
            return 0.30;
        }

        if (target.Contains("analyst") || target.Contains("fin") || target.Contains("ca"))
        {
            if (combined.Contains("b.com") || combined.Contains("commerce") || combined.Contains("finance") || combined.Contains("economics"))
            {
                reasons.Add("Commerce/Economics degree provides strong foundation for financial analysis.");
                return 0.90;
            }
            reasons.Add("Non-finance degree requires foundational accounting and financial modeling bridge courses.");
            return 0.45;
        }

        reasons.Add("Academic background has moderate alignment with target role.");
        return 0.65;
    }

    private static double CalculateSkillAlignment(List<string> userSkills, string targetDomain, List<string> skillGaps, List<string> reasons)
    {
        var upperSkills = userSkills.Select(s => s.ToUpperInvariant()).ToList();
        string target = targetDomain.ToLowerInvariant();

        if (target.Contains("cs") || target.Contains("software"))
        {
            var required = new[] { "DATA STRUCTURES", "ALGORITHMS", "SQL", "SYSTEM DESIGN" };
            int matched = upperSkills.Count(s => s.Contains("PHP") || s.Contains("REACT") || s.Contains("PYTHON") || s.Contains("JAVA") || s.Contains("SQL") || s.Contains("C++"));
            foreach (var req in required)
            {
                if (!upperSkills.Contains(req)) skillGaps.Add(req);
            }

            if (matched >= 2)
            {
                reasons.Add($"Demonstrated software skills ({string.Join(", ", upperSkills.Take(3))}) provide an active practical foundation.");
                return 0.80;
            }
            reasons.Add("Key technical skills (Programming, Data Structures, SQL) need development.");
            return 0.35;
        }

        if (target.Contains("civil") || target.Contains("struct"))
        {
            if (upperSkills.Contains("STAAD") || upperSkills.Contains("AUTOCAD") || upperSkills.Contains("ETABS") || upperSkills.Contains("STRUCTURAL ANALYSIS"))
            {
                reasons.Add("Possesses essential structural analysis & CAD design skills.");
                return 0.85;
            }
            skillGaps.Add("AutoCAD / STAAD.Pro");
            skillGaps.Add("Structural Analysis");
            reasons.Add("CAD modeling and structural simulation skills are missing.");
            return 0.40;
        }

        if (target.Contains("analyst") || target.Contains("fin"))
        {
            if (upperSkills.Contains("EXCEL") || upperSkills.Contains("ACCOUNTING") || upperSkills.Contains("FINANCIAL ANALYSIS"))
            {
                reasons.Add("Existing spreadsheet and accounting skills support analytical workflow.");
                return 0.80;
            }
            skillGaps.Add("Advanced Excel & Financial Modeling");
            skillGaps.Add("Financial Statement Analysis");
            reasons.Add("Financial modeling and data analysis skills require targeted upskilling.");
            return 0.40;
        }

        return 0.60;
    }

    private static double CalculateReadiness(string experience, string status, List<string> reasons)
    {
        if (experience.Contains("1+") || status.Equals("employed", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("Active industry work experience enhances practical readiness.");
            return 0.85;
        }
        if (experience.Contains("internship"))
        {
            reasons.Add("Internship experience provides preliminary workplace exposure.");
            return 0.70;
        }
        reasons.Add("Fresh entry-level status requires practical portfolio building.");
        return 0.50;
    }

    private async Task<List<AlternativeCareerOptionDto>> FindAlternativeCareersAsync(ParsedProfileDto profile)
    {
        var alternatives = new List<AlternativeCareerOptionDto>();
        string spec = profile.Education.Specialization.ToLowerInvariant();
        string degree = profile.Education.Degree.ToLowerInvariant();

        // Query taxonomy for nodes matching user's actual background
        var taxonomy = await _context.CareerTaxonomyV2.Where(t => t.IsActive && t.Level >= 2).ToListAsync();

        if (degree.Contains("b.a") || spec.Contains("arts") || spec.Contains("humanities"))
        {
            alternatives.Add(new AlternativeCareerOptionDto
            {
                NodeId = "bus.analyst",
                Title = "Business Analyst",
                MatchPercent = 88.0,
                WhyMatches = "Leverages strong communication, business logic, and problem-solving without requiring heavy math/CS degrees.",
                RequiredTransitionSkillCount = 2,
                IsEasierTransition = true
            });
            alternatives.Add(new AlternativeCareerOptionDto
            {
                NodeId = "data.analyst",
                Title = "Data Analyst",
                MatchPercent = 82.0,
                WhyMatches = "Focuses on business data visualization, Excel, and SQL rather than complex algorithm development.",
                RequiredTransitionSkillCount = 3,
                IsEasierTransition = true
            });
            alternatives.Add(new AlternativeCareerOptionDto
            {
                NodeId = "mktg.analyst",
                Title = "Marketing Analyst / Product Specialist",
                MatchPercent = 85.0,
                WhyMatches = "Combines qualitative insights, audience understanding, and analytics for rapid career entry.",
                RequiredTransitionSkillCount = 2,
                IsEasierTransition = true
            });
        }
        else if (spec.Contains("civil"))
        {
            alternatives.Add(new AlternativeCareerOptionDto
            {
                NodeId = "sci.eng.civil.construction",
                Title = "Construction Project Management",
                MatchPercent = 90.0,
                WhyMatches = "Combines civil engineering knowledge with site planning, budgeting, and execution.",
                RequiredTransitionSkillCount = 2,
                IsEasierTransition = true
            });
            alternatives.Add(new AlternativeCareerOptionDto
            {
                NodeId = "sci.eng.civil.transport",
                Title = "Transportation & Highway Engineering",
                MatchPercent = 85.0,
                WhyMatches = "Focuses on infrastructure, traffic flow, and highway planning.",
                RequiredTransitionSkillCount = 2,
                IsEasierTransition = true
            });
        }
        else if (spec.Contains("commerce") || spec.Contains("accounting"))
        {
            alternatives.Add(new AlternativeCareerOptionDto
            {
                NodeId = "com.fin.analyst",
                Title = "Financial Analyst",
                MatchPercent = 92.0,
                WhyMatches = "Natural progression from accounting into financial modeling and valuation.",
                RequiredTransitionSkillCount = 2,
                IsEasierTransition = true
            });
            alternatives.Add(new AlternativeCareerOptionDto
            {
                NodeId = "com.bus.analytics",
                Title = "Business Analytics & Intelligence",
                MatchPercent = 86.0,
                WhyMatches = "Blends financial understanding with modern data tools like PowerBI and Excel.",
                RequiredTransitionSkillCount = 2,
                IsEasierTransition = true
            });
        }
        else
        {
            // Generic high-affinity alternatives
            alternatives.Add(new AlternativeCareerOptionDto
            {
                NodeId = "tech.support",
                Title = "Technical Support / QA Engineering",
                MatchPercent = 80.0,
                WhyMatches = "Provides an immediate entry point into software operations.",
                RequiredTransitionSkillCount = 2,
                IsEasierTransition = true
            });
        }

        return alternatives;
    }
}
