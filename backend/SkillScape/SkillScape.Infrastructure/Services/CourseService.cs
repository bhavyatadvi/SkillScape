using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Course>> SearchCoursesAsync(string query, string domain, string level, string type, bool? isFree, decimal? maxPrice, string duration)
        {
            var queryable = _context.Courses
                .Include(c => c.Provider)
                .Include(c => c.CourseSkills)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                query = query.ToLower();
                queryable = queryable.Where(c => c.Title.ToLower().Contains(query) || c.Description.ToLower().Contains(query) || c.CourseSkills.Any(s => s.SkillName.ToLower().Contains(query)));
            }

            if (!string.IsNullOrEmpty(domain))
            {
                queryable = queryable.Where(c => c.Domain == domain);
            }

            if (!string.IsNullOrEmpty(level))
            {
                queryable = queryable.Where(c => c.Level == level);
            }

            if (!string.IsNullOrEmpty(type))
            {
                queryable = queryable.Where(c => c.CertificateType.Contains(type));
            }

            if (isFree.HasValue)
            {
                queryable = queryable.Where(c => c.IsFree == isFree.Value);
            }

            if (maxPrice.HasValue)
            {
                queryable = queryable.Where(c => c.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(duration))
            {
                // Basic implementation for filtering duration
                if (duration.Contains("Months"))
                    queryable = queryable.Where(c => c.DurationUnit == "Months");
                else if (duration.Contains("Weeks"))
                    queryable = queryable.Where(c => c.DurationUnit == "Weeks");
                else if (duration.Contains("Hours"))
                    queryable = queryable.Where(c => c.DurationUnit == "Hours");
            }

            return await queryable
                .OrderByDescending(c => c.RelevanceScore)
                .Take(50)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetTrendingCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Provider)
                .OrderByDescending(c => c.TrendingScore)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetRecommendedCoursesAsync(string userId)
        {
            var latestQuizResult = await _context.QuizResultsV2
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.GeneratedAt)
                .FirstOrDefaultAsync();

            var query = _context.Courses
                .Include(c => c.Provider)
                .Include(c => c.CourseSkills)
                .AsQueryable();

            if (latestQuizResult != null)
            {
                var skillGaps = new List<string>();
                try
                {
                    skillGaps = System.Text.Json.JsonSerializer.Deserialize<List<string>>(latestQuizResult.SkillGapJson ?? "[]") ?? new List<string>();
                }
                catch { }

                if (skillGaps.Any())
                {
                    var lowercaseGaps = skillGaps.Select(g => g.ToLower()).ToList();
                    var allCourses = await query.ToListAsync(); // Pull to memory if EF translates `Contains` poorly for complex collections
                    
                    var matchedCourses = allCourses.Where(c => 
                        c.CourseSkills.Any(s => lowercaseGaps.Any(gap => gap.Contains(s.SkillName.ToLower()) || s.SkillName.ToLower().Contains(gap))) ||
                        lowercaseGaps.Any(gap => c.Title.ToLower().Contains(gap))
                    ).OrderByDescending(c => c.RelevanceScore).ToList();

                    if (matchedCourses.Count >= 3)
                    {
                        return matchedCourses.Take(10);
                    }
                    
                    // Fallback to domain matching
                    var fallbackQuery = _context.Courses.Include(c => c.Provider).AsQueryable();
                    var domainHint = latestQuizResult.PrimaryTaxonomyNodeId.Split('.').FirstOrDefault() ?? "";
                    if (!string.IsNullOrEmpty(domainHint) && domainHint != "Unknown")
                    {
                        var domainMatches = await fallbackQuery.Where(c => c.Domain.ToLower().Contains(domainHint.ToLower()) || c.Description.ToLower().Contains(domainHint.ToLower())).OrderByDescending(c => c.RelevanceScore).Take(10).ToListAsync();
                        if (domainMatches.Any()) return domainMatches;
                    }
                }
            }

            // Fallback for no quiz result or no matches
            return await _context.Courses
                .Include(c => c.Provider)
                .OrderByDescending(c => c.RelevanceScore)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<TrendingSkill>> GetTrendingSkillsAsync()
        {
            return await _context.TrendingSkills
                .OrderByDescending(s => s.TrendingScore)
                .Take(10)
                .ToListAsync();
        }

        public async Task<bool> ToggleSaveCourseAsync(string userId, string courseId)
        {
            var existingSave = await _context.SavedCourses
                .FirstOrDefaultAsync(sc => sc.UserId == userId && sc.CourseId == courseId);

            if (existingSave != null)
            {
                _context.SavedCourses.Remove(existingSave);
                await _context.SaveChangesAsync();
                return false; // Unsaved
            }
            else
            {
                _context.SavedCourses.Add(new SavedCourse
                {
                    UserId = userId,
                    CourseId = courseId
                });
                await _context.SaveChangesAsync();
                return true; // Saved
            }
        }

        public async Task<IEnumerable<Course>> GetSavedCoursesAsync(string userId)
        {
            return await _context.SavedCourses
                .Where(sc => sc.UserId == userId)
                .Include(sc => sc.Course)
                    .ThenInclude(c => c.Provider)
                .Select(sc => sc.Course)
                .ToListAsync();
        }

        public async Task<Course> GetCourseDetailsAsync(string courseId)
        {
            return await _context.Courses
                .Include(c => c.Provider)
                .Include(c => c.CourseSkills)
                .Include(c => c.CourseCareers)
                .Include(c => c.EducationEligibilities)
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }
    }
}
