using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class SalaryIntelligenceService : ISalaryIntelligenceService
{
    private readonly ApplicationDbContext _context;

    public SalaryIntelligenceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryForecastResponseV2> ForecastSalaryAsync(SalaryForecastRequestV2 request)
    {
        var careerPathId = request.CareerPathId?.Trim() ?? string.Empty;
        var career = await _context.CareerPaths.FirstOrDefaultAsync(p => p.Id == careerPathId || p.Title.ToLower() == careerPathId.ToLower());
        var taxonomyCareer = await _context.CareerTaxonomyV2
            .FirstOrDefaultAsync(p => p.Id == careerPathId || p.Name.ToLower() == careerPathId.ToLower());
        
        string careerTitle = career?.Title ?? taxonomyCareer?.Name ?? careerPathId;
        string roleKey = GetDefaultRoleKey($"{careerTitle} {careerPathId}");

        // Taxonomy ids carry domain context (for example arts.law.criminal), while
        // leaf names such as "Criminal Law" are too ambiguous for name-only matching.
        if (taxonomyCareer != null)
        {
            var taxonomyContext = $"{taxonomyCareer.Id} {taxonomyCareer.Name} {taxonomyCareer.Domain} {taxonomyCareer.Subdomain} {taxonomyCareer.Specialization}";
            roleKey = GetDefaultRoleKey(taxonomyContext);
        }

        if (career != null)
        {
            var mapping = await _context.CareerSalaryMappings
                .Include(m => m.CareerPath)
                .FirstOrDefaultAsync(m => m.CareerPathId == career.Id);
            
            if (mapping != null)
            {
                roleKey = mapping.SalaryBenchmarkRoleKey;
            }
        }

        var benchmarks = await _context.SalaryBenchmarks
            .Include(b => b.Sources)
            .Where(b => b.RoleKey == roleKey && b.Country == request.Country)
            .ToListAsync();

        if (!benchmarks.Any())
        {
            var taxonomyForecast = taxonomyCareer == null ? null : await GenerateForecastFromTaxonomyAsync(careerTitle, taxonomyCareer, request);
            return taxonomyForecast ?? GenerateFallbackForecast(careerTitle, request);
        }

        return GenerateForecastFromBenchmarks(careerTitle, benchmarks, request);
    }

    private async Task<SalaryForecastResponseV2?> GenerateForecastFromTaxonomyAsync(string careerTitle, CareerTaxonomyV2 career, SalaryForecastRequestV2 request)
    {
        try
        {
            var salaryRange = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(career.SalaryRangeJson);
            if (salaryRange == null || !salaryRange.TryGetValue("entry", out var entry) || entry <= 0)
            {
                var parentId = career.ParentId;
                var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                while (!string.IsNullOrWhiteSpace(parentId) && visited.Add(parentId))
                {
                    var parent = await _context.CareerTaxonomyV2.FirstOrDefaultAsync(node => node.Id == parentId);
                    if (parent == null) break;
                    salaryRange = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(parent.SalaryRangeJson);
                    if (salaryRange != null && salaryRange.TryGetValue("entry", out entry) && entry > 0) break;
                    parentId = parent.ParentId;
                }
            }
            if (salaryRange == null || !salaryRange.TryGetValue("entry", out entry) || entry <= 0) return null;

            var mid = salaryRange.TryGetValue("mid", out var midValue) && midValue > entry ? midValue : entry * 1.5;
            var senior = salaryRange.TryGetValue("senior", out var seniorValue) && seniorValue > mid ? seniorValue : mid * 1.5;
            var startYear = request.EducationTrack == "PostGraduation" && request.PostGraduationYear.HasValue
                ? request.PostGraduationYear.Value
                : request.GraduationYear;
            var projection = Enumerable.Range(0, 6).Select(year =>
            {
                var progress = year / 5d;
                return new SalaryProjectionPointDto
                {
                    ExperienceYear = year,
                    CalendarYear = startYear + year,
                    P25 = Math.Round(entry + (mid - entry) * progress),
                    P50 = Math.Round(entry * 1.25 + (mid * 1.1 - entry * 1.25) * progress),
                    P75 = Math.Round(Math.Max(entry * 1.5, entry) + (senior - Math.Max(entry * 1.5, entry)) * progress),
                    ValueType = "Indicative"
                };
            }).ToList();

            return new SalaryForecastResponseV2
            {
                CareerTitle = careerTitle,
                EducationTrack = request.EducationTrack,
                Currency = "INR",
                Benchmark = new SalaryBenchmarkDto { P25 = Math.Round(entry), P50 = Math.Round(entry * 1.25), P75 = Math.Round(Math.Max(entry * 1.5, mid)) },
                Projection = projection,
                DataQuality = "Indicative",
                Sources = new List<SalarySourceDto>(),
                Disclaimer = "Indicative salary estimate from the career taxonomy range; actual pay varies by role, location, and employer.",
                Status = "Available"
            };
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    public async Task<BatchSalaryForecastResponse> CompareCareersAsync(BatchSalaryForecastRequest request)
    {
        var response = new BatchSalaryForecastResponse();

        foreach (var id in request.CareerPathIds)
        {
            try
            {
                var singleRequest = new SalaryForecastRequestV2
                {
                    CareerPathId = id,
                    EducationTrack = request.EducationTrack,
                    GraduationYear = request.GraduationYear,
                    PostGraduationYear = request.PostGraduationYear,
                    Country = request.Country,
                    LocationTier = request.LocationTier,
                    SkillReadiness = request.SkillReadiness
                };
                
                var result = await ForecastSalaryAsync(singleRequest);
                response.Results[id] = result;
            }
            catch
            {
                // Skip if career not found
            }
        }

        return response;
    }

    private string GetDefaultRoleKey(string title)
    {
        var t = title.ToLowerInvariant();
        if (t.Contains("law") || t.Contains("legal") || t.Contains("llb") || t.Contains("advocate") || t.Contains("prosecutor") || t.Contains("judge")) return "legal_professional";
        if (t.Contains("pharm.research") || t.Contains("pharmaceutical research") || t.Contains("pharmacy research")) return "pharmacy_research";
        if (t.Contains("pharm.clinical") || t.Contains("clinical pharmacist") || t.Contains("clinical pharmacy")) return "pharmacy_clinical";
        if (t.Contains("pharm.sales") || t.Contains("pharmaceutical sales") || t.Contains("medical representative") || t.Contains("medical sales representative")) return "pharmacy_sales";
        if (t.Contains("arch.urban") || t.Contains("urban design") || t.Contains("urban designer") || t.Contains("urban planning")) return "architecture_urban_design";
        if (t.Contains("arch.") || t.Contains("architect")) return "architecture_general";
        if (t.Contains("software") || t.Contains("developer") || t.Contains("coder") || t.Contains("technology specialization") || t.Contains("backend") || t.Contains("frontend") || t.Contains("full stack") || t.Contains("technology") || t.Contains("tech") || t.Contains("system")) return "software_engineer";
        if (t.Contains("data sci") || t.Contains("data anal") || t.Contains("analytics") || t.Contains("data eng") || t.Contains("statistics")) return "data_analyst";
        if (t.Contains("cyber") || t.Contains("security") || t.Contains("soc") || t.Contains("pentest")) return "cybersecurity_analyst";
        if (t.Contains("network") || t.Contains("telecom") || t.Contains("cloud")) return "network_engineer";
        if (t.Contains("ai") || t.Contains("machine learning") || t.Contains("artificial intelligence") || t.Contains("computer vision") || t.Contains("nlp")) return "ai_engineer";
        
        // Map other engineering to software_engineer as fallback for tech roles
        if (t.Contains("engineering") || t.Contains("engineer") || t.Contains("robotics") || t.Contains("vlsi")) return "software_engineer";
        
        return "general_professional";
    }

    private SalaryForecastResponseV2 GenerateForecastFromBenchmarks(string careerTitle, List<SalaryBenchmark> benchmarks, SalaryForecastRequestV2 request)
    {
        var entryB = benchmarks.FirstOrDefault(b => b.ExperienceBand == "0-3") ?? benchmarks.First();
        var midB = benchmarks.FirstOrDefault(b => b.ExperienceBand == "4-7") ?? entryB;
        var expB = benchmarks.FirstOrDefault(b => b.ExperienceBand == "8+") ?? midB;

        var projection = new List<SalaryProjectionPointDto>();
        
        int startYear = request.EducationTrack == "PostGraduation" && request.PostGraduationYear.HasValue 
            ? request.PostGraduationYear.Value 
            : request.GraduationYear;

        for (int i = 0; i <= 5; i++)
        {
            SalaryBenchmark currentB;
            string valueType = "LimitedData";
            
            if (i == 0) 
            {
                currentB = entryB;
                valueType = "Observed";
            }
            else 
            {
                // We only have bands (0-3, 4-7, 8+), not exact annual salaries.
                // Do not pretend bands are annual observed values.
                if (i <= 3) currentB = entryB; // just passing the band for range reference
                else currentB = midB;
            }

            projection.Add(new SalaryProjectionPointDto
            {
                ExperienceYear = i,
                CalendarYear = startYear + i,
                P25 = Math.Round(currentB.P25Salary),
                P50 = Math.Round(currentB.P50Salary),
                P75 = Math.Round(currentB.P75Salary),
                ValueType = valueType
            });
        }

        var allSources = benchmarks.SelectMany(b => b.Sources)
            .GroupBy(s => s.SourceName)
            .Select(g => g.First())
            .Select(s => new SalarySourceDto
            {
                SourceName = s.SourceName,
                SourceUrl = s.SourceUrl,
                UpdatedDate = s.RetrievedAt,
                SampleSize = s.SampleSize,
                Occupation = entryB.CanonicalRole,
                Location = entryB.Country
            }).ToList();

        return new SalaryForecastResponseV2
        {
            CareerTitle = careerTitle,
            EducationTrack = request.EducationTrack,
            Currency = entryB.Currency,
            Benchmark = new SalaryBenchmarkDto
            {
                P25 = Math.Round(entryB.P25Salary),
                P50 = Math.Round(entryB.P50Salary),
                P75 = Math.Round(entryB.P75Salary)
            },
            Projection = projection,
            DataQuality = entryB.DataQuality,
            Sources = allSources
        };
    }

    private SalaryForecastResponseV2 GenerateFallbackForecast(string title, SalaryForecastRequestV2 request)
    {
        const double entrySalary = 300000;
        const double yearFiveSalary = 600000;
        var projection = new List<SalaryProjectionPointDto>();
        int startYear = request.EducationTrack == "PostGraduation" && request.PostGraduationYear.HasValue 
            ? request.PostGraduationYear.Value 
            : request.GraduationYear;

        for (int i = 0; i <= 5; i++)
        {
            projection.Add(new SalaryProjectionPointDto
            {
                ExperienceYear = i,
                CalendarYear = startYear + i,
                P25 = Math.Round(entrySalary * (1 + (yearFiveSalary / entrySalary - 1) * i / 5d) * 0.75),
                P50 = Math.Round(entrySalary * (1 + (yearFiveSalary / entrySalary - 1) * i / 5d)),
                P75 = Math.Round(entrySalary * (1 + (yearFiveSalary / entrySalary - 1) * i / 5d) * 1.5),
                ValueType = "Indicative"
            });
        }

        return new SalaryForecastResponseV2
        {
            CareerTitle = title,
            EducationTrack = request.EducationTrack,
            Currency = "INR",
            Benchmark = new SalaryBenchmarkDto 
            { 
                P25 = entrySalary * 0.75,
                P50 = entrySalary,
                P75 = entrySalary * 1.5
            },
            Projection = projection,
            DataQuality = "Indicative",
            Sources = new List<SalarySourceDto>(),
            Disclaimer = "Indicative general salary estimate because a career-specific market benchmark is not available.",
            Status = "Available"
        };
    }
}
