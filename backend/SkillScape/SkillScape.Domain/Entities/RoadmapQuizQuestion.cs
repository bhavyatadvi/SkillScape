using System;

namespace SkillScape.Domain.Entities;

public class RoadmapQuizQuestion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string QuizId { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string OptionsJson { get; set; } = "[]"; // 4 options JSON: ["Opt1", "Opt2", "Opt3", "Opt4"]
    public int CorrectOptionIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public string DifficultyTag { get; set; } = "Medium";

    public RoadmapModuleQuiz? Quiz { get; set; }
}
