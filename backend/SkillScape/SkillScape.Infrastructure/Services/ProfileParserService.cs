using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;

namespace SkillScape.Infrastructure.Services;

public class ProfileParserService : IProfileParserService, IOcrIngestionService
{
    private static readonly Dictionary<string, string> DegreeSynonyms = new(StringComparer.OrdinalIgnoreCase)
    {
        { "btech", "B.Tech" },
        { "b.tech", "B.Tech" },
        { "be", "B.E." },
        { "b.e", "B.E." },
        { "bachelor of technology", "B.Tech" },
        { "bachelor of engineering", "B.E." },
        { "bcom", "B.Com" },
        { "b.com", "B.Com" },
        { "bachelor of commerce", "B.Com" },
        { "bsc", "B.Sc" },
        { "b.sc", "B.Sc" },
        { "bachelor of science", "B.Sc" },
        { "ba", "B.A." },
        { "b.a", "B.A." },
        { "bachelor of arts", "B.A." },
        { "mtech", "M.Tech" },
        { "m.tech", "M.Tech" },
        { "msc", "M.Sc" },
        { "m.sc", "M.Sc" },
        { "mba", "MBA" },
        { "mbbs", "MBBS" },
        { "bds", "BDS" },
        { "bca", "BCA" },
        { "mca", "MCA" },
        { "diploma", "Polytechnic Diploma" },
        { "iti", "ITI Trade Certificate" },
        { "10th", "Class 10" },
        { "12th", "Class 12" }
    };

    private static readonly Dictionary<string, string> SpecializationSynonyms = new(StringComparer.OrdinalIgnoreCase)
    {
        { "computer engineering", "Computer Science & Engineering" },
        { "computer science", "Computer Science & Engineering" },
        { "cs", "Computer Science & Engineering" },
        { "cse", "Computer Science & Engineering" },
        { "information technology", "Information Technology" },
        { "it", "Information Technology" },
        { "civil", "Civil Engineering" },
        { "civil engineering", "Civil Engineering" },
        { "mechanical", "Mechanical Engineering" },
        { "mechanical engineering", "Mechanical Engineering" },
        { "electronics", "Electronics & Communication" },
        { "ece", "Electronics & Communication" },
        { "electrical", "Electrical Engineering" },
        { "eee", "Electrical Engineering" },
        { "biology", "Biological Sciences" },
        { "chemistry", "Chemistry" },
        { "physics", "Physics" },
        { "mathematics", "Mathematics" },
        { "maths", "Mathematics" },
        { "math", "Mathematics" },
        { "accounts", "Accounting & Finance" },
        { "accounting", "Accounting & Finance" },
        { "finance", "Finance" },
        { "psychology", "Psychology" },
        { "sociology", "Sociology" },
        { "economics", "Economics" },
        { "law", "Law / Legal Studies" }
    };

    private static readonly string[] RecognizedSkills = new[]
    {
        "php", "react", "node", "python", "java", "c++", "c#", "sql", "javascript", "typescript",
        "excel", "tally", "accounting", "financial analysis", "autocad", "staad", "staad.pro", "etabs",
        "cad", "structural analysis", "surveying", "site coordination", "biology", "chemistry", "physics",
        "data analysis", "machine learning", "ai", "deep learning", "cybersecurity", "ui/ux", "graphic design",
        "sketching", "writing", "journalism", "public speaking", "project management", "marketing"
    };

