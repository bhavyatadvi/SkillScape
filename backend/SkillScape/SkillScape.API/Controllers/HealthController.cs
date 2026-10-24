using Microsoft.AspNetCore.Mvc;

namespace SkillScape.API.Controllers;

/// <summary>
/// Health check endpoint
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Check API health status
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "API is running",
            timestamp = DateTime.UtcNow
        });
    }
}
