using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillScape.Application.DTOs;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public interface IDeterministicQuestionSelector
{
    Task<QuizQuestionV2?> SelectNextQuestionAsync(
        AdaptiveQuizSession session,
        List<string> answeredQuestionIds,
        ParsedProfileDto profile,
        Dictionary<string, double> currentPrior,
        List<string> targetCareerIds,
        string mode = "standard",
        string stage = "",
        List<string>? rejectedDomains = null);
}

public class DeterministicQuestionSelector : IDeterministicQuestionSelector
{
    private readonly ApplicationDbContext _context;
    private readonly IQuestionValidationEngine _validator;
    private readonly ILogger<DeterministicQuestionSelector> _logger;
    private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    // ── Career Name → Taxonomy ID Mapping ────────────────────────────────────
    // Maps user-friendly career names to taxonomy node IDs used in question signals.
    // This is the bridge between what users type and what's in the database.
    private static readonly Dictionary<string, string[]> CareerNameToNodeIds = new(StringComparer.OrdinalIgnoreCase)
    {
        // Cybersecurity
        ["soc analyst"]                  = new[] { "sci.eng.cs.soc", "sci.eng.cs.ir" },
        ["soc"]                          = new[] { "sci.eng.cs.soc" },
        ["security operations"]          = new[] { "sci.eng.cs.soc", "sci.eng.cs.ir" },
        ["penetration tester"]           = new[] { "sci.eng.cs.pentest", "sci.eng.cs.redteam" },
        ["penetration testing"]          = new[] { "sci.eng.cs.pentest" },
        ["ethical hacker"]               = new[] { "sci.eng.cs.pentest", "sci.eng.cs.redteam" },
        ["digital forensics analyst"]    = new[] { "sci.eng.cs.forensics" },
        ["digital forensics"]            = new[] { "sci.eng.cs.forensics" },
        ["forensics analyst"]            = new[] { "sci.eng.cs.forensics" },
        ["grc analyst"]                  = new[] { "sci.eng.cs.grc" },
        ["governance risk compliance"]   = new[] { "sci.eng.cs.grc" },
        ["cloud security engineer"]      = new[] { "sci.eng.cs.cloudsec" },
        ["cloud security"]               = new[] { "sci.eng.cs.cloudsec" },
        ["security architect"]           = new[] { "sci.eng.cs.secarch" },
        ["application security"]         = new[] { "sci.eng.cs.appsec" },
        ["appsec"]                       = new[] { "sci.eng.cs.appsec" },
        ["threat intelligence analyst"]  = new[] { "sci.eng.cs.threat" },
        ["threat hunter"]                = new[] { "sci.eng.cs.threat" },
        ["incident response"]            = new[] { "sci.eng.cs.ir" },
        ["incident responder"]           = new[] { "sci.eng.cs.ir" },

        // Medicine
        ["general medicine"]             = new[] { "sci.med.gen_medicine", "sci.med.internal" },
        ["physician"]                    = new[] { "sci.med.gen_medicine" },
        ["internal medicine"]            = new[] { "sci.med.internal", "sci.med.gen_medicine" },
        ["general surgery"]              = new[] { "sci.med.surgery" },
        ["surgeon"]                      = new[] { "sci.med.surgery" },
        ["surgery"]                      = new[] { "sci.med.surgery" },
        ["pediatrics"]                   = new[] { "sci.med.pediatrics" },
        ["pediatrician"]                 = new[] { "sci.med.pediatrics" },
        ["emergency medicine"]           = new[] { "sci.med.emergency" },
        ["emergency physician"]          = new[] { "sci.med.emergency" },
        ["psychiatry"]                   = new[] { "sci.med.psychiatry" },
        ["psychiatrist"]                 = new[] { "sci.med.psychiatry" },
        ["cardiology"]                   = new[] { "sci.med.cardiology" },
        ["cardiologist"]                 = new[] { "sci.med.cardiology" },
        ["neurology"]                    = new[] { "sci.med.neurology" },
        ["medical research"]             = new[] { "sci.med.research" },
        ["public health"]                = new[] { "sci.med.public_health" },

        // Software Engineering
        ["software engineer"]            = new[] { "sci.eng.cs.software", "sci.eng.cs.backend", "sci.eng.cs.frontend" },
        ["backend developer"]            = new[] { "sci.eng.cs.backend" },
        ["backend engineer"]             = new[] { "sci.eng.cs.backend" },
        ["frontend developer"]           = new[] { "sci.eng.cs.frontend" },
        ["frontend engineer"]            = new[] { "sci.eng.cs.frontend" },
        ["full stack developer"]         = new[] { "sci.eng.cs.fullstack" },
        ["full stack engineer"]          = new[] { "sci.eng.cs.fullstack" },
        ["devops engineer"]              = new[] { "sci.eng.cs.devops" },
        ["devops"]                       = new[] { "sci.eng.cs.devops" },
        ["site reliability engineer"]    = new[] { "sci.eng.cs.sre" },
        ["sre"]                          = new[] { "sci.eng.cs.sre" },
        ["mobile developer"]             = new[] { "sci.eng.cs.mobile" },
        ["ios developer"]                = new[] { "sci.eng.cs.mobile" },
        ["android developer"]            = new[] { "sci.eng.cs.mobile" },
        ["data engineer"]                = new[] { "sci.eng.cs.dataeng" },
        ["data engineering"]             = new[] { "sci.eng.cs.dataeng" },

        // AI/ML
        ["machine learning engineer"]    = new[] { "sci.eng.cs.ai_ml", "sci.eng.cs.ml" },
        ["ml engineer"]                  = new[] { "sci.eng.cs.ml", "sci.eng.cs.ai_ml" },
        ["data scientist"]               = new[] { "sci.eng.cs.data", "sci.eng.cs.ai_ml" },
        ["ai engineer"]                  = new[] { "sci.eng.cs.ai_ml" },
        ["nlp engineer"]                 = new[] { "sci.eng.cs.nlp" },
        ["computer vision engineer"]     = new[] { "sci.eng.cs.cv" },
        ["data analyst"]                 = new[] { "sci.eng.cs.data_analysis" },

        // Finance
        ["investment banker"]            = new[] { "com.finance.ib" },
        ["investment banking"]           = new[] { "com.finance.ib" },
        ["financial analyst"]            = new[] { "com.finance", "com.finance.analyst" },
        ["ca"]                           = new[] { "com.ca" },
        ["chartered accountant"]         = new[] { "com.ca" },
        ["cfa"]                          = new[] { "com.finance" },
        ["banker"]                       = new[] { "com.banking" },
        ["banking"]                      = new[] { "com.banking" },

        // Management
        ["mba"]                          = new[] { "mgmt.mba" },
        ["management consultant"]        = new[] { "mgmt.mba", "com.consulting" },
        ["product manager"]              = new[] { "com.startup", "mgmt.mba" },

        // Government
        ["ias"]                          = new[] { "govt.upsc" },
        ["upsc"]                         = new[] { "govt.upsc" },
        ["civil services"]               = new[] { "govt.upsc" },
    };