    public ParsedProfileDto ParseProfile(
        string phaseKey,
        string? currentStatusRaw,
        string? primaryGoalRaw,
        Dictionary<string, string>? rawFields = null)
    {
        rawFields ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        var parsed = new ParsedProfileDto();

        // Check if we have structured data from the new AfterGraduationForm
        bool isStructured = rawFields.ContainsKey("educationLevel") || rawFields.ContainsKey("major");

        if (isStructured)
        {
            parsed.Education.Level = GetField(rawFields, "educationLevel");
            parsed.Education.Degree = GetField(rawFields, "degree");
            parsed.Education.Specialization = GetField(rawFields, "major");
            
            parsed.CurrentStatus.EmploymentStatus = GetField(rawFields, "currentStatus");
            parsed.CurrentStatus.Role = GetField(rawFields, "currentStatus");
            parsed.CurrentStatus.Experience = DetectExperience(parsed.CurrentStatus.Role);
            
            parsed.PrimaryGoal.Goal = GetField(rawFields, "declaredGoal");
            parsed.PrimaryGoal.GoalCategory = DetectGoalCategory(parsed.PrimaryGoal.Goal);
            parsed.PrimaryGoal.TargetDomain = DetectTargetDomain(parsed.PrimaryGoal.Goal, parsed.Education.Specialization);
            
            parsed.ExtractedFields["goalConfidence"] = GetField(rawFields, "goalConfidence");
        }
        else
        {
            string currentStatusText = currentStatusRaw ?? GetField(rawFields, "currentStatus", "backgroundText", "profileText") ?? string.Empty;
            string primaryGoalText = primaryGoalRaw ?? GetField(rawFields, "primaryGoal", "goalText", "targetGoal") ?? string.Empty;

            string fullText = $"{currentStatusText} {primaryGoalText}".ToLowerInvariant();

            // 1. Detect Education Level & Degree
            parsed.Education.Level = string.IsNullOrWhiteSpace(phaseKey) ? DetectStageFromText(fullText) : phaseKey;
            parsed.Education.Degree = DetectDegree(fullText) ?? GetField(rawFields, "degree", "stream") ?? string.Empty;
            parsed.Education.Specialization = DetectSpecialization(fullText) ?? GetField(rawFields, "specialization", "major") ?? string.Empty;

            // 2. Detect Current Status
            parsed.CurrentStatus.Role = DetectRole(currentStatusText);
            parsed.CurrentStatus.EmploymentStatus = DetectEmploymentStatus(currentStatusText);
            parsed.CurrentStatus.Experience = DetectExperience(currentStatusText);
            parsed.CurrentStatus.Skills = DetectSkills(fullText);

            // 3. Detect Primary Goal & Target Domain
            parsed.PrimaryGoal.Goal = string.IsNullOrWhiteSpace(primaryGoalText) ? "Explore Career Directions" : primaryGoalText.Trim();
            parsed.PrimaryGoal.GoalCategory = DetectGoalCategory(primaryGoalText);
            parsed.PrimaryGoal.TargetDomain = DetectTargetDomain(fullText, parsed.Education.Specialization);
        }

        // 4. Build ExtractedFields Dictionary
        parsed.ExtractedFields["phaseKey"] = string.IsNullOrEmpty(parsed.Education.Level) ? phaseKey : parsed.Education.Level;
        if (!string.IsNullOrEmpty(parsed.Education.Degree)) parsed.ExtractedFields["degree"] = parsed.Education.Degree;
        if (!string.IsNullOrEmpty(parsed.Education.Specialization)) parsed.ExtractedFields["specialization"] = parsed.Education.Specialization;
        if (!string.IsNullOrEmpty(parsed.CurrentStatus.Role)) parsed.ExtractedFields["currentRole"] = parsed.CurrentStatus.Role;
        if (!string.IsNullOrEmpty(parsed.CurrentStatus.EmploymentStatus)) parsed.ExtractedFields["employmentStatus"] = parsed.CurrentStatus.EmploymentStatus;
        if (parsed.CurrentStatus.Skills.Any()) parsed.ExtractedFields["skills"] = string.Join(",", parsed.CurrentStatus.Skills);
        parsed.ExtractedFields["primaryGoal"] = string.IsNullOrEmpty(parsed.PrimaryGoal.Goal) ? "Explore Career Directions" : parsed.PrimaryGoal.Goal;
        if (!string.IsNullOrEmpty(parsed.PrimaryGoal.TargetDomain)) parsed.ExtractedFields["targetDomain"] = parsed.PrimaryGoal.TargetDomain;

        // Merge any user rawFields
        foreach (var kv in rawFields)
        {
            if (!parsed.ExtractedFields.ContainsKey(kv.Key))
            {
                parsed.ExtractedFields[kv.Key] = kv.Value;
            }
        }

        return parsed;
    }

    public Task<string> ExtractTextFromDocumentAsync(byte[] documentBytes, string fileName)
    {
        // Document OCR Pipeline Abstraction
        // Parses text bytes or returns basic extracted placeholder text for PDF/images.
        if (documentBytes == null || documentBytes.Length == 0)
            return Task.FromResult(string.Empty);

        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        string text = System.Text.Encoding.UTF8.GetString(documentBytes);
        
        // Remove non-printable characters for clean NLP processing
        string cleaned = Regex.Replace(text, @"[^\u0009\u000A\u000D\u0020-\u007E]", " ");
        return Task.FromResult(cleaned.Trim());
    }

