using Microsoft.ML.Data;

namespace SkillScape.Infrastructure.Services;

public class QuizData
{
    [LoadColumn(0)]
    public string CombinedAnswers { get; set; } = string.Empty;

    [LoadColumn(1)]
    public string RecommendedDomain { get; set; } = string.Empty;
}

public class QuizPrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedDomain { get; set; } = string.Empty;
    public float[] Score { get; set; } = Array.Empty<float>();
}

public class SalaryData
{
    [LoadColumn(0)]
    public string Domain { get; set; } = string.Empty;

    [LoadColumn(1)]
    public float YearsOfExperience { get; set; }

    [LoadColumn(2)]
    public float Salary { get; set; }
}

public class SalaryPrediction
{
    [ColumnName("Score")]
    public float PredictedSalary { get; set; }
}
