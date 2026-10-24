using Microsoft.AspNetCore.Mvc;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiPromptController : ControllerBase
    {
        private readonly IAiPromptService _aiPromptService;

        public AiPromptController(IAiPromptService aiPromptService)
        {
            _aiPromptService = aiPromptService;
        }

        [HttpPost("message")]
        public ActionResult<object> HandlePromptMessage([FromBody] AiPromptMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Message is required." });
            }

            var parsed = ParsePromptMessage(request.Message);
            if (!parsed.TryGetValue("task", out var taskValue) || !parsed.TryGetValue("category", out var category))
            {
                return BadRequest(new { error = "Task and category are required in the prompt." });
            }

            var task = taskValue.Trim().ToUpperInvariant();
            if (task == "A")
            {
                return Ok(_aiPromptService.GenerateQuizQuestions(category.Trim().ToLowerInvariant()));
            }

            if (task == "B")
            {
                var answers = ExtractAnswers(parsed.GetValueOrDefault("answers"));
                return Ok(_aiPromptService.PredictCareer(category.Trim().ToLowerInvariant(), answers, parsed));
            }

            return BadRequest(new { error = "Unsupported task. Only TASK A and TASK B are supported." });
        }

        [HttpPost("quiz")]
        public ActionResult<List<AiQuizQuestionDto>> GenerateQuiz([FromBody] AiQuizGenerationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Category))
            {
                return BadRequest(new { error = "Category is required." });
            }

            return Ok(_aiPromptService.GenerateQuizQuestions(request.Category.Trim().ToLowerInvariant()));
        }

        [HttpPost("predict")]
        public ActionResult<AiCareerPredictionDto> PredictCareer([FromBody] AiCareerPredictionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Category))
            {
                return BadRequest(new { error = "Category is required." });
            }

            var answers = request.Answers ?? new List<string>();
            return Ok(_aiPromptService.PredictCareer(request.Category.Trim().ToLowerInvariant(), answers, request.ExtraContext));
        }

        private static Dictionary<string, string> ParsePromptMessage(string message)
        {
            var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? currentKey = null;
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var separatorIndex = line.IndexOf(':');
                if (separatorIndex > 0)
                {
                    currentKey = line[..separatorIndex].Trim().ToLowerInvariant();
                    data[currentKey] = line[(separatorIndex + 1)..].Trim();
                }
                else if (currentKey != null)
                {
                    data[currentKey] += " " + line;
                }
            }

            return data;
        }

        private static List<string> ExtractAnswers(string? answersValue)
        {
            if (string.IsNullOrWhiteSpace(answersValue))
            {
                return new List<string>();
            }

            return answersValue
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(answer => answer.Trim())
                .Where(answer => !string.IsNullOrEmpty(answer))
                .ToList();
        }
    }
}