    // ── Domain Keyword → Question Domain Mapping ──────────────────────────────
    // Maps keywords from user profile (degree, major, target careers) to the
    // Domain field used on QuizQuestionV2 records.
    private static readonly Dictionary<string, string> DomainKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["mbbs"] = "Healthcare",
        ["bams"] = "Healthcare",
        ["bhms"] = "Healthcare",
        ["nursing"] = "Healthcare",
        ["medicine"] = "Healthcare",
        ["medical"] = "Healthcare",
        ["pharmacy"] = "Healthcare",
        ["b.pharm"] = "Healthcare",
        ["healthcare"] = "Healthcare",

        ["b.tech"] = "Technology",
        ["btech"] = "Technology",
        ["b.e."] = "Technology",
        ["bca"] = "Technology",
        ["m.tech"] = "Technology",
        ["mtech"] = "Technology",
        ["mca"] = "Technology",
        ["computer"] = "Technology",
        ["software"] = "Technology",
        ["cyber"] = "Technology",
        ["cybersecurity"] = "Technology",
        ["data"] = "Technology",
        ["it"] = "Technology",

        ["b.com"] = "Commerce",
        ["bba"] = "Commerce",
        ["finance"] = "Commerce",
        ["commerce"] = "Commerce",
        ["accounting"] = "Commerce",
        ["economics"] = "Commerce",
        ["business"] = "Commerce",
        ["ca"] = "Commerce",
        ["mba"] = "Commerce",
        ["m.com"] = "Commerce",
        ["mcom"] = "Commerce",

