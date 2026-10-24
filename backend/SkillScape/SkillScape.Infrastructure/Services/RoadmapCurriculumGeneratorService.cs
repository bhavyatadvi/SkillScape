using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public partial class RoadmapCurriculumGeneratorService : IRoadmapCurriculumGeneratorService
{
    private readonly ApplicationDbContext _context;
    private readonly GroqService _groqService;
    private readonly IRoadmapWebSearchService _webSearchService;
    private readonly ILogger<RoadmapCurriculumGeneratorService> _logger;
    private readonly IConfiguration _configuration;

    private static readonly TimeSpan DefaultWebRefreshInterval = TimeSpan.FromDays(7);

    public RoadmapCurriculumGeneratorService(
        ApplicationDbContext context,
        GroqService groqService,
        IRoadmapWebSearchService webSearchService,
        ILogger<RoadmapCurriculumGeneratorService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _groqService = groqService;
        _webSearchService = webSearchService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<RoadmapBlueprint> GetOrGenerateCurriculumAsync(
        string careerTitle,
        string careerKey,
        string? userLevel = null,
        string? startingPointContext = null,
        bool forceRegenerate = false)
    {
        var normalizedKey = (careerKey ?? careerTitle ?? "general").Trim().ToLowerInvariant()
            .Replace(" ", "-").Replace("/", "-").Replace("_", "-");
        if (normalizedKey.StartsWith("career-"))
        {
            normalizedKey = normalizedKey["career-".Length..];
        }

        var configuredRefreshDays = double.TryParse(_configuration["Roadmap:WebRefreshDays"], out var parsedRefreshDays)
            ? parsedRefreshDays
            : 7;
        var refreshDays = configuredRefreshDays;
        var refreshInterval = TimeSpan.FromDays(Math.Max(1, refreshDays));
        var automaticWebRefreshEnabled = bool.TryParse(_configuration["Roadmap:EnableAutomaticWebRefresh"], out var configuredAutoRefresh)
            && configuredAutoRefresh;
        var isStandardDomain = await _context.CareerDomains
            .AnyAsync(d => d.Id == normalizedKey && d.IsActive);

        if (!forceRegenerate)
        {
            var cached = await _context.RoadmapBlueprints
                .Include(b => b.Phases.OrderBy(p => p.PhaseNumber))
                    .ThenInclude(p => p.Modules.OrderBy(m => m.StepNumber))
                        .ThenInclude(m => m.Topics.OrderBy(t => t.DisplayOrder))
                .Include(b => b.Phases)
                    .ThenInclude(p => p.Modules)
                        .ThenInclude(m => m.Quiz!)
                            .ThenInclude(q => q.Questions)
                .FirstOrDefaultAsync(b => b.CareerKey == normalizedKey);

            if (cached != null && IsUsableBlueprint(cached))
            {
                var hasCurrentStandardTemplate = !isStandardDomain
                    || string.Equals(cached.SourceModel, "fallback/curriculum-engine-v3", StringComparison.Ordinal);

                if (hasCurrentStandardTemplate && !IsCorruptedBlueprint(normalizedKey, careerTitle, cached))
                {
                    // A curriculum is a learner-facing contract: changing its phases or
                    // module IDs during a normal page load breaks progress and makes the
                    // same career appear inconsistent. Keep the stored blueprint as the
                    // canonical version. Web refresh remains available as an explicit,
                    // opt-in setting and explicit regeneration still works below.
                    if (!forceRegenerate && (!automaticWebRefreshEnabled || DateTime.UtcNow - cached.LastWebCheckedAt < refreshInterval))
                    {
                        _logger.LogInformation("Returning canonical cached roadmap blueprint for {CareerKey}", normalizedKey);
                        return cached;
                    }

                    var currentWebKnowledge = await FetchWebKnowledgeAsync(careerTitle);
                    var currentKnowledgeHash = ComputeKnowledgeHash(currentWebKnowledge);

                    if (string.IsNullOrWhiteSpace(currentKnowledgeHash))
                    {
                        cached.LastWebCheckedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        _logger.LogWarning("Web review returned no usable knowledge for {CareerKey}; retaining existing curriculum", normalizedKey);
                        return cached;
                    }

                    if (!forceRegenerate && !string.IsNullOrWhiteSpace(currentKnowledgeHash) &&
                        string.Equals(cached.RawPromptHash, currentKnowledgeHash, StringComparison.Ordinal))
                    {
                        cached.LastWebCheckedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Web review found no curriculum changes for {CareerKey}", normalizedKey);
                        return cached;
                    }

                    _logger.LogInformation("Web review found new knowledge for {CareerKey}; regenerating curriculum", normalizedKey);
                    return await GenerateAndPersistAsync(careerTitle, normalizedKey, userLevel, startingPointContext, currentWebKnowledge, cached);
                }

                _logger.LogWarning("Purging corrupted/mismatched cached roadmap blueprint for {CareerKey}", normalizedKey);
                _context.RoadmapBlueprints.Remove(cached);
                await _context.SaveChangesAsync();
            }
        }

        // Standard and recognized career templates must be fast and responsive for navigation.
        // Generate them locally on first access; explicit refreshes (forceRegenerate)
        // still use the full AI/web curriculum path below.
        if (!forceRegenerate)
        {
            return await GenerateAndPersistAsync(
                careerTitle,
                normalizedKey,
                userLevel,
                startingPointContext,
                string.Empty,
                null,
                deterministicOnly: true);
        }

        var webKnowledge = await FetchWebKnowledgeAsync(careerTitle);
        return await GenerateAndPersistAsync(careerTitle, normalizedKey, userLevel, startingPointContext, webKnowledge, null);
    }

    private async Task<RoadmapBlueprint> GenerateAndPersistAsync(
        string careerTitle,
        string normalizedKey,
        string? userLevel,
        string? startingPointContext,
        string webKnowledge,
        RoadmapBlueprint? previous,
        bool deterministicOnly = false)
    {
        RoadmapBlueprint? generated = null;
        if (!deterministicOnly)
        {
            try
            {
                generated = await GenerateViaAiAsync(careerTitle, normalizedKey, userLevel, startingPointContext, webKnowledge);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI roadmap generation failed for {CareerTitle}. Falling back to rich deterministic curriculum.", careerTitle);
            }
        }

        if (generated == null || !IsUsableBlueprint(generated))
        {
            generated = GenerateDeterministicBlueprint(careerTitle, normalizedKey, startingPointContext);
        }

        EnsureBlueprintQuality(generated, careerTitle, normalizedKey, startingPointContext);
        generated.RawPromptHash = ComputeKnowledgeHash(webKnowledge);
        generated.LastWebCheckedAt = DateTime.UtcNow;
        generated.Version = (previous?.Version ?? 0) + 1;

        // Remove old blueprint for this key if forceRegenerate or replacing cached invalid blueprint
        var existing = await _context.RoadmapBlueprints.FirstOrDefaultAsync(b => b.CareerKey == normalizedKey);
        if (existing != null)
        {
            _context.RoadmapBlueprints.Remove(existing);
            await _context.SaveChangesAsync();
        }

        _context.RoadmapBlueprints.Add(generated);
        await _context.SaveChangesAsync();

        return generated;
    }

    private async Task<string> FetchWebKnowledgeAsync(string careerTitle)
    {
        try
        {
            var knowledge = await _webSearchService.SearchCareerKnowledgeAsync(careerTitle);
            _logger.LogInformation("Fetched adaptive web knowledge for {CareerTitle} ({Length} chars)", careerTitle, knowledge.Length);
            return knowledge;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Adaptive web review failed for {CareerTitle}", careerTitle);
            return string.Empty;
        }
    }

    private static string ComputeKnowledgeHash(string knowledge)
    {
        if (string.IsNullOrWhiteSpace(knowledge)) return string.Empty;
        var normalized = Regex.Replace(knowledge, @"\s+", " ").Trim();
        return Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(normalized)));
    }

    private async Task<RoadmapBlueprint?> GenerateViaAiAsync(
        string careerTitle,
        string careerKey,
        string? userLevel,
        string? startingPointContext,
        string liveWebKnowledge)
    {
        const string systemPrompt = @"You are a principal curriculum architect and career instructor. Create a comprehensive, industry-ready curriculum roadmap for a career path.
Return ONLY valid JSON matching this exact JSON schema:
{
  ""careerTitle"": ""String"",
  ""careerKey"": ""String"",
  ""phases"": [
    {
      ""phaseNumber"": 1,
      ""title"": ""Phase 1: Absolute Foundations"",
      ""level"": ""Beginner"",
      ""summary"": ""Clear overview of this phase goals."",
      ""estimatedDuration"": ""3-4 Weeks"",
      ""modules"": [
        {
          ""stepNumber"": 1,
          ""title"": ""Module Title"",
          ""description"": ""Comprehensive 2-4 sentence explanation of concepts, practical applications, and skills learned in this module."",
          ""estimatedHours"": 15,
          ""topics"": [
            {
              ""title"": ""Subtopic Title"",
              ""resourceUrl"": ""https://www.google.com/search?q=Searchable+Topic"",
              ""displayOrder"": 1,
              ""type"": ""Concept"",
              ""isOptional"": false
            }
          ],
          ""quiz"": {
            ""passScorePercentage"": 70,
            ""questions"": [
              {
                ""questionText"": ""Clear multiple-choice question testing module concepts."",
                ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
                ""correctOptionIndex"": 0,
                ""explanation"": ""Explanation of why Option A is correct."",
                ""difficultyTag"": ""Easy""
              }
            ]
          }
        }
      ]
    }
  ]
}

CRITICAL RULES:
1. Provide 5 to 6 distinct Phases spanning Beginner -> Intermediate -> Advanced -> Job-Ready Pro Level tailored SPECIFICALLY to the target career.
2. Each Phase MUST contain 3 to 4 distinct Modules. Do not merge the entire phase into one module.
3. Each Module MUST contain 5 to 8 subtopics with valid types ('Concept', 'HandsOnLab', 'Project', 'Reading').
4. EVERY single Module MUST contain a quiz object with 4 to 5 multiple-choice questions. Each question MUST have exactly 4 options, a valid correctOptionIndex (0-3), and a clear explanation.
5. If a learner starting point context is given (e.g., '12th fail' or non-tech background), Phase 1 MUST explicitly address foundational literacy, prerequisite bridging, or open-schooling/certificate options.
6. Return ONLY raw JSON without markdown code fences or conversational text.
7. Treat the web knowledge as a research input, not as a generic summary. Extract current tools, frameworks, standards, certifications, workflows, and emerging technologies that are genuinely relevant to this career.
8. Add newly relevant technologies to the correct phase/module, preserve timeless fundamentals, and label fast-changing tools as current-practice topics. Never insert unrelated popular technologies just because they appear in the web text.";

        var prompt = new StringBuilder();
        prompt.AppendLine($"Target Career Role: {careerTitle}");
        prompt.AppendLine($"Career Key: {careerKey}");
        if (!string.IsNullOrWhiteSpace(userLevel)) prompt.AppendLine($"User Experience/Education Level: {userLevel}");
        if (!string.IsNullOrWhiteSpace(startingPointContext)) prompt.AppendLine($"Starting Point / Background Context: {startingPointContext}");

        if (!string.IsNullOrWhiteSpace(liveWebKnowledge))
        {
            prompt.AppendLine();
            prompt.AppendLine("--- LIVE REAL-TIME WEB SEARCH & SYLLABUS INSIGHTS ---");
            prompt.AppendLine(liveWebKnowledge);
            prompt.AppendLine("--- END LIVE WEB KNOWLEDGE INSIGHTS ---");
            prompt.AppendLine();
        }

        var rawResponse = await _groqService.GetSkillAnalyzerResponseAsync(systemPrompt, prompt.ToString());
        var cleanJson = CleanJson(rawResponse);

        using var doc = JsonDocument.Parse(cleanJson);
        var root = doc.RootElement;

        var blueprint = new RoadmapBlueprint
        {
            CareerKey = careerKey,
            CareerTitle = GetStringProp(root, "careerTitle", careerTitle),
            GeneratedBy = "ai-websearch",
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            SourceModel = "groq/llama-3.3-70b"
        };

        if (root.TryGetProperty("phases", out var phasesArray) && phasesArray.ValueKind == JsonValueKind.Array)
        {
            int pNum = 1;
            foreach (var phaseElem in phasesArray.EnumerateArray())
            {
                var phase = new RoadmapPhase
                {
                    BlueprintId = blueprint.Id,
                    PhaseNumber = phaseElem.TryGetProperty("phaseNumber", out var pn) ? pn.GetInt32() : pNum,
                    Title = GetStringProp(phaseElem, "title", $"Phase {pNum}: Core Skill Building"),
                    Level = GetStringProp(phaseElem, "level", pNum <= 2 ? "Beginner" : pNum <= 4 ? "Intermediate" : "Advanced"),
                    Summary = GetStringProp(phaseElem, "summary", "Master fundamental competencies for this level."),
                    EstimatedDuration = GetStringProp(phaseElem, "estimatedDuration", "3-4 Weeks")
                };

                if (phaseElem.TryGetProperty("modules", out var modulesArray) && modulesArray.ValueKind == JsonValueKind.Array)
                {
                    int mNum = 1;
                    foreach (var modElem in modulesArray.EnumerateArray())
                    {
                        var module = new RoadmapModule
                        {
                            PhaseId = phase.Id,
                            StepNumber = modElem.TryGetProperty("stepNumber", out var sn) ? sn.GetInt32() : mNum,
                            Title = GetStringProp(modElem, "title", $"Module {mNum}"),
                            Description = GetStringProp(modElem, "description", "Study concepts, execute practical tasks, and pass the end-of-module assessment."),
                            EstimatedHours = modElem.TryGetProperty("estimatedHours", out var eh) ? eh.GetInt32() : 12,
                            IsActive = true
                        };

                        // Topics
                        if (modElem.TryGetProperty("topics", out var topicsArray) && topicsArray.ValueKind == JsonValueKind.Array)
                        {
                            int tOrder = 1;
                            foreach (var topicElem in topicsArray.EnumerateArray())
                            {
                                var topicTitle = GetStringProp(topicElem, "title", $"Topic {tOrder}");
                                string typeStr = GetStringProp(topicElem, "type", "Concept");
                                Enum.TryParse<RoadmapTopicType>(typeStr, true, out var topicType);

                                module.Topics.Add(new RoadmapTopic
                                {
                                    RoadmapModuleId = module.Id,
                                    Title = topicTitle,
                                    ResourceUrl = GetStringProp(topicElem, "resourceUrl", $"https://www.google.com/search?q={Uri.EscapeDataString(topicTitle + " " + careerTitle)}"),
                                    DisplayOrder = topicElem.TryGetProperty("displayOrder", out var doElem) ? doElem.GetInt32() : tOrder,
                                    Type = topicType,
                                    IsOptional = topicElem.TryGetProperty("isOptional", out var ioElem) && ioElem.GetBoolean()
                                });
                                tOrder++;
                            }
                        }

                        // Quiz
                        if (modElem.TryGetProperty("quiz", out var quizElem))
                        {
                            var quiz = new RoadmapModuleQuiz
                            {
                                ModuleId = module.Id,
                                PassScorePercentage = quizElem.TryGetProperty("passScorePercentage", out var ps) ? ps.GetInt32() : 70
                            };

                            if (quizElem.TryGetProperty("questions", out var questionsArray) && questionsArray.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var qElem in questionsArray.EnumerateArray())
                                {
                                    var options = new List<string>();
                                    if (qElem.TryGetProperty("options", out var optsElem) && optsElem.ValueKind == JsonValueKind.Array)
                                    {
                                        options = optsElem.EnumerateArray().Select(o => o.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList();
                                    }

                                    if (options.Count < 4)
                                    {
                                        while (options.Count < 4) options.Add($"Option {options.Count + 1}");
                                    }

                                    quiz.Questions.Add(new RoadmapQuizQuestion
                                    {
                                        QuizId = quiz.Id,
                                        QuestionText = GetStringProp(qElem, "questionText", "Which choice best applies to this module's learning objective?"),
                                        OptionsJson = JsonSerializer.Serialize(options),
                                        CorrectOptionIndex = qElem.TryGetProperty("correctOptionIndex", out var coi) ? coi.GetInt32() : 0,
                                        Explanation = GetStringProp(qElem, "explanation", "Review the module content for key concepts and practices."),
                                        DifficultyTag = GetStringProp(qElem, "difficultyTag", "Medium")
                                    });
                                }
                            }

                            if (quiz.Questions.Count == 0)
                            {
                                AddDefaultQuizQuestions(quiz, module.Title, careerTitle);
                            }

                            module.Quiz = quiz;
                        }

                        if (module.Quiz == null)
                        {
                            var quiz = new RoadmapModuleQuiz { ModuleId = module.Id, PassScorePercentage = 70 };
                            AddDefaultQuizQuestions(quiz, module.Title, careerTitle);
                            module.Quiz = quiz;
                        }

                        phase.Modules.Add(module);
                        mNum++;
                    }
                }

                blueprint.Phases.Add(phase);
                pNum++;
            }
        }

        return blueprint;
    }

    public RoadmapBlueprint GenerateDeterministicBlueprint(string careerTitle, string careerKey, string? startingPointContext = null)
    {
        var category = InferCategory(careerTitle, careerKey, startingPointContext);
        var blueprint = new RoadmapBlueprint
        {
            CareerKey = careerKey,
            CareerTitle = careerTitle,
            GeneratedBy = "deterministic",
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            SourceModel = "fallback/curriculum-engine-v3"
        };

        var phaseTemplates = GetCategoryPhaseTemplates(category, careerTitle, startingPointContext);

        int pNum = 1;
        foreach (var pTemp in phaseTemplates)
        {
            var phase = new RoadmapPhase
            {
                BlueprintId = blueprint.Id,
                PhaseNumber = pNum,
                Title = pTemp.Title,
                Level = pTemp.Level,
                Summary = pTemp.Summary,
                EstimatedDuration = pTemp.Duration
            };

            int mNum = 1;
            foreach (var mTemp in pTemp.Modules)
            {
                var module = new RoadmapModule
                {
                    PhaseId = phase.Id,
                    StepNumber = mNum,
                    Title = mTemp.Title,
                    Description = mTemp.Description,
                    EstimatedHours = mTemp.EstimatedHours,
                    IsActive = true
                };

                int tOrder = 1;
                foreach (var tTemp in mTemp.Topics)
                {
                    module.Topics.Add(new RoadmapTopic
                    {
                        RoadmapModuleId = module.Id,
                        Title = tTemp.Title,
                        ResourceUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(tTemp.Title + " tutorial " + careerTitle)}",
                        DisplayOrder = tOrder,
                        Type = tTemp.Type,
                        IsOptional = tTemp.IsOptional
                    });
                    tOrder++;
                }

                var quiz = new RoadmapModuleQuiz
                {
                    ModuleId = module.Id,
                    PassScorePercentage = 70
                };

                foreach (var qTemp in mTemp.Questions)
                {
                    quiz.Questions.Add(new RoadmapQuizQuestion
                    {
                        QuizId = quiz.Id,
                        QuestionText = qTemp.QuestionText,
                        OptionsJson = JsonSerializer.Serialize(qTemp.Options),
                        CorrectOptionIndex = qTemp.CorrectIndex,
                        Explanation = qTemp.Explanation,
                        DifficultyTag = qTemp.Difficulty
                    });
                }

                while (quiz.Questions.Count < 4)
                {
                    AddDefaultQuizQuestions(quiz, module.Title, careerTitle);
                }

                module.Quiz = quiz;
                phase.Modules.Add(module);
                mNum++;
            }

            blueprint.Phases.Add(phase);
            pNum++;
        }

        return blueprint;
    }

    public static bool IsCorruptedBlueprint(string normalizedKey, string careerTitle, RoadmapBlueprint blueprint)
    {
        var category = InferCategory(careerTitle, normalizedKey, null);
        var combined = (normalizedKey + " " + careerTitle).ToLowerInvariant();
        bool isNonTechDomain = combined.Contains("pcb") || combined.Contains("biology") || combined.Contains("medical") || combined.Contains("physics") || combined.Contains("chemistry") || combined.Contains("pharmacy") || combined.Contains("nursing") || combined.Contains("commerce") || combined.Contains("accounting") || combined.Contains("arts") || combined.Contains("law") || combined.Contains("history") || combined.Contains("civil") || combined.Contains("upsc") || combined.Contains("mechanical") || combined.Contains("pilot") || combined.Contains("architecture") || combined.Contains("marketing") || combined.Contains("humanities") || combined.Contains("government") || combined.Contains("electrical") || combined.Contains("electronics") || combined.Contains("teaching") || combined.Contains("education") || combined.Contains("doctor") || combined.Contains("surgeon") || combined.Contains("clinical");

        if (isNonTechDomain && blueprint.Phases.Count > 0)
        {
            var p1 = blueprint.Phases.FirstOrDefault();
            var m1 = p1?.Modules.FirstOrDefault();
            string p1Title = (p1?.Title ?? "").ToLowerInvariant();
            string m1Title = (m1?.Title ?? "").ToLowerInvariant();

            if (p1Title.Contains("computer science") || p1Title.Contains("programming fundamentals") || m1Title.Contains("programming foundations") || m1Title.Contains("git version") || m1Title.Contains("data structures & algorithm"))
            {
                return true;
            }
        }

        // Structural validity is not enough: an AI response can contain the
        // right number of phases while belonging to the wrong profession.
        // Reject obvious cross-domain contamination before it reaches learners.
        var curriculumText = string.Join(" ", blueprint.Phases.SelectMany(p =>
            new[] { p.Title, p.Summary }.Concat(p.Modules.SelectMany(m =>
                new[] { m.Title, m.Description }.Concat(m.Topics.Select(t => t.Title)))))).ToLowerInvariant();

        var forbiddenTerms = category switch
        {
            "SWE" or "Frontend" or "Backend" or "FullStack" or "DataML" or "DevOpsCloud" or "Product" or "Design" or "Marketing" =>
                new[] { "constitutional law", "criminal law", "legal drafting", "double-entry bookkeeping", "financial accounting", "gst taxation", "bank reconciliation", "organic chemistry", "human physiology", "structural mechanics", "concrete technology", "surveying instruments" },
            "CivilEngineering" =>
                new[] { "react components", "javascript dom", "kubernetes cluster", "neural network", "wireframing", "css flexbox", "html semantics", "docker containerization", "constitutional law", "double-entry bookkeeping" },
            "Cybersecurity" =>
                new[] { "civil engineering", "structural mechanics", "concrete technology", "double-entry bookkeeping", "gst taxation", "organic chemistry", "human physiology" },
            "ElectricalElectronics" =>
                new[] { "react components", "javascript dom", "double-entry bookkeeping", "constitutional law", "civil engineering", "organic chemistry" },
            "ChemicalEngineering" =>
                new[] { "react components", "javascript dom", "kubernetes cluster", "constitutional law", "double-entry bookkeeping" },
            "MedicineClinical" or "SciencePCB" or "BiotechPharma" =>
                new[] { "react components", "javascript dom", "double-entry bookkeeping", "constitutional law", "kubernetes cluster", "git version control" },
            "TeachingEducation" =>
                new[] { "react components", "javascript dom", "kubernetes cluster", "double-entry bookkeeping" },
            "ArchitecturePlanning" =>
                new[] { "react components", "javascript dom", "kubernetes cluster", "double-entry bookkeeping", "constitutional law" },
            "CommerceFinance" =>
                new[] { "react components", "javascript dom", "kubernetes cluster", "neural network", "wireframing", "css flexbox", "html semantics", "docker containerization" },
            "LawLegal" =>
                new[] { "react components", "javascript dom", "double-entry bookkeeping", "financial accounting", "pytorch", "kubernetes cluster", "css flexbox" },
            _ => Array.Empty<string>()
        };

        if (forbiddenTerms.Any(curriculumText.Contains)) return true;

        var requiredTerms = category switch
        {
            "Frontend" => new[] { "semantic html", "css layout", "react components" },
            "Backend" => new[] { "http", "database", "api" },
            "FullStack" => new[] { "frontend foundations", "backend foundations", "full-stack" },
            "DataML" => new[] { "python", "data", "machine learning" },
            "DevOpsCloud" => new[] { "linux", "ci/cd", "cloud" },
            "CivilEngineering" => new[] { "mechanics", "structural", "construction" },
            "Cybersecurity" => new[] { "network", "security", "threat" },
            _ => Array.Empty<string>()
        };

        return requiredTerms.Length > 0 && requiredTerms.Any(term => !curriculumText.Contains(term));
    }

    private static bool IsUsableBlueprint(RoadmapBlueprint blueprint)
    {
        if (blueprint.Phases.Count < 4) return false;

        try
        {
            return blueprint.Phases.All(phase =>
                phase.Modules.Count >= 3 && phase.Modules.Count <= 4 && phase.Modules.All(module =>
                    module.Topics.Count >= 5 && module.Topics.Count <= 8 &&
                    module.Quiz != null &&
                    module.Quiz.Questions.Count >= 4 && module.Quiz.Questions.Count <= 5 &&
                    module.Quiz.Questions.All(question =>
                        JsonSerializer.Deserialize<List<string>>(question.OptionsJson)?.Count == 4 &&
                        question.CorrectOptionIndex >= 0 && question.CorrectOptionIndex <= 3)));
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static void EnsureBlueprintQuality(
        RoadmapBlueprint blueprint,
        string careerTitle,
        string careerKey,
        string? startingPointContext)
    {
        var category = InferCategory(careerTitle, careerKey, startingPointContext);

        // Four phases is a valid compact curriculum. Do not invent a fifth
        // phase merely to satisfy an arbitrary count; add phases only when an
        // AI blueprint explicitly contains them or a domain template requires
        // additional depth.
        while (blueprint.Phases.Count < 4)
        {
            var phaseNumber = blueprint.Phases.Count + 1;
            var phase = new RoadmapPhase
            {
                BlueprintId = blueprint.Id,
                PhaseNumber = phaseNumber,
                Title = GetSupplementalPhaseTitle(category, phaseNumber, careerTitle),
                Level = "ProLevel",
                Summary = GetSupplementalPhaseSummary(category, careerTitle),
                EstimatedDuration = "4-6 Weeks"
            };
            phase.Modules.Add(CreateSupplementalModule(phase, careerTitle, category, 1));
            phase.Modules.Add(CreateSupplementalModule(phase, careerTitle, category, 2));
            phase.Modules.Add(CreateSupplementalModule(phase, careerTitle, category, 3));
            blueprint.Phases.Add(phase);
        }

        foreach (var phase in blueprint.Phases)
        {
            while (phase.Modules.Count < 3)
            {
                phase.Modules.Add(CreateSupplementalModule(phase, careerTitle, category, phase.Modules.Count + 1));
            }

            foreach (var module in phase.Modules)
            {
                while (module.Topics.Count < 5)
                {
                    var topicNumber = module.Topics.Count + 1;
                    module.Topics.Add(new RoadmapTopic
                    {
                        RoadmapModuleId = module.Id,
                        Title = $"Guided practice {topicNumber}: Apply {module.Title} to a real {careerTitle} scenario",
                        ResourceUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(module.Title + " " + careerTitle + " practice")}",
                        DisplayOrder = topicNumber,
                        Type = topicNumber == 3 ? RoadmapTopicType.Project : RoadmapTopicType.HandsOnLab,
                        IsOptional = false
                    });
                }

                if (module.Quiz == null)
                {
                    module.Quiz = new RoadmapModuleQuiz { ModuleId = module.Id, PassScorePercentage = 70 };
                }

                while (module.Quiz.Questions.Count < 4)
                {
                    AddDefaultQuizQuestions(module.Quiz, module.Title, careerTitle);
                }

                if (module.Quiz.Questions.Count > 5)
                {
                    module.Quiz.Questions = module.Quiz.Questions.Take(5).ToList();
                }
            }
        }
    }

    private static string GetSupplementalPhaseTitle(string category, int phaseNumber, string careerTitle) => category switch
    {
        "SWE" => $"Phase {phaseNumber}: Production Engineering & Portfolio Delivery",
        "DataML" => $"Phase {phaseNumber}: Applied Data Products & Model Delivery",
        "DevOpsCloud" => $"Phase {phaseNumber}: Reliable Cloud Operations & Automation",
        "CivilEngineering" => $"Phase {phaseNumber}: Advanced Structural Projects & Site Execution",
        "Cybersecurity" => $"Phase {phaseNumber}: Enterprise Security Architecture & Incident Handling",
        "ElectricalElectronics" => $"Phase {phaseNumber}: Advanced Embedded & Hardware System Prototyping",
        "ChemicalEngineering" => $"Phase {phaseNumber}: Plant Design, Scale-up & Process Safety",
        "MedicineClinical" => $"Phase {phaseNumber}: Advanced Clinical Practice & Patient Management",
        "TeachingEducation" => $"Phase {phaseNumber}: Educational Leadership & Classroom Research",
        "ArchitecturePlanning" => $"Phase {phaseNumber}: Sustainable Architectural Design & Urban Studio",
        "CommerceFinance" => $"Phase {phaseNumber}: Financial Practice & Professional Readiness",
        "LawLegal" => $"Phase {phaseNumber}: Legal Practice & Professional Readiness",
        "SciencePCB" or "BiotechPharma" => $"Phase {phaseNumber}: Applied {careerTitle} Practice & Readiness",
        _ => $"Phase {phaseNumber}: Applied {careerTitle} Practice & Readiness"
    };

    private static string GetSupplementalPhaseSummary(string category, string careerTitle) => category switch
    {
        "SWE" => $"Apply {careerTitle} engineering skills through testing, deployment, documentation, collaboration, and a portfolio-ready product.",
        "DataML" => $"Apply {careerTitle} skills to reproducible analysis, responsible evaluation, deployment, and communication of evidence.",
        "DevOpsCloud" => $"Apply {careerTitle} practices to automation, observability, security, incident response, and reliable delivery.",
        "CivilEngineering" => $"Apply {careerTitle} engineering competencies to structural modeling, site management, safety compliance, and comprehensive civil infrastructure projects.",
        "Cybersecurity" => $"Apply {careerTitle} operations to threat defense, vulnerability assessments, security architecture, and incident response.",
        "ElectricalElectronics" => $"Apply {careerTitle} knowledge to embedded systems, circuit prototyping, power analysis, and hardware verification.",
        "ChemicalEngineering" => $"Apply {careerTitle} principles to unit operations, reaction kinetics, safety protocols, and industrial simulation.",
        "MedicineClinical" => $"Apply {careerTitle} clinical skills to patient evaluations, diagnosis workflows, clinical ethics, and healthcare protocols.",
        "TeachingEducation" => $"Apply {careerTitle} pedagogy to curriculum design, interactive classroom instruction, student assessment, and research.",
        "ArchitecturePlanning" => $"Apply {careerTitle} design principles to spatial planning, construction detailing, environmental sustainability, and portfolio review.",
        "CommerceFinance" => $"Apply {careerTitle} knowledge to realistic statements, controls, analysis, reporting, and professional decisions.",
        "LawLegal" => $"Apply {careerTitle} knowledge to legal research, drafting, analysis, ethics, client communication, and supervised practice.",
        _ => $"Apply {careerTitle} knowledge through domain-specific practice, review, communication, and a demonstrable final deliverable."
    };

    private static RoadmapModule CreateSupplementalModule(RoadmapPhase phase, string careerTitle, string category, int stepNumber)
    {
        var module = new RoadmapModule
        {
            PhaseId = phase.Id,
            StepNumber = stepNumber,
            Title = GetSupplementalModuleTitle(category, careerTitle, phase.Level, stepNumber),
            Description = GetSupplementalModuleDescription(category, careerTitle, phase.Level),
            EstimatedHours = 20,
            IsActive = true
        };

        var topicTitles = GetSupplementalTopics(category, careerTitle);

        for (var index = 0; index < topicTitles.Length; index++)
        {
            module.Topics.Add(new RoadmapTopic
            {
                RoadmapModuleId = module.Id,
                Title = topicTitles[index],
                ResourceUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(topicTitles[index])}",
                DisplayOrder = index + 1,
                Type = index == 2 || index == 4 ? RoadmapTopicType.Project : index == 1 || index == 3 ? RoadmapTopicType.HandsOnLab : RoadmapTopicType.Concept,
                IsOptional = false
            });
        }

        module.Quiz = new RoadmapModuleQuiz { ModuleId = module.Id, PassScorePercentage = 70 };
        AddDefaultQuizQuestions(module.Quiz, module.Title, careerTitle);
        AddDefaultQuizQuestions(module.Quiz, $"advanced application of {module.Title}", careerTitle);
        return module;
    }

    private static string GetSupplementalModuleTitle(string category, string careerTitle, string level, int stepNumber) => category switch
    {
        "SWE" => stepNumber switch
        {
            1 => "Production Project Architecture & Code Quality",
            2 => "Testing, Security & Observability",
            _ => "Deployment, Documentation & Portfolio Review"
        },
        "DataML" => stepNumber switch
        {
            1 => "Reproducible Data Project Delivery",
            2 => "Model Evaluation, Ethics & Communication",
            _ => "Deployment, Monitoring & Portfolio Review"
        },
        "DevOpsCloud" => stepNumber switch
        {
            1 => "Infrastructure Automation & Reliability",
            2 => "Security, Observability & Incident Response",
            _ => "Cloud Delivery, Cost & Operations Review"
        },
        "CivilEngineering" => stepNumber switch
        {
            1 => "Structural Design, Analysis & Code Verification",
            2 => "Construction Project Scheduling & Cost Management",
            _ => "Site Safety, Quality Assurance & Capstone Infrastructure"
        },
        "Cybersecurity" => stepNumber switch
        {
            1 => "Vulnerability Assessment & Threat Modeling",
            2 => "Incident Response, Containment & Forensic Analysis",
            _ => "Cloud Security Architecture & Professional Readiness"
        },
        "ElectricalElectronics" => stepNumber switch
        {
            1 => "Embedded Firmware & Real-Time Prototyping",
            2 => "Power Management, Control & Hardware Debugging",
            _ => "Hardware Product Verification & Capstone Delivery"
        },
        "ChemicalEngineering" => stepNumber switch
        {
            1 => "Unit Operations Design & Equipment Sizing",
            2 => "Process Simulation, Dynamics & Safety Review",
            _ => "Plant Design, Economics & Sustainable Practice"
        },
        "MedicineClinical" => stepNumber switch
        {
            1 => "Diagnostic Evaluation & Case Studies",
            2 => "Pharmacology, Treatment Plans & Patient Care",
            _ => "Clinical Protocols, Medical Ethics & Internship Readiness"
        },
        "TeachingEducation" => stepNumber switch
        {
            1 => "Pedagogical Design & Innovative Lesson Planning",
            2 => "Educational Assessment, Rubrics & EdTech",
            _ => "Classroom Leadership, Action Research & Teaching Portfolio"
        },
        "ArchitecturePlanning" => stepNumber switch
        {
            1 => "BIM Modeling, Detailing & Working Drawings",
            2 => "Sustainable Building Performance & Code Compliance",
            _ => "Comprehensive Architectural Capstone Design Studio"
        },
        "CommerceFinance" => stepNumber switch
        {
            1 => "Financial Reporting & Control Practice",
            2 => "Analysis, Valuation & Decision Support",
            _ => "Professional Reporting & Case Portfolio"
        },
        "LawLegal" => stepNumber switch
        {
            1 => "Legal Research & Authority Analysis",
            2 => "Drafting, Advocacy & Client Communication",
            _ => "Ethics, Practice Management & Case Portfolio"
        },
        _ => $"Applied {careerTitle} Practice {stepNumber}"
    };

    private static string GetSupplementalModuleDescription(string category, string careerTitle, string level) => category switch
    {
        "SWE" => $"Build {careerTitle} capability at the {level} level through implementation, testing, review, deployment, and a portfolio-ready product.",
        "DataML" => $"Build {careerTitle} capability at the {level} level through reproducible evidence, evaluation, communication, and responsible delivery.",
        "DevOpsCloud" => $"Build {careerTitle} capability at the {level} level through automation, observability, security, and reliable operations.",
        "CivilEngineering" => $"Build {careerTitle} capability at the {level} level through structural calculations, drafting, project management, and site supervision.",
        "Cybersecurity" => $"Build {careerTitle} capability at the {level} level through threat analysis, defensive hardening, security testing, and incident response.",
        "ElectricalElectronics" => $"Build {careerTitle} capability at the {level} level through schematic design, embedded programming, testing, and hardware prototyping.",
        "ChemicalEngineering" => $"Build {careerTitle} capability at the {level} level through mass and energy balances, reactor engineering, and process optimization.",
        "MedicineClinical" => $"Build {careerTitle} capability at the {level} level through systematic diagnostic skills, clinical judgment, and patient communication.",
        "TeachingEducation" => $"Build {careerTitle} capability at the {level} level through evidence-based teaching strategies, assessment design, and student engagement.",
        "ArchitecturePlanning" => $"Build {careerTitle} capability at the {level} level through architectural visualization, spatial planning, and structural integration.",
        "CommerceFinance" => $"Build {careerTitle} capability at the {level} level through financial analysis, controls, reporting, and practical cases.",
        "LawLegal" => $"Build {careerTitle} capability at the {level} level through research, drafting, analysis, ethics, and supervised case work.",
        _ => $"Build practical {careerTitle} capability at the {level} level through domain-specific exercises, review, documentation, and a final deliverable."
    };

    private static string[] GetSupplementalTopics(string category, string careerTitle) => category switch
    {
        "SWE" => new[] { "Requirements, architecture, and trade-off decisions", "Code quality, version control, and peer review", "Automated testing and security checks", "Deployment, monitoring, and troubleshooting", $"Documenting and presenting the {careerTitle} product" },
        "DataML" => new[] { "Problem framing, data provenance, and reproducibility", "Data quality, evaluation design, and bias checks", "Model or analysis communication for stakeholders", "Deployment, monitoring, and maintenance", $"Documenting and presenting the {careerTitle} project" },
        "DevOpsCloud" => new[] { "Infrastructure as code and environment management", "CI/CD pipelines and release controls", "Observability, SLOs, and incident response", "Cloud security, cost, and resilience", $"Documenting and presenting the {careerTitle} system" },
        "CivilEngineering" => new[] { "Site surveying, soil report interpretation, and structural codes", "CAD drafting, load calculations, and material quality assurance", "Safety compliance, project scheduling, and contractor coordination", "Structural inspection, defect diagnosis, and environmental compliance", $"Documenting and presenting the {careerTitle} engineering portfolio" },
        "Cybersecurity" => new[] { "Network packet capture, firewall rules, and threat modeling", "Vulnerability scanning, remediation planning, and patch management", "Security policy formulation, access control, and incident triage", "Log correlation, forensic evidence collection, and compliance review", $"Documenting and presenting the {careerTitle} security assessment" },
        "ElectricalElectronics" => new[] { "Schematic capture, circuit simulation, and PCB layout", "Microcontroller programming and peripheral interfacing", "Power electronics, battery management, and thermal dissipation", "Signal integrity, oscilloscope measurements, and hardware testing", $"Documenting and presenting the {careerTitle} prototype" },
        "ChemicalEngineering" => new[] { "Mass and energy balances across process units", "Heat exchanger and distillation column sizing", "Reactor design, catalysis, and reaction kinetics", "Process safety management, HAZOP, and environmental mitigation", $"Documenting and presenting the {careerTitle} process design" },
        "MedicineClinical" => new[] { "Clinical history taking and physical examination techniques", "Diagnostic investigations, lab values, and differential diagnosis", "Pharmacotherapeutic strategies, drug interactions, and dosage", "Patient safety, ethical consent, and interprofessional communication", $"Presenting a {careerTitle} clinical case study" },
        "TeachingEducation" => new[] { "Formulating learning objectives and alignment with standards", "Developing differentiated lesson plans and student activities", "Classroom management, student engagement, and behavior support", "Formative assessment design, grading rubrics, and feedback", $"Documenting and demonstrating the {careerTitle} teaching portfolio" },
        "ArchitecturePlanning" => new[] { "Conceptual sketching, spatial zoning, and massing studies", "BIM modeling, technical detailing, and schedule generation", "Climate analysis, daylighting, and passive energy design", "Zoning ordinances, building codes, and accessibility compliance", $"Documenting and presenting the {careerTitle} architectural portfolio" },
        "CommerceFinance" => new[] { "Financial statement interpretation and controls", "Ratio analysis, forecasting, and valuation", "Risk, compliance, and professional judgment", "Client-ready reporting and decision memos", $"Presenting a {careerTitle} case portfolio" },
        "LawLegal" => new[] { "Primary authorities, precedent, and legal research", "Issue spotting, argument structure, and citation", "Drafting pleadings, contracts, or legal memoranda", "Ethics, confidentiality, and client communication", $"Presenting a {careerTitle} case portfolio" },
        _ => new[] { $"{careerTitle} workflow analysis and requirements", $"Tools and environment setup for {careerTitle}", $"Hands-on implementation of a {careerTitle} task", "Quality review and troubleshooting", $"Documenting and presenting the {careerTitle} deliverable" }
    };

    public static string InferCategory(string title, string key, string? startingPoint = null)
    {
        var normalizedKey = (key ?? string.Empty).Trim().ToLowerInvariant().Replace("_", "-");
        if (normalizedKey.StartsWith("career-"))
        {
            normalizedKey = normalizedKey["career-".Length..];
        }
        var standardCategory = normalizedKey switch
        {
            "frontend" or "frontend-dev" or "frontend-developer" => "Frontend",
            "backend" or "backend-dev" or "backend-developer" => "Backend",
            "fullstack" or "full-stack" or "fullstack-dev" or "full-stack-developer" => "FullStack",
            "data" or "data-science" or "data-scientist" or "data-analyst" => "DataML",
            "ml" or "machine-learning" or "ai-engineer" => "DataML",
            "cybersecurity" or "cyber-security" or "infosec" or "cybersecurity-analyst" => "Cybersecurity",
            "financial-analyst" or "accountant" or "chartered-accountant" or "tax-consultant" => "CommerceFinance",
            "business-analyst" or "product-manager" or "project-manager" or "operations-manager" => "Product",
            "marketing-manager" or "digital-marketer" => "Marketing",
            "civil-engineering" or "civil-engineer" or "structural-engineering" => "CivilEngineering",
            "electrical-engineering" or "electrical-engineer" or "electronics" => "ElectricalElectronics",
            "chemical-engineering" or "chemical-engineer" => "ChemicalEngineering",
            "devops" or "devops-cloud" or "cloud-architect" => "DevOpsCloud",
            "business" or "management" => "Product",
            "design" or "creative-design" or "uiux-designer" or "ui-ux-designer" => "Design",
            "healthcare" or "medicine" => "MedicineClinical",
            "teaching" or "education" => "TeachingEducation",
            "architecture" => "ArchitecturePlanning",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(standardCategory)) return standardCategory;

        var combined = (title + " " + key + " " + (startingPoint ?? "")).ToLowerInvariant();

        // 1. Civil Engineering - MUST precede GovCivil so "civil" doesn't trigger UPSC
        if (combined.Contains("civil engineer") || combined.Contains("civil engineering") || 
            combined.Contains("structural engineer") || combined.Contains("structural engineering") ||
            combined.Contains("geotechnical") || combined.Contains("transportation engineering") ||
            combined.Contains("construction engineer") || combined.Contains("construction management") ||
            combined.Contains("surveying") || combined.Contains("building technology") ||
            combined.Contains("sci.eng.civil"))
            return "CivilEngineering";

        // 2. Cybersecurity - MUST precede DevOpsCloud so "cyber/security" doesn't get routed to Linux/Docker
        if (combined.Contains("cyber") || combined.Contains("infosec") || 
            combined.Contains("security analyst") || combined.Contains("penetration test") || 
            combined.Contains("ethical hack") || combined.Contains("soc analyst") || 
            combined.Contains("cryptograph") || combined.Contains("network security") ||
            combined.Contains("incident responder") || combined.Contains("malware") ||
            combined.Contains("vulnerability analyst") || combined.Contains("cs.sec"))
            return "Cybersecurity";

        // 3. Electrical & Electronics Engineering
        if (combined.Contains("electrical") || combined.Contains("electronics") || 
            combined.Contains("embedded systems") || combined.Contains("vlsi") || 
            combined.Contains("circuit design") || combined.Contains("power systems") || 
            combined.Contains("robotics hardware") || combined.Contains("microcontroller") || 
            combined.Contains("iot") || combined.Contains("telecommunication") || 
            combined.Contains("signal processing") || combined.Contains("sci.eng.elec"))
            return "ElectricalElectronics";

        // 4. Chemical Engineering
        if (combined.Contains("chemical engineer") || combined.Contains("chemical engineering") || 
            combined.Contains("process engineer") || combined.Contains("petroleum") || 
            combined.Contains("biochemical engineering") || combined.Contains("refinery process"))
            return "ChemicalEngineering";

        // 5. Architecture & Spatial Planning (excluding software/cloud/solution architects)
        bool isSoftwareArchitect = combined.Contains("software architect") || combined.Contains("cloud architect") || 
                                   combined.Contains("solution architect") || combined.Contains("systems architect") ||
                                   combined.Contains("enterprise architect") || combined.Contains("secarch");
        if (!isSoftwareArchitect && (combined.Contains("architect") || combined.Contains("architecture") || 
            combined.Contains("urban planner") || combined.Contains("landscape architect") || 
            combined.Contains("interior designer") || combined.Contains("building design")))
            return "ArchitecturePlanning";

        // 6. Teaching & Education
        if (combined.Contains("teacher") || combined.Contains("teaching") || combined.Contains("educator") || 
            combined.Contains("professor") || combined.Contains("lecturer") || combined.Contains("pedagogy") || 
            combined.Contains("curriculum developer") || combined.Contains("instructional design") || 
            combined.Contains("edu.") || combined.Contains("school teacher"))
            return "TeachingEducation";

        // 7. Clinical Medicine & Healthcare
        if (combined.Contains("doctor") || combined.Contains("physician") || combined.Contains("surgeon") || 
            combined.Contains("mbbs") || combined.Contains("bds") || combined.Contains("dentist") || 
            combined.Contains("nurse") || combined.Contains("nursing") || combined.Contains("clinic") || 
            combined.Contains("medical") || combined.Contains("pharmacist") || combined.Contains("pharmacy") || 
            combined.Contains("pediatrician") || combined.Contains("cardiologist") || combined.Contains("neurologist") ||
            combined.Contains("radiologist") || combined.Contains("pathologist") || combined.Contains("sci.med"))
            return "MedicineClinical";

        // 8. General PCB (Biology / Pure Life Sciences)
        if (combined.Contains("pcb") || combined.Contains("biology") || combined.Contains("botany") || 
            combined.Contains("zoology") || combined.Contains("microbiology") || combined.Contains("biomedical") || 
            combined.Contains("bams") || combined.Contains("bhms") || combined.Contains("allied health"))
            return "SciencePCB";

        // 9. Biotech & Pharma
        if (combined.Contains("biotech") || combined.Contains("biotechnology") || combined.Contains("pharma") || 
            combined.Contains("pharmaceutical") || combined.Contains("drug discovery"))
            return "BiotechPharma";

        // 10. Science PCM (Pure Sciences)
        if (combined.Contains("pcm") || combined.Contains("physics") || combined.Contains("chemistry") || 
            combined.Contains("mathematics") || combined.Contains("applied physics") || combined.Contains("maths") || 
            combined.Contains("astronomy") || combined.Contains("astrophysics"))
            return "SciencePCM";

        // 11. Commerce & Finance
        if (combined.Contains("commerce") || combined.Contains("accounting") || combined.Contains("ca") || 
            combined.Contains("chartered accountant") || combined.Contains("finance") || combined.Contains("cfa") || 
            combined.Contains("banking") || combined.Contains("taxation") || combined.Contains("audit") || 
            combined.Contains("tally") || combined.Contains("com.fin"))
            return "CommerceFinance";

        // 12. Law & Legal
        if (combined.Contains("law") || combined.Contains("legal") || combined.Contains("llb") || 
            combined.Contains("advocate") || combined.Contains("judiciary") || combined.Contains("jurisprudence") || 
            combined.Contains("clat") || combined.Contains("court") || combined.Contains("attorney"))
            return "LawLegal";

        // 13. Arts & Humanities
        if (combined.Contains("arts") || combined.Contains("humanities") || combined.Contains("sociology") || 
            combined.Contains("political science") || combined.Contains("history") || combined.Contains("psychology") || 
            combined.Contains("literature") || combined.Contains("journalism") || combined.Contains("media"))
            return "ArtsHumanities";

        // 14. Government & Civil Services (UPSC, SSC, IAS - note: require explicit civil services/upsc terms, NOT bare civil)
        if (combined.Contains("upsc") || combined.Contains("ssc") || combined.Contains("civil service") || 
            combined.Contains("civil services") || combined.Contains("government") || combined.Contains("bank po") || 
            combined.Contains("rbi") || combined.Contains("ias") || combined.Contains("ips") || 
            combined.Contains("public administration") || combined.Contains("gov.civil") || combined.Contains("gov."))
            return "GovCivil";

        // 15. Product, Project Management, Business Analysis & Operations
        if (combined.Contains("product manager") || combined.Contains("scrum") || 
            combined.Contains("project manager") || combined.Contains("agile") || 
            combined.Contains("business analyst") || combined.Contains("business analysis") || 
            combined.Contains("operations manager") || combined.Contains("operations"))
            return "Product";

        // 16. Entrepreneurship & Business
        if (combined.Contains("entrepreneur") || combined.Contains("startup") || combined.Contains("business") || 
            combined.Contains("founder") || combined.Contains("d2c") || combined.Contains("bootstrap"))
            return "Entrepreneurship";

        // 17. Web / Software families
        if (combined.Contains("full-stack") || combined.Contains("full stack") || combined.Contains("fullstack"))
            return "FullStack";

        if (combined.Contains("frontend") || combined.Contains("front-end") || combined.Contains("front end") || combined.Contains("ui developer"))
            return "Frontend";

        if (combined.Contains("backend") || combined.Contains("back-end") || combined.Contains("back end") || combined.Contains("api developer"))
            return "Backend";

        if (combined.Contains("data") || combined.Contains("machine learning") || combined.Contains("ai") || 
            combined.Contains("analytics") || combined.Contains("python") || combined.Contains("deep learning") || 
            combined.Contains("intelligence"))
            return "DataML";

        if (combined.Contains("design") || combined.Contains("ui") || combined.Contains("ux") || 
            combined.Contains("graphic") || combined.Contains("figma") || combined.Contains("wireframe"))
            return "Design";

        if (combined.Contains("market") || combined.Contains("seo") || combined.Contains("growth") || combined.Contains("content") || combined.Contains("copywrit"))
            return "Marketing";

        // 17. DevOps & Cloud (without cybersecurity, which was handled earlier)
        if (combined.Contains("devops") || combined.Contains("cloud") || combined.Contains("aws") || 
            combined.Contains("azure") || combined.Contains("gcp") || combined.Contains("sysadmin") || 
            combined.Contains("site reliability") || combined.Contains("sre") || combined.Contains("kubernetes") || 
            combined.Contains("terraform") || combined.Contains("docker"))
            return "DevOpsCloud";

        if (combined.Contains("mechanical") || combined.Contains("cad") || combined.Contains("robotics") || 
            combined.Contains("automation") || combined.Contains("industrial") || combined.Contains("solidworks") || 
            combined.Contains("sci.eng.mech"))
            return "MechanicalCore";

        // Tech check: EXCLUDE bare "engineering"! Only software / computing roles are routed to SWE
        bool isTech = combined.Contains("software") || combined.Contains("developer") || combined.Contains("programmer") || 
                      combined.Contains("coding") || combined.Contains("fullstack") || combined.Contains("frontend") || 
                      combined.Contains("backend") || combined.Contains("web") || combined.Contains("app") || 
                      combined.Contains("computer science") || combined.Contains("computer engineering") || 
                      combined.Contains("software engineering") || combined.Contains("sci.eng.cs");
        if (isTech) return "SWE";

        return "GeneralProfessional";
    }

    private class PhaseTemp
    {
        public string Title { get; set; } = "";
        public string Level { get; set; } = "Beginner";
        public string Summary { get; set; } = "";
        public string Duration { get; set; } = "3-4 Weeks";
        public List<ModuleTemp> Modules { get; set; } = new();
    }

    private class ModuleTemp
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int EstimatedHours { get; set; } = 15;
        public List<TopicTemp> Topics { get; set; } = new();
        public List<QuestionTemp> Questions { get; set; } = new();
    }

    private class TopicTemp
    {
        public string Title { get; set; } = "";
        public RoadmapTopicType Type { get; set; } = RoadmapTopicType.Concept;
        public bool IsOptional { get; set; } = false;
    }

    private class QuestionTemp
    {
        public string QuestionText { get; set; } = "";
        public string[] Options { get; set; } = Array.Empty<string>();
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; } = "";
        public string Difficulty { get; set; } = "Medium";
    }

    private static List<PhaseTemp> GetSpecializedWebPhaseTemplates(string category, string careerTitle)
    {
        var phases = category switch
        {
            "Frontend" => new[]
            {
                ("Web Foundations & Accessibility", "Beginner", new[] { ("Semantic HTML & Web Accessibility", new[] { "Document structure and semantic elements", "Forms, validation, and accessible labels", "DOM tree and browser rendering", "ARIA landmarks and keyboard navigation", "SEO-friendly page structure" }), ("CSS Layout & Responsive Design", new[] { "Cascade, specificity, and inheritance", "Box model and modern typography", "Flexbox layout patterns", "CSS Grid composition", "Responsive and mobile-first design" }), ("Browser Tools & Version Control", new[] { "Browser DevTools inspection", "Git branching and pull requests", "npm scripts and package management", "HTTP basics and browser caching", "Debugging a responsive page" }) }),
                ("JavaScript & TypeScript Engineering", "Intermediate", new[] { ("JavaScript Language Core", new[] { "Values, scope, and closures", "Functions and asynchronous execution", "Arrays, objects, and modules", "Error handling and debugging", "ES modules and browser APIs" }), ("TypeScript for UI Applications", new[] { "Types, interfaces, and generics", "Narrowing and safe API data", "Component prop and state types", "tsconfig and build tooling", "Refactoring JavaScript to TypeScript" }), ("Browser Interaction & Data", new[] { "Events and event delegation", "Forms and client-side validation", "Fetch, JSON, and request states", "Local storage and URL state", "Loading, empty, and error experiences" }) }),
                ("Component Frameworks & UI Architecture", "Intermediate", new[] { ("React Components & Composition", new[] { "Component boundaries and props", "State, effects, and rendering", "Reusable component patterns", "Controlled and uncontrolled inputs", "Composition and custom hooks" }), ("Routing, State & API Integration", new[] { "Client-side routing", "Server state and caching", "Authentication-aware UI", "Pagination and optimistic updates", "Form and API error handling" }), ("Design Systems & Product UI", new[] { "Design tokens and reusable primitives", "Responsive navigation and layouts", "Accessibility testing in components", "Data tables, filters, and dashboards", "Visual consistency and UI review" }) }),
                ("Frontend Quality, Performance & Security", "Advanced", new[] { ("Testing User Interfaces", new[] { "Unit tests for utilities", "Component tests and user behavior", "Mocking network requests", "End-to-end browser tests", "Regression test strategy" }), ("Performance Engineering", new[] { "Core Web Vitals", "Code splitting and lazy loading", "Image and asset optimization", "Rendering and memoization", "Performance profiling with DevTools" }), ("Frontend Security & Reliability", new[] { "XSS and safe rendering", "CSRF and token handling", "Content Security Policy basics", "Dependency and supply-chain hygiene", "Accessible failure recovery" }) }),
                ("Production Frontend & Portfolio", "ProLevel", new[] { ("Production Delivery", new[] { "Build and environment configuration", "CI checks and preview deployments", "Monitoring client errors", "Release and rollback workflow", "Production documentation" }), ("Capstone Product Interface", new[] { "User journeys and acceptance criteria", "Information architecture", "Responsive implementation", "Real API integration", "Usability and accessibility review" }), ("Portfolio & Interview Readiness", new[] { "Technical case study writing", "Explaining architectural decisions", "Frontend system design", "Code review and collaboration", "Portfolio deployment and presentation" }) })
            },
            "Backend" => new[]
            {
                ("Programming, Linux & HTTP Foundations", "Beginner", new[] { ("Backend Programming Core", new[] { "Types, control flow, and functions", "Object-oriented and modular design", "Exceptions and structured logging", "Testing small units", "Git and collaborative workflow" }), ("Linux, Processes & Networking", new[] { "Shell navigation and permissions", "Processes, ports, and environment variables", "DNS, TCP/IP, and TLS basics", "SSH and service configuration", "Reading logs and diagnosing failures" }), ("HTTP & API Fundamentals", new[] { "HTTP methods and status codes", "Headers, cookies, and content types", "REST resource design", "Validation and error contracts", "API documentation with OpenAPI" }) }),
                ("Databases, Persistence & API Design", "Intermediate", new[] { ("Relational Database Design", new[] { "Entities, keys, and relationships", "Normalization and constraints", "SQL joins and aggregation", "Transactions and isolation", "Indexes and query plans" }), ("Persistence & Data Access", new[] { "ORM mapping and migrations", "Repositories and unit of work", "Connection pooling", "Caching and invalidation", "Data consistency and retries" }), ("Production API Development", new[] { "Pagination, filtering, and sorting", "Authentication and authorization", "Idempotency and rate limiting", "Versioning and backward compatibility", "Contract and integration tests" }) }),
                ("Architecture, Security & Distributed Services", "Advanced", new[] { ("Application Architecture", new[] { "Layered and clean architecture", "Dependency injection", "Domain boundaries", "Background jobs and queues", "Configuration and feature flags" }), ("Backend Security", new[] { "Identity and claims", "OAuth2 and JWT validation", "Input validation and threat modeling", "Secrets and key rotation", "Audit logging and privacy" }), ("Distributed Systems", new[] { "Synchronous versus asynchronous calls", "Message delivery and retries", "Consistency and failure modes", "Service discovery and resilience", "Distributed tracing" }) }),
                ("Scalability, Observability & Cloud", "Advanced", new[] { ("Performance and Scalability", new[] { "Profiling CPU and memory", "Caching strategies", "Async and concurrency", "Load testing", "Horizontal scaling" }), ("Observability and Operations", new[] { "Structured logs", "Metrics and dashboards", "Health checks and SLOs", "Incident response", "Backup and disaster recovery" }), ("Cloud and Container Delivery", new[] { "Container images and registries", "CI/CD for backend services", "Managed databases", "Network and IAM design", "Safe deployment strategies" }) }),
                ("Production Backend & Portfolio", "ProLevel", new[] { ("Production Service Capstone", new[] { "Requirements and service boundaries", "Secure API implementation", "Database and migration plan", "Automated test pipeline", "Deployment and monitoring" }), ("System Design & Review", new[] { "Capacity estimation", "High-availability trade-offs", "Data and API contracts", "Security review", "Architecture decision records" }), ("Portfolio & Interview Readiness", new[] { "Backend case study writing", "Debugging and incident walkthroughs", "Code review practices", "Technical communication", "Portfolio and resume evidence" }) })
            },
            _ => new[]
            {
                ("Full-Stack Foundations", "Beginner", new[] { ("Web, Programming & Git Fundamentals", new[] { "HTML, CSS, and browser basics", "Programming logic and JavaScript", "Git branches and collaboration", "HTTP request and response flow", "Developer tools and debugging" }), ("Frontend Foundations", new[] { "Semantic page composition", "Responsive CSS layouts", "JavaScript DOM and events", "Forms and validation", "Accessible user experiences" }), ("Backend Foundations", new[] { "API endpoints and routing", "Server-side validation", "Authentication concepts", "SQL data modeling", "Error handling and logging" }) }),
                ("Frontend Product Engineering", "Intermediate", new[] { ("React and TypeScript Applications", new[] { "Components and composition", "State and effects", "Type-safe props and data", "Routing and forms", "Reusable UI architecture" }), ("UX, Design Systems & Accessibility", new[] { "User journeys and information architecture", "Design tokens", "Keyboard and screen-reader support", "Responsive product layouts", "Usability feedback" }), ("Frontend Data Integration", new[] { "API client patterns", "Caching and request states", "Authentication-aware screens", "Pagination and filtering", "Optimistic updates" }) }),
                ("Backend, Data & Security Engineering", "Advanced", new[] { ("API and Service Architecture", new[] { "REST resource modeling", "Layered architecture", "Validation and error contracts", "Background jobs", "API documentation" }), ("Databases and Persistence", new[] { "Relational schema design", "SQL joins and indexes", "Transactions", "ORM migrations", "Caching and consistency" }), ("Application Security", new[] { "Authentication and authorization", "Secure sessions and tokens", "Input and output protection", "Secrets management", "Audit logs and threat modeling" }) }),
                ("Integration, Testing & Deployment", "Advanced", new[] { ("Full-Stack Testing", new[] { "Unit and component tests", "API integration tests", "End-to-end user flows", "Test data and fixtures", "Regression strategy" }), ("Performance and Reliability", new[] { "Browser performance", "API profiling", "Caching and scaling", "Logs, metrics, and tracing", "Failure recovery" }), ("Cloud and CI/CD Delivery", new[] { "Containers and environment configuration", "CI/CD pipelines", "Cloud deployment", "Database backups", "Monitoring and rollback" }) }),
                ("Production Full-Stack Capstone", "ProLevel", new[] { ("End-to-End Product Build", new[] { "Product requirements and user journeys", "Accessible frontend implementation", "Secure backend APIs", "Persistent data and migrations", "Integrated testing" }), ("Production Readiness Review", new[] { "Architecture and capacity decisions", "Security and privacy review", "Performance testing", "Deployment and observability", "Operational documentation" }), ("Portfolio and Job Readiness", new[] { "Technical case study", "System design explanation", "Git and code review evidence", "Live demo and README", "Interview project walkthrough" }) })
            }
        };

        return phases.Select((phase, phaseIndex) => new PhaseTemp
        {
            Title = $"Phase {phaseIndex + 1}: {phase.Item1}",
            Level = phase.Item2,
            Summary = $"Build career-ready {category.ToLowerInvariant()} capability through structured concepts, practical work, and measurable deliverables.",
            Duration = phaseIndex == 0 ? "4-6 Weeks" : "5-7 Weeks",
            Modules = phase.Item3.Select((module, moduleIndex) => new ModuleTemp
            {
                Title = module.Item1,
                Description = $"Develop {module.Item1.ToLowerInvariant()} through guided study, implementation, review, and a {careerTitle}-relevant work artifact.",
                EstimatedHours = 18 + (phaseIndex * 2),
                Topics = module.Item2.Select((topic, topicIndex) => new TopicTemp
                {
                    Title = topic,
                    Type = topicIndex == module.Item2.Length - 1 ? RoadmapTopicType.Project : topicIndex % 2 == 0 ? RoadmapTopicType.Concept : RoadmapTopicType.HandsOnLab
                }).ToList(),
                Questions = new List<QuestionTemp>
                {
                    new QuestionTemp
                    {
                        QuestionText = $"Which practice best demonstrates competence in {module.Item1} for {careerTitle}?",
                        Options = new[] { "Apply the concepts to a tested, documented work artifact", "Skip requirements and copy an unrelated solution", "Avoid review and validation", "Memorize terms without implementation" },
                        CorrectIndex = 0,
                        Explanation = "A tested and documented artifact demonstrates both understanding and practical execution.",
                        Difficulty = phaseIndex < 2 ? "Easy" : "Medium"
                    }
                }
            }).ToList()
        }).ToList();
    }

    private static List<PhaseTemp> GetCategoryPhaseTemplates(string category, string careerTitle, string? startingPoint)
    {
        var combinedCtx = (careerTitle + " " + (startingPoint ?? "")).ToLowerInvariant();
        bool isLowEducation = combinedCtx.Contains("12th fail")
            || combinedCtx.Contains("dropout")
            || combinedCtx.Contains("no degree")
            || combinedCtx.Contains("fail")
            || combinedCtx.Contains("failed");

        if (category is "Frontend" or "Backend" or "FullStack")
            return GetSpecializedWebPhaseTemplates(category, careerTitle);

        if (category == "CivilEngineering")
            return GetCivilEngineeringPhaseTemplates(careerTitle);

        if (category == "Cybersecurity")
            return GetCybersecurityPhaseTemplates(careerTitle);

        if (category == "ElectricalElectronics")
            return GetElectricalElectronicsPhaseTemplates(careerTitle);

        if (category == "ChemicalEngineering")
            return GetChemicalEngineeringPhaseTemplates(careerTitle);

        if (category == "MedicineClinical")
            return GetMedicineClinicalPhaseTemplates(careerTitle);

        if (category == "TeachingEducation")
            return GetTeachingEducationPhaseTemplates(careerTitle);

        if (category == "ArchitecturePlanning")
            return GetArchitecturePlanningPhaseTemplates(careerTitle);

        if (category == "SciencePCB")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: Cell Biology, Biomolecules & Human Physiology",
                    Level = "Beginner",
                    Summary = "Master foundational cell structure, biomolecules (Proteins, Lipids, DNA/RNA), enzymes, and human organ systems.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Structural Cell Biology & Biomolecular Chemistry",
                            Description = "Study organelle architecture, cell division (Mitosis/Meiosis), enzyme kinetics, and nucleic acid replication.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Cell Theory, Organelles & Membrane Transport", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Biomolecules: Amino Acids, Carbohydrates & Lipids", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Enzyme Catalysis, Inhibition & Metabolic Pathways", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Cell Cycle, Mitosis, Meiosis & Regulation", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which organelle is primarily responsible for ATP synthesis during cellular respiration?",
                                    Options = new[] { "Mitochondria", "Golgi Apparatus", "Endoplasmic Reticulum", "Lysosome" },
                                    CorrectIndex = 0,
                                    Explanation = "Mitochondria generate ATP via oxidative phosphorylation.",
                                    Difficulty = "Easy"
                                },
                                new QuestionTemp
                                {
                                    QuestionText = "What phase of cell division involves chromosome alignment at the cell equator?",
                                    Options = new[] { "Metaphase", "Prophase", "Anaphase", "Telophase" },
                                    CorrectIndex = 0,
                                    Explanation = "During Metaphase, sister chromatids line up at the equatorial plate.",
                                    Difficulty = "Medium"
                                }
                            }
                        },
                        new ModuleTemp
                        {
                            Title = "Human Physiology & Organ Systems",
                            Description = "Explore the circulatory, respiratory, nervous, endocrine, and digestive systems.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Cardiovascular & Respiratory Mechanics", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Nervous System Signal Conduction & Synapses", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Endocrine Glands, Hormonal Regulation & Feedback Loops", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Digestive & Excretory System Homeostasis", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which hormone regulates blood glucose levels by promoting cellular uptake of glucose?",
                                    Options = new[] { "Insulin", "Glucagon", "Thyroxine", "Adrenaline" },
                                    CorrectIndex = 0,
                                    Explanation = "Insulin secreted by pancreatic beta cells facilitates glucose entry into body cells.",
                                    Difficulty = "Easy"
                                },
                                new QuestionTemp
                                {
                                    QuestionText = "What functional unit of the human kidney filters blood to form urine?",
                                    Options = new[] { "Nephron", "Neuron", "Alveolus", "Villus" },
                                    CorrectIndex = 0,
                                    Explanation = "Nephrons filter blood, reabsorb essential nutrients, and produce urine.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Organic Chemistry, Thermodynamics & Genetics",
                    Level = "Intermediate",
                    Summary = "Understand organic reaction mechanisms, physical chemistry equilibrium, Mendelian genetics, and molecular inheritance.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Organic Reactions & Physical Chemistry Principles",
                            Description = "Master functional groups, reaction mechanisms, chemical kinetics, and solutions.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Reaction Mechanisms: Nucleophilic & Electrophilic Substitutions", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Chemical Kinetics, Rate Laws & Catalysis", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Thermodynamics, Enthalpy & Free Energy (Delta G)", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Equilibrium & Buffer Solutions in Biological Systems", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What does a negative Gibbs Free Energy change (Delta G < 0) indicate for a biological reaction?",
                                    Options = new[] { "The reaction is exergonic and spontaneous", "The reaction requires external energy input", "The reaction is at equilibrium", "The reaction rate is zero" },
                                    CorrectIndex = 0,
                                    Explanation = "Delta G < 0 signifies a spontaneous exergonic process.",
                                    Difficulty = "Medium"
                                }
                            }
                        },
                        new ModuleTemp
                        {
                            Title = "Genetics & Molecular Inheritance",
                            Description = "Study DNA replication, transcription, translation, Mendelian crosses, and genetic disorders.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Mendelian Laws & Monohybrid/Dihybrid Crosses", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "DNA Central Dogma: Replication, Transcription & Translation", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Chromosomal Mutations & Pedigree Analysis", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which enzyme synthesizes mRNA from a DNA template during transcription?",
                                    Options = new[] { "RNA Polymerase", "DNA Ligase", "Helicase", "Reverse Transcriptase" },
                                    CorrectIndex = 0,
                                    Explanation = "RNA Polymerase transcribes DNA into pre-mRNA.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Pathology, Pharmacology & Diagnostic Procedures",
                    Level = "Advanced",
                    Summary = "Examine medical microbiology, immune response mechanisms, drug classifications, and clinical laboratory procedures.",
                    Duration = "6 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Microbiology, Immunology & Clinical Pathology",
                            Description = "Study pathogenic bacteria, viruses, innate/adaptive immunity, antibody structure, and blood analysis.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Pathogenic Microorganisms & Mechanism of Infection", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Innate vs Adaptive Immunity & Antibody Kinetics", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Clinical Hematology: Complete Blood Counts (CBC) & Pathology", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which antibody class is primarily responsible for mucosal immunity and present in body secretions?",
                                    Options = new[] { "IgA", "IgG", "IgE", "IgM" },
                                    CorrectIndex = 0,
                                    Explanation = "IgA is the predominant immunoglobulin in secretions like saliva, tears, and mucous membranes.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: Clinical Ethics, Specialization & Healthcare Pathways",
                    Level = "ProLevel",
                    Summary = "Prepare for medical entrance exams (NEET/NEXT/USMLE), hospital internship workflows, bioethics, and specialized clinical fields.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Healthcare Bioethics & Clinical Career Specialization",
                            Description = "Master medical ethics, patient consent, clinical research protocols, and competitive exam strategies.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Medical Ethics, Informed Consent & Patient Confidentiality", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Clinical Trial Phases & Good Clinical Practice (GCP)", Type = RoadmapTopicType.Reading },
                                new TopicTemp { Title = "Specialization Roadmap: MBBS, BDS, Pharmacy, Biotechnology & Research", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What is the primary ethical purpose of Informed Consent in clinical practice?",
                                    Options = new[] { "To respect patient autonomy and ensure understanding of risks before treatment", "To legal shield hospitals from all errors", "To increase medical billing", "To speed up surgery procedures" },
                                    CorrectIndex = 0,
                                    Explanation = "Informed consent upholds patient autonomy by ensuring voluntary decision-making with full risk awareness.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "BiotechPharma")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: Recombinant DNA, Cell Culture & Biomolecular Techniques",
                    Level = "Beginner",
                    Summary = "Master DNA cloning vectors, restriction endonucleases, PCR amplification, and mammalian/microbial cell culture fundamentals.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Recombinant DNA Technology & PCR",
                            Description = "Learn gene cloning, plasmid vectors, restriction enzymes, gel electrophoresis, and PCR reaction design.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Restriction Enzymes & Ligase Mechanisms", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Plasmid Vectors & Transformation Protocol", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "PCR Cycle Optimization & Gel Electrophoresis", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "DNA Sequencing: Sanger vs Next-Generation (NGS)", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which enzyme joins DNA fragments together by forming phosphodiester bonds?",
                                    Options = new[] { "DNA Ligase", "DNA Polymerase", "Restriction Endonuclease", "Helicase" },
                                    CorrectIndex = 0,
                                    Explanation = "DNA Ligase seals nicked DNA strands by forming phosphodiester bonds.",
                                    Difficulty = "Easy"
                                },
                                new QuestionTemp
                                {
                                    QuestionText = "What step in a PCR cycle melts double-stranded DNA into single strands?",
                                    Options = new[] { "Denaturation (~95°C)", "Annealing (~55°C)", "Extension (~72°C)", "Hold (~4°C)" },
                                    CorrectIndex = 0,
                                    Explanation = "High temperatures around 95°C break hydrogen bonds to denature double-stranded DNA.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Bioprocess Engineering & Fermentation",
                    Level = "Intermediate",
                    Summary = "Understand bioreactor operation, mass transfer, downstream purification, and recombinant protein production.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Bioreactor Operations & Downstream Processing",
                            Description = "Master batch/fed-batch fermentation, dissolved oxygen control, filtration, and chromatography purification.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Bioreactor Design & Scale-Up Fundamentals", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Fermentation Kinetics & Nutrient Media Formulations", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Chromatography: Affinity, Ion-Exchange & Size Exclusion", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Biopharmaceutical Product Lyophilization & Quality Control", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which chromatography technique separates proteins primarily based on charge interaction?",
                                    Options = new[] { "Ion-Exchange Chromatography", "Size-Exclusion Chromatography", "Hydrophobic Interaction Chromatography", "Affinity Chromatography" },
                                    CorrectIndex = 0,
                                    Explanation = "Ion-exchange chromatography separates biomolecules according to net surface charge.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Pharmacology & Clinical Drug Development",
                    Level = "Advanced",
                    Summary = "Study pharmacokinetics (ADME), pharmacodynamics, toxicological screening, and clinical trial phase management.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Pharmacokinetics, ADME & Drug Discovery",
                            Description = "Analyze Absorption, Distribution, Metabolism, Excretion (ADME), IC50 evaluation, and lead compound optimization.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Pharmacokinetics (ADME) & Bioavailability Metrics", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Target Identification & High-Throughput Screening (HTS)", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Clinical Trial Phases (Phase I - IV) & GCP Guidelines", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What clinical trial phase focuses primarily on initial human safety and dosage in a small group of healthy volunteers?",
                                    Options = new[] { "Phase I", "Phase II", "Phase III", "Phase IV" },
                                    CorrectIndex = 0,
                                    Explanation = "Phase I clinical trials evaluate safety, pharmacokinetics, and tolerable dose ranges in healthy human subjects.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: Regulatory Affairs & Bioinformatics Capstone",
                    Level = "ProLevel",
                    Summary = "Master FDA/DCGI drug approval filings (IND/NDA), bio-patent claims, and BLAST/BLASTp sequence alignments.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Bioinformatics & Pharmaceutical Regulatory Submissions",
                            Description = "Use NCBI Entrez, UniProt, and BLAST for sequence analysis while preparing regulatory eCTD submissions.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "NCBI Entrez, UniProt & Sequence Alignment (BLAST)", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Regulatory Dossier Filings (IND, NDA, ANDA) & cGMP", Type = RoadmapTopicType.Reading },
                                new TopicTemp { Title = "Intellectual Property & Biotech Patent Strategy", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What does cGMP stand for in pharmaceutical manufacturing quality control?",
                                    Options = new[] { "Current Good Manufacturing Practice", "Central Growth Medical Procedure", "Certified Gene Mutation Protocol", "Clinical Grade Material Process" },
                                    CorrectIndex = 0,
                                    Explanation = "cGMP regulations enforce proper design, monitoring, and control of manufacturing processes and facilities.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "SciencePCM")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: Classical Mechanics, Differential Calculus & Wave Motion",
                    Level = "Beginner",
                    Summary = "Master Newtonian mechanics, vector calculus, differential equations, and mechanical wave harmonics.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Newtonian Mechanics & Differential Calculus",
                            Description = "Learn kinematics, dynamics, energy conservation, momentum vectors, and derivatives.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Vectors, Kinematics & Newton's Laws of Motion", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Work, Energy, Power & Impulse-Momentum", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Differential Calculus: Limits, Derivatives & Rates of Change", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Simple Harmonic Motion (SHM) & Wave Motion", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "According to Newton's Second Law, force is equal to what rate of change?",
                                    Options = new[] { "Rate of change of momentum (dp/dt)", "Rate of change of velocity", "Rate of change of kinetic energy", "Rate of change of displacement" },
                                    CorrectIndex = 0,
                                    Explanation = "Force F = dp/dt. When mass is constant, this simplifies to F = ma.",
                                    Difficulty = "Easy"
                                },
                                new QuestionTemp
                                {
                                    QuestionText = "What is the derivative of sin(x) with respect to x?",
                                    Options = new[] { "cos(x)", "-cos(x)", "tan(x)", "-sin(x)" },
                                    CorrectIndex = 0,
                                    Explanation = "d/dx[sin(x)] = cos(x).",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Electromagnetism, Optics & Integral Calculus",
                    Level = "Intermediate",
                    Summary = "Explore Maxwell's equations, electrostatic fields, wave optics, definite integrals, and differential equations.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Electrostatics, Magnetism & Integral Calculus",
                            Description = "Study Coulomb's law, Gauss's law, Faraday induction, definite integrals, and area under curves.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Coulomb's Law, Electric Fields & Gauss's Law", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Magnetic Fields, Biot-Savart Law & Ampere's Law", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Electromagnetic Induction & Faraday's Law", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Integral Calculus: Definite Integrals & Applications", Type = RoadmapTopicType.Concept }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which law states that induced electromotive force (EMF) is proportional to the rate of change of magnetic flux?",
                                    Options = new[] { "Faraday's Law of Induction", "Ampere's Law", "Ohm's Law", "Coulomb's Law" },
                                    CorrectIndex = 0,
                                    Explanation = "Faraday's law dictates that EMF = -d(phi)/dt.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Physical Chemistry, Thermodynamics & Linear Algebra",
                    Level = "Advanced",
                    Summary = "Understand chemical kinetics, quantum states, atomic structure, matrices, and linear transformation systems.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Thermodynamics, Quantum Physics & Linear Algebra",
                            Description = "Examine laws of thermodynamics, wave-particle duality, matrices, determinants, and vector spaces.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Laws of Thermodynamics & Entropy", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Photoelectric Effect & Wave-Particle Duality", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Matrices, Determinants & Systems of Linear Equations", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Eigenvalues, Eigenvectors & Vector Spaces", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "If a matrix has a determinant equal to zero, what can be inferred about its inverse?",
                                    Options = new[] { "The matrix is singular and does not have an inverse", "The inverse is the identity matrix", "The inverse is equal to its transpose", "The inverse has infinite determinant" },
                                    CorrectIndex = 0,
                                    Explanation = "A zero determinant means the matrix is non-invertible (singular).",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: Numerical Methods, Applied Science & Career Pathways",
                    Level = "ProLevel",
                    Summary = "Apply numerical methods, mathematical modeling, competitive exam preparation (JEE/GATE), and research pathways.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Numerical Analysis, Modeling & Specialization",
                            Description = "Master Newton-Raphson methods, numerical integration (Trapezoidal/Simpson), and career pathways in Engineering & Physical Sciences.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Numerical Root Finding (Newton-Raphson) & Integration", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Mathematical Modeling of Physical Systems", Type = RoadmapTopicType.Project },
                                new TopicTemp { Title = "Career & Academic Pathways: Engineering, Astrophysics, Pure Physics & Applied Math", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What is the primary advantage of numerical methods like Newton-Raphson in physics and engineering?",
                                    Options = new[] { "Solving non-linear equations that lack closed-form analytical solutions", "Replacing experimental measurements entirely", "Eliminating all mathematical errors", "Speeding up paper printing" },
                                    CorrectIndex = 0,
                                    Explanation = "Numerical techniques approximate roots for complex non-linear systems.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "CommerceFinance")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: Financial Accounting & Commercial Law",
                    Level = "Beginner",
                    Summary = "Master double-entry bookkeeping, general ledgers, financial statements, and business law fundamentals.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Double-Entry Bookkeeping & Financial Statements",
                            Description = "Learn debit/credit rules, trial balances, P&L statements, and balance sheet preparation.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Accounting Principles & Double-Entry Journal Entries", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "General Ledger Posting & Trial Balance Reconciliation", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Preparing Income Statement & Balance Sheet", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Under the fundamental accounting equation, Assets equal what?",
                                    Options = new[] { "Liabilities + Owner's Equity", "Revenue - Expenses", "Cash + Inventory", "Gross Profit - Net Profit" },
                                    CorrectIndex = 0,
                                    Explanation = "Assets = Liabilities + Owner's Equity is the core balancing equation.",
                                    Difficulty = "Easy"
                                },
                                new QuestionTemp
                                {
                                    QuestionText = "Which account type increases with a debit entry?",
                                    Options = new[] { "Assets and Expenses", "Liabilities and Equity", "Revenue and Retained Earnings", "Notes Payable" },
                                    CorrectIndex = 0,
                                    Explanation = "Asset and Expense accounts increase with debits and decrease with credits.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Corporate Finance, Ratio Analysis & Taxation",
                    Level = "Intermediate",
                    Summary = "Understand Time Value of Money, NPV, IRR, ratio analysis, direct/indirect taxation (GST & Income Tax).",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Financial Management, Valuation & Taxation",
                            Description = "Calculate Net Present Value (NPV), Internal Rate of Return (IRR), working capital, and GST filings.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Time Value of Money: Discounted Cash Flow (DCF), NPV & IRR", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Financial Ratio Analysis (Liquidity, Solvency & Profitability)", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Direct Tax (Income Tax Slabs & Deductions) & Indirect Tax (GST)", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "If a project's Net Present Value (NPV) is positive at a company's hurdle rate, what should the investment decision be?",
                                    Options = new[] { "Accept the project because it creates net value", "Reject the project immediately", "Delay for 10 years", "Ignore the NPV" },
                                    CorrectIndex = 0,
                                    Explanation = "A positive NPV indicates the project's projected cash inflows exceed costs.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Auditing, Financial Modeling & Banking Systems",
                    Level = "Advanced",
                    Summary = "Master statutory auditing standards, Excel financial modeling, equity research, and commercial banking operations.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Financial Modeling & Statutory Auditing",
                            Description = "Build 3-statement financial models in Excel, perform audit risk assessments, and evaluate internal controls.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Excel 3-Statement Financial Modeling & Forecasting", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Auditing Standards: Materiality, Vouching & Internal Control Test", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Commercial Banking Operations & Credit Analysis", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What statement links the Income Statement and the Balance Sheet by tracking operational cash flows?",
                                    Options = new[] { "Cash Flow Statement", "Statement of Retained Earnings", "Trial Balance", "Bank Reconciliation Statement" },
                                    CorrectIndex = 0,
                                    Explanation = "The Cash Flow Statement reconciles net income to actual cash changes across operating, investing, and financing activities.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: Professional Certifications & Strategic Finance",
                    Level = "ProLevel",
                    Summary = "Prepare for CA, CFA, CS, ACCA, or Investment Banking roles with strategic portfolio management and M&A evaluation.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Strategic Investment Banking & Certification Roadmap",
                            Description = "Evaluate Mergers & Acquisitions (M&A), LBO models, risk management, and professional exam paths (CA/CFA/CS).",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Mergers & Acquisitions (M&A) Due Diligence & Valuation", Type = RoadmapTopicType.Project },
                                new TopicTemp { Title = "Portfolio Theory & Risk Management (Sharpe Ratio, Beta)", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Career Certification Roadmap: CA, CFA, ACCA, CS & Investment Banking", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What does Beta measure in Capital Asset Pricing Model (CAPM)?",
                                    Options = new[] { "Systematic risk/volatility relative to the overall market", "Company dividend yield", "Inflation rate", "Short-term debt ratio" },
                                    CorrectIndex = 0,
                                    Explanation = "Beta quantifies a stock's systematic market risk.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "LawLegal")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: Constitutional Law & Legal Reasoning",
                    Level = "Beginner",
                    Summary = "Master Preamble, Fundamental Rights, Court Structure, Jurisprudence, and Legal Case Analysis.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Indian Constitution & Judicial System",
                            Description = "Study Fundamental Rights (Articles 12-35), Writs, Supreme Court precedent, and legal reasoning.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Preamble & Fundamental Rights (Articles 14, 19, 21)", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Writ Jurisdiction (Habeas Corpus, Mandamus, Certiorari)", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Hierarchy of Indian Courts & Precedent (Stare Decisis)", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which constitutional article guarantees Protection of Life and Personal Liberty in India?",
                                    Options = new[] { "Article 21", "Article 14", "Article 19", "Article 32" },
                                    CorrectIndex = 0,
                                    Explanation = "Article 21 states no person shall be deprived of life or personal liberty except according to procedure established by law.",
                                    Difficulty = "Easy"
                                },
                                new QuestionTemp
                                {
                                    QuestionText = "Which writ literally means 'We Command' and orders a public authority to perform its duty?",
                                    Options = new[] { "Mandamus", "Habeas Corpus", "Quo Warranto", "Certiorari" },
                                    CorrectIndex = 0,
                                    Explanation = "Mandamus is issued by a court commanding a public officer to perform a duty.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Contract Law, Torts & Criminal Law (IPC/BNS)",
                    Level = "Intermediate",
                    Summary = "Understand elements of a valid contract, civil tort liability, criminal intention (Mens Rea), and Bharatiya Nyaya Sanhita.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Contractual Obligations, Torts & Criminal Jurisprudence",
                            Description = "Analyze offer, acceptance, consideration, breach of contract, negligence, and criminal trial procedures.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Essentials of Valid Contract & Breach Remedies", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Law of Torts: Negligence, Strict Liability & Defamation", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Criminal Law Principles: Mens Rea, Actus Reus & BNS Overview", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What fundamental legal principle requires both a guilty mind (Mens Rea) and a prohibited act (Actus Reus) for criminal liability?",
                                    Options = new[] { "Actus non facit reum nisi mens sit rea", "Res ipsa loquitur", "Caveat emptor", "Audi alteram partem" },
                                    CorrectIndex = 0,
                                    Explanation = "An act does not make a person guilty unless their mind is also guilty.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Corporate Law, Intellectual Property & Arbitration",
                    Level = "Advanced",
                    Summary = "Master Company Law, mergers, trademarks, patents, copyright, Alternative Dispute Resolution (ADR), and arbitration.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Corporate Governance & Intellectual Property Law",
                            Description = "Draft corporate agreements, understand board responsibilities, IP registration, and commercial arbitration procedures.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Companies Act Framework & Corporate Governance", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Intellectual Property: Patents, Trademarks & Copyrights", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Alternative Dispute Resolution (ADR) & Arbitration Proceedings", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What legal doctrine separates a corporation as a distinct entity from its shareholders?",
                                    Options = new[] { "Corporate Personality / Salomon v Salomon", "Promissory Estoppel", "Ultra Vires", "Strict Liability" },
                                    CorrectIndex = 0,
                                    Explanation = "The doctrine of corporate personality recognizes a company as an independent legal person.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: Litigation Practice, Legal Drafting & Judicial Exam Pathway",
                    Level = "ProLevel",
                    Summary = "Master pleading drafting, moot court advocacy, court etiquette, Bar Council exams (AIBE), and Judicial Services (PCS-J).",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Legal Drafting, Advocacy & Bar Qualification",
                            Description = "Draft plaints, written statements, bail applications, affidavits, and prepare for AIBE & Judicial exams.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Drafting Plaints, Written Statements & Affidavits", Type = RoadmapTopicType.Project },
                                new TopicTemp { Title = "Courtroom Advocacy, Cross-Examination & Trial Tactics", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Career Pathways: AIBE Bar License, Corporate Law Firm practice & Judicial Services", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What document is filed by a plaintiff to initiate a civil lawsuit in court?",
                                    Options = new[] { "Plaint", "Written Statement", "Rejoinder", "Bail Application" },
                                    CorrectIndex = 0,
                                    Explanation = "A plaint is the initial pleading filed by the plaintiff stating the cause of action.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "ArtsHumanities")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: World History, Political Theory & Philosophy",
                    Level = "Beginner",
                    Summary = "Explore historical epochs, political ideologies (Democracy, Socialism, Liberalism), and critical philosophical frameworks.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Historical Epochs & Political Thought",
                            Description = "Study major world revolutions, constitutional philosophy, human rights evolution, and historiography.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "World History & Decolonization Movements", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Classical & Modern Political Philosophy (Plato, Locke, Marx)", Type = RoadmapTopicType.Reading },
                                new TopicTemp { Title = "Foundations of Human Rights & Social Justice", Type = RoadmapTopicType.Concept }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which philosopher formulated the concept of 'Social Contract' where citizens surrender certain liberties in exchange for state protection?",
                                    Options = new[] { "John Locke / Thomas Hobbes", "Aristotle", "Friedrich Nietzsche", "Immanuel Kant" },
                                    CorrectIndex = 0,
                                    Explanation = "Locke, Hobbes, and Rousseau pioneered Social Contract theory.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Sociology, Research Methods & Behavioral Psychology",
                    Level = "Intermediate",
                    Summary = "Master qualitative research methods, social stratification, cultural analysis, and cognitive psychology principles.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Sociological Frameworks & Qualitative Research",
                            Description = "Learn ethnographic research, survey methodology, cognitive bias, and social structure analysis.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Sociological Theory (Durkheim, Weber, Bourdieu)", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Qualitative Research: Interviews, Focus Groups & Case Studies", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Cognitive Psychology & Human Behavior Dynamics", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What research methodology relies on immersive observation of communities in their natural environment?",
                                    Options = new[] { "Ethnography", "Randomized Controlled Trial", "Statistical Regression", "Laboratory Experiment" },
                                    CorrectIndex = 0,
                                    Explanation = "Ethnography involves immersive qualitative observation of social groups.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Public Policy, Ethics & Media Studies",
                    Level = "Advanced",
                    Summary = "Examine policy formulation, media ethics, journalism, discourse analysis, and international relations.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Public Policy Analysis & Digital Media Ethics",
                            Description = "Analyze legislative policy funnels, media framing, misinformation, and global diplomatic affairs.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Public Policy Evaluation & Impact Assessment", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Media Literacy, Journalism Ethics & Fact-Checking", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "International Relations & Geopolitical Analysis", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "In media studies, what does 'Agenda-Setting Theory' suggest?",
                                    Options = new[] { "Media doesn't tell people what to think, but what topics to think about", "Media dictates every individual thought", "Media has zero impact on public opinion", "Media only reports sports events" },
                                    CorrectIndex = 0,
                                    Explanation = "Agenda-setting theory posits that news coverage influences the perceived importance of topics.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: Academic Publishing, Civil Services & Career Pathways",
                    Level = "ProLevel",
                    Summary = "Prepare for research publications, Civil Services (UPSC), NGO/Think-tank leadership, or Higher Education (NET/PhD).",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Thesis Writing, Policy Advocacy & Career Options",
                            Description = "Structure academic literature reviews, write policy briefs, and target careers in Public Policy, Publishing, or Academia.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Structuring Academic Papers & Policy Briefs", Type = RoadmapTopicType.Project },
                                new TopicTemp { Title = "Grant Writing & NGO/Think-Tank Leadership", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Career Specializations: UPSC/State PSC, Research Fellowships, Journalism & Academia", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What is the primary purpose of a Policy Brief in governance and think-tanks?",
                                    Options = new[] { "To provide concise, evidence-based recommendations to decision-makers", "To write fiction stories", "To calculate corporate tax liability", "To design software UI" },
                                    CorrectIndex = 0,
                                    Explanation = "A policy brief distills complex research into actionable recommendations for policymakers.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "GovCivil")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: Indian Polity, Constitution & Governance",
                    Level = "Beginner",
                    Summary = "Master Indian Constitution, Parliamentary procedures, Judiciary, Executive structure, and Panchayat Raj system.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Indian Polity & Constitutional Structure",
                            Description = "Study Articles, Amendments, Federalism, Center-State relations, and Constitutional Bodies (ECI, UPSC, CAG).",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Salient Features of Indian Constitution & Preamble", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Executive, Legislative & Judicial Branch Functions", Type = RoadmapTopicType.Reading },
                                new TopicTemp { Title = "Panchayati Raj (73rd & 74th Amendments)", Type = RoadmapTopicType.Concept }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which constitutional amendment established Panchayati Raj institutions in India?",
                                    Options = new[] { "73rd Amendment Act", "42nd Amendment Act", "44th Amendment Act", "86th Amendment Act" },
                                    CorrectIndex = 0,
                                    Explanation = "The 73rd Amendment Act of 1992 gave constitutional status to Panchayati Raj institutions.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Indian Economy, Geography & Environment",
                    Level = "Intermediate",
                    Summary = "Understand Five-Year planning, RBI monetary policy, physical geography of India, and climate change agreements.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Economic Growth, Physical Geography & Ecology",
                            Description = "Analyze GDP growth, inflation indices, monsoons, river systems, biodiversity hotspots, and environmental laws.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Indian Economy: Fiscal Policy, Banking & Budget", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Physical & Human Geography of India", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Ecology, Biodiversity Conservation & Climate Agreements", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which institution acts as the lender of last resort and manages monetary policy in India?",
                                    Options = new[] { "Reserve Bank of India (RBI)", "State Bank of India (SBI)", "NITI Aayog", "Ministry of Finance" },
                                    CorrectIndex = 0,
                                    Explanation = "The RBI formulates monetary policy, regulates currency, and acts as lender of last resort.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Ethics, Integrity & Mains Answer Writing",
                    Level = "Advanced",
                    Summary = "Master GS Paper IV Ethics, emotional intelligence, case studies, administrative integrity, and structured Mains answer writing.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Ethics, Administrative Values & Case Studies",
                            Description = "Analyze moral dilemmas, probity in governance, public service values, and practice 150-word answer drafting.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Human Values, Ethics & Moral Thinkers", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Emotional Intelligence & Probity in Governance", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Mains Answer Writing Structure & Case Study Analysis", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What key administrative quality ensures an official remains unbiased regardless of political pressure?",
                                    Options = new[] { "Political Neutrality / Impartiality", "Strict Favoritism", "Personal Ambition", "Blind Compliance" },
                                    CorrectIndex = 0,
                                    Explanation = "Impartiality and political neutrality guarantee fair, non-partisan administration.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: Mock Tests, Current Affairs & Interview Board Preparation",
                    Level = "ProLevel",
                    Summary = "Complete full-length Prelims/Mains mock series, master current affairs synthesis, and prepare for personality test interviews.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Current Affairs Integration & Personality Interview",
                            Description = "Synthesize news events, prepare Detailed Application Form (DAF) responses, and practice mock interviews.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "International Security & Diplomatic Relations", Type = RoadmapTopicType.Reading },
                                new TopicTemp { Title = "Prelims / Mains Speed Mock Test Strategy", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Personality Test Preparation & DAF Analysis", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What is the primary objective of the Civil Services Personality Test (Interview)?",
                                    Options = new[] { "To evaluate a candidate's mental alertness, moral integrity, and administrative suitability", "To re-test textbook memory", "To test handwriting speed", "To test typing speed" },
                                    CorrectIndex = 0,
                                    Explanation = "The personality test assesses intellectual qualities, social cohesion, and leadership potential.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "Entrepreneurship")
        {
            var p1Title = isLowEducation ? "Phase 1: Foundational Literacy, Communication & Mindset" : "Phase 1: Idea Validation & Market Research";
            var p1Summary = isLowEducation
                ? "Build foundational financial literacy, basic English/vernacular business communication, digital tool usage, and open-schooling options before commercial execution."
                : "Validate real market demand, define your value proposition, and interview early target customers before spending money.";

            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = p1Title,
                    Level = "Beginner",
                    Summary = p1Summary,
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Foundations of Value Creation & Customer Understanding",
                            Description = "Understand how businesses solve real human problems and turn customer pain points into economic value.",
                            EstimatedHours = 12,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Understanding Customer Pain Points & Need Analysis", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Basic Financial Literacy & Cash Flow Awareness", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Digital Tools: Google Workspace, WhatsApp Business & UPI", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Market Research & Competitor Mapping", Type = RoadmapTopicType.Reading },
                                new TopicTemp { Title = "Open Schooling / Certificate Options for Non-Degree Founders", Type = RoadmapTopicType.Reading, IsOptional = true }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What is the most critical first step before spending capital on product inventory or software?",
                                    Options = new[] { "Renting a prime office space", "Validating customer demand and willingness to pay", "Printing glossy business cards", "Hiring a 10-member team" },
                                    CorrectIndex = 1,
                                    Explanation = "Validating customer willingness to pay ensures you do not waste money building something nobody wants.",
                                    Difficulty = "Easy"
                                },
                                new QuestionTemp
                                {
                                    QuestionText = "What does cash flow management primarily measure in a business?",
                                    Options = new[] { "Total social media followers", "The timing of cash coming in vs cash going out", "The age of the founder", "The size of the office" },
                                    CorrectIndex = 1,
                                    Explanation = "Cash flow measures the exact inflow and outflow of operational cash to prevent insolvency.",
                                    Difficulty = "Easy"
                                }
                            }
                        },
                        new ModuleTemp
                        {
                            Title = "Market Validation & Customer Discovery",
                            Description = "Conduct structured interviews, construct basic survey funnels, and test problem-solution fit.",
                            EstimatedHours = 15,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Formulating Hypotheses for Your Business Idea", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Conducting Unbiased Customer Interviews (The Mom Test)", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Analyzing Competitor Pricing & Value Gaps", Type = RoadmapTopicType.Reading },
                                new TopicTemp { Title = "Defining Your Minimum Viable Value Proposition", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Why should you ask customers about past behavior rather than hypothetical future promises during discovery?",
                                    Options = new[] { "Past behavior reveals actual willingness to spend and real friction", "Future promises are always 100% accurate", "Past behavior is easy to guess", "It satisfies legal compliance" },
                                    CorrectIndex = 0,
                                    Explanation = "Past action reflects true priorities and habits, whereas future opinions are often overly polite or unreliable.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: MVP Building & Legal Compliance (India Context)",
                    Level = "Intermediate",
                    Summary = "Register MSME/Udyam, complete GST/PAN requirements, establish basic legal entity, and build a Minimum Viable Product.",
                    Duration = "4-6 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "MSME, Udyam Registration & Compliance Basics",
                            Description = "Master India business registration, MSME/Udyam benefits, GST thresholds, and basic banking setup.",
                            EstimatedHours = 15,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Sole Proprietorship vs OPC vs LLP vs Pvt Ltd Overview", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Free MSME / Udyam Registration Step-by-Step", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "GST Basics, Thresholds (₹20L/₹40L) & Invoice Rules", Type = RoadmapTopicType.Reading },
                                new TopicTemp { Title = "Opening Current Bank Accounts & PAN Mapping", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Basic Trademark & Intellectual Property Protection", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which registration provides official MSME recognition and priority sector benefits in India at zero government fee?",
                                    Options = new[] { "Udyam Registration", "Import Export Code", "SEBI Registration", "FSSAI License" },
                                    CorrectIndex = 0,
                                    Explanation = "Udyam registration is free on the official government portal and provides MSME classification.",
                                    Difficulty = "Easy"
                                }
                            }
                        },
                        new ModuleTemp
                        {
                            Title = "Building & Testing the Minimum Viable Product (MVP)",
                            Description = "Create the simplest working version of your service or product to start accepting real orders.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "No-Code Tools: Shopify, WordPress, Razorpay/Instamojo", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Service Delivery Workflow Design", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Landing Page Creation & Conversion Copywriting", Type = RoadmapTopicType.Project },
                                new TopicTemp { Title = "Beta Testing with First 10 Paying Customers", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What is the core objective of a Minimum Viable Product (MVP)?",
                                    Options = new[] { "To showcase full complex features on day one", "To collect maximum validated learning about customers with minimal effort", "To win international design awards", "To maximize immediate profit margins" },
                                    CorrectIndex = 1,
                                    Explanation = "An MVP tests core hypotheses with real users at minimum build cost.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Sales, Customer Acquisition & Bookkeeping",
                    Level = "Advanced",
                    Summary = "Acquire your first 100 paying customers through outbound/inbound sales and maintain strict double-entry bookkeeping.",
                    Duration = "6 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Direct Sales & Digital Customer Acquisition",
                            Description = "Master organic social outreach, cold outreach, WhatsApp funnels, and local SEO.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "B2B Outbound Prospecting & Pitching", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Meta & Google Ads Micro-Budgets (₹500/day testing)", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Customer Retention & Referral Mechanics", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Handling Objections & Closing Sales", Type = RoadmapTopicType.Concept }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What does CAC stand for in business metrics?",
                                    Options = new[] { "Customer Acquisition Cost", "Capital Annual Credit", "Corporate Asset Charge", "Customer Account Clearance" },
                                    CorrectIndex = 0,
                                    Explanation = "CAC measures the total sales and marketing cost required to acquire a single paying customer.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: Bootstrapping vs Funding & Team Building",
                    Level = "ProLevel",
                    Summary = "Evaluate angel/seed funding vs sustainable bootstrapping, hire key talent, and establish operating SOPs.",
                    Duration = "8 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Funding Routes: Bootstrapping, Bank Loans, Grants & VC Capital",
                            Description = "Understand term sheets, equity dilution, Mudra/CGTMSE bank loans, and startup India seed fund schemes.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Government Schemes: Startup India Seed Fund, Mudra & CGTMSE", Type = RoadmapTopicType.Reading },
                                new TopicTemp { Title = "Pitch Deck Structure & Financial Modeling", Type = RoadmapTopicType.Project },
                                new TopicTemp { Title = "Equity Dilution, Valuation & Cap Tables", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Angel Investors vs VC Investment Mechanics", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What is equity dilution in startup funding?",
                                    Options = new[] { "The reduction in existing shareholders' ownership percentage when new shares are issued", "The loss of cash in a bank account", "The reduction of product prices", "Decreasing company employee headcount" },
                                    CorrectIndex = 0,
                                    Explanation = "Dilution occurs when new investor equity shares decrease original founders' percentage ownership.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "DataML")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: Python, Applied Statistics & Data Analysis",
                    Level = "Beginner",
                    Summary = "Master Python data structures, NumPy vectorized operations, Pandas dataframes, and Exploratory Data Analysis (EDA).",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Python Data Analysis Stack (NumPy & Pandas)",
                            Description = "Learn memory-efficient data manipulation, cleaning missing values, grouping, and aggregations.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "NumPy N-Dimensional Arrays & Vectorization", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Pandas DataFrames: Cleaning, Indexing & Merging", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Exploratory Data Analysis (EDA) & Feature Distributions", Type = RoadmapTopicType.Project },
                                new TopicTemp { Title = "Data Visualization with Matplotlib & Seaborn", Type = RoadmapTopicType.HandsOnLab }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Why is vectorization in NumPy significantly faster than Python loops?",
                                    Options = new[] { "It executes operations in C-compiled continuous memory blocks without Python overhead", "It uses cloud servers automatically", "It deletes unused variables", "It ignores datatype checking" },
                                    CorrectIndex = 0,
                                    Explanation = "NumPy vectorization delegates operations to C-level contiguous memory blocks.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Supervised & Unsupervised Machine Learning",
                    Level = "Intermediate",
                    Summary = "Build and evaluate regression, classification, clustering, and decision tree models using Scikit-Learn.",
                    Duration = "6 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Core Machine Learning Algorithms",
                            Description = "Implement Linear/Logistic Regression, Random Forests, XGBoost, and K-Means clustering.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Supervised Learning: Regression & Classification Metrics", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Decision Trees, Random Forests & Gradient Boosting (XGBoost)", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Unsupervised Learning: K-Means & Dimensionality Reduction (PCA)", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Cross-Validation & Hyperparameter Tuning (GridSearch)", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What metric is best suited for evaluating a severely imbalanced classification dataset?",
                                    Options = new[] { "F1-Score / PR-AUC", "Raw Accuracy", "Mean Absolute Error", "R-Squared" },
                                    CorrectIndex = 0,
                                    Explanation = "F1-Score balances Precision and Recall, unlike Accuracy which can be misleading on skewed data.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Deep Learning, Neural Networks & Natural Language Processing",
                    Level = "Advanced",
                    Summary = "Construct Artificial Neural Networks, CNNs, Transformers, and LLM fine-tuning using PyTorch.",
                    Duration = "6 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "PyTorch Deep Learning & Transformer Architectures",
                            Description = "Train neural networks with backpropagation, attention mechanisms, HuggingFace Transformers, and RAG pipelines.",
                            EstimatedHours = 25,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Neural Network Architecture & Backpropagation in PyTorch", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Convolutional Networks (CNNs) & Computer Vision Basics", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Transformer Architecture, Self-Attention & HuggingFace", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Retrieval-Augmented Generation (RAG) & Vector Databases", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What core mechanism powers the Transformer architecture's ability to process context?",
                                    Options = new[] { "Self-Attention Mechanism", "Recurrent Hidden State Loops", "Pooling Layers", "K-Nearest Neighbors" },
                                    CorrectIndex = 0,
                                    Explanation = "Self-attention computes dynamic weights across all input tokens simultaneously.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: MLOps, Model Serving & Production Capstone",
                    Level = "ProLevel",
                    Summary = "Deploy models via FastAPI, track experiments with MLflow, containerize with Docker, and monitor model drift.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Production MLOps Pipeline & Model Serving",
                            Description = "Serve real-time inference APIs, automate CI/CD training pipelines, and monitor data drift.",
                            EstimatedHours = 25,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "REST API Inference with FastAPI & Pydantic", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Experiment Tracking with MLflow & DVC", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Docker Containerization & Cloud Deployment", Type = RoadmapTopicType.Project },
                                new TopicTemp { Title = "Monitoring Data Drift & Automated Model Retraining", Type = RoadmapTopicType.Concept }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What is data drift in machine learning production systems?",
                                    Options = new[] { "Change in input data distribution over time reducing model accuracy", "Network packet loss", "Slower database queries", "Hard drive failure" },
                                    CorrectIndex = 0,
                                    Explanation = "Data drift occurs when real-world data shifts away from training data distributions.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "Design")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: UX Research, User Psychology & Wireframing",
                    Level = "Beginner",
                    Summary = "Master user interviews, persona creation, information architecture, wireframing, and Figma fundamentals.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "User Experience Research & Figma Fundamentals",
                            Description = "Conduct qualitative user interviews, construct journey maps, and build low-fidelity Figma wireframes.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "User Research Methods & Empathy Mapping", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Information Architecture & User Flow Diagrams", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Figma Canvas Setup, Frames & Vector Tools", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Low-Fidelity Wireframing & Structural Layouts", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What is the primary goal of low-fidelity wireframing in UX design?",
                                    Options = new[] { "To quickly iterate on layout and hierarchy without getting distracted by visual detail", "To produce final production assets for developers", "To test server response speeds", "To write brand marketing copy" },
                                    CorrectIndex = 0,
                                    Explanation = "Low-fidelity wireframes focus purely on structural layout, workflow, and content hierarchy.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Visual Design Systems & UI Components",
                    Level = "Intermediate",
                    Summary = "Design atomic components, Figma Auto-Layout, color theory, typography scales, and responsive design grid systems.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Figma Auto-Layout & Design System Creation",
                            Description = "Master auto-layout flex box logic, design tokens, component variants, and accessible color contrast.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Figma Auto-Layout 5.0, Padding & Constraints", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Color Theory, Accessibility (WCAG 2.1) & Contrast", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Typography Scales, Hierarchy & Pairings", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Building Reusable Component Libraries & Variants", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What minimum contrast ratio is required for normal body text under WCAG 2.1 AA standards?",
                                    Options = new[] { "4.5:1", "2.0:1", "10:1", "1.5:1" },
                                    CorrectIndex = 0,
                                    Explanation = "WCAG 2.1 AA requires a contrast ratio of at least 4.5:1 for standard body text.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Interactive Prototyping & Usability Testing",
                    Level = "Advanced",
                    Summary = "Create advanced interactive Figma prototypes with smart-animate, conduct usability testing sessions, and hand off to developers.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Advanced Figma Prototyping & Developer Handoff",
                            Description = "Animate complex component states, record usability tests with Maze/Lookback, and export spec tokens.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Interactive Components & Smart Animate Transitions", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Moderated & Unmoderated Usability Testing", Type = RoadmapTopicType.Project },
                                new TopicTemp { Title = "Design Handoff: Tokens, Assets & Dev Specs", Type = RoadmapTopicType.Concept }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Why are interactive prototypes essential before developer handoff?",
                                    Options = new[] { "They allow stakeholders to test real interaction flows and catch usability issues early", "They replace front-end code", "They reduce image asset file sizes", "They generate database tables" },
                                    CorrectIndex = 0,
                                    Explanation = "Interactive prototypes validate micro-interactions and overall UX before code engineering.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "Product")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: Product Discovery, User Research & Problem Framing",
                    Level = "Beginner",
                    Summary = "Master product discovery, user persona mapping, opportunity solution trees, and problem definition.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "User Discovery & Problem Framing",
                            Description = "Conduct customer interviews, define user pain points, and frame measurable product opportunity statements.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Continuous Discovery Habits & User Interviews", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Opportunity Solution Trees & Value Proposition Mapping", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Jobs-To-Be-Done (JTBD) Framework", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What is the primary core focus of the Jobs-To-Be-Done (JTBD) framework?",
                                    Options = new[] { "Understanding the underlying progress a user is trying to make in a specific circumstance", "Counting daily active users", "Measuring page load speeds", "Writing code test suites" },
                                    CorrectIndex = 0,
                                    Explanation = "JTBD focuses on the fundamental progress users seek rather than just feature preferences.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Product Strategy, Prioritization & PRDs",
                    Level = "Intermediate",
                    Summary = "Write detailed Product Requirement Documents (PRDs), apply RICE/Kano prioritization, and build strategic roadmaps.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "PRD Authoring & Feature Prioritization Frameworks",
                            Description = "Structure comprehensive PRDs, define acceptance criteria, and prioritize features using RICE (Reach, Impact, Confidence, Effort).",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Writing Production PRDs with Functional Specs", Type = RoadmapTopicType.Project },
                                new TopicTemp { Title = "RICE & Kano Feature Prioritization Models", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Sprint Backlog Grooming & User Story Mapping", Type = RoadmapTopicType.Concept }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "In the RICE prioritization formula (Reach * Impact * Confidence / Effort), what does Effort represent?",
                                    Options = new[] { "The total team-time required to deliver the feature", "The financial budget in dollars", "The number of lines of code", "The customer rating" },
                                    CorrectIndex = 0,
                                    Explanation = "Effort measures person-months or sprint-time required to implement the feature.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Analytics, A/B Testing & Agile Execution",
                    Level = "Advanced",
                    Summary = "Master product analytics (Mixpanel/Amplitude), funnel drop-off analysis, A/B testing statistical significance, and Scrum execution.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Product Analytics & Conversion Funnel Optimization",
                            Description = "Analyze retention cohorts, conversion funnels, event tracking taxonomies, and A/B test variants.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Event Taxonomy & Tracking Plan Design", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Cohort Retention Analysis & Funnel Conversion Drop-Offs", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Hypothesis-Driven A/B Testing & p-value Significance", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What metric best measures long-term product-market fit and user habituation?",
                                    Options = new[] { "Cohort Retention Curve flattening over time", "Total registered user signups", "App store downloads", "Number of marketing emails sent" },
                                    CorrectIndex = 0,
                                    Explanation = "A flattening retention curve indicates users return consistently, validating real product-market fit.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: Product Go-To-Market (GTM) & Leadership",
                    Level = "ProLevel",
                    Summary = "Lead product GTM launches, align cross-functional engineering/design teams, and present executive metrics.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Go-To-Market Strategy & Product Leadership",
                            Description = "Plan launch strategy, define North Star Metrics, and manage stakeholder communication.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Defining Your North Star Metric & Input Metrics", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Go-To-Market (GTM) Cross-Functional Launch Plan", Type = RoadmapTopicType.Project },
                                new TopicTemp { Title = "Executive Stakeholder Alignment & Presentation", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What is a North Star Metric in product management?",
                                    Options = new[] { "A single key metric that best captures the core value product delivers to customers", "Total server uptime", "Average employee tenure", "Total raw line count of code" },
                                    CorrectIndex = 0,
                                    Explanation = "The North Star Metric captures the primary customer value driver aligning the whole product team.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "Marketing")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: Marketing Fundamentals, Branding & Copywriting",
                    Level = "Beginner",
                    Summary = "Master consumer psychology, value positioning, copywriting formulas (AIDA, PAS), and brand identity.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Brand Positioning & Direct Response Copywriting",
                            Description = "Learn consumer psychology, headline writing, value proposition design, and call-to-action (CTA) mechanics.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Target Audience Persona & Value Proposition Positioning", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Copywriting Frameworks: AIDA (Attention, Interest, Desire, Action)", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Landing Page Headline & High-Converting CTA Design", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What does the AIDA acronym stand for in copywriting?",
                                    Options = new[] { "Attention, Interest, Desire, Action", "Awareness, Intent, Demand, Acquisition", "Analysis, Interaction, Design, Automation", "Audience, Impulse, Direction, Agreement" },
                                    CorrectIndex = 0,
                                    Explanation = "AIDA guides copy structure from capturing attention to driving final action.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Search Engine Optimization (SEO) & Content Strategy",
                    Level = "Intermediate",
                    Summary = "Master keyword research, technical SEO, on-page optimization, link building, and content marketing funnels.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Technical & On-Page SEO Execution",
                            Description = "Perform keyword intent mapping, optimize meta tags, site architecture, core web vitals, and backlinks.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Keyword Research & Search Intent Mapping", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "On-Page SEO: Headings, Schema Markup & Internal Links", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Technical SEO: Site Speed, Crawlability & Indexing", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Authority Link Building & Digital PR", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What meta tag provides search engines with a concise summary of a webpage for search results snippets?",
                                    Options = new[] { "Meta Description", "Meta Title", "Canonical Tag", "Robots Tag" },
                                    CorrectIndex = 0,
                                    Explanation = "The meta description tag summarizes page content displayed in search result snippets.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Performance Marketing (Paid Ads) & Email Automation",
                    Level = "Advanced",
                    Summary = "Master Google Search Ads, Meta Pixel retargeting, ROAS tracking, and lifecycle email automation funnels.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Paid Acquisition Ads & Email Lifecycle Funnels",
                            Description = "Run Meta/Google ad campaigns, set up custom conversion events, and build automated email welcome/nurture sequences.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Google Search Ads: Keywords, Quality Score & Bidding Strategies", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Meta Ads Manager: Audience Targeting, Pixel & Creative Testing", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Email Marketing Automation (Klaviyo/Mailchimp) & Drip Sequences", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What metric measures ad profitability by dividing total revenue generated by paid ad spend?",
                                    Options = new[] { "ROAS (Return On Ad Spend)", "CTR (Click Through Rate)", "CPM (Cost Per Mille)", "Bounce Rate" },
                                    CorrectIndex = 0,
                                    Explanation = "ROAS = Revenue / Ad Cost, quantifying gross return on marketing investments.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: Growth Hacking, Analytics & Campaign Capstone",
                    Level = "ProLevel",
                    Summary = "Master Google Analytics 4 (GA4), marketing attribution models, referral funnels, and full-funnel GTM campaign execution.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "GA4 Marketing Analytics & Attribution Modeling",
                            Description = "Track multi-touch attribution, set up custom GA4 events, evaluate LTV/CAC ratios, and launch an integrated marketing campaign.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Google Analytics 4 (GA4) Event Tracking & Exploration Reports", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Attribution Models: First-Touch vs Last-Touch vs Data-Driven", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Full-Funnel Campaign Capstone & Growth Strategy Presentation", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "In GA4 analytics, what parameter is added to URLs to track specific campaign sources and mediums?",
                                    Options = new[] { "UTM Parameters (utm_source, utm_medium, utm_campaign)", "API Secret Key", "CSS Class Name", "SQL Query String" },
                                    CorrectIndex = 0,
                                    Explanation = "UTM parameters tag incoming traffic to track campaign performance accurately in GA4.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "DevOpsCloud")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: Linux Administration, Networking & Shell Scripting",
                    Level = "Beginner",
                    Summary = "Master Linux file systems, bash scripting, SSH keys, DNS, TCP/IP networking, and process management.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Linux System Administration & Bash Scripting",
                            Description = "Learn file permissions, systemctl services, grep/sed/awk text processing, bash automation, and SSH hardening.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Linux Directory Hierarchy & Permission Management (chmod/chown)", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Bash Automation Scripts & Cron Jobs", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Networking: TCP/IP, DNS Resolution, Subnets & SSH Security", Type = RoadmapTopicType.Concept }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which Linux command changes file read/write/execute permissions?",
                                    Options = new[] { "chmod", "chown", "systemctl", "iptables" },
                                    CorrectIndex = 0,
                                    Explanation = "chmod modifies mode bits of file permissions.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Cloud Infrastructure (AWS/Azure), IAM & VPCs",
                    Level = "Intermediate",
                    Summary = "Build cloud infrastructure, EC2/Virtual Machines, IAM security policies, VPC networking, and S3 storage buckets.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "AWS/Azure Infrastructure & Network Security",
                            Description = "Configure Virtual Private Clouds (VPC), security groups, load balancers (ALB), auto-scaling, and S3 storage.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "AWS IAM Roles, Policies & Least Privilege Access", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "VPC Architecture: Public/Private Subnets & NAT Gateways", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "EC2 Compute Provisioning, ALB & Auto-Scaling Groups", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "In cloud networking, what device enables instances in a private subnet to connect to the internet while preventing inbound internet connections?",
                                    Options = new[] { "NAT Gateway", "Internet Gateway", "Route 53", "S3 Bucket" },
                                    CorrectIndex = 0,
                                    Explanation = "A NAT Gateway allows outbound internet access for private subnet nodes while blocking external initiated inbound traffic.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: Docker Containerization & Kubernetes Orchestration",
                    Level = "Advanced",
                    Summary = "Containerize multi-container microservices with Docker Compose and orchestrate deployments, services, and ingress on Kubernetes.",
                    Duration = "6 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Containerization & Kubernetes Production Cluster Setup",
                            Description = "Build multi-stage Dockerfiles, configure Kubernetes Pods, Deployments, Services, ConfigMaps, and Helm charts.",
                            EstimatedHours = 25,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Multi-Stage Dockerfile Optimization & Docker Compose", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Kubernetes Core: Pods, Deployments, Services & Ingress", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Helm Package Management & Cluster Operations", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What Kubernetes resource maintains a set of replicated Pods to ensure application availability?",
                                    Options = new[] { "Deployment / ReplicaSet", "ConfigMap", "Ingress", "PersistentVolume" },
                                    CorrectIndex = 0,
                                    Explanation = "Deployments manage ReplicaSets to ensure specified pod counts remain running.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: Infrastructure as Code (Terraform) & Observability",
                    Level = "ProLevel",
                    Summary = "Provision declarative infrastructure using Terraform, automate CI/CD pipelines, and set up Prometheus/Grafana monitoring.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Terraform IaC, CI/CD & Prometheus Observability",
                            Description = "Write modular HCL Terraform scripts, automate GitHub Actions deployment pipelines, and configure Grafana metrics dashboards.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Terraform HCL Syntax, State Locking & Modules", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "GitOps Pipelines with GitHub Actions & ArgoCD", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Observability Stack: Prometheus Metrics & Grafana Dashboards", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Why is remote state locking crucial when using Terraform in a DevOps engineering team?",
                                    Options = new[] { "Prevents concurrent execution conflicts that could corrupt infrastructure state", "Increases network download speed", "Encrypts local source code", "Reduces cloud server costs" },
                                    CorrectIndex = 0,
                                    Explanation = "State locking prevents simultaneous Terraform runs from overwriting state files.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "MechanicalCore")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = "Phase 1: Engineering Mechanics, Thermodynamics & Strength of Materials",
                    Level = "Beginner",
                    Summary = "Master statics, dynamics, stress-strain relations, thermodynamics cycles (Carnot, Rankine, Otto), and material properties.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Statics, Material Strength & Thermodynamics",
                            Description = "Calculate axial stress, shear stress, bending moments, torsion, and thermodynamic energy equations.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Statics: Free Body Diagrams & Equilibrium Equations", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Strength of Materials: Stress-Strain Curve & Bending Moments", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "First & Second Laws of Thermodynamics & Heat Engines", Type = RoadmapTopicType.Concept }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "What property represents the ratio of tensile stress to tensile strain in the elastic region of a material?",
                                    Options = new[] { "Young's Modulus of Elasticity (E)", "Shear Modulus (G)", "Poisson's Ratio", "Yield Strength" },
                                    CorrectIndex = 0,
                                    Explanation = "Young's Modulus E = Stress / Strain in the linear elastic zone.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 2: Fluid Mechanics, Heat Transfer & Machine Elements",
                    Level = "Intermediate",
                    Summary = "Analyze Bernoulli's equation, conduction/convection/radiation, gears, bearings, shafts, and mechanical linkages.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "Fluid Dynamics & Machine Element Design",
                            Description = "Study pipe flow losses, heat exchangers, gear train ratios, belt drives, and fatigue failure criteria.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "Fluid Mechanics: Continuity & Bernoulli's Equations", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Heat Transfer: Conduction, Convection & Exchanger Design", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Design of Machine Elements: Shafts, Spur Gears & Bearings", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "Which equation expresses energy conservation along a streamline for an incompressible, inviscid fluid?",
                                    Options = new[] { "Bernoulli's Equation", "Navier-Stokes Equation", "Fourier's Law", "Hooke's Law" },
                                    CorrectIndex = 0,
                                    Explanation = "Bernoulli's equation relates pressure, kinetic, and potential energy heads along a fluid streamline.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 3: 3D CAD Modeling (SolidWorks/Fusion 360) & FEA Simulation",
                    Level = "Advanced",
                    Summary = "Design parametric 3D mechanical parts, complex assemblies, engineering drawings (GD&T), and Finite Element Analysis (FEA).",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "SolidWorks Parametric 3D CAD & Finite Element Analysis",
                            Description = "Build 3D models, apply Geometric Dimensioning and Tolerancing (GD&T), and run structural FEA stress simulations.",
                            EstimatedHours = 22,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "3D Sketching, Feature Modeling & Assemblies in SolidWorks", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Geometric Dimensioning & Tolerancing (GD&T / ASME Y14.5)", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = "Finite Element Analysis (FEA): Meshing & Von Mises Stress Testing", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "In FEA structural analysis, what stress criterion is used to predict yielding of ductile materials under multiaxial loading?",
                                    Options = new[] { "Von Mises Stress", "Maximum Principal Stress", "Mohr-Coulomb Failure", "Buckling Load Factor" },
                                    CorrectIndex = 0,
                                    Explanation = "Von Mises yield criterion evaluates distortion energy for ductile metals under combined loading.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = "Phase 4: CNC Machining, Robotics & Industrial Automation",
                    Level = "ProLevel",
                    Summary = "Master G-code programming, 3D printing additive manufacturing, PLC ladder logic, and industrial robotics integration.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = "CNC Programming, Manufacturing & Automation",
                            Description = "Write CNC G-code/M-code, configure PLC automation, study 3D printing slicing, and design robotic end-effectors.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = "CNC G-Code / M-Code Machining & CAM Toolpaths", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Additive Manufacturing (3D Printing Slicing & Materials)", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = "Industrial Robotics, Pneumatics & PLC Automation", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = "In CNC machining, what does the G-code 'G00' command instruct the machine tool to do?",
                                    Options = new[] { "Perform a rapid linear positioning movement without cutting", "Spindle stop", "Tool change", "Linear cutting feed rate" },
                                    CorrectIndex = 0,
                                    Explanation = "G00 triggers rapid motion to move tool axes quickly to a target position.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                }
            };
        }

        if (category == "GeneralProfessional")
        {
            return new List<PhaseTemp>
            {
                new PhaseTemp
                {
                    Title = $"Phase 1: Foundational Principles & Core Theory of {careerTitle}",
                    Level = "Beginner",
                    Summary = $"Master fundamental concepts, terminology, history, and core operating principles essential for a career in {careerTitle}.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = $"Core Foundations & Frameworks in {careerTitle}",
                            Description = $"Understand the foundational principles, industry terminology, key responsibilities, and standards in {careerTitle}.",
                            EstimatedHours = 15,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = $"Core Concepts & Terminology of {careerTitle}", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = $"Industry Standards & Methodologies", Type = RoadmapTopicType.Reading },
                                new TopicTemp { Title = $"Essential Tools & Hardware Setup", Type = RoadmapTopicType.HandsOnLab }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = $"What is the primary objective of mastering foundational principles in {careerTitle}?",
                                    Options = new[]
                                    {
                                        $"To build a solid understanding of industry standards and core workflows in {careerTitle}",
                                        "To skip fundamental concepts and jump to senior roles",
                                        "To memorize decorative terms without practical application",
                                        "To avoid continuous learning"
                                    },
                                    CorrectIndex = 0,
                                    Explanation = $"Foundational knowledge establishes the base necessary to excel in {careerTitle}.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = $"Phase 2: Applied Skills & Practical Workflows for {careerTitle}",
                    Level = "Intermediate",
                    Summary = $"Apply practical techniques, use specialized software/tools, and execute domain workflows for {careerTitle}.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = $"Practical Execution & Tooling for {careerTitle}",
                            Description = $"Develop hands-on capabilities, execute standard processes, and deliver quality outputs in {careerTitle}.",
                            EstimatedHours = 18,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = $"Practical Process Execution & Workflow", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = $"Domain Tooling & Software Mastery", Type = RoadmapTopicType.HandsOnLab },
                                new TopicTemp { Title = $"Quality Assurance & Peer Review Standards", Type = RoadmapTopicType.Concept }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = $"When executing practical tasks in {careerTitle}, what practice guarantees high quality results?",
                                    Options = new[]
                                    {
                                        "Validating outputs against domain requirements and industry benchmarks",
                                        "Assuming initial attempts are always defect-free",
                                        "Ignoring safety and quality guidelines",
                                        "Proceeding without documentation"
                                    },
                                    CorrectIndex = 0,
                                    Explanation = "Validation against requirements ensures consistent high quality execution.",
                                    Difficulty = "Medium"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = $"Phase 3: Advanced Projects & Case Analysis in {careerTitle}",
                    Level = "Advanced",
                    Summary = $"Solve complex scenarios, lead real-world projects, manage risk, and optimize performance in {careerTitle}.",
                    Duration = "5 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = $"Advanced Problem Solving & Project Delivery",
                            Description = $"Handle multi-variable challenges, execute comprehensive portfolio projects, and optimize outcomes.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = $"Complex Problem Analysis & Strategy", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = $"Real-World Project Delivery & Portfolio Case", Type = RoadmapTopicType.Project },
                                new TopicTemp { Title = $"Risk Mitigation & Operations Management", Type = RoadmapTopicType.Reading }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = $"What is the main purpose of executing real-world portfolio projects in {careerTitle}?",
                                    Options = new[]
                                    {
                                        "To demonstrate practical competence and solve real industry challenges",
                                        "To fulfill a formal formality",
                                        "To avoid working with teams",
                                        "To increase project complexity unnecessarily"
                                    },
                                    CorrectIndex = 0,
                                    Explanation = "Portfolio projects prove practical ability to employers and clients.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                },
                new PhaseTemp
                {
                    Title = $"Phase 4: Professional Leadership, Ethics & Career Growth in {careerTitle}",
                    Level = "ProLevel",
                    Summary = $"Master professional ethics, project management, client relations, continuous learning, and career advancement in {careerTitle}.",
                    Duration = "4 Weeks",
                    Modules = new List<ModuleTemp>
                    {
                        new ModuleTemp
                        {
                            Title = $"Ethics, Leadership & Career Mastery in {careerTitle}",
                            Description = $"Understand professional codes of conduct, team leadership, strategic decision making, and career growth pathways.",
                            EstimatedHours = 20,
                            Topics = new List<TopicTemp>
                            {
                                new TopicTemp { Title = $"Professional Ethics & Compliance Standards", Type = RoadmapTopicType.Concept },
                                new TopicTemp { Title = $"Stakeholder Communication & Leadership", Type = RoadmapTopicType.Reading },
                                new TopicTemp { Title = $"Career Advancement & Specialization Pathways", Type = RoadmapTopicType.Project }
                            },
                            Questions = new List<QuestionTemp>
                            {
                                new QuestionTemp
                                {
                                    QuestionText = $"Why are professional ethics essential for long-term career growth in {careerTitle}?",
                                    Options = new[]
                                    {
                                        "Ethics build trust, credibility, and uphold public and client safety",
                                        "Ethics limit professional opportunities",
                                        "Ethics are only for academic tests",
                                        "Ethics increase software subscription fees"
                                    },
                                    CorrectIndex = 0,
                                    Explanation = "Ethical conduct builds institutional trust and long-term reputation.",
                                    Difficulty = "Easy"
                                }
                            }
                        }
                    }
                }
            };
        }

        // SWE Category
        if (category == "SWE")
        {
            return new List<PhaseTemp>
        {
            new PhaseTemp
            {
                Title = "Phase 1: Computer Science & Programming Fundamentals",
                Level = "Beginner",
                Summary = "Master core logic, data types, control flow, algorithms, and git version control.",
                Duration = "4 Weeks",
                Modules = new List<ModuleTemp>
                {
                    new ModuleTemp
                    {
                        Title = "Programming Foundations & Syntax",
                        Description = "Learn variables, loops, conditionals, functions, and memory basics in modern languages.",
                        EstimatedHours = 15,
                        Topics = new List<TopicTemp>
                        {
                            new TopicTemp { Title = "Variables, Primitive Types & Memory", Type = RoadmapTopicType.Concept },
                            new TopicTemp { Title = "Control Flow: Conditionals & Loops", Type = RoadmapTopicType.HandsOnLab },
                            new TopicTemp { Title = "Functions, Parameters & Return Values", Type = RoadmapTopicType.HandsOnLab },
                            new TopicTemp { Title = "Git Version Control & GitHub Fundamentals", Type = RoadmapTopicType.Project }
                        },
                        Questions = new List<QuestionTemp>
                        {
                            new QuestionTemp
                            {
                                QuestionText = "What is the primary function of Git version control?",
                                Options = new[] { "To host databases in the cloud", "To track code modifications and collaborate safely across branches", "To style HTML pages", "To run server CPUs" },
                                CorrectIndex = 1,
                                Explanation = "Git tracks historical edits and manages code branching for team development.",
                                Difficulty = "Easy"
                            }
                        }
                    },
                    new ModuleTemp
                    {
                        Title = "Data Structures & Algorithm Basics",
                        Description = "Understand arrays, hash tables, linked lists, queues, and Big-O notation.",
                        EstimatedHours = 18,
                        Topics = new List<TopicTemp>
                        {
                            new TopicTemp { Title = "Array Manipulation & Searching", Type = RoadmapTopicType.HandsOnLab },
                            new TopicTemp { Title = "Hash Tables / Dictionaries & O(1) Lookups", Type = RoadmapTopicType.Concept },
                            new TopicTemp { Title = "Stacks, Queues & Recursion Fundamentals", Type = RoadmapTopicType.HandsOnLab },
                            new TopicTemp { Title = "Big-O Time & Space Complexity Analysis", Type = RoadmapTopicType.Concept }
                        },
                        Questions = new List<QuestionTemp>
                        {
                            new QuestionTemp
                            {
                                QuestionText = "What is the average time complexity of looking up a key in a Hash Table?",
                                Options = new[] { "O(1)", "O(n)", "O(n log n)", "O(n^2)" },
                                CorrectIndex = 0,
                                Explanation = "Hash tables compute key indices via hash functions, providing constant-time O(1) average lookup.",
                                Difficulty = "Easy"
                            }
                        }
                    }
                }
            },
            new PhaseTemp
            {
                Title = "Phase 2: Core Domain Architecture & Frameworks",
                Level = "Intermediate",
                Summary = "Build production applications using modern web/mobile frameworks, APIs, and databases.",
                Duration = "6 Weeks",
                Modules = new List<ModuleTemp>
                {
                    new ModuleTemp
                    {
                        Title = "Web/Application Framework Architecture",
                        Description = "Master component architectures, routing, state management, and HTTP API integration.",
                        EstimatedHours = 20,
                        Topics = new List<TopicTemp>
                        {
                            new TopicTemp { Title = "Component State & Lifecycle Management", Type = RoadmapTopicType.HandsOnLab },
                            new TopicTemp { Title = "RESTful API Design & HTTP Verbs", Type = RoadmapTopicType.Concept },
                            new TopicTemp { Title = "Asynchronous Data Fetching & Error Handling", Type = RoadmapTopicType.HandsOnLab },
                            new TopicTemp { Title = "Client-Side Routing & Page Layouts", Type = RoadmapTopicType.Project }
                        },
                        Questions = new List<QuestionTemp>
                        {
                            new QuestionTemp
                            {
                                QuestionText = "Which HTTP method should be used to idempotently update an existing resource?",
                                Options = new[] { "PUT", "POST", "GET", "DELETE" },
                                CorrectIndex = 0,
                                Explanation = "PUT replaces or updates a resource idempotently.",
                                Difficulty = "Medium"
                            }
                        }
                    },
                    new ModuleTemp
                    {
                        Title = "Database Design & SQL Persistence",
                        Description = "Design normalized relational schemas, write complex JOIN queries, and use Entity Framework / ORMs.",
                        EstimatedHours = 18,
                        Topics = new List<TopicTemp>
                        {
                            new TopicTemp { Title = "Relational Schema Normalization (1NF, 2NF, 3NF)", Type = RoadmapTopicType.Concept },
                            new TopicTemp { Title = "SQL Queries: JOINs, Aggregations & Subqueries", Type = RoadmapTopicType.HandsOnLab },
                            new TopicTemp { Title = "ORM Mapping & EF Core Migrations", Type = RoadmapTopicType.HandsOnLab },
                            new TopicTemp { Title = "Database Indexing & Performance Tuning", Type = RoadmapTopicType.Concept }
                        },
                        Questions = new List<QuestionTemp>
                        {
                            new QuestionTemp
                            {
                                QuestionText = "What is the primary benefit of adding a database index to a column?",
                                Options = new[] { "Speeds up SELECT queries at the cost of slight write overhead", "Encrypts sensitive data automatically", "Deletes duplicate rows", "Reduces total storage size" },
                                CorrectIndex = 0,
                                Explanation = "Indexes create lookup trees that accelerate read queries while adding minor overhead to INSERT/UPDATE ops.",
                                Difficulty = "Medium"
                            }
                        }
                    }
                }
            },
            new PhaseTemp
            {
                Title = "Phase 3: System Design, Testing & Security",
                Level = "Advanced",
                Summary = "Implement automated testing, microservices/clean architecture, JWT auth, and cloud deployments.",
                Duration = "6 Weeks",
                Modules = new List<ModuleTemp>
                {
                    new ModuleTemp
                    {
                        Title = "Clean Architecture & Backend Security",
                        Description = "Implement layered architecture, dependency injection, JWT authentication, and CORS policies.",
                        EstimatedHours = 22,
                        Topics = new List<TopicTemp>
                        {
                            new TopicTemp { Title = "Domain-Driven & Clean Architecture Layers", Type = RoadmapTopicType.Concept },
                            new TopicTemp { Title = "JWT Authentication & Claims-Based Authorization", Type = RoadmapTopicType.HandsOnLab },
                            new TopicTemp { Title = "Dependency Injection Patterns & Lifetimes", Type = RoadmapTopicType.Concept },
                            new TopicTemp { Title = "API Security: CORS, Rate Limiting & Input Sanitization", Type = RoadmapTopicType.Project }
                        },
                        Questions = new List<QuestionTemp>
                        {
                            new QuestionTemp
                            {
                                QuestionText = "Where is the payload signature verified in a Stateless JWT Token auth scheme?",
                                Options = new[] { "On the server using a secret key or public certificate", "In the client browser local storage", "Inside the DNS server", "In the database table on every request" },
                                CorrectIndex = 0,
                                Explanation = "The backend server verifies the JWT cryptographic signature using its secret or public key without querying the DB.",
                                Difficulty = "Medium"
                            }
                        }
                    }
                }
            },
            new PhaseTemp
            {
                Title = "Phase 4: Capstone Portfolio & Job Readiness",
                Level = "ProLevel",
                Summary = "Build a full-stack capstone project, prepare technical system design interviews, and target job roles.",
                Duration = "4 Weeks",
                Modules = new List<ModuleTemp>
                {
                    new ModuleTemp
                    {
                        Title = "Production Capstone Project Delivery",
                        Description = "Deploy a full-stack SaaS project to Docker/Cloud with CI/CD GitHub Actions pipelines.",
                        EstimatedHours = 25,
                        Topics = new List<TopicTemp>
                        {
                            new TopicTemp { Title = "Containerization with Docker & Docker Compose", Type = RoadmapTopicType.HandsOnLab },
                            new TopicTemp { Title = "CI/CD Workflows via GitHub Actions", Type = RoadmapTopicType.HandsOnLab },
                            new TopicTemp { Title = "Cloud Deployment (Azure / AWS / Vercel)", Type = RoadmapTopicType.Project },
                            new TopicTemp { Title = "Portfolio Documentation & Readme Presentation", Type = RoadmapTopicType.Project }
                        },
                        Questions = new List<QuestionTemp>
                        {
                            new QuestionTemp
                            {
                                QuestionText = "What does Docker containerization guarantee across development and production environments?",
                                Options = new[] { "Identical execution environment and dependencies regardless of host OS", "Free unlimited cloud hosting", "Automatic code generation", "Zero network latency" },
                                CorrectIndex = 0,
                                Explanation = "Docker packages code and dependencies together, eliminating 'works on my machine' issues.",
                                Difficulty = "Easy"
                            }
                        }
                    }
                }
            }
        };
        }

        // Unmatched or niche career domain: fall back safely to dynamic GeneralProfessional tailored to careerTitle
        return GetCategoryPhaseTemplates("GeneralProfessional", careerTitle, startingPoint);
    }

    private static void AddDefaultQuizQuestions(RoadmapModuleQuiz quiz, string moduleTitle, string careerTitle)
    {
        quiz.Questions.Add(new RoadmapQuizQuestion
        {
            QuizId = quiz.Id,
            QuestionText = $"What is the primary objective of mastering {moduleTitle} in the context of {careerTitle}?",
            OptionsJson = JsonSerializer.Serialize(new[]
            {
                $"To build core competency and apply practical industry patterns in {careerTitle}",
                "To memorize syntax without understanding principles",
                "To skip fundamental concepts and jump straight to senior roles",
                "To satisfy decorative resume bullet points"
            }),
            CorrectOptionIndex = 0,
            Explanation = $"Mastering {moduleTitle} equips you with actionable skills required for practical work as a {careerTitle}.",
            DifficultyTag = "Easy"
        });

        quiz.Questions.Add(new RoadmapQuizQuestion
        {
            QuizId = quiz.Id,
            QuestionText = $"When applying concepts from {moduleTitle} as a {careerTitle}, which practice ensures the best execution quality?",
            OptionsJson = JsonSerializer.Serialize(new[]
            {
                "Testing and validating outcomes against requirements",
                "Assuming the initial attempt is always error-free",
                "Ignoring documentation and industry standards",
                "Working without version control or record keeping"
            }),
            CorrectOptionIndex = 0,
            Explanation = "Validation and testing ensure your implementation meets standard requirements and edge cases.",
            DifficultyTag = "Medium"
        });
    }

    private static string CleanJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "{}";
        var match = Regex.Match(raw.Trim(), @"```(?:json)?\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : raw.Trim();
    }

    private static string GetStringProp(JsonElement elem, string prop, string fallback)
    {
        if (elem.TryGetProperty(prop, out var p) && p.ValueKind == JsonValueKind.String)
        {
            var val = p.GetString()?.Trim();
            if (!string.IsNullOrEmpty(val)) return val;
        }
        return fallback;
    }
}
