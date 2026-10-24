using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillScape.Infrastructure.Services;

namespace SkillScape.API.Controllers
{
    [ApiController]
    [Route("api/careerswitch")]
    public class CareerSwitchController : ControllerBase
    {
        private readonly IReverseCareerFinderService _service;

        public CareerSwitchController(IReverseCareerFinderService service)
        {
            _service = service;
        }

        [HttpPost("alternatives")]
        [Authorize]
        public async Task<IActionResult> GetAlternatives([FromBody] ReverseCareerFinderDto request)
        {
            if (string.IsNullOrEmpty(request.CurrentCareerNodeId))
                return BadRequest("CurrentCareerNodeId is required");

            var results = await _service.GetAlternativesAsync(request);
            return Ok(results);
        }
    }
}
