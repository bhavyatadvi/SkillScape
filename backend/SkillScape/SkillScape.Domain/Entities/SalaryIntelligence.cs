using System;
using System.Collections.Generic;

namespace SkillScape.Domain.Entities;

public class SalaryBenchmark
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RoleKey { get; set; } = string.Empty;
    public string CanonicalRole { get; set; } = string.Empty;
    public string OccupationFamily { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Country { get; set; } = "India";
    public string Currency { get; set; } = "INR";
    public string ExperienceBand { get; set; } = string.Empty; // e.g. "0-1", "2-3", "4-5"
    public double P25Salary { get; set; }
    public double P50Salary { get; set; }
    public double P75Salary { get; set; }
    public int DataSourceCount { get; set; }
    public DateTime SourceDate { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string DataQuality { get; set; } = "Medium"; // High, Medium, Low, Unavailable
    public string Notes { get; set; } = string.Empty;

    public ICollection<SalaryBenchmarkSource> Sources { get; set; } = new List<SalaryBenchmarkSource>();
}

public class SalaryBenchmarkSource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SalaryBenchmarkId { get; set; } = string.Empty;
    public SalaryBenchmark Benchmark { get; set; } = null!;
    
    public string SourceName { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
    public int SampleSize { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public string Methodology { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class CareerSalaryMapping
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CareerPathId { get; set; } = string.Empty;
    public CareerPath CareerPath { get; set; } = null!;
    
    public string SalaryBenchmarkRoleKey { get; set; } = string.Empty; // Maps to SalaryBenchmark.RoleKey
    
    public string MappingMethod { get; set; } = string.Empty;
    public double MappingConfidence { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