    private static string GetField(Dictionary<string, string> fields, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (fields.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val))
                return val.Trim();
        }
        return string.Empty;
    }

    private static string DetectStageFromText(string text)
    {
        if (text.Contains("10th") || text.Contains("class 10") || text.Contains("matriculation")) return "After10th";
        if (text.Contains("12th") || text.Contains("class 12") || text.Contains("higher secondary")) return "After12th";
        if (text.Contains("m.tech") || text.Contains("m.sc") || text.Contains("mba") || text.Contains("post grad") || text.Contains("master")) return "AfterPostGraduation";
        if (text.Contains("switch") || text.Contains("transition")) return "CareerSwitch";
        if (text.Contains("upskill") || text.Contains("certification")) return "Upskilling";
        if (text.Contains("startup") || text.Contains("business") || text.Contains("entrepreneur")) return "Entrepreneurship";
        if (text.Contains("upsc") || text.Contains("government") || text.Contains("bank po") || text.Contains("ssc")) return "GovernmentJobs";
        return "AfterGraduation";
    }

    private static string? DetectDegree(string text)
    {
        foreach (var kv in DegreeSynonyms)
        {
            if (Regex.IsMatch(text, $@"\b{Regex.Escape(kv.Key)}\b", RegexOptions.IgnoreCase))
                return kv.Value;
        }
        return null;
    }

    private static string? DetectSpecialization(string text)
    {
        foreach (var kv in SpecializationSynonyms)
        {
            if (Regex.IsMatch(text, $@"\b{Regex.Escape(kv.Key)}\b", RegexOptions.IgnoreCase))
                return kv.Value;
        }
        return null;
    }

    private static string DetectRole(string text)
    {
        if (text.Contains("unemployed") || text.Contains("looking for work") || text.Contains("seeking job")) return "Unemployed Graduate";
        if (text.Contains("intern") || text.Contains("internship")) return "Intern / Trainee";
        if (text.Contains("accounts assistant") || text.Contains("accountant")) return "Accounts Assistant";
        if (text.Contains("developer") || text.Contains("engineer") || text.Contains("programmer")) return "Software Engineer";
        if (text.Contains("student")) return "Student";
        return "Graduate Candidate";
    }

    private static string DetectEmploymentStatus(string text)
    {
        if (text.Contains("unemployed") || text.Contains("jobless") || text.Contains("looking for job")) return "unemployed";
        if (text.Contains("working") || text.Contains("employed") || text.Contains("job as")) return "employed";
        return "student";
    }

    private static string DetectExperience(string text)
    {
        if (text.Contains("internship") || text.Contains("intern")) return "Internship Experience";
        if (text.Contains("fresher") || text.Contains("no experience")) return "Fresher";
        if (Regex.IsMatch(text, @"\b[1-9]\s*year")) return "1+ Years Experience";
        return "Entry Level";
    }

    private static List<string> DetectSkills(string text)
    {
        var skills = new List<string>();
        foreach (var skill in RecognizedSkills)
        {
            if (Regex.IsMatch(text, $@"\b{Regex.Escape(skill)}\b", RegexOptions.IgnoreCase))
            {
                skills.Add(skill.ToUpperInvariant());
            }
        }
        return skills.Distinct().ToList();
    }

    private static string DetectGoalCategory(string text)
    {
        if (text.Contains("job") || text.Contains("work") || text.Contains("career") || text.Contains("role")) return "employment";
        if (text.Contains("higher study") || text.Contains("master") || text.Contains("phd") || text.Contains("m.tech")) return "higher_education";
        if (text.Contains("switch") || text.Contains("move into") || text.Contains("transition")) return "switch";
        return "employment";
    }

    private static string DetectTargetDomain(string text, string currentSpec)
    {
        if (text.Contains("structural")) return "sci.eng.civil.struct";
        if (text.Contains("ai") || text.Contains("machine learning") || text.Contains("data scientist") || text.Contains("data science")) return "sci.eng.cs.ai";
        if (text.Contains("software") || text.Contains("developer") || text.Contains("web") || text.Contains("coding")) return "sci.eng.cs";
        if (text.Contains("civil") || text.Contains("construction") || text.Contains("building") || text.Contains("structure")) return "sci.eng.civil";
        if (text.Contains("financial analyst") || text.Contains("finance") || text.Contains("investment")) return "com.fin.analyst";
        if (text.Contains("accounting") || text.Contains("chartered accountancy") || text.Contains("ca")) return "com.fin.ca";
        if (text.Contains("medical") || text.Contains("doctor") || text.Contains("hospital")) return "sci.med.mbbs";
        if (text.Contains("research") || text.Contains("phd") || text.Contains("scientist")) return "sci.res";
        if (text.Contains("government") || text.Contains("upsc") || text.Contains("civil services")) return "gov.civil";
        if (text.Contains("bank") || text.Contains("banking")) return "gov.bank";

        return currentSpec.ToLowerInvariant() switch
        {
            var s when s.Contains("computer") => "sci.eng.cs",
            var s when s.Contains("civil") => "sci.eng.civil",
            var s when s.Contains("finance") => "com.fin",
            var s when s.Contains("psychology") => "art.hum.psych",
            _ => "sci.eng.cs"
        };
    }
}
