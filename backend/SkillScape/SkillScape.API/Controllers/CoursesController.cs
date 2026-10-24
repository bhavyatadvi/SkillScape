using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Application.DTOs;

namespace SkillScape.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Course>>> SearchCourses([FromQuery] string query = "", [FromQuery] string domain = "", [FromQuery] string level = "", [FromQuery] string type = "", [FromQuery] bool? isFree = null, [FromQuery] decimal? maxPrice = null, [FromQuery] string duration = "")
        {
            var courses = await _courseService.SearchCoursesAsync(query, domain, level, type, isFree, maxPrice, duration);
            return Ok(courses);
        }

        [HttpGet("trending")]
        public async Task<ActionResult<IEnumerable<Course>>> GetTrendingCourses()
        {
            var courses = await _courseService.GetTrendingCoursesAsync();
            return Ok(courses);
        }


        [HttpGet("trending-skills")]
        public async Task<ActionResult<IEnumerable<TrendingSkill>>> GetTrendingSkills()
        {
            var skills = await _courseService.GetTrendingSkillsAsync();
            return Ok(skills);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourseDetails(string id)
        {
            var course = await _courseService.GetCourseDetailsAsync(id);
            if (course == null) return NotFound();
            
            return Ok(course);
        }

        [HttpPost("{id}/save")]
        [Authorize]
        public async Task<ActionResult> ToggleSaveCourse(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var isSaved = await _courseService.ToggleSaveCourseAsync(userId, id);
            return Ok(new { saved = isSaved });
        }

        [HttpGet("saved")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Course>>> GetSavedCourses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var courses = await _courseService.GetSavedCoursesAsync(userId);
            return Ok(courses);
        }
    }
}