        ["m.sc"] = "PureScience",
        ["msc"] = "PureScience",
        ["science"] = "PureScience",
        ["physics"] = "PureScience",
        ["mathematics"] = "PureScience",
        ["chemistry"] = "PureScience",
        ["biology"] = "PureScience",
        ["biotechnology"] = "PureScience",

        ["m.a."] = "Arts",
        ["ma"] = "Arts",
        ["arts"] = "Arts",
        ["humanities"] = "Arts",

        ["llb"] = "Law",
        ["ba llb"] = "Law",
        ["llm"] = "Law",
        ["law"] = "Law",

        ["b.arch"] = "Architecture",
        ["architecture"] = "Architecture",
        ["design"] = "Design",
        ["b.des"] = "Design",
    };

    private readonly IProfileValidationEngine _profileValidator;

    public DeterministicQuestionSelector(
        ApplicationDbContext context,
        IQuestionValidationEngine validator,
        IProfileValidationEngine profileValidator,
        ILogger<DeterministicQuestionSelector> logger)
    {
        _context = context;
        _validator = validator;
        _profileValidator = profileValidator;
        _logger = logger;
    }

    public async Task<QuizQuestionV2?> SelectNextQuestionAsync(
        AdaptiveQuizSession session,
        List<string> answeredQuestionIds,
        ParsedProfileDto profile,
        Dictionary<string, double> currentPrior,
        List<string> targetCareerIds,
        string mode = "standard",
        string stage = "",
        List<string>? rejectedDomains = null)
    {
        rejectedDomains ??= new List<string>();
        // 1. Fetch all active questions not yet answered in this session, strictly matching PhaseKey and SubPhaseKey if applicable
        var query = _context.QuizQuestionsV2
            .AsNoTracking()
            .Where(q => q.IsActive && !answeredQuestionIds.Contains(q.Id) && q.PhaseKey == session.PhaseKey && !q.Text.Contains("(Scenario") && !q.Id.StartsWith("FB_PROFILE_"));

        var candidateQuestions = await query.ToListAsync();

        if (session.PhaseKey != null && session.PhaseKey.Equals("After12th", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(session.SubPhaseKey))
            {
                candidateQuestions = candidateQuestions.Where(q =>
                    !string.IsNullOrEmpty(q.SubPhaseKey) &&
                    (q.SubPhaseKey.Equals(session.SubPhaseKey, StringComparison.OrdinalIgnoreCase) ||
                     (session.SubPhaseKey.Equals("pcmb", StringComparison.OrdinalIgnoreCase) &&
                      (q.SubPhaseKey.Equals("pcm", StringComparison.OrdinalIgnoreCase) ||
                       q.SubPhaseKey.Equals("pcb", StringComparison.OrdinalIgnoreCase))))).ToList();
            }
        }
        else if (!string.IsNullOrEmpty(session.SubPhaseKey))
        {
            candidateQuestions = candidateQuestions.Where(q => 
                string.IsNullOrEmpty(q.SubPhaseKey) || 
                q.SubPhaseKey.Equals(session.SubPhaseKey, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // 1.5 STRICT DUPLICATE PREVENTION: Reject questions with identical text or options to those already answered
        var answeredQuestions = await _context.QuizQuestionsV2
            .AsNoTracking()
            .Where(q => answeredQuestionIds.Contains(q.Id))
            .ToListAsync();
            
        var answeredTexts = answeredQuestions.Select(q => NormalizeText(q.Text)).ToHashSet();
        var answeredOptions = answeredQuestions.Select(q => GetOptionSignature(q)).Where(s => !string.IsNullOrEmpty(s)).ToHashSet();

        candidateQuestions = candidateQuestions.Where(q => 
            !answeredTexts.Contains(NormalizeText(q.Text)) && 
            !answeredOptions.Contains(GetOptionSignature(q))
        ).ToList();

        // 2. Validate — reject malformed questions
        candidateQuestions = candidateQuestions
            .Where(q => _validator.ValidateQuestion(q).IsValid)
            .ToList();

        if (!candidateQuestions.Any())
        {
            _logger.LogWarning("[DeterministicSelector] No valid candidate questions available.");
            return null;
        }

        // 3. Determine the user's academic domain from AssessmentContext
        AssessmentContext ctx = new();
        if (!string.IsNullOrWhiteSpace(session.AssessmentContextJson) && session.AssessmentContextJson != "{}")
        {
            try { ctx = JsonSerializer.Deserialize<AssessmentContext>(session.AssessmentContextJson, _jsonOpts) ?? new(); }
            catch { }
        }
        
        string academicDomain = DetermineAcademicDomain(profile); // Fallback
        if (ctx.EligibleDomains.Any()) 
        {
            academicDomain = ctx.EligibleDomains.First(); // Use primary eligible domain from mapper
        }
        string phaseKey = session.PhaseKey ?? "";

        _logger.LogInformation("[DeterministicSelector] PhaseKey={Phase}, AcademicDomain={Domain}, TargetCareers={Careers}, EligibleDomains={ED}",
            phaseKey, academicDomain, string.Join(", ", targetCareerIds), string.Join(", ", ctx.EligibleDomains));

        // 4. Map user-entered target career names to taxonomy node IDs
        var resolvedTargetNodeIds = ResolveTargetCareers(targetCareerIds);

        // 5. PROFILE-VALIDATED POOL — Apply strict profile boundaries
        var allowedQuestionPool = new List<QuizQuestionV2>();
        foreach (var q in candidateQuestions)
        {
            if (_profileValidator.IsQuestionAllowed(q, ctx, answeredQuestionIds, out var reason))
            {
                bool isRejected = false;
                if (!string.IsNullOrEmpty(q.Domain) && rejectedDomains.Contains(q.Domain, StringComparer.OrdinalIgnoreCase)) isRejected = true;
                if (!string.IsNullOrEmpty(q.Subdomain) && rejectedDomains.Contains(q.Subdomain, StringComparer.OrdinalIgnoreCase)) isRejected = true;
                
                if (!isRejected)
                {
                    allowedQuestionPool.Add(q);
                }
                else
                {
                    _logger.LogWarning("[ProfileValidation] Rejected {Id} because it targets a rejected domain.", q.Id);
                }
            }
            else
            {
                _logger.LogWarning("[ProfileValidation] Rejected {Id}: {Reason}", q.Id, reason);
            }
        }

        if (!allowedQuestionPool.Any())
        {
            // A profile restriction is an invariant, not a preference.  Falling back
            // to the whole phase here caused a Civil Engineering session to continue
            // with Mechanical, Electrical, and CSE questions after its Civil bank was
            // exhausted.  Returning null lets the caller complete the assessment
            // cleanly instead of silently crossing the learner's major boundary.
            _logger.LogWarning(
                "[DeterministicSelector] No questions remain within the profile boundary for phase {Phase}; refusing cross-major fallback.",
                session.PhaseKey);
            return null;
        }

        // 5b. DOMAIN-FILTERED POOL (For legacy After10th/After12th fallback behavior)
        var domainPool = FilterByAcademicDomain(allowedQuestionPool, academicDomain, phaseKey, ctx);

        _logger.LogInformation("[DeterministicSelector] Domain pool size: {Count}", domainPool.Count);

        // 5c. STAGE FILTER
        if (stage == "BroadFieldDiscovery")
        {
            var broadQuestions = domainPool.Where(q => 
                (q.Domain != null && q.Domain.Equals("Universal", StringComparison.OrdinalIgnoreCase)) ||
                (q.Dimension != null && q.Dimension.Equals("domain_preference", StringComparison.OrdinalIgnoreCase)) ||
                (q.Intent != null && q.Intent.Equals("broad_field_preference", StringComparison.OrdinalIgnoreCase))
            ).ToList();

            if (broadQuestions.Any())
            {
                domainPool = broadQuestions;
                _logger.LogInformation("[DeterministicSelector] Stage is BroadFieldDiscovery, filtered down to {Count} broad questions.", domainPool.Count);
            }
        }

        // 6. TARGET CAREER MODE — Select questions that best discriminate between target careers
        if (resolvedTargetNodeIds.Any() && domainPool.Any())
        {
            var targetedQuestion = SelectByTargetDiscrimination(domainPool, resolvedTargetNodeIds, currentPrior);
            if (targetedQuestion != null)
            {
                _logger.LogInformation("[DeterministicSelector] Selected targeted question: {Id}", targetedQuestion.Id);
                return targetedQuestion;
            }
        }

        // 7. Fallback to domain pool (information gain)
        if (domainPool.Any())
        {
            var q = SelectByInformationGain(domainPool, currentPrior);
            _logger.LogInformation("[DeterministicSelector] Selected by info gain from domain pool: {Id}", q.Id);
            return q;
        }

        // 8. The narrow academic-domain pool may be empty even though the
        // question has already passed the phase and profile-boundary checks.
        // Use that safe pool rather than failing the whole quiz at startup.
        var safeFallback = SelectByInformationGain(allowedQuestionPool, currentPrior);
        _logger.LogInformation("[DeterministicSelector] Selected safe same-phase fallback: {Id}", safeFallback.Id);
        return safeFallback;
    }

    // ── Domain Detection ──────────────────────────────────────────────────────

    private static string DetermineAcademicDomain(ParsedProfileDto profile)
    {
        var degree = (profile.Education?.Degree ?? "").ToLowerInvariant();
        var specialization = (profile.Education?.Specialization ?? "").ToLowerInvariant();
        var major = profile.ExtractedFields.TryGetValue("major", out var m) ? m.ToLowerInvariant() : "";
        var targetCareers = profile.ExtractedFields.TryGetValue("targetCareers", out var tc) ? tc.ToLowerInvariant() : "";

        var combined = $"{degree} {specialization} {major} {targetCareers}";

        foreach (var kv in DomainKeywords)
        {
            if (combined.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;
        }

        // Infer from target career names
        if (combined.Contains("soc") || combined.Contains("pentest") || combined.Contains("forensics") ||
            combined.Contains("cybersec") || combined.Contains("security analyst"))
            return "Technology";

        if (combined.Contains("general medicine") || combined.Contains("surgery") || combined.Contains("pediatrics"))
            return "Healthcare";

        return ""; // Unknown — don't filter
    }

    // ── Domain-Based Question Filtering ──────────────────────────────────────

    private static List<QuizQuestionV2> FilterByAcademicDomain(
        List<QuizQuestionV2> questions, string academicDomain, string phaseKey, AssessmentContext ctx = null)
    {
        // ── STRICT FILTERING FOR AFTER 12TH PCM (ENGINEERING) ──
        if (ctx != null && phaseKey.Equals("After12th", StringComparison.OrdinalIgnoreCase) && 
            ctx.DegreeIntent is "BE_BTECH" or "CONSIDERING_BE_BTECH")
        {
            // Lock to Engineering domain
            var engMatches = questions.Where(q => 
                (!string.IsNullOrEmpty(q.Domain) && q.Domain.Equals("Engineering", StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(q.Domain) && q.Domain.Equals("Technology", StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // If a specific field is locked (mid-quiz)
            if (!string.IsNullOrEmpty(ctx.LockedDomain))
            {
                var lockedMatches = engMatches.Where(q => 
                    !string.IsNullOrEmpty(q.Subdomain) && 
                    q.Subdomain.Equals(ctx.LockedDomain, StringComparison.OrdinalIgnoreCase)
                ).ToList();
                
                return lockedMatches;
            }
            // If the user has a preferred set of engineering fields
            else if (ctx.PreferredEngineeringFields.Any())
            {
                var prefMatches = engMatches.Where(q => 
                    !string.IsNullOrEmpty(q.Subdomain) && 
                    ctx.PreferredEngineeringFields.Contains(q.Subdomain, StringComparer.OrdinalIgnoreCase)
                ).ToList();

                // If user selected preferred fields, we MUST strictly bound them to those fields.
                // Do NOT fallback to other engineering branches if we run out of questions.
                return prefMatches;
            }

            return engMatches.Any() ? engMatches : questions.Where(q => q.PhaseKey.Equals(phaseKey, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // ── STRICT FILTERING FOR AFTER GRADUATION ──
        if (ctx != null && phaseKey.Equals("AfterGraduation", StringComparison.OrdinalIgnoreCase))
        {
            if (!ctx.CareerChange && ctx.PreferenceMode.Equals("SELECTED", StringComparison.OrdinalIgnoreCase) && ctx.SelectedFields.Any())
            {
                // A deliberate target selection takes precedence over the user's
                // current major. For example, a Mechanical Engineering graduate
                // who selected Aerospace should receive Aerospace questions.
                return questions.Where(q => ctx.SelectedFields.Any(field =>
                    FieldMatches(field, q.Subdomain) || FieldMatches(field, q.Domain))).ToList();
            }

            if (!ctx.CareerChange && ctx.EligibleDomains.Any())
            {
                // Strict filter: only return questions in the user's EligibleDomains
                var gradMatches = questions.Where(q =>
                    !string.IsNullOrEmpty(q.Domain) && 
                    ctx.EligibleDomains.Contains(q.Domain, StringComparer.OrdinalIgnoreCase)).ToList();
                
                return gradMatches;
            }
        }

        // ── STRICT FILTERING FOR AFTER POST-GRADUATION ──
        if (ctx != null && phaseKey.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase))
        {
            if (!ctx.CareerChange && ctx.EligibleDomains.Any())
            {
                // Strict filter: only return questions in the user's EligibleDomains
                var pgMatches = questions.Where(q =>
                    !string.IsNullOrEmpty(q.Domain) && 
                    ctx.EligibleDomains.Contains(q.Domain, StringComparer.OrdinalIgnoreCase)).ToList();
                
                // If the user explicitly provided targets, we should ideally restrict further.
                // However, the Domain filter prevents M.Tech CSE from getting Physics questions.
                return pgMatches;
            }
        }

        if (string.IsNullOrEmpty(academicDomain))
        {
            // No domain known — return phase-aligned questions only
            return questions.Where(q =>
                !string.IsNullOrEmpty(q.PhaseKey) && q.PhaseKey.Equals(phaseKey, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Strict filter: only return questions in the user's academic domain
        var domainMatches = questions.Where(q =>
            !string.IsNullOrEmpty(q.Domain) && q.Domain.Equals(academicDomain, StringComparison.OrdinalIgnoreCase)).ToList();



        if (domainMatches.Any())
            return domainMatches;

        // If no domain-specific questions exist, return phase-aligned questions (legacy behavior for non-AfterGraduation)
        return questions.Where(q =>
            !string.IsNullOrEmpty(q.PhaseKey) && q.PhaseKey.Equals(phaseKey, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private static bool FieldMatches(string selectedField, string questionField)
    {
        if (string.IsNullOrWhiteSpace(selectedField) || string.IsNullOrWhiteSpace(questionField)) return false;
        static string Normalize(string value) => new(value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray());
        var selected = Normalize(selectedField);
        var question = Normalize(questionField);
        selected = selected.Replace("engineering", string.Empty, StringComparison.Ordinal);
        question = question.Replace("engineering", string.Empty, StringComparison.Ordinal);
        return selected.Length > 0 && question.Length > 0 &&
            (selected == question || selected.Contains(question, StringComparison.Ordinal) || question.Contains(selected, StringComparison.Ordinal));
    }

    // ── Target Career Discrimination ──────────────────────────────────────────

    private List<string> ResolveTargetCareers(List<string> targetCareerNames)
    {
        var resolved = new List<string>();
        foreach (var name in targetCareerNames)
        {
            // Try exact match first
            if (CareerNameToNodeIds.TryGetValue(name.Trim(), out var ids))
            {
                resolved.AddRange(ids);
            }
            else
            {
                // Try partial match
                var matchKey = CareerNameToNodeIds.Keys
                    .FirstOrDefault(k => name.Contains(k, StringComparison.OrdinalIgnoreCase)
                                      || k.Contains(name.Trim(), StringComparison.OrdinalIgnoreCase));
                if (matchKey != null)
                    resolved.AddRange(CareerNameToNodeIds[matchKey]);
                else
                    // Use the name as-is (might already be a taxonomy ID)
                    resolved.Add(name.Trim());
            }
        }
        return resolved.Distinct().ToList();
    }

    private QuizQuestionV2? SelectByTargetDiscrimination(
        List<QuizQuestionV2> pool, List<string> targetNodeIds, Dictionary<string, double> prior)
    {
        var targetSet = new HashSet<string>(targetNodeIds, StringComparer.OrdinalIgnoreCase);

        // Find questions that specifically discriminate between these target careers
        var targetedQuestions = pool.Where(q =>
        {
            if (string.IsNullOrWhiteSpace(q.TargetDiscriminatesJson) || q.TargetDiscriminatesJson == "[]")
                return false;
            try
            {
                var targets = JsonSerializer.Deserialize<List<string>>(q.TargetDiscriminatesJson, _jsonOpts) ?? new();
                return targets.Any(t => targetSet.Contains(t));
            }
            catch { return false; }
        }).ToList();

        if (!targetedQuestions.Any())
            return null;

        // Find the two leading target careers to maximize discrimination
        var priorOrderedTargets = prior
            .Where(kv => targetSet.Contains(kv.Key))
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .ToList();

        string topCareer = priorOrderedTargets.FirstOrDefault() ?? targetNodeIds.First();
        string secondCareer = priorOrderedTargets.Skip(1).FirstOrDefault() ?? targetNodeIds.Last();

        return targetedQuestions
            .OrderByDescending(q => CalculateDiscriminationPower(q, topCareer, secondCareer))
            .ThenBy(q => q.DisplayOrder)
            .First();
    }

    private static double CalculateDiscriminationPower(QuizQuestionV2 q, string careerA, string careerB)
    {
        if (string.IsNullOrWhiteSpace(q.OptionsJson)) return 0;
        try
        {
            using var doc = JsonDocument.Parse(q.OptionsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return 0;

            double maxWeightA = 0, maxWeightB = 0;
            foreach (var opt in doc.RootElement.EnumerateArray())
            {
                if (opt.TryGetProperty("signals", out var signalsEl) && signalsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var s in signalsEl.EnumerateArray())
                    {
                        string nodeId = s.GetProperty("nodeId").GetString() ?? "";
                        double weight = s.GetProperty("weight").GetDouble();
                        if (nodeId.Equals(careerA, StringComparison.OrdinalIgnoreCase))
                            maxWeightA = Math.Max(maxWeightA, weight);
                        if (nodeId.Equals(careerB, StringComparison.OrdinalIgnoreCase))
                            maxWeightB = Math.Max(maxWeightB, weight);
                    }
                }
            }
            return Math.Abs(maxWeightA - maxWeightB);
        }
        catch { return 0; }
    }

    private static double CalculateExpectedInformationGain(QuizQuestionV2 q, Dictionary<string, double> prior)
    {
        double currentEntropy = ShannonEntropy(prior);
        if (string.IsNullOrWhiteSpace(q.OptionsJson)) return 0;
        
        try
        {
            using var doc = JsonDocument.Parse(q.OptionsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return 0;
            
            double expectedPostEntropy = 0.0;
            int numOptions = doc.RootElement.GetArrayLength();
            if (numOptions == 0) return 0;

            foreach (var opt in doc.RootElement.EnumerateArray())
            {
                var simPrior = new Dictionary<string, double>(prior);
                if (opt.TryGetProperty("signals", out var signalsEl) && signalsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var s in signalsEl.EnumerateArray())
                    {
                        string nodeId = s.GetProperty("nodeId").GetString() ?? "";
                        double weight = s.GetProperty("weight").GetDouble();
                        
                        if (simPrior.ContainsKey(nodeId))
                        {
                            simPrior[nodeId] *= (1.0 + weight);
                        }
                        else if (!nodeId.StartsWith("reject."))
                        {
                            simPrior[nodeId] = weight * 0.0001; // EPSILON
                        }
                    }
                }
                
                // Smoothing
                foreach (var key in simPrior.Keys.ToList())
                {
                    simPrior[key] = Math.Max(simPrior[key], 0.0001);
                }
                
                // Normalize
                double total = simPrior.Values.Sum();
                if (total > 0)
                {
                    foreach (var key in simPrior.Keys.ToList()) simPrior[key] /= total;
                }
                
                expectedPostEntropy += ShannonEntropy(simPrior) / numOptions;
            }
            return currentEntropy - expectedPostEntropy;
        }
        catch { return 0; }
    }

    private static double ShannonEntropy(Dictionary<string, double> distribution)
    {
        double entropy = 0.0;
        foreach (var p in distribution.Values)
        {
            if (p > 0) entropy -= p * Math.Log2(p);
        }
        return entropy;
    }

    private static QuizQuestionV2 SelectByInformationGain(List<QuizQuestionV2> pool, Dictionary<string, double> prior)
    {
        // Select the question whose options provide maximum entropy reduction (Information Gain)
        return pool.OrderByDescending(q => CalculateExpectedInformationGain(q, prior))
                   .ThenBy(q => q.DisplayOrder)
                   .First();
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        var t = text.ToLowerInvariant();
        t = t.Replace("(scenario 2)", "")
             .Replace("(scenario 3)", "")
             .Replace("scenario 2", "")
             .Replace("scenario 3", "")
             .Replace("question 2", "")
             .Replace("question 3", "");
        return new string(t.Where(c => char.IsLetterOrDigit(c)).ToArray());
    }

    private static string GetOptionSignature(QuizQuestionV2 q)
    {
        if (string.IsNullOrWhiteSpace(q.OptionsJson)) return "";
        try 
        {
            var options = JsonSerializer.Deserialize<List<JsonElement>>(q.OptionsJson, _jsonOpts) ?? new();
            var texts = options.Select(o => o.TryGetProperty("text", out var t) ? NormalizeText(t.GetString() ?? "") : "").OrderBy(t => t);
            return string.Join("|", texts);
        }
        catch { return ""; }
    }
}
