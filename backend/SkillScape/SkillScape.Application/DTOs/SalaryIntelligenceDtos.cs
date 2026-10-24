using System;
using System.Collections.Generic;

namespace SkillScape.Application.DTOs;

public class SalaryForecastRequestV2
{
    public string CareerPathId { get; set; } = string.Empty;
    public string EducationTrack { get; set; } = "Graduation"; // "Graduation" or "PostGraduation"
    public int GraduationYear { get; set; }
    public int? PostGraduationYear { get; set; }
    public string Country { get; set; } = "India";
    public string LocationTier { get; set; } = "Tier 1"; // Tier 1, Tier 2, Tier 3
    public string SkillReadiness { get; set; } = "Intermediate"; // Beginner, Intermediate, Advanced
}

public class SalaryForecastResponseV2
{
    public string CareerTitle { get; set; } = string.Empty;
    public string EducationTrack { get; set; } = string.Empty;
    public string Currency { get; set; } = "INR";
    public SalaryBenchmarkDto Benchmark { get; set; } = new();
    public List<SalaryProjectionPointDto> Projection { get; set; } = new();
    public string DataQuality { get; set; } = string.Empty;
    public List<SalarySourceDto> Sources { get; set; } = new();
    public string Disclaimer { get; set; } = "Market estimates are indicative and not guaranteed compensation.";
    public string Status { get; set; } = "Available";
}

public class SalaryBenchmarkDto
{
    public double P25 { get; set; }
    public double P50 { get; set; }
    public double P75 { get; set; }
    public string Status { get; set; } = "Available";
}

public class SalaryProjectionPointDto
{
    public int ExperienceYear { get; set; }
    public int CalendarYear { get; set; }
    public double P25 { get; set; }
    public double P50 { get; set; }
    public double P75 { get; set; }
    public string ValueType { get; set; } = "Available";
}

public class SalarySourceDto
{
    public string SourceName { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public DateTime UpdatedDate { get; set; }
    public int SampleSize { get; set; }
    public string Occupation { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class BatchSalaryForecastRequest
{
    public List<string> CareerPathIds { get; set; } = new();
    public string EducationTrack { get; set; } = "Graduation";
    public int GraduationYear { get; set; }
    public int? PostGraduationYear { get; set; }
    public string Country { get; set; } = "India";
    public string LocationTier { get; set; } = "Tier 1";
    public string SkillReadiness { get; set; } = "Intermediate";
}

public class BatchSalaryForecastResponse
{
    public Dictionary<string, SalaryForecastResponseV2> Results { get; set; } = new();
}
