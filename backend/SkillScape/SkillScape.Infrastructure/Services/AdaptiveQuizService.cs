using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

// ─────────────────────────────────────────────────────────────────────────────
// ADAPTIVE QUIZ SERVICE V2
//
// Algorithm Overview:
//
//   1. Initialize: uniform prior probability distribution over all taxonomy
//      nodes applicable to the phase.
//
//   2. Eligibility Filtering: if a hard constraint is violated by the student's
//      profile (e.g. no Physics in stream → cannot recommend Engineering),
//      set that node's probability to 0.0 and renormalize.
//
//   3. Per Answer: Bayesian update using option signal weights.
//      P'(node) = P(node) × signalWeight(selectedOption, node) + ε  (smoothing)
//      Then normalize so ∑P'(node) = 1.0.
//
//   4. Next Question: information-gain-based selection.
//      For each remaining question, compute expected entropy reduction.
//      Select question with highest expected information gain.
//      Specifically prefer questions that discriminate between top-2 nodes.
//
//   5. Result: node with highest probability is the primary recommendation.
//      Confidence = 1 - H(distribution) / H_max, where H is Shannon entropy.
// ─────────────────────────────────────────────────────────────────────────────

public class AdaptiveQuizService : IAdaptiveQuizService
{
    private readonly ApplicationDbContext _context;
    private readonly IQuestionGenerationService _questionGenerationService;
    private readonly IDuplicateDetector _duplicateDetector;
    private readonly IProfileParserService _profileParserService;
    private readonly IGoalValidationEngine _goalValidationEngine;
    private readonly IQuizMlPredictionService _mlPredictionService;
    private readonly IDynamicRoadmapEngine _roadmapEngine;
    private readonly IDynamicLlmQuestionGenerator _llmGenerator;
    private readonly IMultiDimensionalScoringEngine _scoringEngine;
    private readonly IDeterministicQuestionSelector _deterministicSelector;
    private readonly QuizStageEngine _stageEngine = new();
    private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    // Smoothing constant to prevent zero-probability collapse
    private const double EPSILON = 0.0001;

    // Keep enough answers for stable recommendations across every education stage.
    private const int MIN_QUESTIONS = 12;
    private const int MAX_QUESTIONS = 15;
    private const int MAX_PROFILE_FALLBACK_QUESTIONS = 20;

    // Legacy stopping constants — still used by ShouldStop() for non-AfterGrad phases
    private const double ENTROPY_THRESHOLD = 0.3;
    private const double MIN_LEAD_GAP = 0.30;

    private readonly IAssessmentBlueprintService _blueprintService;
    private readonly IProfileMapperService _profileMapperService;
    private readonly IProfileValidationEngine _profileValidator;
    private readonly IRoadmapCurriculumGeneratorService? _curriculumGenerator;

