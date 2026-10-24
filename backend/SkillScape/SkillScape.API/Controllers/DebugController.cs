using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using SkillScape.Infrastructure.Data;
using SkillScape.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace SkillScape.API.Controllers;

[ApiController]
[Route("api/debug")]
[Authorize(Roles = "Admin")]
public class DebugController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DebugController> _logger;

    public DebugController(
        ApplicationDbContext db,
        IWebHostEnvironment environment,
        ILogger<DebugController> logger)
    {
        _db = db;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet("test-save")]
    public async Task<IActionResult> TestSave()
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        try
        {
            var user = _db.Users.FirstOrDefault();
            if (user == null) return BadRequest("No user");

            var result = new QuizResult
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                RecommendedDomainId = "fullstack",
                ScoresJson = "{}",
                RecommendationReason = "Test",
                CompletedAt = DateTime.UtcNow
            };

            _db.QuizResults.Add(result);
            await _db.SaveChangesAsync();

            return Ok("Saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Development debug save failed.");
            return StatusCode(500, new { Message = "Debug save failed." });
        }
    }
}
