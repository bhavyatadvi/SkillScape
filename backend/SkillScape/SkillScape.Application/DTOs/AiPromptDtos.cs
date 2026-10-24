using System.Text.Json.Serialization;

namespace SkillScape.Application.DTOs
{
    public class AiPromptMessageRequest
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class AiQuizGenerationRequest
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;
    }

    public class AiCareerPredictionRequest
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("answers")]
        public List<string> Answers { get; set; } = new();

        [JsonPropertyName("extra_context")]
        public Dictionary<string, string>? ExtraContext { get; set; }
    }

    public class AiQuizQuestionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("qualification_level")]
        public string QualificationLevel { get; set; } = string.Empty;

        [JsonPropertyName("options")]
        public List<AiQuizOptionDto> Options { get; set; } = new();
    }

    public class AiQuizOptionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;

        [JsonPropertyName("weight")]
        public int Weight { get; set; }
    }

    public class AiCareerPredictionDto
    {
        [JsonPropertyName("recommended_domain")]
        public string RecommendedDomain { get; set; } = string.Empty;

        [JsonPropertyName("recommended_title")]
        public string RecommendedTitle { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }

        [JsonPropertyName("all_scores")]
        public Dictionary<string, int> AllScores { get; set; } = new();

        [JsonPropertyName("secondary_domain")]
        public string SecondaryDomain { get; set; } = string.Empty;

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;

        [JsonPropertyName("key_strengths")]
        public List<string> KeyStrengths { get; set; } = new();

        [JsonPropertyName("recommended_skills")]
        public List<string> RecommendedSkills { get; set; } = new();

        [JsonPropertyName("salary_india_inr")]
        public string SalaryIndiaInr { get; set; } = string.Empty;

        [JsonPropertyName("salary_global_usd")]
        public string SalaryGlobalUsd { get; set; } = string.Empty;

        [JsonPropertyName("job_growth")]
        public string JobGrowth { get; set; } = string.Empty;

        [JsonPropertyName("five_year_salary_trend")]
        public Dictionary<string, string> FiveYearSalaryTrend { get; set; } = new();

        [JsonPropertyName("next_steps")]
        public List<string> NextSteps { get; set; } = new();

        [JsonPropertyName("certifications")]
        public List<string> Certifications { get; set; } = new();

        [JsonPropertyName("timeline")]
        public string Timeline { get; set; } = string.Empty;
    }
}