    public AdaptiveQuizService(
        ApplicationDbContext context,
        IQuestionGenerationService questionGenerationService,
        IDuplicateDetector duplicateDetector,
        IProfileParserService? profileParserService = null,
        IGoalValidationEngine? goalValidationEngine = null,
        IQuizMlPredictionService? mlPredictionService = null,
        IDynamicRoadmapEngine? roadmapEngine = null,
        IDynamicLlmQuestionGenerator? llmGenerator = null,
        IMultiDimensionalScoringEngine? scoringEngine = null,
        IAssessmentBlueprintService? blueprintService = null,
        IDeterministicQuestionSelector? deterministicSelector = null,
        IProfileMapperService? profileMapperService = null,
        IProfileValidationEngine? profileValidator = null,
        IRoadmapCurriculumGeneratorService? curriculumGenerator = null)
    {
        _context = context;
        _questionGenerationService = questionGenerationService;
        _duplicateDetector = duplicateDetector;
        _profileParserService = profileParserService ?? new ProfileParserService();
        _goalValidationEngine = goalValidationEngine ?? new GoalValidationEngine(context);
        _mlPredictionService = mlPredictionService ?? new QuizMlPredictionService();
        _roadmapEngine = roadmapEngine ?? new DynamicRoadmapEngine();
        _llmGenerator = llmGenerator ?? throw new InvalidOperationException("ILlmQuestionGeneratorService must be injected via DI.");
        _scoringEngine = scoringEngine ?? new MultiDimensionalScoringEngine();
        _blueprintService = blueprintService ?? throw new InvalidOperationException("IAssessmentBlueprintService must be injected via DI.");
        _profileValidator = profileValidator ?? new ProfileValidationEngine(Microsoft.Extensions.Logging.Abstractions.NullLogger<ProfileValidationEngine>.Instance);
        _deterministicSelector = deterministicSelector ?? new DeterministicQuestionSelector(context, new QuestionValidationEngine(), _profileValidator, Microsoft.Extensions.Logging.Abstractions.NullLogger<DeterministicQuestionSelector>.Instance);
        _profileMapperService = profileMapperService ?? throw new InvalidOperationException("IProfileMapperService must be injected via DI.");
        _curriculumGenerator = curriculumGenerator;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public async Task<(AdaptiveQuizSessionDto Session, AdaptiveQuizNextResponse FirstQuestion)> StartSessionAsync(
        string userId,
        AdaptiveQuizStartRequest request)
    {
        try
        {
            var profile = request.Profile ?? new PhaseProfileInputDto();
            string subPhaseKey = profile.SubPhaseKey ?? string.Empty;
            var rawFields = profile.Fields ?? new Dictionary<string, string>();

            // 1. Parse Natural Language Profile (Current Status + Primary Goal)
            var parsedProfile = _profileParserService.ParseProfile(
                request.PhaseKey,
                rawFields.TryGetValue("currentStatus", out var cs) ? cs : null,
                rawFields.TryGetValue("primaryGoal", out var pg) ? pg : null,
                rawFields);

            // 1b. Map Initial Academic Profile to Assessment Context
            var assessmentContext = _profileMapperService.MapProfile(profile);

            // 2. Load all taxonomy nodes applicable to this phase and strictly filtered by profile.
            // Goal alignment was calculated here but never used by the quiz response; when the
            // goal was a weak fit it also performed an extra taxonomy query before returning the
            // first question. Keep goal validation for result flows that consume it, not startup.
            var nodes = await LoadApplicableTaxonomyNodesAsync(request.PhaseKey, subPhaseKey, parsedProfile, assessmentContext);

            // 4. Initialize uniform probability distribution
            var prior = InitializeUniformPrior(nodes.Select(n => n.Id).ToList());

            // Boost target domain in prior if goal parsing identified a target domain
            if (!string.IsNullOrEmpty(parsedProfile.PrimaryGoal.TargetDomain) && prior.ContainsKey(parsedProfile.PrimaryGoal.TargetDomain))
            {
                prior[parsedProfile.PrimaryGoal.TargetDomain] *= 2.5;
            }

            // 5. Apply hard eligibility constraints from profile
            var constraints = await _context.EligibilityConstraintsV2
                .AsNoTracking()
                .Where(c => c.PhaseKey == request.PhaseKey || c.PhaseKey == string.Empty)
                .ToListAsync();

            ApplyEligibilityConstraints(prior, constraints, parsedProfile.ExtractedFields);

            Normalize(prior);

            var targetCareerList = new List<string>();
            if (rawFields.TryGetValue("targetCareers", out var tcStr) && !string.IsNullOrWhiteSpace(tcStr))
            {
                targetCareerList = tcStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            }
            // In discovery mode, targetCareerList remains empty so we can dynamically score all domains or use subdomain scoring.

            // 6. Create session
            var session = new AdaptiveQuizSession
            {
                UserId = userId,
                AssessmentType = string.IsNullOrWhiteSpace(request.AssessmentType) ? request.PhaseKey : request.AssessmentType,
                PhaseKey = request.PhaseKey,
                SubPhaseKey = subPhaseKey,
                BackgroundProfileJson = JsonSerializer.Serialize(parsedProfile.ExtractedFields),
                ProbabilityDistributionJson = JsonSerializer.Serialize(prior),
                AnsweredQuestionIdsJson = "[]",
                AnswersJson = "[]",
                StateJson = "{}",
                Status = "Active",
                Mode = targetCareerList.Any() ? "TargetCareer" : "Discovery",
                TargetCareersJson = JsonSerializer.Serialize(targetCareerList),
                AssessmentContextJson = JsonSerializer.Serialize(assessmentContext),
                StartedAt = DateTime.UtcNow
            };

            _context.AdaptiveQuizSessions.Add(session);
            await _context.SaveChangesAsync();

            // 8. Select and return first question using fallback hierarchy
            var firstQuestion = await SelectNextQuestionAsync(session, request.PhaseKey, subPhaseKey, new List<string>(), prior, parsedProfile, new List<EvidenceItemDto>(), new List<string>());
            
            if (firstQuestion == null)
            {
                throw new InvalidOperationException($"No questions available for this profile (Phase: {request.PhaseKey}, SubPhase: {subPhaseKey}). Please check question bank seeding and profile constraints.");
            }

            var initialState = new Dictionary<string, object>();
            initialState["shownQuestionIds"] = new List<string> { firstQuestion.Id };
            if (firstQuestion.Id.StartsWith("FB_"))
            {
                initialState["usedFallbackIds"] = new List<string> { firstQuestion.Id };
            }
            session.StateJson = JsonSerializer.Serialize(initialState);
            await _context.SaveChangesAsync();

            var plan = string.IsNullOrEmpty(session.QuestionPlanJson) ? new List<AssessmentSlot>() : JsonSerializer.Deserialize<List<AssessmentSlot>>(session.QuestionPlanJson, _jsonOpts) ?? new();
            int totalQuestions = plan.Count > 0 ? plan.Count : 15;

            var sessionDto = MapSessionToDto(session);
            var nextResponse = MapQuestionToResponse(firstQuestion, 1, totalQuestions, prior);

            return (sessionDto, nextResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[FATAL ERROR in StartSessionAsync]: {ex}\n\n");
            throw;
        }
    }

    public async Task<AdaptiveQuizNextResponse> AnswerAndGetNextAsync(
        string userId,
        AdaptiveQuizAnswerRequest request)
    {
        AdaptiveQuizSession? answerCommittedSession = null;
        Dictionary<string, double>? committedProbabilityDistribution = null;
        var committedAnswerCount = 0;
        try
        {
            var session = await _context.AdaptiveQuizSessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == userId
                && (s.Status == "Active" || s.Status == "ReadyToSubmit"))
            ?? throw new KeyNotFoundException("Active quiz session not found.");

        // Deserialize current state
        var prior = JsonSerializer.Deserialize<Dictionary<string, double>>(session.ProbabilityDistributionJson, _jsonOpts)
            ?? throw new InvalidOperationException("Invalid probability distribution in session.");
        var answeredIds = JsonSerializer.Deserialize<List<string>>(session.AnsweredQuestionIdsJson, _jsonOpts) ?? new();
        var answers = JsonSerializer.Deserialize<List<object>>(session.AnswersJson, _jsonOpts) ?? new();

        var stateObjRead = JsonSerializer.Deserialize<Dictionary<string, object>>(session.StateJson ?? "{}", _jsonOpts) ?? new Dictionary<string, object>();
        var shownIds = stateObjRead.TryGetValue("shownQuestionIds", out var shownObj) && shownObj is JsonElement shownElement && shownElement.ValueKind == JsonValueKind.Array
            ? shownElement.EnumerateArray().Select(x => x.GetString() ?? "").Where(id => !string.IsNullOrEmpty(id)).ToList()
            : new List<string>();
        if (!shownIds.Contains(request.QuestionId, StringComparer.OrdinalIgnoreCase))
            shownIds.Add(request.QuestionId);
        stateObjRead["shownQuestionIds"] = shownIds;
        session.StateJson = JsonSerializer.Serialize(stateObjRead);

        if (answeredIds.Contains(request.QuestionId, StringComparer.OrdinalIgnoreCase))
        {
            // The user went to 'Previous' and changed their answer.
            // Ideally we'd replay the session for exact Bayesian math, but for now we just remove the old answer entry to allow the update.
            answeredIds.RemoveAll(id => id.Equals(request.QuestionId, StringComparison.OrdinalIgnoreCase));
            var oldAnswers = JsonSerializer.Deserialize<List<SessionAnswer>>(session.AnswersJson, _jsonOpts) ?? new();
            oldAnswers.RemoveAll(a => a.QuestionId.Equals(request.QuestionId, StringComparison.OrdinalIgnoreCase));
            session.AnswersJson = JsonSerializer.Serialize(oldAnswers);
        }

        // Look up the question by ID only — PhaseKey filter is removed because domain-specific
        // question banks (Cybersecurity, Medical, etc.) share the same PhaseKey but serve
        // different academic domains, and their IDs are globally unique.
        var question = await _context.QuizQuestionsV2
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId);

        // Recover an in-progress BA session created before curated questions were
        // persisted at selection time. The question ID is stable, so rebuild the
        // same question, persist it, and allow this submitted answer to proceed.
        if (question == null && request.QuestionId.StartsWith("FB_", StringComparison.OrdinalIgnoreCase))
        {
            var recoveryContext = string.IsNullOrWhiteSpace(session.AssessmentContextJson) || session.AssessmentContextJson == "{}"
                ? new AssessmentContext()
                : JsonSerializer.Deserialize<AssessmentContext>(session.AssessmentContextJson, _jsonOpts) ?? new AssessmentContext();
            question = FieldSpecificQuestionBank.GetQuestionById(request.QuestionId, session.PhaseKey, recoveryContext.SelectedFields);
            if (question != null)
            {
                _context.QuizQuestionsV2.Add(question);
                await _context.SaveChangesAsync();
            }
        }

        if (question == null)
            throw new KeyNotFoundException($"Question '{request.QuestionId}' not found in the question bank. Ensure the question bank is seeded correctly.");

        // Parse option signals from JSON
        var optionsData = ParseOptionsJson(question.OptionsJson);

        // Bayesian update
        if (request.OptionIndex < 0 || request.OptionIndex >= optionsData.Count)
            throw new ArgumentOutOfRangeException(nameof(request.AnswerId), "The selected answer is not valid for this question.");

        var opt = optionsData[request.OptionIndex];
        if (opt.IsRejection)
        {
            var rejectedDomainsList = new List<string>();
            var rejectionState = JsonSerializer.Deserialize<Dictionary<string, object>>(session.StateJson ?? "{}", _jsonOpts) ?? new Dictionary<string, object>();
            if (rejectionState.TryGetValue("rejectedDomains", out var rejObj1) && rejObj1 is JsonElement je && je.ValueKind == JsonValueKind.Array)
            {
                rejectedDomainsList = je.EnumerateArray().Select(x => x.GetString() ?? "").ToList();
            }
            
            if (!string.IsNullOrEmpty(opt.Domain) && !rejectedDomainsList.Contains(opt.Domain, StringComparer.OrdinalIgnoreCase))
            {
                rejectedDomainsList.Add(opt.Domain);
            }
            if (!string.IsNullOrEmpty(opt.Subdomain) && !rejectedDomainsList.Contains(opt.Subdomain, StringComparer.OrdinalIgnoreCase))
            {
                rejectedDomainsList.Add(opt.Subdomain);
            }

            // Map "Cybersecurity" to all related subdomains so they get rejected properly
            if (rejectedDomainsList.Contains("Cybersecurity", StringComparer.OrdinalIgnoreCase) || 
                (opt.Domain != null && opt.Domain.Equals("Cybersecurity", StringComparison.OrdinalIgnoreCase)))
            {
                var cybAliases = new[] { 
                    "Security Operations", "Offensive Security", "Defensive Security", 
                    "Governance Risk Compliance", "Incident Response", "Application Security", 
                    "Cloud Security", "Digital Forensics", "Threat Intelligence" 
                };
                foreach(var alias in cybAliases)
                {
                    if (!rejectedDomainsList.Contains(alias, StringComparer.OrdinalIgnoreCase))
                    {
                        rejectedDomainsList.Add(alias);
                    }
                }
            }

            rejectionState["rejectedDomains"] = rejectedDomainsList;
            session.StateJson = JsonSerializer.Serialize(rejectionState);
        }

        var signals = optionsData[request.OptionIndex].Signals;

        // The answer and Bayesian state are the essential quiz data. Keep them
        // recordable even if optional career/subdomain scoring encounters a bad
        // signal in a legacy question row.
        try
        {
            await _scoringEngine.UpdateScoresAsync(session, question, request.OptionIndex);
        }
        catch (Exception scoringError)
        {
            Console.WriteLine($"[AdaptiveQuiz] Supplemental score update failed for {question.Id}; recording answer with Bayesian signals. {scoringError}");
        }
        
        var stateObj = JsonSerializer.Deserialize<Dictionary<string, object>>(session.StateJson ?? "{}", _jsonOpts) ?? new Dictionary<string, object>();
        var rejectedDomains = new List<string>();
        if (stateObj.TryGetValue("rejectedDomains", out var rejObj) && rejObj is JsonElement jeRej && jeRej.ValueKind == JsonValueKind.Array)
            rejectedDomains = jeRej.EnumerateArray().Select(x => x.GetString() ?? "").ToList();

        foreach (var sig in signals)
        {
            if (sig.NodeId.StartsWith("reject.", StringComparison.OrdinalIgnoreCase))
            {
                var rejDomain = sig.NodeId.Substring(7);
                if (!rejectedDomains.Contains(rejDomain)) rejectedDomains.Add(rejDomain);
                // Zero out the prior for this domain and all its subdomains
                foreach (var key in prior.Keys.ToList())
                {
                    if (key.Equals(rejDomain, StringComparison.OrdinalIgnoreCase) || key.StartsWith(rejDomain + ".", StringComparison.OrdinalIgnoreCase))
                    {
                        prior[key] = 0.0;
                    }
                }
            }
        }
        stateObj["rejectedDomains"] = rejectedDomains;
        session.StateJson = JsonSerializer.Serialize(stateObj);

        BayesianUpdate(prior, signals);
        Normalize(prior);

        // Record answer
        answeredIds.Add(request.QuestionId);
        var newAnswer = new SessionAnswer { QuestionId = request.QuestionId, OptionIndex = request.OptionIndex };
        var answersList = JsonSerializer.Deserialize<List<SessionAnswer>>(session.AnswersJson, _jsonOpts) ?? new();
        answersList.Add(newAnswer);
        
        // Persist updated state
        session.ProbabilityDistributionJson = JsonSerializer.Serialize(prior);
        session.AnsweredQuestionIdsJson = JsonSerializer.Serialize(answeredIds);
        session.AnswersJson = JsonSerializer.Serialize(answersList);

        // ── FIELD LOCKING LOGIC (AFTER 12TH PCM) ──
        if (session.PhaseKey == "After12th")
        {
            var ctx = string.IsNullOrEmpty(session.AssessmentContextJson) ? new AssessmentContext() 
                : JsonSerializer.Deserialize<AssessmentContext>(session.AssessmentContextJson, _jsonOpts) ?? new();

            if (ctx.DegreeIntent is "BE_BTECH" or "CONSIDERING_BE_BTECH" && string.IsNullOrEmpty(ctx.LockedDomain))
            {
                var subScoresRaw = string.IsNullOrEmpty(session.SubdomainScoresJson) 
                    ? new Dictionary<string, dynamic>() 
                    : JsonSerializer.Deserialize<Dictionary<string, dynamic>>(session.SubdomainScoresJson, _jsonOpts) ?? new();

                var subdomainStats = new List<(string Name, double Score, int Count)>();
                foreach (var kvp in subScoresRaw)
                {
                    try
                    {
                        var el = (JsonElement)kvp.Value;
                        double rawScore = el.TryGetProperty("RawScore", out var rs) ? rs.GetDouble() : 0;
                        double maxScore = el.TryGetProperty("MaxPossibleScore", out var ms) ? ms.GetDouble() : 0;
                        int count = el.TryGetProperty("EvidenceCount", out var ec) ? ec.GetInt32() : 0;
                        
                        if (maxScore > 0 && count > 0)
                        {
                            subdomainStats.Add((kvp.Key, rawScore / maxScore, count));
                        }
                    }
                    catch { }
                }

                if (subdomainStats.Any())
                {
                    var sorted = subdomainStats.OrderByDescending(s => s.Score).ToList();
                    var top = sorted.First();
                    
                    int minFieldQuestions = 4; // Configurable
                    double confidenceThreshold = 0.75; // Configurable
                    double minScoreMargin = 0.10; // Configurable

                    if (top.Count >= minFieldQuestions && top.Score >= confidenceThreshold)
                    {
                        bool isClearWinner = sorted.Count == 1 || (top.Score - sorted[1].Score >= minScoreMargin);
                        if (isClearWinner)
                        {
                            ctx.LockedDomain = top.Name;
                            session.AssessmentContextJson = JsonSerializer.Serialize(ctx, _jsonOpts);
                            Console.WriteLine($"[AdaptiveQuizService] Locked Engineering Field: {top.Name} with score {top.Score:P} after {top.Count} questions.");
                        }
                    }
                }
            }
        }

        // Update intents
        var stateObjUpdate = JsonSerializer.Deserialize<Dictionary<string, object>>(session.StateJson ?? "{}", _jsonOpts) ?? new Dictionary<string, object>();
        var currentIntents = new List<string>();
        if (stateObjUpdate.TryGetValue("askedIntents", out var intentsObjUpdate) && intentsObjUpdate is JsonElement jeUpdate && jeUpdate.ValueKind == JsonValueKind.Array)
        {
            currentIntents = jeUpdate.EnumerateArray().Select(x => x.GetString() ?? "").ToList();
        }
        if (!string.IsNullOrEmpty(question.Intent))
        {
            currentIntents.Add(question.Intent);
            stateObjUpdate["askedIntents"] = currentIntents;
            session.StateJson = JsonSerializer.Serialize(stateObjUpdate);
        }

        // Commit the submitted answer before doing any work to generate the next
        // question. If candidate selection or duplicate checking fails afterward,
        // the learner's answer is still safely recorded.
        _context.AdaptiveQuizSessions.Update(session);
        await _context.SaveChangesAsync();
        answerCommittedSession = session;
        committedProbabilityDistribution = new Dictionary<string, double>(prior, StringComparer.OrdinalIgnoreCase);
        committedAnswerCount = answeredIds.Count;

        // Determine if we should stop
        bool shouldStop = ShouldStop(session, answeredIds.Count);

        AdaptiveQuizNextResponse response;
        if (shouldStop)
        {
            session.Status = "ReadyToSubmit";
            response = new AdaptiveQuizNextResponse
            {
                IsComplete = true,
                QuestionNumber = answeredIds.Count,
                EstimatedRemaining = 0,
                CurrentLeadingDomainNames = GetTopDomainNames(prior, 3)
            };
        }
        else
        {
            var profileFieldsNext = JsonSerializer.Deserialize<Dictionary<string, string>>(session.BackgroundProfileJson, _jsonOpts) ?? new();
            var parsedProfileNext = new ParsedProfileDto { ExtractedFields = profileFieldsNext };
            parsedProfileNext.Education.Specialization = profileFieldsNext.TryGetValue("specialization", out var s2) ? s2 : "";
            
            // Build evidence items and texts for LLM contextualization
            var nodeMap = await _context.CareerTaxonomyV2.ToDictionaryAsync(n => n.Id, n => n);
            var evidenceItems = await BuildEvidenceLogAsync(answersList, prior, nodeMap);
            var answeredTexts = evidenceItems.Select(e => e.QuestionText).ToList();

            // Extract asked intents, current stage, and used fallback IDs from state
            var askedIntents = new List<string>();
            if (stateObjRead.TryGetValue("askedIntents", out var intentsObj) && intentsObj is JsonElement je && je.ValueKind == JsonValueKind.Array)
                askedIntents = je.EnumerateArray().Select(x => x.GetString() ?? "").ToList();

            string savedStage = stateObjRead.TryGetValue("currentStage", out var stageObjReadVal) && stageObjReadVal is JsonElement jeStage
                ? jeStage.GetString() ?? string.Empty : string.Empty;

            var usedFallbackIds = new List<string>();
            if (stateObjRead.TryGetValue("usedFallbackIds", out var fbObj) && fbObj is JsonElement jeFb && jeFb.ValueKind == JsonValueKind.Array)
                usedFallbackIds = jeFb.EnumerateArray().Select(x => x.GetString() ?? "").ToList();

            var rejectedDomainsList = new List<string>();
            if (stateObjRead.TryGetValue("rejectedDomains", out var rejObjList) && rejObjList is JsonElement jeRejList && jeRejList.ValueKind == JsonValueKind.Array)
                rejectedDomainsList = jeRejList.EnumerateArray().Select(x => x.GetString() ?? "").ToList();

            QuizQuestionV2 nextQuestion;
            try
            {
                nextQuestion = await SelectNextQuestionAsync(
                    session,
                    session.PhaseKey, session.SubPhaseKey, answeredIds, prior,
                    parsedProfileNext, evidenceItems, answeredTexts, askedIntents,
                    savedStage, usedFallbackIds, rejectedDomainsList, shownIds);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("QUESTION_GENERATION_EXHAUSTED"))
            {
                // The field selector can exhaust a stage before the assessment's
                // minimum is reached. Keep MBBS Explore sessions going with an
                // unused medical question instead of returning a result after only
                // a few answers.
                var continuationMajor = ResolveProfileMajor(session, parsedProfileNext);
                if (session.PhaseKey.Equals("AfterGraduation", StringComparison.OrdinalIgnoreCase) &&
                    answeredIds.Count < MIN_QUESTIONS &&
                    FieldSpecificQuestionBank.IsMedicalProfile(continuationMajor))
                {
                    var medicalQuestion = FieldSpecificQuestionBank.GetUnusedMedicalQuestion(
                        session.PhaseKey, shownIds.Concat(answeredIds));
                    if (medicalQuestion != null)
                    {
                        var storedQuestion = await _context.QuizQuestionsV2
                            .FirstOrDefaultAsync(question => question.Id == medicalQuestion.Id);
                        if (storedQuestion == null)
                            _context.QuizQuestionsV2.Add(medicalQuestion);
                        else
                            medicalQuestion = storedQuestion;

                        if (!shownIds.Contains(medicalQuestion.Id, StringComparer.OrdinalIgnoreCase))
                            shownIds.Add(medicalQuestion.Id);
                        if (!usedFallbackIds.Contains(medicalQuestion.Id, StringComparer.OrdinalIgnoreCase))
                            usedFallbackIds.Add(medicalQuestion.Id);
                        stateObjRead["shownQuestionIds"] = shownIds;
                        stateObjRead["usedFallbackIds"] = usedFallbackIds;
                        session.StateJson = JsonSerializer.Serialize(stateObjRead);
                        session.Status = "Active";
                        _context.AdaptiveQuizSessions.Update(session);
                        await _context.SaveChangesAsync();

                        var continuationPlan = string.IsNullOrEmpty(session.QuestionPlanJson)
                            ? new List<AssessmentSlot>()
                            : JsonSerializer.Deserialize<List<AssessmentSlot>>(session.QuestionPlanJson, _jsonOpts) ?? new();
                        int continuationTotal = continuationPlan.Count > 0 ? continuationPlan.Count : MAX_QUESTIONS;
                        return MapQuestionToResponse(medicalQuestion, answeredIds.Count + 1, continuationTotal, prior);
                    }
                }

                // For other phases, or once the assessment minimum is reached,
                // completing is still the right response when no suitable item remains.
                session.Status = "ReadyToSubmit";
                _context.AdaptiveQuizSessions.Update(session);
                await _context.SaveChangesAsync();

                return new AdaptiveQuizNextResponse
                {
                    IsComplete = true,
                    QuestionNumber = answeredIds.Count,
                    EstimatedRemaining = 0,
                    CurrentLeadingDomainNames = GetTopDomainNames(prior, 3)
                };
            }

            // Persist updated stage and fallback ID list
            var newStage = _stageEngine.GetCurrentStage(prior, answeredIds.Count, savedStage);
            stateObjRead["currentStage"] = newStage.ToString();
            if (nextQuestion.Id.StartsWith("FB_"))
                usedFallbackIds.Add(nextQuestion.Id);
            if (!shownIds.Contains(nextQuestion.Id, StringComparer.OrdinalIgnoreCase))
                shownIds.Add(nextQuestion.Id);
            stateObjRead["shownQuestionIds"] = shownIds;
            stateObjRead["usedFallbackIds"] = usedFallbackIds;
            session.StateJson = JsonSerializer.Serialize(stateObjRead);

            var plan = string.IsNullOrEmpty(session.QuestionPlanJson) ? new List<AssessmentSlot>() : JsonSerializer.Deserialize<List<AssessmentSlot>>(session.QuestionPlanJson, _jsonOpts) ?? new();
            int totalQuestions = plan.Count > 0 ? plan.Count : 15;
            
            response = MapQuestionToResponse(nextQuestion, answeredIds.Count + 1, totalQuestions, prior);
        }

            _context.AdaptiveQuizSessions.Update(session);
            await _context.SaveChangesAsync();

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[FATAL ERROR in AnswerAndGetNextAsync]: {ex}\n\n");
            if (answerCommittedSession != null && committedProbabilityDistribution != null)
            {
                // Do not tell the client the answer failed after it has been
                // committed. Complete the session so it can still produce its
                // recommendation from the answers already recorded.
                answerCommittedSession.Status = "ReadyToSubmit";
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception completionSaveError)
                {
                    Console.WriteLine($"[AdaptiveQuiz] Could not mark the committed session ready to submit: {completionSaveError}");
                }

                return new AdaptiveQuizNextResponse
                {
                    IsComplete = true,
                    QuestionNumber = committedAnswerCount,
                    EstimatedRemaining = 0,
                    CurrentLeadingDomainNames = GetTopDomainNames(committedProbabilityDistribution, 3)
                };
            }
            throw;
        }
    }

    public async Task<QuizV2ResultDto> SubmitSessionAsync(string userId, string sessionId)
    {
        var session = await _context.AdaptiveQuizSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId)
            ?? throw new KeyNotFoundException("Quiz session not found.");

        if (session.Status == "Completed")
        {
            // Return existing result
            var existingResult = await _context.QuizResultsV2.FirstOrDefaultAsync(r => r.SessionId == sessionId);
            if (existingResult != null) return await MapResultToDtoAsync(existingResult);
        }

        var prior = JsonSerializer.Deserialize<Dictionary<string, double>>(session.ProbabilityDistributionJson, _jsonOpts)
            ?? new Dictionary<string, double>();

        var answeredIds = JsonSerializer.Deserialize<List<string>>(session.AnsweredQuestionIdsJson, _jsonOpts) ?? new();
        var rawAnswers = JsonSerializer.Deserialize<List<SessionAnswer>>(session.AnswersJson, _jsonOpts) ?? new();
        var minimumRequiredQuestions =
            string.Equals(session.PhaseKey, "After12th", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(session.SubPhaseKey, "pcb", StringComparison.OrdinalIgnoreCase)
                ? 16
                : MIN_QUESTIONS;
        var validAnsweredIdCount = answeredIds
            .Where(questionId => !string.IsNullOrWhiteSpace(questionId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        if (validAnsweredIdCount < minimumRequiredQuestions ||
            rawAnswers.Count(answer => answer != null && !string.IsNullOrWhiteSpace(answer.QuestionId)) < minimumRequiredQuestions)
        {
            throw new InvalidOperationException(
                $"At least {minimumRequiredQuestions} quiz responses are required before submitting this assessment.");
        }

        // Get all taxonomy nodes to map IDs to names
        var allNodes = await _context.CareerTaxonomyV2.ToListAsync();
        var nodeMap = allNodes.ToDictionary(n => n.Id, n => n);

        var scoringEngineResult = await _scoringEngine.FinalizeSessionAsync(session);
        
        string bestMatchId = "";
        var parsedTargetCareers = new List<ScoredCareerDto>();
        var degrees = new List<ScoredNodeDto>();
        var profQuals = new List<ScoredNodeDto>();
        var careers = new List<ScoredNodeDto>();

        try
        {
            var ctx = string.IsNullOrEmpty(session.AssessmentContextJson) ? new AssessmentContext() 
                : JsonSerializer.Deserialize<AssessmentContext>(session.AssessmentContextJson, _jsonOpts) ?? new();

            var predictionsData = JsonSerializer.Deserialize<List<dynamic>>(scoringEngineResult.TopPredictionsJson, _jsonOpts) ?? new();
            foreach (var p in predictionsData)
            {
                string rawNodeId = p.TryGetProperty("nodeId", out JsonElement nEl) ? nEl.GetString() ?? "" : "";
                string nodeId = rawNodeId;
                double matchPercent = p.TryGetProperty("matchPercent", out JsonElement mEl) ? mEl.GetDouble() : 0;

                if (session.PhaseKey.Equals("After12th", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var degree in FindRelatedAfter12thDegrees(nodeId, matchPercent, nodeMap, session.SubPhaseKey))
                    {
                        if (!degrees.Any(d => d.Name.Equals(degree.Name, StringComparison.OrdinalIgnoreCase)))
                            degrees.Add(degree);
                    }
                    continue;
                }

                // Several AfterPostGraduation banks score legacy trait IDs
                // instead of taxonomy IDs (arch.urban, phd.ai, pharm.clinical,
                // etc.). Resolve known traits before eligibility filtering so
                // valid profile evidence is not mistaken for an unknown domain.
                nodeId = ResolveScoredTraitNodeId(nodeId, nodeMap);
                
                string careerName = nodeId;
                string nodeType = "career";
                string domainId = "";

                if (nodeMap.TryGetValue(nodeId, out var nd))
                {
                    careerName = nd.Name;
                    nodeType = nd.NodeType ?? "career";
                    domainId = nd.Id.Split('.').Length > 1 ? nd.Id.Split('.')[1] : "";
                }
                else if (nodeId.Contains('.'))
                {
                    careerName = nodeId.ToLowerInvariant() switch
                    {
                        "phd.academia" => "University Professor / Academic Researcher",
                        "phd.industry" or "phd.industry_rnd" => "Industry Research & Development Scientist",
                        "phd.startup" => "Deep-Tech Startup Researcher",
                        "phd.government" => "Government Research Scientist",
                        _ => CareerDisplayNameService.GetDisplayName(nodeId)
                    };
                    domainId = nodeId.Split('.')[1];
                }

                // Strict invariant check: Ensure the career matches the eligible domain if in AfterPostGraduation
                if (session.PhaseKey == "AfterPostGraduation" && !ctx.CareerChange && ctx.EligibleDomains.Any())
                {
                    // Taxonomy domain labels are the canonical source of truth. The old
                    // short-ID allowlist only knew Technology/Science/Commerce/Healthcare,
                    // so valid Architecture, Law, Pharmacy and other PG careers were
                    // discarded while generic Research/PhD nodes leaked through.
                    bool isDomainValid = nd != null && ctx.EligibleDomains.Any(ed =>
                        AreEquivalentCareerDomains(nd.Domain, ed));
                    foreach (var ed in ctx.EligibleDomains)
                    {
                        if ((AreEquivalentCareerDomains(ed, "Technology") || AreEquivalentCareerDomains(ed, "Engineering")) && (domainId == "eng" || domainId == "cs" || domainId == "tech" || nodeId.Contains("cybersec"))) isDomainValid = true;
                        if (AreEquivalentCareerDomains(ed, "Science") && (domainId == "pure" || domainId == "sci")) isDomainValid = true;
                        if (ed.Equals("Commerce", StringComparison.OrdinalIgnoreCase) && domainId == "finance") isDomainValid = true;
                        if (ed.Equals("Healthcare", StringComparison.OrdinalIgnoreCase) && (domainId == "med" || domainId == "health")) isDomainValid = true;
                        if (IsScoredTraitCompatibleWithDomain(rawNodeId, ed)) isDomainValid = true;
                    }
                    
                    if (!isDomainValid && domainId != "")
                    {
                        Console.WriteLine($"[AdaptiveQuizService] Rejected invalid candidate {nodeId} for AfterPostGraduation profile.");
                        continue; // Skip invalid domain
                    }
                }

                // Strict invariant check: Ensure the career matches explicit targets if in AfterGraduation
                if (session.PhaseKey == "AfterGraduation" && 
                    (ctx.CareerGoalMode == "selected_options" || ctx.CareerGoalMode == "specific_career") && 
                    ctx.TargetCareers.Any())
                {
                    // For targeted modes, the final result MUST be one of the explicitly selected target careers
                    // or a child specialization of it.
                    bool isTargetMatch = ctx.TargetCareers.Any(tc => 
                        nodeId.Equals(tc, StringComparison.OrdinalIgnoreCase) || 
                        nodeId.StartsWith(tc + ".", StringComparison.OrdinalIgnoreCase));
                        
                    if (!isTargetMatch)
                    {
                        Console.WriteLine($"[AdaptiveQuizService] Rejected {nodeId} because it is outside selected options ({string.Join(",", ctx.TargetCareers)}).");
                        continue; // Skip out-of-bounds candidate
                    }
                }

                var scoredNode = new ScoredNodeDto { Name = careerName, Score = (int)Math.Round(matchPercent) };

                if (nodeType == "degree")
                {
                    degrees.Add(scoredNode);
                }
                else if (nodeType == "professional_qualification")
                {
                    profQuals.Add(scoredNode);
                }
                else
                {
                    if (string.IsNullOrEmpty(bestMatchId)) bestMatchId = nodeId;
                    if (!parsedTargetCareers.Any(c => c.NodeId.Equals(nodeId, StringComparison.OrdinalIgnoreCase)))
                    {
                        careers.Add(scoredNode);
                        parsedTargetCareers.Add(await CreateScoredCareerAsync(nodeId, careerName, (int)Math.Round(matchPercent), nodeMap));
                    }
                }
            }
        }
        catch { }

        if (session.PhaseKey.Equals("After12th", StringComparison.OrdinalIgnoreCase) && degrees.Any())
        {
            var selectedDegrees = degrees
                .OrderByDescending(d => d.Score)
                .Take(4)
                .ToList();

            degrees = selectedDegrees;
            parsedTargetCareers = new List<ScoredCareerDto>();
            foreach (var degree in degrees)
            {
                var degreeNodeId = nodeMap.Values.FirstOrDefault(n => n.Name.Equals(degree.Name, StringComparison.OrdinalIgnoreCase))?.Id ?? string.Empty;
                parsedTargetCareers.Add(await CreateScoredCareerAsync(degreeNodeId, degree.Name, (int)Math.Round(degree.Score), nodeMap));
            }
        }

        if (session.PhaseKey.Equals("After12th", StringComparison.OrdinalIgnoreCase) && !degrees.Any())
        {
            var fallbackDegrees = FindRelatedAfter12thDegrees(
                scoringEngineResult.PrimaryTaxonomyNodeId,
                100,
                nodeMap,
                session.SubPhaseKey);
            degrees = fallbackDegrees.Take(4).ToList();
            parsedTargetCareers = new List<ScoredCareerDto>();
            foreach (var degree in degrees)
            {
                var degreeNodeId = nodeMap.Values.FirstOrDefault(n => n.Name.Equals(degree.Name, StringComparison.OrdinalIgnoreCase))?.Id ?? string.Empty;
                parsedTargetCareers.Add(await CreateScoredCareerAsync(degreeNodeId, degree.Name, (int)Math.Round(degree.Score), nodeMap));
            }
        }

        // If there are no target careers matching the criteria, do not fabricate a fallback.
        // The frontend will receive empty arrays and handle the 'InsufficientConfidence' or 'MoreInformationNeeded' state natively.

        // Compatibility Score & Close Match Warning
        string compScoreText = "0/100 current profile alignment.";
        string closeMatchWarn = "";
        // A completed quiz must never return an "Unknown" recommendation.
        // When evidence is genuinely insufficient, return the dedicated
        // inconclusive state rather than fabricating a career result.
        string bestMatchName = "Career exploration";
        string confidenceText = "Medium";
        
        if (parsedTargetCareers.Any())
        {
            bestMatchName = parsedTargetCareers[0].CareerName;
            compScoreText = $"{parsedTargetCareers[0].Score}/100 current profile alignment.";
            if (parsedTargetCareers.Count > 1 && (parsedTargetCareers[0].Score - parsedTargetCareers[1].Score) < 5)
            {
                closeMatchWarn = "These careers are closely matched based on your responses.";
            }
        }
        else if (profQuals.Any())
        {
            bestMatchName = profQuals[0].Name;
            compScoreText = $"{profQuals[0].Score}/100 current profile alignment.";
        }
        else if (degrees.Any())
        {
            bestMatchName = degrees[0].Name;
            compScoreText = $"{degrees[0].Score}/100 current profile alignment.";
        }

        // Subdomain / Specialization
        string specializationText = "";
        if (!string.IsNullOrEmpty(session.SubdomainScoresJson))
        {
            try
            {
                var subScores = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(session.SubdomainScoresJson, _jsonOpts) ?? new();
                var validSubs = new List<(string Id, int Score)>();
                foreach (var sub in subScores)
                {
                    if (!string.IsNullOrEmpty(bestMatchId) && !sub.Key.StartsWith(bestMatchId, StringComparison.OrdinalIgnoreCase)) continue;

                    int evidenceCount = sub.Value.TryGetProperty("EvidenceCount", out JsonElement ev) ? ev.GetInt32() : 0;
                    if (evidenceCount >= 2)
                    {
                        double maxScore = sub.Value.TryGetProperty("MaxPossibleScore", out JsonElement maxEl) ? maxEl.GetDouble() : 0;
                        double rawScore = sub.Value.TryGetProperty("RawScore", out JsonElement rawEl) ? rawEl.GetDouble() : 0;
                        if (maxScore > 0)
                        {
                            validSubs.Add((sub.Key, (int)Math.Round((rawScore / maxScore) * 100)));
                        }
                    }
                }
                if (validSubs.Any())
                {
                    var topSub = validSubs.OrderByDescending(x => x.Score).First();
                    string subName = nodeMap.TryGetValue(topSub.Id, out var nd) ? nd.Name : CareerDisplayNameService.GetDisplayName(topSub.Id);
                    specializationText = $"Primary: {subName}";
                }
            }
            catch { }
        }

        // Competencies
        var topCompetencies = new List<string>();
        var devAreas = new List<string>();
        if (!string.IsNullOrEmpty(session.CompetencyScoresJson))
        {
            try
            {
                var compScores = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(session.CompetencyScoresJson, _jsonOpts) ?? new();
                var parsedComps = new List<(string Name, int Score)>();
                foreach (var comp in compScores)
                {
                    double maxScore = comp.Value.TryGetProperty("MaxPossibleScore", out JsonElement maxEl) ? maxEl.GetDouble() : 0;
                    double rawScore = comp.Value.TryGetProperty("RawScore", out JsonElement rawEl) ? rawEl.GetDouble() : 0;
                    if (maxScore > 0)
                    {
                        string cName = comp.Key.Replace("_", " ");
                        cName = char.ToUpper(cName[0]) + cName.Substring(1);
                        parsedComps.Add((cName, (int)Math.Round((rawScore / maxScore) * 100)));
                    }
                }
                topCompetencies = parsedComps.OrderByDescending(x => x.Score).Take(3).Select(x => x.Name).ToList();
                devAreas = parsedComps.OrderBy(x => x.Score).Take(2).Select(x => x.Name).ToList();
            }
            catch { }
        }

        var evidenceItems = await BuildEvidenceLogAsync(rawAnswers, prior, nodeMap);
        
        var primaryNodeId = !string.IsNullOrWhiteSpace(bestMatchId)
            ? bestMatchId
            : scoringEngineResult.PrimaryTaxonomyNodeId;
        var primaryNode = nodeMap.TryGetValue(primaryNodeId, out var pn) ? pn : null;
        var nextSteps = BuildCareerNextSteps(primaryNode);
        var primaryCareerInfo = parsedTargetCareers.FirstOrDefault();

        double confidence = parsedTargetCareers.Any() ? parsedTargetCareers[0].Score / 100.0 : 0.5;
        if (confidence * 100 >= 80) confidenceText = "High";
        else if (confidence * 100 >= 50) confidenceText = "Medium";
        else confidenceText = "Low";

        var relevantEvidence = FilterEvidenceForCareer(evidenceItems, primaryNodeId);
        // A degree node (especially the generic PhD node) is not a career
        // recommendation for someone already taking the postgraduation quiz.
        var hasRecommendation = parsedTargetCareers.Any() || profQuals.Any() ||
            (!session.PhaseKey.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase) && degrees.Any());
        string whyRecommended = hasRecommendation
            ? GenerateEvidenceBasedExplanation(new List<TaxonomyMatchDto>(), relevantEvidence, 0.9)
            : "Your answers did not provide enough consistent evidence for a reliable career recommendation. Review your study field and preferred work setting, then retake the assessment for a more useful match.";

        // Persist result
        var result = new QuizResultV2
        {
            SessionId = sessionId,
            UserId = userId,
            PhaseKey = session.PhaseKey,
            PrimaryTaxonomyNodeId = primaryNodeId,
            TopPredictionsJson = scoringEngineResult.TopPredictionsJson,
            ConfidenceScore = Math.Round(confidence * 100, 1),
            EvidenceLogJson = JsonSerializer.Serialize(evidenceItems), // Store ALL evidence in DB
            SkillGapJson = JsonSerializer.Serialize(GetSkillGap(primaryNode, relevantEvidence)),
            EducationPathJson = JsonSerializer.Serialize(BuildEducationPath(primaryNode, session.PhaseKey, relevantEvidence)),
            RecommendedCoursesJson = JsonSerializer.Serialize(GetRecommendedCourses(primaryNode)),
            EligibilityWarningsJson = JsonSerializer.Serialize(DetectContradictions(prior, parsedTargetCareers.Select(c => new TaxonomyMatchDto { Name = c.CareerName, Probability = c.Score / 100.0, NodeId = c.CareerName }).ToList())),
            ModelUsed = "RuleBased_Bayesian",
            RuleBasedScoresJson = session.CareerScoresJson ?? "{}",
            WhyRecommended = whyRecommended,
            GeneratedAt = DateTime.UtcNow
        };

        _context.QuizResultsV2.Add(result);
        session.Status = "Completed";
        session.CompletedAt = DateTime.UtcNow;
        _context.AdaptiveQuizSessions.Update(session);
        await _context.SaveChangesAsync();

        if (hasRecommendation && !string.IsNullOrWhiteSpace(userId))
        {
            try
            {
                var targetKey = (!string.IsNullOrWhiteSpace(result.PrimaryTaxonomyNodeId) ? result.PrimaryTaxonomyNodeId : bestMatchName)
                    .Trim().ToLowerInvariant().Replace(" ", "-").Replace("/", "-");
                var targetTitle = !string.IsNullOrWhiteSpace(bestMatchName) && bestMatchName != "Career exploration"
                    ? bestMatchName
                    : (primaryNode?.Name ?? CareerDisplayNameService.GetDisplayName(result.PrimaryTaxonomyNodeId ?? ""));

                if (!string.IsNullOrWhiteSpace(targetTitle))
                {
                    var existingSaved = await _context.UserSavedRoadmaps
                        .Where(u => u.UserId == userId)
                        .ToListAsync();

                    var userRoadmap = existingSaved.FirstOrDefault(u => u.CareerKey == targetKey);
                    if (userRoadmap == null)
                    {
                        RoadmapBlueprint? bp = null;
                        if (_curriculumGenerator != null)
                        {
                            bp = await _curriculumGenerator.GetOrGenerateCurriculumAsync(targetTitle, targetKey);
                        }
                        else
                        {
                            bp = await _context.RoadmapBlueprints.FirstOrDefaultAsync(b => b.CareerKey == targetKey);
                        }

                        if (bp != null)
                        {
                            foreach (var s in existingSaved) s.IsDefault = false;
                            _context.UserSavedRoadmaps.Add(new UserSavedRoadmap
                            {
                                UserId = userId,
                                CareerKey = targetKey,
                                CareerTitle = targetTitle,
                                BlueprintId = bp.Id,
                                IsDefault = true,
                                CreatedAt = DateTime.UtcNow,
                                LastOpenedAt = DateTime.UtcNow
                            });
                            await _context.SaveChangesAsync();
                        }
                    }
                    else if (!userRoadmap.IsDefault)
                    {
                        foreach (var s in existingSaved) s.IsDefault = (s.CareerKey == targetKey);
                        userRoadmap.LastOpenedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {
                // Non-blocking: background auto-save should never fail quiz submission
            }
        }

        // PHASE 6B - Education Calculation
        CareerTaxonomyV2? degreeNode = null;
        var bestDegree = degrees.FirstOrDefault();
        if (bestDegree != null)
        {
            degreeNode = nodeMap.Values.FirstOrDefault(n => n.NodeType == "degree" && n.Name.Equals(bestDegree.Name, StringComparison.OrdinalIgnoreCase));
        }
        if (degreeNode == null && primaryNode != null && !string.IsNullOrEmpty(primaryNode.RequiredDegreesJson))
        {
            try
            {
                var reqDegrees = JsonSerializer.Deserialize<List<string>>(primaryNode.RequiredDegreesJson, _jsonOpts);
                if (reqDegrees != null && reqDegrees.Any())
                {
                    if (nodeMap.TryGetValue(reqDegrees.First(), out var reqDegNode))
                    {
                        degreeNode = reqDegNode;
                    }
                }
            }
            catch { }
        }

        string qualId = degreeNode?.Id ?? "Unavailable";
        string qualName = degreeNode?.Name ?? "Unavailable";
        int? durationValue = null;
        string durationStatus = "Unavailable";
        
        if (degreeNode != null && !string.IsNullOrEmpty(degreeNode.TimeToCompleteQualification))
        {
            var match = System.Text.RegularExpressions.Regex.Match(degreeNode.TimeToCompleteQualification, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int d))
            {
                durationValue = d;
                durationStatus = "Success";
            }
        }

        var profileFieldsNext = string.IsNullOrEmpty(session.BackgroundProfileJson) ? new Dictionary<string, string>() : JsonSerializer.Deserialize<Dictionary<string, string>>(session.BackgroundProfileJson, _jsonOpts) ?? new();
        var canonicalEducation = await FetchCanonicalEducationAsync(primaryNode?.Id ?? "", nodeMap, profileFieldsNext);

        return new QuizV2ResultDto
        {
            ResultId = result.Id,
            BestMatchName = bestMatchName,
            CompatibilityScoreText = compScoreText,
            ConfidenceText = confidenceText,
            TargetCareerComparison = parsedTargetCareers,
            StrongestCompetencies = topCompetencies,
            SpecializationText = specializationText,
            DevelopmentAreas = devAreas,
            RecommendedNextSteps = nextSteps,
            WhyRecommended = whyRecommended,
            CloseMatchWarning = closeMatchWarn,
            ExpectedSalaryRange = "",
            ExpectedTimeToJobReady = "",
            ConfidenceScore = result.ConfidenceScore,
            EvidenceItems = relevantEvidence,
            ModelUsed = result.ModelUsed,
            ResultStatus = hasRecommendation ? "Success" : "InsufficientConfidence",
            GeneratedAt = result.GeneratedAt,
            PrimaryDomain = new ScoredNodeDto { Name = primaryNode?.Name ?? bestMatchName, Score = Math.Round(confidence, 2) },
            Fields = new List<ScoredNodeDto>(),
            Specializations = new List<ScoredNodeDto>(),
            CareerFamilies = new List<ScoredNodeDto>(),
            Confidence = new ConfidenceDto { Level = "Moderate", Score = Math.Round(confidence, 2) },
            Evidence = new List<string>(),
            Eligibility = new EligibilityStatusDto { IsEligible = true, Message = "Mock Eligibility" },
            Explanation = new ExplanationDto { WhyRecommended = whyRecommended },
            
            // New categorized fields
            RecommendedDegree = degrees.FirstOrDefault(),
            AlternativeDegrees = degrees.Skip(1).ToList(),
            ProfessionalQualification = profQuals.FirstOrDefault(),
            RecommendedCareer = careers.FirstOrDefault(),
            AlternativeCareers = careers.Skip(1).ToList(),

            // Canonical Mapping (Phase 1A)
            Career = primaryNode != null ? new CanonicalCareerDto
            {
                Id = primaryNode.Id,
                Name = bestMatchName
            } : null,
            Compatibility = new CanonicalCompatibilityDto
            {
                Score = Math.Round(confidence, 2),
                Level = compScoreText,
                Explanation = whyRecommended
            },
            Education = canonicalEducation,
            Salary = await FetchCanonicalSalaryAsync(bestMatchName ?? primaryNode?.Id ?? string.Empty, primaryNodeId),
            Forecast = await FetchCanonicalForecastAsync(bestMatchName ?? primaryNode?.Id ?? string.Empty, primaryNodeId)
        };
    }

    private async Task<CanonicalSalaryDto> FetchCanonicalSalaryAsync(string careerId, string careerNodeId = "")
    {
        var salaryService = new SalaryIntelligenceService(_context);
        var salaryLookupId = !string.IsNullOrWhiteSpace(careerNodeId)
            ? $"{careerNodeId}|{careerId}"
            : careerId;
        var forecast = await salaryService.ForecastSalaryAsync(new SalaryForecastRequestV2 { CareerPathId = salaryLookupId });
        if (salaryLookupId != careerId)
            forecast.CareerTitle = careerId;

        if (forecast.Status == "Unavailable" || forecast.Benchmark.Status == "Unavailable")
        {
            return new CanonicalSalaryDto { Status = "Unavailable" };
        }

        return new CanonicalSalaryDto
        {
            Status = "Available",
            MinSalary = (int)forecast.Benchmark.P25,
            MaxSalary = (int)forecast.Benchmark.P75,
            SalaryUnit = forecast.Currency,
            SalaryRoleName = forecast.CareerTitle,
            Source = forecast.Sources.FirstOrDefault()?.SourceName ?? "SkillScape Intelligence",
            ExperienceMinYears = 0,
            ExperienceMaxYears = 2
        };
    }

    private async Task<CanonicalForecastDto> FetchCanonicalForecastAsync(string careerId, string careerNodeId = "")
    {
        var salaryService = new SalaryIntelligenceService(_context);
        var salaryLookupId = !string.IsNullOrWhiteSpace(careerNodeId)
            ? $"{careerNodeId}|{careerId}"
            : careerId;
        var forecast = await salaryService.ForecastSalaryAsync(new SalaryForecastRequestV2 { CareerPathId = salaryLookupId });

        if (forecast.Status == "Unavailable" || !forecast.Projection.Any())
        {
            return new CanonicalForecastDto
            {
                Status = "Unavailable",
                Years = Enumerable.Range(0, 6).Select(i => new CanonicalForecastYearDto
                {
                    YearNumber = i,
                    ValueType = "Unavailable"
                }).ToList()
            };
        }

        return new CanonicalForecastDto
        {
            Status = "Available",
            Years = forecast.Projection.Select(p => new CanonicalForecastYearDto
            {
                YearNumber = p.ExperienceYear,
                CalendarYear = p.CalendarYear,
                ValueType = p.ValueType,
                MinSalary = (int)p.P25,
                MaxSalary = (int)p.P75,
                SalaryUnit = forecast.Currency
            }).ToList()
        };
    }

    
    private Task<CanonicalEducationDto?> FetchCanonicalEducationAsync(string nodeId, IReadOnlyDictionary<string, CareerTaxonomyV2> nodeMap, Dictionary<string, string> profileFields)
    {
        nodeMap.TryGetValue(nodeId, out var primaryNode);
        CareerTaxonomyV2? degreeNode = null;
        if (primaryNode != null && !string.IsNullOrEmpty(primaryNode.RequiredDegreesJson))
        {
            try
            {
                var reqDegrees = JsonSerializer.Deserialize<List<string>>(primaryNode.RequiredDegreesJson, _jsonOpts);
                if (reqDegrees != null)
                {
                    foreach (var requiredDegree in reqDegrees)
                    {
                        degreeNode = nodeMap.TryGetValue(requiredDegree, out var reqDegNode)
                            ? reqDegNode
                            : nodeMap.Values.FirstOrDefault(candidate => candidate.NodeType.Equals("degree", StringComparison.OrdinalIgnoreCase)
                                && (candidate.Name.Equals(requiredDegree, StringComparison.OrdinalIgnoreCase)
                                    || candidate.Name.Contains(requiredDegree, StringComparison.OrdinalIgnoreCase)
                                    || requiredDegree.Contains(candidate.Name, StringComparison.OrdinalIgnoreCase)));
                        if (degreeNode != null) break;
                    }
                }
            }
            catch { }
        }

        string qualId = degreeNode?.Id ?? "Unavailable";
        string qualName = degreeNode?.Name ?? "Unavailable";
        int? durationValue = null;
        string durationStatus = "Unavailable";
        
        if (degreeNode != null && !string.IsNullOrEmpty(degreeNode.TimeToCompleteQualification))
        {
            var match = System.Text.RegularExpressions.Regex.Match(degreeNode.TimeToCompleteQualification, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int d))
            {
                durationValue = d;
                durationStatus = "Success";
            }
        }

        int? expGradYear = null;
        double? remainingDuration = null;
        string gradStatus = "Unknown";
        string currentStatus = profileFields.TryGetValue("currentStatus", out var cs) ? cs : 
                              (profileFields.TryGetValue("employmentStatus", out var empStat) ? empStat : "Unknown");
        string phaseKey = profileFields.TryGetValue("phaseKey", out var pk) ? pk : 
                         (profileFields.TryGetValue("subPhaseKey", out var spk) ? spk : "Unknown");
        string edLevel = profileFields.TryGetValue("educationLevel", out var el) ? el : "Unknown";
        
        int currentYearValue = DateTime.UtcNow.Year;

        int currentYearOfStudy = 0;
        if (profileFields.TryGetValue("currentYearOfStudy", out var cyStr) && int.TryParse(cyStr, out int cy)) {
            currentYearOfStudy = cy;
        } else if (currentStatus.Contains("Year 1") || currentStatus.Contains("1st Year")) {
            currentYearOfStudy = 1;
        } else if (currentStatus.Contains("Year 2") || currentStatus.Contains("2nd Year")) {
            currentYearOfStudy = 2;
        } else if (currentStatus.Contains("Year 3") || currentStatus.Contains("3rd Year")) {
            currentYearOfStudy = 3;
        }

        if (profileFields.TryGetValue("graduationYear", out var gradYearStr) && int.TryParse(gradYearStr, out int gY))
        {
            expGradYear = gY;
            gradStatus = "Verified";
            remainingDuration = Math.Max(0, gY - currentYearValue);
        }
        else if (edLevel.Contains("Recently Graduated") || edLevel.Contains("Work Experience") || edLevel.Contains("Unemployed") || 
                 currentStatus.Contains("Recently Graduated") || currentStatus.Contains("Working") || currentStatus.Contains("Unemployed") ||
                 currentStatus.Contains("Graduated"))
        {
            expGradYear = null;
            gradStatus = "Verified"; 
            remainingDuration = 0;
            currentStatus = "Graduated";
        }
        else if (edLevel.Contains("Final Year") || currentStatus.Contains("Final Year"))
        {
            expGradYear = currentYearValue;
            gradStatus = "Calculated";
            remainingDuration = 0.5;
        }
        else if (currentYearOfStudy > 0 && durationValue.HasValue)
        {
            int remaining = durationValue.Value - currentYearOfStudy;
            remainingDuration = Math.Max(0, remaining);
            expGradYear = currentYearValue + (int)remainingDuration;
            gradStatus = "Calculated";
        }
        else if (phaseKey == "After12th" && durationValue.HasValue)
        {
            expGradYear = currentYearValue + durationValue.Value;
            gradStatus = "Estimated";
            remainingDuration = durationValue.Value;
        }
        else if (phaseKey == "AfterPostGraduation" || phaseKey.Contains("PostGraduation"))
        {
            if (durationValue.HasValue) 
            {
                expGradYear = currentYearValue + durationValue.Value;
                gradStatus = "Estimated";
                remainingDuration = durationValue.Value;
            }
        }
        else
        {
            gradStatus = "Unknown";
            expGradYear = null;
            remainingDuration = null;
        }

        CanonicalEducationDto? canonicalEducation = null;
        if (primaryNode != null)
        {
            var requiredNames = new List<string>();
            try
            {
                requiredNames = JsonSerializer.Deserialize<List<string>>(primaryNode.RequiredDegreesJson, _jsonOpts) ?? new List<string>();
            }
            catch { }
            var careerEducationName = degreeNode?.Name
                ?? requiredNames.FirstOrDefault()
                ?? $"Relevant qualification in {primaryNode.Name}";
            if (!durationValue.HasValue)
            {
                durationValue = careerEducationName.Contains("LLB", StringComparison.OrdinalIgnoreCase) ? 5 : 3;
                durationStatus = "Estimated";
            }
            canonicalEducation = new CanonicalEducationDto
            {
                QualificationId = degreeNode?.Id ?? string.Empty,
                QualificationName = careerEducationName,
                QualificationLevel = "Degree",
                Duration = durationValue,
                DurationUnit = "years",
                DurationStatus = durationStatus,
                ExpectedGraduationYear = expGradYear,
                RemainingDuration = remainingDuration,
                CurrentStatus = currentStatus,
                GraduationStatus = gradStatus,
                Source = "CareerTaxonomyV2"
            };
        }

        return Task.FromResult(canonicalEducation);
    }

    private static List<string> BuildCareerNextSteps(CareerTaxonomyV2? career)
    {
        var careerName = career?.Name ?? "your recommended career";
        var lower = $"{career?.Id} {careerName} {career?.Domain} {career?.Subdomain} {career?.Specialization}".ToLowerInvariant();
        if (lower.Contains("law") || lower.Contains("legal") || lower.Contains("llb"))
        {
            return new List<string>
            {
                "Compare integrated 5-year BA LLB/BBA LLB programs with the 3-year LLB route after graduation.",
                "Check entrance requirements such as CLAT, AILET, or the chosen university's admission test.",
                "Complete a recognized law degree, enroll with a State Bar Council, and meet the AIBE requirements to practice.",
                $"Seek court, legal aid, or chamber internships related to {careerName}."
            };
        }

        var requiredDegrees = new List<string>();
        try { requiredDegrees = JsonSerializer.Deserialize<List<string>>(career?.RequiredDegreesJson ?? "[]", _jsonOpts) ?? new List<string>(); } catch { }
        return new List<string>
        {
            requiredDegrees.Count > 0 ? $"Review recognized programs for {requiredDegrees[0]} and confirm admission requirements." : $"Find accredited education programs relevant to {careerName}.",
            $"Build practical experience through projects, internships, or supervised work in {careerName}.",
            "Check the entrance exams, certifications, or professional registration expected by employers in your location."
        };
    }


private async Task<ScoredCareerDto> CreateScoredCareerAsync(string nodeId, string careerName, int score, IReadOnlyDictionary<string, CareerTaxonomyV2> nodeMap)
    {
        nodeMap.TryGetValue(nodeId, out var node);
        return new ScoredCareerDto
        {
            NodeId = nodeId,
            CareerName = careerName,
            Score = score,
            ExpectedSalaryRange = "",
            ExpectedTimeToJobReady = "",
            Salary = await FetchCanonicalSalaryAsync(careerName, nodeId),
            Forecast = await FetchCanonicalForecastAsync(careerName, nodeId),
            Education = await FetchCanonicalEducationAsync(nodeId, nodeMap, new Dictionary<string, string>())
        };
    }

    public async Task<QuizV2ResultDto?> GetLatestResultAsync(string userId)
    {
        var result = await _context.QuizResultsV2
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.GeneratedAt)
            .FirstOrDefaultAsync();

        if (result == null) return null;
        return await MapResultToDtoAsync(result);
    }

    public async Task<List<QuizV2ResultDto>> GetResultsAsync(string userId)
    {
        var results = await _context.QuizResultsV2
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync();

        var mapped = new List<QuizV2ResultDto>();
        foreach(var res in results) mapped.Add(await MapResultToDtoAsync(res));
        return mapped;
    }

    public async Task<QuizV2ResultDto?> GetResultByIdAsync(string userId, string resultId)
    {
        var result = await _context.QuizResultsV2
            .FirstOrDefaultAsync(r => r.UserId == userId && r.Id == resultId);

        if (result == null) return null;
        return await MapResultToDtoAsync(result);
    }

    public async Task<bool> DeleteResultAsync(string userId, string resultId)
    {
        var result = await _context.QuizResultsV2
            .FirstOrDefaultAsync(r => r.UserId == userId && r.Id == resultId);

        if (result == null) return false;

        _context.QuizResultsV2.Remove(result);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<List<PhaseInfoDto>> GetPhasesAsync()
    {
        var phases = new List<PhaseInfoDto>
        {
            new() { PhaseKey = "After10th",        Title = "After 10th Grade",         Description = "Choosing your stream: Science (PCM/PCB), Commerce, Arts, Diploma, or ITI",   Icon = "school",       SubPhases = Array.Empty<string>(),                              RequiresBackgroundForm = true },
            new() { PhaseKey = "After12th",         Title = "After 12th Grade",         Description = "Choosing your degree: Engineering, Medical, Commerce, Law, Design, and more", Icon = "graduation-cap", SubPhases = new[]{"pcm","pcb","pcmb","commerce","arts","vocational"},  RequiresBackgroundForm = true },
            new() { PhaseKey = "AfterGraduation",   Title = "After Graduation",         Description = "MBA, M.Tech, GATE, UPSC, direct job, or startup",                            Icon = "award",        SubPhases = Array.Empty<string>(),                              RequiresBackgroundForm = true },
            new() { PhaseKey = "AfterPostGraduation",Title = "After Post-Graduation",  Description = "PhD, senior technical role, professorship, or management",                    Icon = "book-marked",  SubPhases = Array.Empty<string>(),                              RequiresBackgroundForm = true },
            new() { PhaseKey = "CareerSwitch",      Title = "Career Switch",            Description = "Transitioning to a new field using your existing experience",                 Icon = "shuffle",      SubPhases = Array.Empty<string>(),                              RequiresBackgroundForm = true },
            new() { PhaseKey = "GovernmentJobs",    Title = "Government Jobs",          Description = "Preparing for UPSC, banking, SSC, railways, defence, or PSU exams",          Icon = "landmark",     SubPhases = Array.Empty<string>(),                              RequiresBackgroundForm = true },
        };
        return Task.FromResult(phases);
    }

    // ── Private: Data Loading ─────────────────────────────────────────────────

    private static List<ScoredNodeDto> FindRelatedAfter12thDegrees(
        string nodeId,
        double matchPercent,
        Dictionary<string, CareerTaxonomyV2> nodeMap,
        string subPhaseKey)
    {
        var ancestors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var currentId = nodeId;
        while (!string.IsNullOrWhiteSpace(currentId))
        {
            ancestors.Add(currentId);
            if (!nodeMap.TryGetValue(currentId, out var currentNode) || string.IsNullOrWhiteSpace(currentNode.ParentId))
                break;
            currentId = currentNode.ParentId;
        }

        // Some legacy signals refer to a specialization that is absent from
        // the current taxonomy seed. Prefixes still identify its degree family.
        var parts = nodeId.Split('.', StringSplitOptions.RemoveEmptyEntries);
        for (var length = parts.Length - 1; length >= 1; length--)
            ancestors.Add(string.Join('.', parts.Take(length)));

        return nodeMap.Values
            .Where(node => string.Equals(node.NodeType, "degree", StringComparison.OrdinalIgnoreCase))
            .Where(node => IsDegreeAllowedForStream(node.Id, subPhaseKey))
            .Where(node => !string.IsNullOrWhiteSpace(node.ParentId) && ancestors.Contains(node.ParentId))
            .Select(node => new ScoredNodeDto { Name = NormalizeDegreeName(node.Name), Score = matchPercent })
            .ToList();
    }

    private static string ResolveScoredTraitNodeId(string nodeId, IReadOnlyDictionary<string, CareerTaxonomyV2> nodeMap)
    {
        if (nodeMap.ContainsKey(nodeId))
            return nodeId;

        var normalizedId = nodeId.ToLowerInvariant();
        var taxonomyId = normalizedId switch
        {
            "arch.urban" => "design.arch.urban",
            "arch.sustainable" or "arch.tech" or "arch.research" or "arch" => "design.arch",
            "phd.ai" => "sci.eng.cs.ai_ml",
            "phd.cloud" => "sci.eng.cs.cloud",
            "phd.cyber" => "tech.cyber",
            "phd.civil" => "sci.eng.civil",
            "phd.vlsi" => "sci.eng.ece",
            "phd.materials" or "phd.applied" or "phd.theoretical" => "sci.eng",
            // Electrical and electronics question banks contain legacy skill
            // traits. Treat them as evidence for the parent engineering career,
            // never as standalone careers such as "Circuits".
            "eng.ee.circuits" or "eng.ee.telecom" or "eng.ece.comm" or "eng.ece.embedded" or "eng.ece.signal" or "eng.ece.vlsi" => "sci.eng.ece",
            "eng.ee.power" or "eng.ee.control" => "sci.eng.elec",
            "eng.chem.bio" or "eng.chem.environment" or "eng.chem.materials" or "eng.chem.process" => "sci.eng.chem",
            "eng" => "sci.eng",
            "arts" => "arts",
            "arts.design" => "des",
            "arts.history" or "arts.journalism" => "arts",
            "arts.psychology" => "arts.psych",
            "bus" => "com.mgmt",
            "bus.finance" => "com.finance",
            "bus.hr" => "com.hr",
            "bus.marketing" or "com.marketing" => "com.marketing",
            "com.hr" => "com",
            "com.acc.prof" => "com.acc.prof",
            "com.fin.inv" => "com.finance",
            "law" or "law.civil" or "law.corporate" or "law.criminal" or "law.ip" => "arts.law",
            "des" => "des",
            "des.fashion" or "des.graphic" or "des.product" or "des.uiux" => "des",
            "pharm" or "pharm.clinical" or "pharm.quality" or "pharm.research" or "pharm.sales" => "sci.med.pharma",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(taxonomyId) || !nodeMap.ContainsKey(taxonomyId))
        {
            // Seed banks may introduce a new sub-specialty before the taxonomy
            // catalog does. Collapse those trait keys to an existing parent
            // occupation so a skill label is never shown as the learner's career.
            taxonomyId = normalizedId switch
            {
                var id when id.StartsWith("eng.ee.", StringComparison.Ordinal) => "sci.eng.elec",
                var id when id.StartsWith("eng.ece.", StringComparison.Ordinal) => "sci.eng.ece",
                var id when id.StartsWith("eng.chem.", StringComparison.Ordinal) => "sci.eng.chem",
                var id when id.StartsWith("eng.me.", StringComparison.Ordinal) => "sci.eng.mech",
                var id when id.StartsWith("eng.civil.", StringComparison.Ordinal) => "sci.eng.civil",
                var id when id.StartsWith("sci.eng.elec.", StringComparison.Ordinal) => "sci.eng.elec",
                var id when id.StartsWith("sci.eng.ece.", StringComparison.Ordinal) => "sci.eng.ece",
                var id when id.StartsWith("sci.eng.chem.", StringComparison.Ordinal) => "sci.eng.chem",
                var id when id.StartsWith("sci.eng.mech.", StringComparison.Ordinal) => "sci.eng.mech",
                var id when id.StartsWith("sci.eng.civil.", StringComparison.Ordinal) => "sci.eng.civil",
                var id when id.StartsWith("arts.", StringComparison.Ordinal) => "arts",
                var id when id.StartsWith("bus.", StringComparison.Ordinal) => "com",
                var id when id.StartsWith("com.", StringComparison.Ordinal) => "com",
                var id when id.StartsWith("law.", StringComparison.Ordinal) => "arts.law",
                var id when id.StartsWith("des.", StringComparison.Ordinal) => "des",
                var id when id.StartsWith("pharm.", StringComparison.Ordinal) => "sci.med.pharma",
                _ => string.Empty
            };
        }

        return !string.IsNullOrEmpty(taxonomyId) && nodeMap.ContainsKey(taxonomyId)
            ? taxonomyId
            : nodeId;
    }

    private static bool IsScoredTraitCompatibleWithDomain(string traitId, string eligibleDomain)
    {
        var id = (traitId ?? string.Empty).ToLowerInvariant();
        var domain = id switch
        {
            var value when value.StartsWith("arch.") || value == "arch" => "Architecture",
            "phd.ai" or "phd.civil" or "phd.cloud" or "phd.cyber" or "phd.materials" or "phd.vlsi" or "phd.applied" or "phd.theoretical" => "Technology",
            var value when value.StartsWith("pharm.") || value == "pharm" => "Pharmacy",
            // These are post-PhD work-path signals (academic, industry R&D,
            // startup, government), not degree/domain IDs. They are valid
            // outcomes for any selected postgraduate discipline.
            "phd.academia" or "phd.industry" or "phd.industry_rnd" or "phd.startup" or "phd.government" => "AnyPostgraduateDomain",
            var value when value.StartsWith("med.") || value == "med" || value.StartsWith("dm.") || value.StartsWith("mch.") || value.StartsWith("fnb.") || value.StartsWith("afterpg.medical_super.") || value.StartsWith("afterpg.adv_dip.health") => "Healthcare",
            var value when value.StartsWith("law.") || value == "law" || value.StartsWith("diploma.law.") || value.StartsWith("afterpg.adv_dip.law") => "Law",
            var value when value.StartsWith("arts.") || value == "arts" || value.StartsWith("afterpg.adv_dip.arts") => "Humanities",
            var value when value.StartsWith("com.") || value == "com" || value.StartsWith("prof.fin.") || value.StartsWith("bus.") || value == "bus" || value.StartsWith("dip.mgmt.") || value.StartsWith("afterpg.adv_dip.mgmt") => "Commerce",
            var value when value.StartsWith("des.") || value == "des" => "Design",
            var value when value.StartsWith("sci.") || value == "sci" || value.StartsWith("phd.bio") || value.StartsWith("phd.chem") || value.StartsWith("phd.earth") || value.StartsWith("phd.math") || value.StartsWith("phd.physics") => "Science",
            var value when value.StartsWith("eng.") || value == "eng" || value.StartsWith("tech.") || value == "tech" || value.StartsWith("afterpg.adv_dip.tech") => "Technology",
            _ => string.Empty
        };

        return domain.Equals("AnyPostgraduateDomain", StringComparison.OrdinalIgnoreCase) ||
            AreEquivalentCareerDomains(domain, eligibleDomain);
    }

    private static bool AreEquivalentCareerDomains(string? first, string? second)
    {
        static string Canonicalize(string? value)
        {
            var domain = (value ?? string.Empty).Trim();
            return domain.ToLowerInvariant() switch
            {
                "engineering" or "eng" or "technology" or "tech" => "technology",
                "purescience" or "pure science" or "science" or "sciences" => "science",
                "pharmacy" or "pharmaceutical sciences" or "healthcare" or "health care" or "medicine" or "medical" => "healthcare",
                "commerce" or "business" or "management" => "commerce",
                "humanities" or "arts" or "liberal arts" => "humanities",
                "law" or "legal" => "law",
                "architecture" or "architectural studies" => "architecture",
                "design" or "creative arts" => "design",
                _ => domain.ToLowerInvariant()
            };
        }

        var canonicalFirst = Canonicalize(first);
        return canonicalFirst.Length > 0 && canonicalFirst == Canonicalize(second);
    }

    private static string NormalizeDegreeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Undergraduate degree programme";

        return name.Trim() switch
        {
            "B.Tech Computer Science Engineering" => "Bachelor of Technology in Computer Science and Engineering (B.Tech CSE)",
            "B.Tech Artificial Intelligence / ML" => "Bachelor of Technology in Artificial Intelligence and Machine Learning (B.Tech AI/ML)",
            "B.Tech Mechanical Engineering" => "Bachelor of Technology in Mechanical Engineering (B.Tech Mechanical)",
            "B.Tech Electronics & Communication" => "Bachelor of Technology in Electronics and Communication Engineering (B.Tech ECE)",
            "B.Sc Computer Science" => "Bachelor of Science in Computer Science (B.Sc CS)",
            "B.Sc Data Science" => "Bachelor of Science in Data Science (B.Sc Data Science)",
            "B.Sc Mathematics" => "Bachelor of Science in Mathematics (B.Sc Mathematics)",
            "B.Sc Biotechnology" => "Bachelor of Science in Biotechnology (B.Sc Biotechnology)",
            "B.Sc Nursing" => "Bachelor of Science in Nursing (B.Sc Nursing)",
            _ => name
        };
    }

    private static bool IsDegreeAllowedForStream(string degreeId, string subPhaseKey)
    {
        var id = degreeId.ToLowerInvariant();
        var stream = (subPhaseKey ?? string.Empty).ToLowerInvariant();
        return stream switch
        {
            "pcm" => id.StartsWith("deg.btech") || id is "deg.bca" or "deg.barch" || id.StartsWith("deg.bsc.cs") || id.StartsWith("deg.bsc.data") || id.StartsWith("deg.bsc.math"),
            "pcb" => id is "deg.mbbs" or "deg.bds" or "deg.bpharm" or "deg.bpt" || id.StartsWith("deg.bsc.biotech") || id.StartsWith("deg.bsc.nursing"),
            "pcmb" => id.StartsWith("deg.btech") || id is "deg.bca" or "deg.barch" or "deg.mbbs" or "deg.bds" or "deg.bpharm" or "deg.bpt" || id.StartsWith("deg.bsc."),
            "commerce" => id.StartsWith("deg.bcom") || id.StartsWith("deg.bba") || id.StartsWith("deg.bsc.eco"),
            "arts" => id.StartsWith("deg.ba") || id.StartsWith("deg.bdes") || id.StartsWith("deg.bfa") || id.StartsWith("deg.bjmc") || id.StartsWith("deg.llb"),
            "vocational" => id.StartsWith("deg.diploma") || id.StartsWith("deg.iti"),
            _ => false
        };
    }

    private async Task<List<CareerTaxonomyV2>> LoadApplicableTaxonomyNodesAsync(string phaseKey, string subPhaseKey, ParsedProfileDto profile, AssessmentContext ctx = null)
    {
        var all = await _context.CareerTaxonomyV2
            .AsNoTracking()
            .Where(n => n.IsActive)
            .ToListAsync();
        return all.Where(n =>
        {
            var phases = JsonSerializer.Deserialize<List<string>>(n.PhasesApplicableJson, _jsonOpts) ?? new();
            bool phaseMatches = phases.Contains(phaseKey)
                || phases.Contains($"{phaseKey}_{subPhaseKey}")
                || (string.IsNullOrEmpty(subPhaseKey) && phases.Contains(phaseKey));

            if (!phaseMatches)
            {
                // Fallback for general phases that aren't explicitly seeded in the taxonomy DB
                if (phaseKey != "AfterGraduation" && phaseKey != "CareerSwitch" && phaseKey != "GovernmentJobs")
                {
                    return false;
                }
            }

            // Strict filtering for After12th Stream
            if (phaseKey == "After12th" && !string.IsNullOrEmpty(subPhaseKey))
            {
                string sk = subPhaseKey.ToLowerInvariant();
                if (sk == "pcm")
                {
                    return n.Id.StartsWith("sci.eng") || n.Id.StartsWith("sci.math") || n.Id.StartsWith("deg.btech") || n.Id.StartsWith("deg.bsc") || n.Id.StartsWith("deg.bca") || n.Id.StartsWith("deg.barch");
                }
                else if (sk == "pcb")
                {
                    return n.Id.StartsWith("sci.med") || n.Id.StartsWith("sci.bio") || n.Id.StartsWith("deg.mbbs") || n.Id.StartsWith("deg.bds") || n.Id.StartsWith("deg.bsc") || n.Id.StartsWith("deg.bpharm") || n.Id.StartsWith("deg.bpt") || n.Id.StartsWith("sci.agri");
                }
                else if (sk == "pcmb")
                {
                    return n.Id.StartsWith("sci.eng") || n.Id.StartsWith("sci.math") || n.Id.StartsWith("sci.med") || n.Id.StartsWith("sci.bio") || n.Id.StartsWith("sci.agri") || n.Id.StartsWith("deg.btech") || n.Id.StartsWith("deg.bsc") || n.Id.StartsWith("deg.bca") || n.Id.StartsWith("deg.barch") || n.Id.StartsWith("deg.mbbs") || n.Id.StartsWith("deg.bds") || n.Id.StartsWith("deg.bpharm") || n.Id.StartsWith("deg.bpt");
                }
                else if (sk == "commerce")
                {
                    return n.Id.StartsWith("com") || n.Id.StartsWith("deg.bcom") || n.Id.StartsWith("deg.bba") || n.Id.StartsWith("prof.ca") || n.Id.StartsWith("prof.cs") || n.Id.StartsWith("prof.cma") || n.Id.StartsWith("deg.bsc.eco");
                }
                else if (sk == "arts")
                {
                    return n.Id.StartsWith("arts") || n.Id.StartsWith("design") || n.Id.StartsWith("deg.ba") || n.Id.StartsWith("deg.bdes") || n.Id.StartsWith("deg.bfa") || n.Id.StartsWith("deg.llb") || n.Id.StartsWith("deg.bjmc");
                }
                else if (sk == "vocational")
                {
                    return n.Id.StartsWith("voc") || n.Id.StartsWith("deg.diploma") || n.Id.StartsWith("deg.iti");
                }
            }

            // Strict filtering for AfterGraduation / AfterPostGraduation
            if (phaseKey == "AfterGraduation" || phaseKey == "AfterPostGraduation")
            {
                if (ctx != null && ctx.EligibleDomains.Any())
                {
                    // Ensure the taxonomy node belongs to one of the eligible domains
                    return ctx.EligibleDomains.Any(domain => AreEquivalentCareerDomains(n.Domain, domain)) ||
                           (ctx.EligibleDomains.Any(domain => AreEquivalentCareerDomains(domain, "Humanities")) && n.Id.StartsWith("arts", StringComparison.OrdinalIgnoreCase)) ||
                           (ctx.EligibleDomains.Any(domain => AreEquivalentCareerDomains(domain, "Commerce")) && n.Id.StartsWith("com", StringComparison.OrdinalIgnoreCase)) ||
                           (ctx.EligibleDomains.Any(domain => AreEquivalentCareerDomains(domain, "Design")) && n.Id.StartsWith("design", StringComparison.OrdinalIgnoreCase)) ||
                           n.Id.StartsWith("gov") || 
                           n.Id.StartsWith("edu"); // always allow general gov/edu fallback nodes
                }
                
                return true;
            }

            return true;
        }).ToList();
    }

    private async Task<List<QuizQuestionV2>> LoadQuestionsAsync(string phaseKey, string subPhaseKey)
    {
        var query = _context.QuizQuestionsV2.Where(q => q.IsActive && q.PhaseKey == phaseKey);
        var questions = await query.ToListAsync();

        if (phaseKey.Equals("After12th", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(subPhaseKey))
            {
                // Hard stream filter: MUST match exactly (case-insensitive).
                // Do NOT allow empty subPhaseKeys to bypass the stream constraint.
                questions = questions.Where(q =>
                    !string.IsNullOrEmpty(q.SubPhaseKey) &&
                    (q.SubPhaseKey.Equals(subPhaseKey, StringComparison.OrdinalIgnoreCase) ||
                     (subPhaseKey.Equals("pcmb", StringComparison.OrdinalIgnoreCase) &&
                      (q.SubPhaseKey.Equals("pcm", StringComparison.OrdinalIgnoreCase) || q.SubPhaseKey.Equals("pcb", StringComparison.OrdinalIgnoreCase))))
                ).ToList();
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(subPhaseKey))
            {
                questions = questions.Where(q => 
                    string.IsNullOrEmpty(q.SubPhaseKey) || 
                    q.SubPhaseKey.Equals(subPhaseKey, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
        }

        return questions.OrderBy(q => q.DisplayOrder).ToList();
    }

    // ── Private: Probability Engine ───────────────────────────────────────────

    private static Dictionary<string, double> InitializeUniformPrior(List<string> nodeIds)
    {
        double uniform = nodeIds.Count > 0 ? 1.0 / nodeIds.Count : 0.0;
        return nodeIds.ToDictionary(id => id, _ => uniform);
    }

    private static void BayesianUpdate(Dictionary<string, double> prior, List<OptionSignal> signals)
    {
        foreach (var signal in signals)
        {
            if (prior.ContainsKey(signal.NodeId))
            {
                // Multiply by signal weight (posterior ∝ prior × likelihood)
                prior[signal.NodeId] *= (1.0 + signal.Weight);
            }
            else
            {
                // The signal points to a node not in the current prior
                // (edge case: question crosses phase boundaries); add with dampened weight
                prior[signal.NodeId] = signal.Weight * EPSILON;
            }
        }

        // Apply smoothing to prevent collapse
        foreach (var key in prior.Keys.ToList())
        {
            prior[key] = Math.Max(prior[key], EPSILON);
        }
    }

    private static void Normalize(Dictionary<string, double> distribution)
    {
        double total = distribution.Values.Sum();
        if (total <= 0) return;

        foreach (var key in distribution.Keys.ToList())
        {
            distribution[key] /= total;
        }
    }

    private static void ApplyEligibilityConstraints(
        Dictionary<string, double> prior,
        List<EligibilityConstraintV2> constraints,
        Dictionary<string, string> profileFields)
    {
        foreach (var constraint in constraints)
        {
            if (!prior.ContainsKey(constraint.TaxonomyNodeId)) continue;

            // Check if the required subject is present in the student's profile
            bool subjectPresent = ProfileHasSubject(profileFields, constraint.RequiredSubjectCode);

            if (!subjectPresent)
            {
                if (constraint.IsHardConstraint)
                {
                    // Hard: eliminate the node
                    prior[constraint.TaxonomyNodeId] = 0.0;
                }
                else
                {
                    // Soft: apply penalty multiplier
                    prior[constraint.TaxonomyNodeId] *= constraint.SoftPenaltyFactor;
                }
            }
        }
    }

    private static bool ProfileHasSubject(Dictionary<string, string> fields, string requiredSubjectCode)
    {
        // Check multiple field keys where subjects might be stored
        string[] subjectKeys = { "subjects", "stream", "subjectCombination", "mainSubjects" };

        foreach (var key in subjectKeys)
        {
            if (fields.TryGetValue(key, out var value))
            {
                if (value.Contains(requiredSubjectCode, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        // Also check if stream itself implies the subject (e.g. pcm implies MATHEMATICS, PHYSICS)
        if (fields.TryGetValue("subPhaseKey", out var spk) || fields.TryGetValue("stream", out spk))
        {
            return requiredSubjectCode switch
            {
                "MATHEMATICS" or "PHYSICS" => spk?.ToLower() is "pcm" or "pcmb" or "science" or "engineering",
                "BIOLOGY" => spk?.ToLower() is "pcb" or "pcmb" or "biology",
                "CHEMISTRY" => spk?.ToLower() is "pcm" or "pcb" or "pcmb" or "science",
                _ => false
            };
        }

        return false;
    }

    // ── Private: Information-Gain Question Selection ──────────────────────────

    private async Task<QuizQuestionV2> SelectNextQuestionAsync(
        AdaptiveQuizSession session,
        string phaseKey,
        string subPhaseKey,
        List<string> answeredIds,
        Dictionary<string, double> prior,
        ParsedProfileDto profile,
        List<EvidenceItemDto> evidenceItems = null,
        List<string> answeredTexts = null,
        List<string> askedIntents = null,
        string savedStage = null,
        List<string> usedFallbackIds = null,
        List<string> rejectedDomains = null,
        List<string> shownQuestionIds = null)
    {
        evidenceItems   ??= new List<EvidenceItemDto>();
        answeredTexts   ??= new List<string>();
        askedIntents    ??= new List<string>();
        usedFallbackIds ??= new List<string>();
        rejectedDomains ??= new List<string>();
        shownQuestionIds ??= new List<string>();
        var excludedQuestionIds = answeredIds.Concat(shownQuestionIds).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        // ── 1. Determine current quiz stage ───────────────────────────────────
        var stage = _stageEngine.GetCurrentStage(prior, answeredIds.Count, savedStage);

        System.Diagnostics.Debug.WriteLine(
            $"[AdaptiveQuiz] Stage={stage} AnsweredCount={answeredIds.Count} " +
            $"TopCandidate={prior.OrderByDescending(kv => kv.Value).FirstOrDefault().Key} " +
            $"TopScore={prior.OrderByDescending(kv => kv.Value).FirstOrDefault().Value:P1}");

        // ── 2. Select Next Question Deterministically ──────
        var targetCareers = new List<string>();
        if (!string.IsNullOrWhiteSpace(session.TargetCareersJson) && session.TargetCareersJson != "[]")
        {
            try
            {
                targetCareers = JsonSerializer.Deserialize<List<string>>(session.TargetCareersJson, _jsonOpts) ?? new();
            }
            catch { }
        }

        if (!targetCareers.Any())
        {
            targetCareers = prior.OrderByDescending(p => p.Value).Take(5).Select(p => p.Key).ToList();
        }

        QuizQuestionV2? selectedQuestion = null;
        var profileMajor = ResolveProfileMajor(session, profile);
        var assessmentContext = string.IsNullOrWhiteSpace(session.AssessmentContextJson) || session.AssessmentContextJson == "{}"
            ? new AssessmentContext()
            : JsonSerializer.Deserialize<AssessmentContext>(session.AssessmentContextJson, _jsonOpts) ?? new AssessmentContext();
        var selectedBaFields = assessmentContext.SelectedFields ?? new List<string>();
        var isBaProfile = session.PhaseKey.Equals("AfterGraduation", StringComparison.OrdinalIgnoreCase) &&
            QuizProfileClassifier.IsBachelorOfArts(assessmentContext.Degree, assessmentContext.Major);
        var hasSpecificSelectedFields = assessmentContext.PreferenceMode.Equals("SELECTED", StringComparison.OrdinalIgnoreCase)
            && selectedBaFields.Count > 0;

        // Always let the established adaptive selector choose first. Curated
        // field banks remain a fallback only when the normal candidate pool is
        // exhausted, so each next question follows the learner's updated answers.
        selectedQuestion = await _deterministicSelector.SelectNextQuestionAsync(
            session, excludedQuestionIds, profile, prior, targetCareers, session.Mode ?? "TargetCareer", stage.ToString(), rejectedDomains);
        while (selectedQuestion != null && await _duplicateDetector.IsDuplicateAsync(selectedQuestion, excludedQuestionIds))
        {
            // The active deterministic path historically skipped the semantic detector.
            // Exclude its duplicate result and let the same selector choose the next candidate.
            if (excludedQuestionIds.Contains(selectedQuestion.Id, StringComparer.OrdinalIgnoreCase))
            {
                selectedQuestion = null;
                break;
            }
            excludedQuestionIds.Add(selectedQuestion.Id);
            selectedQuestion = await _deterministicSelector.SelectNextQuestionAsync(
                session, excludedQuestionIds, profile, prior, targetCareers, session.Mode ?? "TargetCareer", stage.ToString(), rejectedDomains);
        }

        // A field can legitimately have fewer seeded questions than the assessment
        // length. Continue with the existing field-specific bank before using the
        // generic profile prompts. The field bank was previously implemented but
        // never called by this live selection path, which is why learners received
        // one bank question followed by the same generic fallback set.
        if (selectedQuestion == null)
        {
            QuizQuestionV2? fallbackQuestion = null;
            if (session.PhaseKey.Equals("AfterGraduation", StringComparison.OrdinalIgnoreCase) ||
                session.PhaseKey.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase))
            {
                var major = ResolveProfileMajor(session, profile);
                fallbackQuestion = FieldSpecificQuestionBank.GetNextQuestion(major, stage, usedFallbackIds, prior, session.PhaseKey, selectedBaFields, hasSpecificSelectedFields);

                // Semantic deduplication can reject a curated fallback if the
                // same concept was already covered by a seeded question. Advance
                // through this field's bank rather than abandoning it for generic
                // prompts after the first collision.
                while (fallbackQuestion != null && await _duplicateDetector.IsDuplicateAsync(fallbackQuestion, excludedQuestionIds))
                {
                    usedFallbackIds.Add(fallbackQuestion.Id);
                    fallbackQuestion = FieldSpecificQuestionBank.GetNextQuestion(major, stage, usedFallbackIds, prior, session.PhaseKey, selectedBaFields, hasSpecificSelectedFields);
                }

                // The generic bank starts with a fixed domain survey (including
                // unrelated careers). A learner who explicitly selected fields
                // must continue with a prompt about those fields instead.
                if (hasSpecificSelectedFields && fallbackQuestion?.Id.StartsWith("FB_GEN_", StringComparison.OrdinalIgnoreCase) == true)
                    fallbackQuestion = null;

                if (fallbackQuestion != null)
                {
                    Console.WriteLine($"[AdaptiveQuiz] Using field-specific fallback question {fallbackQuestion.Id} for major {major} at stage {stage}.");
                }
            }

            fallbackQuestion ??= CreateProfileFallbackQuestion(session, profile, prior, usedFallbackIds);
            if (fallbackQuestion != null)
            {
                if (await _duplicateDetector.IsDuplicateAsync(fallbackQuestion, excludedQuestionIds))
                    throw new InvalidOperationException("QUESTION_GENERATION_EXHAUSTED: No non-duplicate fallback question is available.");
                var existingQuestion = _context.QuizQuestionsV2.Local.FirstOrDefault(question =>
                    question.Id.Equals(fallbackQuestion.Id, StringComparison.OrdinalIgnoreCase));
                existingQuestion ??= await _context.QuizQuestionsV2.FirstOrDefaultAsync(question => question.Id == fallbackQuestion.Id);
                if (existingQuestion != null)
                {
                    selectedQuestion = existingQuestion;
                }
                else
                {
                    _context.QuizQuestionsV2.Add(fallbackQuestion);
                    selectedQuestion = fallbackQuestion;
                }
            }
        }

        if (selectedQuestion != null)
        {
            if (session.PhaseKey.Equals("After12th", StringComparison.OrdinalIgnoreCase))
            {
                var matchesSelectedStream = !string.IsNullOrEmpty(selectedQuestion.SubPhaseKey) &&
                    (selectedQuestion.SubPhaseKey.Equals(session.SubPhaseKey, StringComparison.OrdinalIgnoreCase) ||
                     (session.SubPhaseKey.Equals("pcmb", StringComparison.OrdinalIgnoreCase) &&
                      (selectedQuestion.SubPhaseKey.Equals("pcm", StringComparison.OrdinalIgnoreCase) ||
                       selectedQuestion.SubPhaseKey.Equals("pcb", StringComparison.OrdinalIgnoreCase))));

                if (!matchesSelectedStream)
                {
                    throw new InvalidOperationException("QUESTION_GENERATION_EXHAUSTED: " +
                        $"The selected question ({selectedQuestion.Id}) has stream '{selectedQuestion.SubPhaseKey}' which does not match the user's stream '{session.SubPhaseKey}'. " +
                        "Cross-stream contamination blocked.");
                }
            }
            
            // ── FINAL SAFETY ASSERTION (Profile Boundaries) ──
            AssessmentContext ctx = new();
            if (!string.IsNullOrWhiteSpace(session.AssessmentContextJson) && session.AssessmentContextJson != "{}")
            {
                try { ctx = JsonSerializer.Deserialize<AssessmentContext>(session.AssessmentContextJson, _jsonOpts) ?? new(); }
                catch { }
            }
            
            if (ctx.PreferenceMode.Equals("EXPLORE", StringComparison.OrdinalIgnoreCase) &&
                !ctx.CareerChange &&
                !_profileValidator.IsQuestionAllowed(selectedQuestion, ctx, answeredIds, out var reason))
            {
                throw new InvalidOperationException("QUESTION_GENERATION_EXHAUSTED: " +
                    $"Selected question '{selectedQuestion.Id}' is outside this learner's Explore field. Reason: {reason}");
            }

            return selectedQuestion;
        }

        // If no more valid questions exist, DO NOT fallback to random global questions.
        throw new InvalidOperationException("QUESTION_GENERATION_EXHAUSTED: " +
            "We're unable to find a suitable next question for this assessment stream.");


    }

    private static string ResolveProfileMajor(AdaptiveQuizSession session, ParsedProfileDto profile)
    {
        AssessmentContext context = new();
        if (!string.IsNullOrWhiteSpace(session.AssessmentContextJson) && session.AssessmentContextJson != "{}")
        {
            try { context = JsonSerializer.Deserialize<AssessmentContext>(session.AssessmentContextJson, _jsonOpts) ?? new(); }
            catch { }
        }

        if (!string.IsNullOrWhiteSpace(context.Major)) return context.Major;
        if (session.PhaseKey.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase))
        {
            var postgraduateMajor = !string.IsNullOrWhiteSpace(context.PostGraduationMajor)
                ? context.PostGraduationMajor
                : profile.ExtractedFields.TryGetValue("postGraduationMajor", out var submittedPostgraduateMajor)
                    ? submittedPostgraduateMajor
                    : string.Empty;
            var previousDegree = !string.IsNullOrWhiteSpace(context.PreviousDegree)
                ? context.PreviousDegree
                : profile.ExtractedFields.TryGetValue("previousDegree", out var submittedPreviousDegree)
                    ? submittedPreviousDegree
                    : string.Empty;
            if (!string.IsNullOrWhiteSpace(postgraduateMajor) || !string.IsNullOrWhiteSpace(previousDegree))
                return $"{previousDegree} {postgraduateMajor}".Trim();
        }
        if (profile.ExtractedFields.TryGetValue("major", out var submittedMajor) && !string.IsNullOrWhiteSpace(submittedMajor))
            return submittedMajor;
        if (!string.IsNullOrWhiteSpace(profile.Education?.Specialization)) return profile.Education.Specialization;
        return "your chosen field";
    }

    private static QuizQuestionV2? CreateProfileFallbackQuestion(
        AdaptiveQuizSession session,
        ParsedProfileDto profile,
        Dictionary<string, double> prior,
        List<string> usedFallbackIds)
    {
        if (!session.PhaseKey.Equals("AfterGraduation", StringComparison.OrdinalIgnoreCase) &&
            !session.PhaseKey.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        if (usedFallbackIds.Count >= MAX_PROFILE_FALLBACK_QUESTIONS)
        {
            return null;
        }


        AssessmentContext context = new();
        if (!string.IsNullOrWhiteSpace(session.AssessmentContextJson) && session.AssessmentContextJson != "{}")
        {
            try { context = JsonSerializer.Deserialize<AssessmentContext>(session.AssessmentContextJson, _jsonOpts) ?? new(); }
            catch { }
        }

        string major = session.PhaseKey.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase)
            ? $"{context.PreviousDegree} {context.PostGraduationMajor}".Trim()
            : context.Major;
        if (string.IsNullOrWhiteSpace(major)) major = context.Major;
        if (string.IsNullOrWhiteSpace(major) && profile.ExtractedFields.TryGetValue("major", out var submittedMajor))
            major = submittedMajor;
        if (string.IsNullOrWhiteSpace(major)) major = "your chosen field";

        bool isBaProfile = session.PhaseKey.Equals("AfterGraduation", StringComparison.OrdinalIgnoreCase) &&
            QuizProfileClassifier.IsBachelorOfArts(context.Degree, context.Major);
        bool hasExplicitFieldSelection = session.PhaseKey.Equals("AfterGraduation", StringComparison.OrdinalIgnoreCase) &&
            context.PreferenceMode.Equals("SELECTED", StringComparison.OrdinalIgnoreCase) && context.SelectedFields.Any();
        var selectedFieldTargets = hasExplicitFieldSelection
            ? context.SelectedFields.SelectMany(field =>
            {
                var token = new string(field.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray())
                    .Replace("engineering", string.Empty, StringComparison.Ordinal);
                return prior.Keys.Where(key => !string.IsNullOrEmpty(token) &&
                    key.Contains(token, StringComparison.OrdinalIgnoreCase));
            }).Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var selectedBaTargets = isBaProfile && hasExplicitFieldSelection
            ? FieldSpecificQuestionBank.GetSelectedFieldTargetNodeIds(context.SelectedFields)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var selectedTargets = selectedFieldTargets.Count > 0 ? selectedFieldTargets : selectedBaTargets;
        var candidates = (hasExplicitFieldSelection
                ? prior.Where(item => selectedTargets.Contains(item.Key))
                : prior.AsEnumerable())
            .Where(item => item.Value > 0)
            .OrderByDescending(item => item.Value)
            .Take(4)
            .Select(item => item.Key)
            .ToList();

        if (hasExplicitFieldSelection && !candidates.Any())
        {
            candidates = selectedTargets.Count > 0
                ? selectedTargets.Take(4).ToList()
                : prior.OrderByDescending(item => item.Value).Take(4).Select(item => item.Key).ToList();
        }

        if (!candidates.Any())
        {
            throw new InvalidOperationException($"CreateProfileFallbackQuestion failed: candidates is empty. Prior count: {prior.Count}, Phase: {session.PhaseKey}");
        }

        int sequence = usedFallbackIds.Count + 1;

        var fallbackQuestions = session.PhaseKey.Equals("AfterPostGraduation", StringComparison.OrdinalIgnoreCase)
            ? new[]
            {
                new { Prompt = $"How would you most like to apply your advanced knowledge of {major}?", Options = new[] { "Conduct research and contribute new evidence", "Teach or design learning for others", "Advise organizations or public programmes", "Lead specialist work in an industry role" } },
                new { Prompt = $"Which challenge in {major} would you most enjoy taking ownership of?", Options = new[] { "Developing a rigorous study or evaluation", "Helping learners understand difficult ideas", "Turning evidence into practical policy or programme changes", "Applying specialist expertise to solve an organizational problem" } },
                new { Prompt = $"What would make the next stage of your career in {major} meaningful?", Options = new[] { "Publishing useful research", "Seeing students or teams grow", "Improving outcomes for communities", "Building a trusted expert reputation" } },
                new { Prompt = $"Which postgraduate strength do you want to use most often?", Options = new[] { "Research methods and critical analysis", "Teaching and clear communication", "Policy analysis and social impact", "Specialist practice and leadership" } }
            }
            : hasExplicitFieldSelection
            ? new[]
            {
                new { Prompt = $"You selected {string.Join(", ", context.SelectedFields)}. Which challenge in this field would you most like to take on?", Options = new[] { "Research a question and build a well-supported answer", "Create or improve something people can use", "Apply specialist knowledge to a practical problem", "Coordinate people and resources to deliver a project" } },
                new { Prompt = $"Within {string.Join(", ", context.SelectedFields)}, which kind of work best fits your strengths?", Options = new[] { "Analyze information and explain what it means", "Develop a new idea, design, or approach", "Help people or organizations solve a real need", "Plan work and bring a project through to completion" } },
                new { Prompt = $"What would you most like to contribute in {string.Join(", ", context.SelectedFields)}?", Options = new[] { "Useful research and reliable evidence", "A creative solution to an important problem", "Better outcomes for people or communities", "A successful project delivered with others" } },
                new { Prompt = $"Which outcome would make work in {string.Join(", ", context.SelectedFields)} feel successful?", Options = new[] { "A clear discovery that informs decisions", "A new idea put into practice", "A problem solved for the people affected", "A complex effort completed to a high standard" } }
            }
            : isBaProfile
            ? new[]
            {
                new { Prompt = $"Within your selected BA path(s) — {string.Join(", ", context.SelectedFields ?? new List<string>())} — which skill would you most like to demonstrate?", Options = new[] { "Research a question and support conclusions with evidence", "Write or edit clear content for a specific audience", "Explain ideas and help people learn", "Analyze a social issue and recommend a practical response" } },
                new { Prompt = $"For {major}, which kind of task feels closest to your strengths?", Options = new[] { "Interview people and interpret what they share", "Compare sources and produce accurate writing", "Support learners or community members", "Review evidence and prepare policy or programme recommendations" } },
                new { Prompt = $"What would you most like to contribute through {major}?", Options = new[] { "Useful research about people and society", "Reliable reporting or thoughtful publications", "Better learning and access to support", "More effective public or community programmes" } },
                new { Prompt = $"Which outcome would make work in {major} feel successful?", Options = new[] { "A strong study that informs decisions", "A story or publication that reaches its audience", "A learner or community member making progress", "A policy or programme improving people's lives" } }
            }
            : new[]
        {
            new
            {
                Prompt = $"Within {major}, which type of work would you find most rewarding?",
                Options = new[] 
                {
                    "Solving complex technical problems that require deep focus",
                    "Managing project timelines, budgets, and coordinating with teams",
                    "Directly interacting with clients to understand and solve their needs",
                    "Researching trends and building long-term strategic roadmaps"
                }
            },
            new
            {
                Prompt = $"When facing a challenge in {major}, which contribution would you most want to make?",
                Options = new[]
                {
                    "Analyzing the core issue and proposing a data-driven solution",
                    "Organizing the team's efforts to ensure smooth execution",
                    "Communicating the issue clearly to stakeholders and managing expectations",
                    "Innovating a completely new approach that disrupts the status quo"
                }
            },
            new
            {
                Prompt = $"Which working style best matches the kind of career you want in {major}?",
                Options = new[]
                {
                    "Independent, focused work with clear deliverables",
                    "Highly structured, process-oriented work in a team setting",
                    "Collaborative, discussion-based work with a variety of people",
                    "Flexible, exploratory work with room for creativity"
                }
            },
            new
            {
                Prompt = $"What outcome would make you feel most successful in a {major} role?",
                Options = new[]
                {
                    "Developing a highly efficient and flawless technical solution",
                    "Delivering a complex project on time and under budget",
                    "Building strong relationships and receiving positive client feedback",
                    "Pioneering a new methodology or product that drives the industry forward"
                }
            }
        };

        var selected = fallbackQuestions[(sequence - 1) % fallbackQuestions.Length];

        var options = selected.Options.Select((label, index) => new
        {
            index,
            text = label,
            signals = new[]
            {
                new { nodeId = candidates[index % candidates.Count], weight = 0.8 }
            }
        }).ToList();

        return new QuizQuestionV2
        {
            Id = $"FB_PROFILE_{session.Id}_{sequence:D2}",
            PhaseKey = session.PhaseKey,
            Domain = hasExplicitFieldSelection
                ? DomainTaxonomyEngine.Classify("", "", "", broadField: string.Join(" ", context.SelectedFields)).PrimaryDomain
                : context.EligibleDomains.FirstOrDefault() ?? "Universal",
            Subdomain = hasExplicitFieldSelection
                ? context.SelectedFields.FirstOrDefault() ?? string.Empty
                : context.EligibleSubdomains.FirstOrDefault() ?? string.Empty,
            QuestionType = "ProfileFallback",
            LanguageComplexity = "Simple",
            Dimension = "profile_work_preference",
            Text = selected.Prompt,
            Intent = "Profile-specific career preference",
            TargetDiscriminatesJson = JsonSerializer.Serialize(candidates),
            OptionsJson = JsonSerializer.Serialize(options),
            DisplayOrder = 1000 + sequence,
            IsActive = true
        };
    }

    private static double CalculateExpectedInformationGain(QuizQuestionV2 q, Dictionary<string, double> prior)
    {
        // Shannon entropy of current distribution
        double currentEntropy = ShannonEntropy(prior);

        List<List<OptionSignal>> optionSignals;
        try
        {
            optionSignals = ParseOptionsJson(q.OptionsJson).Select(o => o.Signals).ToList();
        }
        catch
        {
            return 0.0;
        }

        if (!optionSignals.Any()) return 0.0;

        // For each possible answer, calculate the posterior and its entropy
        // Weight by the probability of each answer happening (based on prior)
        double expectedPostEntropy = 0.0;
        int numOptions = optionSignals.Count;

        foreach (var signals in optionSignals)
        {
            // Simulate a Bayesian update with this answer
            var simPrior = new Dictionary<string, double>(prior);
            BayesianUpdate(simPrior, signals);
            Normalize(simPrior);

            double postEntropy = ShannonEntropy(simPrior);

            // Weight by uniform probability over options (we don't know what the student will pick)
            expectedPostEntropy += postEntropy / numOptions;
        }

        return currentEntropy - expectedPostEntropy;
    }

    private static double ShannonEntropy(Dictionary<string, double> distribution)
    {
        double entropy = 0.0;
        foreach (var p in distribution.Values)
        {
            if (p > 0)
                entropy -= p * Math.Log2(p);
        }
        return entropy;
    }

    // ── Private: Stopping Criterion ───────────────────────────────────────────

    private static bool ShouldStop(AdaptiveQuizSession session, int questionsAnswered)
    {
        int minQuestions = MIN_QUESTIONS;
        int maxQuestions = MAX_QUESTIONS;
        if (string.Equals(session.PhaseKey, "After12th", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(session.SubPhaseKey, "pcb", StringComparison.OrdinalIgnoreCase))
        {
            minQuestions = 16;
            maxQuestions = 18;
        }

        // Do not let a temporary score lead end the assessment before the learner
        // has answered enough questions for a useful career recommendation.
        if (questionsAnswered < minQuestions) return false;

        // Always finish by the server-defined maximum, even when scores remain close.
        if (questionsAnswered >= maxQuestions) return true;

        var careerScores = string.IsNullOrEmpty(session.CareerScoresJson) 
            ? new Dictionary<string, dynamic>() 
            : JsonSerializer.Deserialize<Dictionary<string, dynamic>>(session.CareerScoresJson, _jsonOpts) ?? new();

        var scoredLeaders = new List<(string Id, double Score)>();
        foreach (var kvp in careerScores)
        {
            double maxScore = kvp.Value.TryGetProperty("MaxPossibleScore", out JsonElement maxEl) ? maxEl.GetDouble() : 0;
            double rawScore = kvp.Value.TryGetProperty("RawScore", out JsonElement rawEl) ? rawEl.GetDouble() : 0;
            double normalized = maxScore > 0 ? (rawScore / maxScore) * 100 : 0;
            scoredLeaders.Add((kvp.Key, normalized));
        }

        if (scoredLeaders.Count >= 2)
        {
            scoredLeaders = scoredLeaders.OrderByDescending(x => x.Score).ToList();
            double topScore = scoredLeaders[0].Score;
            double secondScore = scoredLeaders[1].Score;

            // Allow early completion only after the full minimum question count.
            if (questionsAnswered >= minQuestions && topScore > 0 && (topScore - secondScore) > 15.0)
            {
                return true;
            }
        }

        return false;
    }

    // ── Private: Result Generation ────────────────────────────────────────────

    private async Task<List<EvidenceItemDto>> BuildEvidenceLogAsync(
        List<SessionAnswer> rawAnswers,
        Dictionary<string, double> finalDistribution,
        Dictionary<string, CareerTaxonomyV2> nodeMap)
    {
        var evidence = new List<EvidenceItemDto>();

        foreach (var answer in rawAnswers)
        {
            var q = await _context.QuizQuestionsV2.FindAsync(answer.QuestionId);
            if (q == null) continue;

            var options = ParseOptionsJson(q.OptionsJson);
            if (answer.OptionIndex < 0 || answer.OptionIndex >= options.Count) continue;

            var selectedOption = options[answer.OptionIndex];

            var topSignals = selectedOption.Signals
                .OrderByDescending(s => s.Weight)
                .Take(3)
                .Select(s => new SignalContributionDto
                {
                    NodeId = s.NodeId,
                    NodeName = nodeMap.TryGetValue(s.NodeId, out var n) ? n.Name : s.NodeId,
                    Weight = s.Weight
                })
                .ToList();

            evidence.Add(new EvidenceItemDto
            {
                QuestionText = q.Text,
                SelectedOptionText = selectedOption.Text,
                SignalsContributed = topSignals
            });
        }

        // Deduplicate evidence to prevent repetitive "Why this matches you" statements in the UI
        return evidence
            .GroupBy(e => e.SelectedOptionText?.Trim().ToLowerInvariant())
            .Select(g => g.First())
            .ToList();
    }

    private static EducationPathDto BuildEducationPath(CareerTaxonomyV2? node, string phaseKey, List<EvidenceItemDto> evidence)
    {
        if (node == null) return new EducationPathDto();

        List<string>? entranceExams = null;
        List<string>? requiredDegrees = null;
        try
        {
            entranceExams = JsonSerializer.Deserialize<List<string>>(node.EntranceExamsJson ?? "[]", _jsonOpts);
            requiredDegrees = JsonSerializer.Deserialize<List<string>>(node.RequiredDegreesJson ?? "[]", _jsonOpts);
        }
        catch { }

        var nextStep = DeriveNextStep(node);
        
        // Dynamically adjust NextStep based on evidence (e.g., if they prefer practical execution)
        if (evidence.Any(e => e.SelectedOptionText.Contains("practical", StringComparison.OrdinalIgnoreCase) || e.SelectedOptionText.Contains("hands-on", StringComparison.OrdinalIgnoreCase)))
        {
            nextStep = "Focus on securing internships and building a hands-on portfolio before applying for roles.";
        }

        return new EducationPathDto
        {
            Stage = phaseKey switch
            {
                "After10th" => "After 10th Grade",
                "After12th" => "After 12th Grade",
                "AfterGraduation" => "After Graduation",
                _ => phaseKey
            },
            DegreeType = requiredDegrees?.FirstOrDefault() ?? string.Empty,
            Duration = DeriveDuration(node),
            EntranceExams = entranceExams ?? new List<string>(),
            TopInstitutionTypes = JsonSerializer.Deserialize<List<string>>(node.InstitutionTypesJson ?? "[]", _jsonOpts) ?? new(),
            TypicalSubjects = new List<string>(),
            NextStep = nextStep
        };
    }

    private static string DeriveDuration(CareerTaxonomyV2 node)
    {
        return node.Id switch
        {
            var n when n.StartsWith("sci.eng") => "4 years (B.Tech/B.E.)",
            var n when n.StartsWith("sci.med.mbbs") => "5.5 years (MBBS + Internship)",
            var n when n.StartsWith("sci.med.bds") => "5 years (BDS + Internship)",
            var n when n.StartsWith("sci.med.pharma") => "4 years (B.Pharm) or 2 years (D.Pharm)",
            var n when n.StartsWith("com.ca") => "3–5 years (CA Foundation → Final)",
            var n when n.StartsWith("design.arch") => "5 years (B.Arch)",
            var n when n.StartsWith("arts.law") => "5 years (BA LLB) or 3 years (LLB after graduation)",
            var n when n.StartsWith("mgmt.mba") => "2 years (Full-time MBA)",
            var n when n.StartsWith("voc.iti") => "1–2 years (ITI Trade Certificate)",
            var n when n.StartsWith("voc.diploma") => "3 years (Polytechnic Diploma)",
            _ => string.Empty
        };
    }

    private static string DeriveNextStep(CareerTaxonomyV2 node)
    {
        return node.Level switch
        {
            2 or 3 => "Explore specializations within this field and prepare for entrance exams",
            4 => "Focus on the specific role and target companies/institutions",
            _ => "Explore this domain further and select a specific degree program"
        };
    }

    private static List<string> GetSkillGap(CareerTaxonomyV2? node, List<EvidenceItemDto> evidence)
    {
        if (node == null) return new List<string>();
        try
        {
            var baseGaps = JsonSerializer.Deserialize<List<string>>(node.RecommendedCoursesJson ?? "[]", _jsonOpts) ?? new();
            
            // Filter out skills the user already claims to have or don't need based on evidence
            var knownSkills = new List<string>();
            foreach(var item in evidence)
            {
                if (item.SelectedOptionText.Contains("strong", StringComparison.OrdinalIgnoreCase) || 
                    item.SelectedOptionText.Contains("proficient", StringComparison.OrdinalIgnoreCase))
                {
                    knownSkills.Add(item.SelectedOptionText.ToLower());
                }
            }

            var finalGaps = baseGaps.Where(gap => !knownSkills.Any(ks => ks.Contains(gap.ToLower()))).ToList();
            
            if (evidence.Any(e => e.SelectedOptionText.Contains("analytical", StringComparison.OrdinalIgnoreCase)))
            {
                finalGaps.Insert(0, "Advanced Data Modeling & Analytics");
            }
            else if (evidence.Any(e => e.SelectedOptionText.Contains("management", StringComparison.OrdinalIgnoreCase)))
            {
                finalGaps.Insert(0, "Agile Project Management & Leadership");
            }

            return finalGaps.Take(5).ToList(); // Ensure we don't return too many
        }
        catch { return new List<string>(); }
    }

    private static List<string> GetRecommendedCourses(CareerTaxonomyV2? node)
    {
        if (node == null) return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(node.RecommendedCoursesJson ?? "[]", _jsonOpts) ?? new();
        }
        catch { return new List<string>(); }
    }

    private static List<string> GetTypicalRoles(CareerTaxonomyV2? node)
    {
        if (node == null) return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(node.TypicalRolesJson ?? "[]", _jsonOpts) ?? new();
        }
        catch { return new List<string>(); }
    }

    // ── Private: Confidence Calculation ──────────────────────────────────────

    private static double CalculateConfidence(Dictionary<string, double> distribution)
    {
        if (!distribution.Any()) return 0.0;

        double entropy = ShannonEntropy(distribution);
        double maxEntropy = Math.Log2(Math.Max(distribution.Count, 1));

        if (maxEntropy <= 0) return 1.0;

        // Confidence is inversely proportional to entropy
        double confidence = 1.0 - (entropy / maxEntropy);

        // Additionally consider the gap between top-1 and top-2
        var sorted = distribution.OrderByDescending(kv => kv.Value).ToList();
        double leadGap = sorted.Count >= 2 ? sorted[0].Value - sorted[1].Value : sorted[0].Value;
        double gapBonus = Math.Min(leadGap * 0.5, 0.2); // Max 20% bonus from gap

        return Math.Min(confidence + gapBonus, 1.0);
    }

    private static List<EvidenceItemDto> FilterEvidenceForCareer(List<EvidenceItemDto> allEvidence, string canonicalCareerId)
    {
        if (string.IsNullOrEmpty(canonicalCareerId) || allEvidence == null) return allEvidence ?? new List<EvidenceItemDto>();
        
        return allEvidence.Where(e => e.SignalsContributed != null && e.SignalsContributed.Any(s => 
            !string.IsNullOrEmpty(s.NodeId) && (
                s.NodeId.Equals(canonicalCareerId, StringComparison.OrdinalIgnoreCase) ||
                s.NodeId.StartsWith(canonicalCareerId + ".", StringComparison.OrdinalIgnoreCase) ||
                canonicalCareerId.StartsWith(s.NodeId + ".", StringComparison.OrdinalIgnoreCase)
            )
        )).ToList();
    }

    // ── Private: Explanation Generation ──────────────────────────────────────

    private static string GenerateEvidenceBasedExplanation(
        List<TaxonomyMatchDto> topPredictions,
        List<EvidenceItemDto> evidence,
        double confidence)
    {
        if (!evidence.Any())
            return "Insufficient data to generate a personalized explanation.";

        var explanations = new List<string>();
        
        var strongSignals = evidence
            .SelectMany(e => e.SignalsContributed)
            .GroupBy(s => s.NodeName)
            .Select(g => new { Name = g.Key, Weight = g.Sum(x => x.Weight) })
            .OrderByDescending(x => x.Weight)
            .Take(3)
            .Select(x => x.Name.ToLower())
            .ToList();

        if (strongSignals.Any())
        {
            if (strongSignals.Count > 1)
            {
                var last = strongSignals.Last();
                var rest = strongSignals.Take(strongSignals.Count - 1);
                return $"Your responses showed stronger alignment with {string.Join(", ", rest)} and {last} than with the other assessed paths.";
            }
            return $"Your responses showed stronger alignment with {strongSignals.First()} than with the other assessed paths.";
        }

        return "Your answers consistently pointed towards these specialized areas.";
    }

    private static List<string> DetectContradictions(Dictionary<string, double> prior, List<TaxonomyMatchDto> topPredictions)
    {
        var warnings = new List<string>();
        if (topPredictions.Count >= 2)
        {
            var top1 = topPredictions[0];
            var top2 = topPredictions[1];
            // If the top 2 predictions are very close but belong to completely different Level 1 domains
            if (top1.Probability - top2.Probability < 0.05)
            {
                var top1Domain = top1.NodeId.Split('.').FirstOrDefault();
                var top2Domain = top2.NodeId.Split('.').FirstOrDefault();
                if (top1Domain != top2Domain && !string.IsNullOrEmpty(top1Domain) && !string.IsNullOrEmpty(top2Domain))
                {
                    warnings.Add($"Your answers show mixed signals. You are showing strong alignment for both {top1.Name} and {top2.Name}, which are fundamentally different fields. Consider which core skills you truly prefer applying every day.");
                }
            }
        }
        return warnings;
    }


    private static List<HierarchicalNodeDto> BuildHierarchicalBreakdown(Dictionary<string, double> prior, Dictionary<string, CareerTaxonomyV2> nodeMap)
    {
        // Calculate aggregate probabilities for each node level (e.g. sci = sum(sci.*), sci.eng = sum(sci.eng.*))
        var aggregates = new Dictionary<string, double>();
        foreach (var kvp in prior)
        {
            var parts = kvp.Key.Split('.');
            string currentPath = "";
            for (int i = 0; i < parts.Length; i++)
            {
                currentPath = i == 0 ? parts[i] : $"{currentPath}.{parts[i]}";
                if (!aggregates.ContainsKey(currentPath)) aggregates[currentPath] = 0.0;
                // Only add the value at the exact key, since we want to avoid double counting if the prior has parent and child?
                // Wait, prior only has leaf nodes or whatever the signals targeted. 
                // We just sum them. If prior contains a parent and a child, this will double count the child's probability for the parent.
                // Assuming prior sums to 1 across whatever nodes were updated. We should just sum them.
                if (i < parts.Length - 1) { // If it's a parent, add the value of the leaf
                    aggregates[currentPath] = Math.Max(aggregates[currentPath], kvp.Value);
                }
            }
            // Add the leaf value to itself
            if (!aggregates.ContainsKey(kvp.Key)) aggregates[kvp.Key] = 0.0;
            if (parts.Length == 1) aggregates[kvp.Key] += kvp.Value; // If it's a root node with no children, just add it.
            // Wait, a cleaner approach:
        }

        // Simpler aggregation:
        aggregates.Clear();
        foreach (var kvp in prior)
        {
            var parts = kvp.Key.Split('.');
            string currentPath = "";
            for (int i = 0; i < parts.Length; i++)
            {
                currentPath = i == 0 ? parts[i] : $"{currentPath}.{parts[i]}";
                if (!aggregates.ContainsKey(currentPath)) aggregates[currentPath] = 0.0;
                aggregates[currentPath] += kvp.Value;
            }
        }

        // Build the tree
        var roots = new List<HierarchicalNodeDto>();
        var dtoMap = new Dictionary<string, HierarchicalNodeDto>();

        var sortedKeys = aggregates.Keys.OrderBy(k => k.Length).ToList();
        
        foreach (var key in sortedKeys)
        {
            var nodeName = nodeMap.TryGetValue(key, out var taxonomyNode) ? taxonomyNode.Name : CareerDisplayNameService.GetDisplayName(key);
            var dto = new HierarchicalNodeDto
            {
                NodeId = key,
                Name = nodeName,
                Probability = Math.Round(aggregates[key], 4),
                MatchPercent = Math.Round(aggregates[key] * 100, 1)
            };
            dtoMap[key] = dto;

            var lastDot = key.LastIndexOf('.');
            if (lastDot > 0)
            {
                var parentKey = key.Substring(0, lastDot);
                if (dtoMap.TryGetValue(parentKey, out var parentDto))
                {
                    parentDto.Children.Add(dto);
                }
            }
            else
            {
                roots.Add(dto);
            }
        }

        foreach (var dto in dtoMap.Values)
        {
            dto.Children = dto.Children.OrderByDescending(c => c.Probability).ToList();
        }

        return roots.OrderByDescending(r => r.Probability).ToList();
    }

    // ── Private: Mapping Helpers ──────────────────────────────────────────────

    private static AdaptiveQuizSessionDto MapSessionToDto(AdaptiveQuizSession session)
        => new()
        {
            SessionId = session.Id,
            PhaseKey = session.PhaseKey,
            SubPhaseKey = session.SubPhaseKey,
            Status = session.Status,
            StartedAt = session.StartedAt
        };

    private static AdaptiveQuizNextResponse MapQuestionToResponse(
        QuizQuestionV2 q,
        int questionNumber,
        int totalQuestions,
        Dictionary<string, double> prior)
    {
        var options = ParseOptionsJson(q.OptionsJson);

        return new AdaptiveQuizNextResponse
        {
            Question = new QuizQuestionV2Dto
            {
                Id = q.Id,
                Text = q.Text,
                QuestionType = q.QuestionType,
                Options = options.Select(o => new QuizOptionV2Dto { Index = o.Index, Text = o.Text }).ToList()
            },
            IsComplete = false,
            QuestionNumber = questionNumber,
            EstimatedRemaining = Math.Max(0, totalQuestions - questionNumber),
            Progress = totalQuestions == 0 ? 0.0 : Math.Round((double)questionNumber / totalQuestions, 2),
            CurrentLeadingDomainNames = GetTopDomainNames(prior, 3)
        };
    }

    private static List<string> GetTopDomainNames(Dictionary<string, double> prior, int topN)
    {
        return prior
            .OrderByDescending(kv => kv.Value)
            .Take(topN)
            .Select(kv => CareerDisplayNameService.GetDisplayName(kv.Key))
            .ToList();
    }

    private async Task<QuizV2ResultDto> MapResultToDtoAsync(QuizResultV2 result)
    {
        List<TaxonomyMatchDto>? predictions = null;
        List<EvidenceItemDto>? evidence = null;
        EducationPathDto? educationPath = null;
        List<string>? skillGap = null;
        List<string>? courses = null;

        try { predictions = JsonSerializer.Deserialize<List<TaxonomyMatchDto>>(result.TopPredictionsJson, _jsonOpts); } catch { }
        try { evidence = JsonSerializer.Deserialize<List<EvidenceItemDto>>(result.EvidenceLogJson, _jsonOpts); } catch { }
        try { educationPath = JsonSerializer.Deserialize<EducationPathDto>(result.EducationPathJson, _jsonOpts); } catch { }
        try { skillGap = JsonSerializer.Deserialize<List<string>>(result.SkillGapJson, _jsonOpts); } catch { }
        try { courses = JsonSerializer.Deserialize<List<string>>(result.RecommendedCoursesJson, _jsonOpts); } catch { }

        predictions ??= new();
        
        // Populate Canonical Education using same logic as SubmitSession
        var session = await _context.AdaptiveQuizSessions.FirstOrDefaultAsync(s => s.Id == result.SessionId);
        var allNodes = await _context.CareerTaxonomyV2.ToListAsync();
        var nodeMap = allNodes.ToDictionary(n => n.Id, n => n);
        
        foreach (var p in predictions)
        {
            if (string.IsNullOrEmpty(p.Name) && nodeMap.TryGetValue(p.NodeId, out var nd))
            {
                p.Name = nd.Name;
            }
        }
        
        CareerTaxonomyV2? degreeNode = null;
        var primaryId = result.PrimaryTaxonomyNodeId;
        nodeMap.TryGetValue(primaryId ?? "", out var primaryNode);
        
        if (primaryNode != null && !string.IsNullOrEmpty(primaryNode.RequiredDegreesJson))
        {
            try
            {
                var reqDegrees = JsonSerializer.Deserialize<List<string>>(primaryNode.RequiredDegreesJson, _jsonOpts);
                if (reqDegrees != null && reqDegrees.Any())
                {
                    if (nodeMap.TryGetValue(reqDegrees.First(), out var reqDegNode))
                    {
                        degreeNode = reqDegNode;
                    }
                }
            }
            catch { }
        }

        string qualId = degreeNode?.Id ?? "Unavailable";
        string qualName = degreeNode?.Name ?? "Unavailable";
        int? durationValue = null;
        string durationStatus = "Unavailable";
        
        if (degreeNode != null && !string.IsNullOrEmpty(degreeNode.TimeToCompleteQualification))
        {
            var match = System.Text.RegularExpressions.Regex.Match(degreeNode.TimeToCompleteQualification, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int d))
            {
                durationValue = d;
                durationStatus = "Success";
            }
        }

        var profileFieldsNext = session == null || string.IsNullOrEmpty(session.BackgroundProfileJson) ? new Dictionary<string, string>() : JsonSerializer.Deserialize<Dictionary<string, string>>(session.BackgroundProfileJson, _jsonOpts) ?? new();

        int? expGradYear = null;
        double? remainingDuration = null;
        string gradStatus = "Unavailable";
        string currentStatus = profileFieldsNext.TryGetValue("employmentStatus", out var empStat) ? empStat : "Unknown";
        int currentYearValue = DateTime.UtcNow.Year;

        if (profileFieldsNext.TryGetValue("graduationYear", out var gradYearStr) && int.TryParse(gradYearStr, out int gY))
        {
            expGradYear = gY;
            gradStatus = "Success";
            remainingDuration = Math.Max(0, gY - currentYearValue);
        }
        else if (durationValue.HasValue)
        {
            expGradYear = currentYearValue + durationValue.Value;
            gradStatus = "Estimated";
            remainingDuration = durationValue.Value;
        }

        CanonicalEducationDto? canonicalEducation = null;
        if (qualId != "Unavailable" && qualName != "Unavailable")
        {
            canonicalEducation = new CanonicalEducationDto
            {
                QualificationId = qualId,
                QualificationName = qualName,
                QualificationLevel = "Degree",
                Duration = durationValue,
                DurationUnit = "years",
                DurationStatus = durationStatus,
                ExpectedGraduationYear = expGradYear,
                RemainingDuration = remainingDuration,
                CurrentStatus = currentStatus,
                GraduationStatus = gradStatus,
                Source = "CareerTaxonomyV2"
            };
        }
        
        var bestMatchName = predictions.FirstOrDefault()?.Name ?? "Career exploration";
        var compScoreText = predictions.Any() ? $"{Math.Round(predictions[0].MatchPercent)}/100 current profile alignment." : "0/100 current profile alignment.";

        var targetCareerComparison = new List<ScoredCareerDto>();
        foreach (var prediction in predictions)
        {
            targetCareerComparison.Add(await CreateScoredCareerAsync(
                prediction.NodeId,
                prediction.Name,
                (int)Math.Round(prediction.MatchPercent),
                nodeMap));
        }

        return new QuizV2ResultDto
        {
            ResultId = result.Id,
            Recommendations = predictions,
            PhaseKey = result.PhaseKey,
            PrimaryRecommendation = predictions.FirstOrDefault() ?? new(),
            AlternativeRecommendations = predictions.Skip(1).ToList(),
            TargetCareerComparison = targetCareerComparison,
            ConfidenceScore = result.ConfidenceScore,
            EvidenceItems = FilterEvidenceForCareer(evidence ?? new(), predictions.FirstOrDefault()?.NodeId),
            WhyRecommended = result.WhyRecommended,
            SkillGap = skillGap ?? new(),
            EducationPath = educationPath,
            Education = canonicalEducation,
            RecommendedCourses = courses ?? new(),
            ModelUsed = result.ModelUsed,
            ResultStatus = result.ConfidenceScore >= 40 ? "Success" : "InsufficientConfidence",
            GeneratedAt = result.GeneratedAt,
            PrimaryDomain = new ScoredNodeDto { Name = predictions.FirstOrDefault()?.Name ?? "Career exploration", Score = 0.0 },
            Fields = new List<ScoredNodeDto>(),
            Specializations = new List<ScoredNodeDto>(),
            CareerFamilies = new List<ScoredNodeDto>(),
            Confidence = new ConfidenceDto { Level = "Moderate", Score = result.ConfidenceScore },
            Evidence = new List<string>(),
            Eligibility = new EligibilityStatusDto { IsEligible = true, Message = "Mock Eligibility" },
            Explanation = new ExplanationDto { WhyRecommended = result.WhyRecommended },
            
            // Reconstructed missing fields
            BestMatchName = bestMatchName,
            CompatibilityScoreText = compScoreText,
            ConfidenceText = result.ConfidenceScore >= 80 ? "High" : (result.ConfidenceScore >= 50 ? "Medium" : "Low"),
            StrongestCompetencies = new List<string>(),
            DevelopmentAreas = new List<string>(),
            RecommendedNextSteps = new List<string> { "Review related subdomains in detail", "Explore required certifications for the primary specialization" },
            SpecializationText = "",
            ExpectedSalaryRange = "",
            ExpectedTimeToJobReady = "",
            Career = primaryNode != null ? new CanonicalCareerDto { Id = primaryNode.Id, Name = bestMatchName } : null,
            Compatibility = new CanonicalCompatibilityDto { Score = Math.Round(result.ConfidenceScore, 2), Level = compScoreText, Explanation = result.WhyRecommended },
            Salary = await FetchCanonicalSalaryAsync(bestMatchName, predictions.FirstOrDefault()?.NodeId ?? ""),
            Forecast = await FetchCanonicalForecastAsync(bestMatchName, predictions.FirstOrDefault()?.NodeId ?? "")
        };
    }

    private static string BuildHierarchyPath(string nodeId, Dictionary<string, CareerTaxonomyV2> nodeMap)
    {
        var parts = new List<string>();
        var current = nodeId;

        while (!string.IsNullOrEmpty(current) && nodeMap.TryGetValue(current, out var node))
        {
            parts.Insert(0, node.Name);
            current = node.ParentId;
        }

        return string.Join(" / ", parts);
    }

    // ── Private: JSON Parsing ─────────────────────────────────────────────────

    private static List<QuizOptionData> ParseOptionsJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<QuizOptionData>>(json, _jsonOpts) ?? new();
        }
        catch
        {
            return new List<QuizOptionData>();
        }
    }

    private static bool IsRenderableQuestion(QuizQuestionV2 question)
    {
        var options = ParseOptionsJson(question.OptionsJson);
        return options.Count >= 2 && options.All(option => !string.IsNullOrWhiteSpace(option.Text));
    }

    private static string FormatNodeIdToName(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId)) return string.Empty;
        var parts = nodeId.Split('.');
        var lastPart = parts.LastOrDefault() ?? nodeId;
        
        var words = lastPart.Split('_', '-');
        var name = string.Join(" ", words.Select(w => 
            w.Length > 0 ? char.ToUpper(w[0]) + w.Substring(1).ToLower() : w));

        // Fix known acronyms
        name = name.Replace("Ai", "AI")
                   .Replace("Ml", "ML")
                   .Replace("It", "IT")
                   .Replace("Hr", "HR")
                   .Replace("Cs", "CS")
                   .Replace("Mbbs", "MBBS")
                   .Replace("Pm", "Project Management");

        return name;
    }

    // ── Private: Helper Models ────────────────────────────────────────────────

    private class QuizOptionData
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("signals")]
        public List<OptionSignal> Signals { get; set; } = new();

        [JsonPropertyName("isRejection")]
        public bool IsRejection { get; set; }

        [JsonPropertyName("domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("subdomain")]
        public string? Subdomain { get; set; }
    }

    private class OptionSignal
    {
        [JsonPropertyName("nodeId")]
        public string NodeId { get; set; } = string.Empty;

        [JsonPropertyName("trait")]
        public string Trait
        {
            set
            {
                if (string.IsNullOrWhiteSpace(NodeId))
                    NodeId = value ?? string.Empty;
            }
        }

        [JsonPropertyName("weight")]
        public double Weight { get; set; }
    }

    private class SessionAnswer
    {
        [JsonPropertyName("questionId")]
        public string QuestionId { get; set; } = string.Empty;

        [JsonPropertyName("optionIndex")]
        public int OptionIndex { get; set; }
    }
}
