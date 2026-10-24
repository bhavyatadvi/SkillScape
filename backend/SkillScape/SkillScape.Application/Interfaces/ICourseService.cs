using System.Collections.Generic;
using System.Threading.Tasks;
using SkillScape.Domain.Entities;

namespace SkillScape.Application.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> SearchCoursesAsync(string query, string domain, string level, string type, bool? isFree, decimal? maxPrice, string duration);
        Task<IEnumerable<Course>> GetTrendingCoursesAsync();
        Task<IEnumerable<Course>> GetRecommendedCoursesAsync(string userId);
        Task<IEnumerable<TrendingSkill>> GetTrendingSkillsAsync();
        Task<bool> ToggleSaveCourseAsync(string userId, string courseId);
        Task<IEnumerable<Course>> GetSavedCoursesAsync(string userId);
        Task<Course> GetCourseDetailsAsync(string courseId);
    }
}
