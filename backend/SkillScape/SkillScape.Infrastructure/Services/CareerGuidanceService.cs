using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;

namespace SkillScape.Infrastructure.Services;

public class CareerGuidanceService : ICareerGuidanceService
{
    private readonly ApplicationDbContext _context;
    private readonly IRoadmapService _roadmapService;

    public CareerGuidanceService(ApplicationDbContext context, IRoadmapService roadmapService)
    {
        _context = context;
        _roadmapService = roadmapService;
    }

    public async Task<List<CareerPathDto>> GetCareerPathsTreeAsync(string streamType)
    {
        var allPaths = await _context.CareerPaths
            .Where(p => p.StreamType.Contains(streamType))
            .ToListAsync();

        var rootPaths = allPaths
            .Where(p => string.IsNullOrEmpty(p.ParentCareerId) || !allPaths.Any(ap => ap.Id == p.ParentCareerId))
            .ToList();

        var dtos = new List<CareerPathDto>();
        foreach (var root in rootPaths)
        {
            dtos.Add(MapCareerPathToDto(root, allPaths));
        }

        return dtos;
    }

    public async Task<CareerPathDto?> GetCareerPathByIdAsync(string id)
    {
        var path = await _context.CareerPaths.FindAsync(id);
        if (path == null) return null;

        var allPaths = await _context.CareerPaths
            .Where(p => p.StreamType.Contains(path.StreamType))
            .ToListAsync();

        return MapCareerPathToDto(path, allPaths);
    }

    public async Task<List<HierarchicalQuizQuestionDto>> GetQuizQuestionsAsync(string streamType)
    {
        var questions = await _context.HierarchicalQuizQuestions
            .Where(q => q.StreamType.Contains(streamType))
            .OrderBy(q => q.HierarchyLevel)
            .ToListAsync();

        var dtos = new List<HierarchicalQuizQuestionDto>();
        foreach (var q in questions)
        {
            List<HierarchicalQuizOptionDto> optionsList;
            try
            {
                optionsList = JsonSerializer.Deserialize<List<HierarchicalQuizOptionDto>>(q.OptionsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                              ?? new List<HierarchicalQuizOptionDto>();
            }
            catch
            {
                optionsList = new List<HierarchicalQuizOptionDto>();
            }

            dtos.Add(new HierarchicalQuizQuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                TargetCareerTags = q.TargetCareerTags,
                HierarchyLevel = q.HierarchyLevel,
                StreamType = q.StreamType,
                AptitudeType = q.AptitudeType,
                Options = optionsList
            });
        }

        return dtos;
    }

    private async Task VerifyUserExistsAsync(string userId)
    {
        var exists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!exists)
        {
            throw new UnauthorizedAccessException("User session is invalid. The database was reset or your account no longer exists. Please log in again.");
        }
    }

    public async Task<UserCareerProfileDto> SubmitQuizAnswersAsync(string userId, QuizSubmissionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        await VerifyUserExistsAsync(userId);

        // 1. Gather all questions for this stream to retrieve weights
        var dbQuestions = await _context.HierarchicalQuizQuestions
            .Where(q => q.StreamType.Contains(request.SelectedStream))
            .ToListAsync();

        var requiredAnswerCount = Math.Min(SubmitQuizRequest.MinimumRequiredQuestions, dbQuestions.Count);
        if (requiredAnswerCount == 0)
            throw new InvalidOperationException("No quiz questions are configured for the selected stream.");

        var questionsById = dbQuestions.ToDictionary(question => question.Id, StringComparer.Ordinal);
        var optionsByQuestionId = new Dictionary<string, List<HierarchicalQuizOptionDto>>(StringComparer.Ordinal);
        foreach (var question in dbQuestions)
        {
            try
            {
                optionsByQuestionId[question.Id] =
                    JsonSerializer.Deserialize<List<HierarchicalQuizOptionDto>>(
                        question.OptionsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            catch (JsonException)
            {
                optionsByQuestionId[question.Id] = new();
            }
        }

        var validAnswers = new List<QuizAnswerInputDto>();
        var answeredQuestionIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var answer in request.Answers ?? new List<QuizAnswerInputDto>())
        {
            if (answer == null ||
                !questionsById.ContainsKey(answer.QuestionId) ||
                answeredQuestionIds.Contains(answer.QuestionId) ||
                !optionsByQuestionId.TryGetValue(answer.QuestionId, out var answerOptions) ||
                answer.OptionIndex < 0 ||
                answer.OptionIndex >= answerOptions.Count)
            {
                continue;
            }

            answeredQuestionIds.Add(answer.QuestionId);
            validAnswers.Add(answer);
        }

        if (validAnswers.Count < requiredAnswerCount)
        {
            throw new ArgumentException(
                $"At least {requiredAnswerCount} valid quiz responses are required for the selected stream.",
                nameof(request));
        }

        var accumulatedWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var answerLog = new Dictionary<string, string>();

        if (request.ClientWeights != null && request.ClientWeights.Any())
        {
            foreach (var kvp in request.ClientWeights)
            {
                accumulatedWeights[kvp.Key] = kvp.Value;
            }
        }
        else
        {
            foreach (var ans in validAnswers)
            {
                var question = questionsById[ans.QuestionId];
                var selectedOption = optionsByQuestionId[ans.QuestionId][ans.OptionIndex];
                answerLog[question.QuestionText] = selectedOption.Text;

                foreach (var weight in selectedOption.Weights ?? new Dictionary<string, double>())
                {
                    if (accumulatedWeights.ContainsKey(weight.Key))
                    {
                        accumulatedWeights[weight.Key] += weight.Value;
                    }
                    else
                    {
                        accumulatedWeights[weight.Key] = weight.Value;
                    }
                }
            }
        }

        // 3. Fetch all paths for this stream and score them
        var paths = await _context.CareerPaths
            .Where(p => p.StreamType.Contains(request.SelectedStream))
            .ToListAsync();

        if (!paths.Any())
        {
            throw new InvalidOperationException("No career paths configured for the selected stream.");
        }

        // Determine the target level and prediction level based on education stage
        int targetLevel = 3;
        string predictionLevel = "career";

        if (request.SelectedStream.Equals("After10th", StringComparison.OrdinalIgnoreCase))
        {
            targetLevel = 2; // Sub-stream level (PCM, PCB, etc.) as requested
            predictionLevel = "sub_stream";
        }
        else if (request.SelectedStream.Equals("After12th", StringComparison.OrdinalIgnoreCase))
        {
            targetLevel = 3; // Specialization level
            predictionLevel = "specialization";
        }

        var scoredPaths = new List<(CareerPath Path, double Score)>();
        foreach (var path in paths)
        {
            // IMPORTANT: Only recommend paths matching the exact target hierarchy level
            if (path.Level != targetLevel)
            {
                continue;
            }

            double score = 0;
            var tags = path.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var tag in tags)
            {
                if (accumulatedWeights.TryGetValue(tag, out var weightValue))
                {
                    score += weightValue;
                }
            }

            // Check if exact title matches the accumulated weights (frontend scoring outputs exact titles)
            if (accumulatedWeights.TryGetValue(path.Title, out var titleWeight))
            {
                score += titleWeight * 5; // Heavy boost for exact title match
            }

            // Depth weight removed to avoid skewing level comparison since we already filter by level
            scoredPaths.Add((path, score));
        }

        // Apply Hard Filters from BackgroundContext
        if (request.BackgroundContext != null && request.BackgroundContext.TryGetValue("stream", out var bgStream))
        {
            // If After12th, the stream from UI might be pcm, pcb, commerce, etc.
            if (bgStream.Equals("commerce", StringComparison.OrdinalIgnoreCase)) bgStream = "com";
            if (bgStream.Equals("arts", StringComparison.OrdinalIgnoreCase)) bgStream = "art";
            if (bgStream.Equals("vocational", StringComparison.OrdinalIgnoreCase)) bgStream = "voc";

            // Filter out paths whose ParentCareerId doesn't contain the selected substream.
            scoredPaths = scoredPaths.Where(x => (x.Path.ParentCareerId != null && x.Path.ParentCareerId.Contains(bgStream, StringComparison.OrdinalIgnoreCase))
                                              || x.Path.StreamType.Contains("all", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var topPaths = scoredPaths
            .OrderByDescending(x => x.Score)
            .Take(10)
            .ToList();

        if (!topPaths.Any())
        {
            return new UserCareerProfileDto
            {
                ResultStatus = "InsufficientConfidence",
                WhyRecommended = "No matching paths found.",
                ConfidenceScore = 0
            };
        }

        var top10 = topPaths.Select(x => x.Path).ToList();
        var recommendedCareer = top10.First();
        double highestScore = topPaths[0].Score;
        double secondHighestScore = topPaths.Count > 1 ? topPaths[1].Score : 0;
        
        // 4. Calculate actual confidence score
        double totalScore = topPaths.Sum(x => x.Score) + 0.1;
        double confidence = (highestScore / totalScore) * 100 * 2.5;
        if (confidence > 99.0) confidence = 99.0;
        
        // Explicit fail state if highest match is below 20%
        if (highestScore == 0 || confidence < 20.0)
        {
            return new UserCareerProfileDto
            {
                ResultStatus = "InsufficientConfidence",
                WhyRecommended = "Insufficient data to make a confident career prediction. Please provide more responses.",
                ConfidenceScore = confidence
            };
        }

        // 5. Predict starting salary
        double baseSalary = 400000;
        try
        {
            var salData = JsonSerializer.Deserialize<Dictionary<string, double>>(recommendedCareer.SalaryDataJson);
            if (salData != null && salData.Any())
            {
                baseSalary = salData.Values.First();
            }
        }
        catch { }

        // 6. Generate roadmap
        List<RoadmapMilestoneDto> roadmap;
        try
        {
            roadmap = JsonSerializer.Deserialize<List<RoadmapMilestoneDto>>(recommendedCareer.RoadmapJson) 
                      ?? new List<RoadmapMilestoneDto>();
        }
        catch
        {
            roadmap = new List<RoadmapMilestoneDto>();
        }

        // 7. Calculate skill gap
        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == userId)
            .Select(us => us.Skill!.Name)
            .ToListAsync();

        List<string> requiredSkills;
        try
        {
            requiredSkills = JsonSerializer.Deserialize<List<string>>(recommendedCareer.SkillsRequired)
                             ?? new List<string>();
        }
        catch
        {
            requiredSkills = recommendedCareer.SkillsRequired
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        var skillGap = requiredSkills
            .Where(reqSkill => !userSkills.Any(uSkill => uSkill.Equals(reqSkill, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // 8. Generate dynamic explanations & pros/cons warnings
        var topDimensions = accumulatedWeights.OrderByDescending(kv => kv.Value).Take(3).Select(kv => kv.Key).ToList();
        
        var dimensionTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "mathematical_reasoning", "Mathematics-based problems and analytical reasoning" },
            { "biological_interest", "living organisms, health, and biological processes" },
            { "commerce_finance_interest", "working with numbers in financial and business contexts" },
            { "humanities_social_science_interest", "society, historical events, and understanding human behavior" },
            { "technical_hands_on_interest", "practical machine mechanics and building hands-on solutions" },
            { "scientific_reasoning", "understanding how physical systems work" },
            { "business_interest", "corporate growth and market dynamics" },
            { "language_interest", "communication, literature, and linguistics" }
        };

        var matchedSentences = new List<string>();
        foreach(var dim in topDimensions)
        {
            if (dimensionTranslations.TryGetValue(dim, out var text))
            {
                matchedSentences.Add(text);
            }
            else
            {
                matchedSentences.Add(dim.Replace("_", " ")); // Fallback
            }
        }

        string whyRecommended = $"You consistently preferred activities involving {string.Join(", ", matchedSentences)}. Your strongest signals therefore point toward {recommendedCareer.Title}.";
        
        string whyNotRecommended = recommendedCareer.AutomationRisk > 40.0
            ? $"Warning: This field carries an estimated AI automation risk of {recommendedCareer.AutomationRisk}%. You should prioritize creative problem-solving and leadership skills to remain resilient."
            : $"Resilience: This domain is highly robust with low automation risk ({recommendedCareer.AutomationRisk}%), though it requires a timeline of {recommendedCareer.RoadmapTimeline} to reach top seniority.";

        // 9. Generate strengths & weaknesses
        var strengths = new List<string> { $"Strong capability for {recommendedCareer.Title} requirements" };
        var weaknesses = new List<string> { $"Needs to bridge skills: {string.Join(", ", skillGap.Take(2))}" };

        foreach (var sen in matchedSentences.Take(2))
        {
             strengths.Add(char.ToUpper(sen[0]) + sen.Substring(1));
        }

        if (recommendedCareer.DifficultyLevel == "Hard" || recommendedCareer.DifficultyLevel == "Extremely Hard")
            weaknesses.Add("High complexity barrier: demands extensive, continuous certification upgrades");

        var strengthsWeaknesses = new Dictionary<string, List<string>>
        {
            { "Strengths", strengths },
            { "Weaknesses", weaknesses }
        };

        // 10. Generate 10-year salary trajectory graph
        var growthGraph = new List<SalaryTrendPointDto>();
        int startYr = DateTime.Now.Year;
        double currentSal = baseSalary;
        double cagr = recommendedCareer.IndustryGrowth / 100.0;
        if (cagr <= 0) cagr = 0.08;
        for (int i = 0; i < 10; i++)
        {
            growthGraph.Add(new SalaryTrendPointDto
            {
                Year = startYr + i,
                SalaryIndia = Math.Round(currentSal),
                SalaryGlobal = Math.Round(currentSal * 2.3)
            });
            currentSal *= (1 + cagr);
        }

        // 11. Serialize top 10 matches
        var allPathsForMapping = await _context.CareerPaths.ToListAsync();
        var topMatchesDtos = topPaths.Select(tp => {
            var dto = MapCareerPathToDto(tp.Path, allPathsForMapping);
            dto.MatchScore = Math.Min(99, Math.Round((tp.Score / totalScore) * 100 * 2.5));
            return dto;
        }).ToList();

        // 12. Save or update profile
        var profile = await _context.UserCareerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        bool isNew = profile == null;
        if (profile == null)
        {
            profile = new UserCareerProfile { UserId = userId };
        }

        profile.SelectedHierarchy = request.SelectedStream;
        profile.QuizAnswersJson = JsonSerializer.Serialize(answerLog);
        profile.RecommendedCareerId = recommendedCareer.Id;
        profile.ConfidenceScore = confidence;
        profile.PredictedSalary = baseSalary;
        profile.RoadmapJson = JsonSerializer.Serialize(roadmap);
        profile.SkillGapJson = JsonSerializer.Serialize(skillGap);
        profile.TopMatchesJson = JsonSerializer.Serialize(topMatchesDtos);
        profile.WhyRecommended = whyRecommended;
        profile.WhyNotRecommended = whyNotRecommended;
        profile.StrengthsWeaknessesJson = JsonSerializer.Serialize(strengthsWeaknesses);
        profile.CareerGrowthGraphJson = JsonSerializer.Serialize(growthGraph);

        // Retain bookmarks if update
        if (isNew) profile.BookmarkedPaths = "[]";

        if (isNew)
        {
            _context.UserCareerProfiles.Add(profile);
        }
        else
        {
            _context.UserCareerProfiles.Update(profile);
        }

        await _context.SaveChangesAsync();

        return MapProfileToDto(profile, recommendedCareer);
    }

    public async Task<UserCareerProfileDto?> GetUserCareerProfileAsync(string userId)
    {
        await VerifyUserExistsAsync(userId);
        var profile = await _context.UserCareerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null) return null;

        var recCareer = await _context.CareerPaths.FindAsync(profile.RecommendedCareerId);
        return MapProfileToDto(profile, recCareer);
    }

    public async Task<bool> ToggleBookmarkPathAsync(string userId, string pathId)
    {
        await VerifyUserExistsAsync(userId);
        var profile = await _context.UserCareerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            // Create a dummy user profile just to save bookmarks if they haven't done the quiz
            var defaultCareer = await _context.CareerPaths.OrderBy(p => p.Level).FirstOrDefaultAsync();
            if (defaultCareer == null) return false;

            profile = new UserCareerProfile
            {
                UserId = userId,
                SelectedHierarchy = "DirectExplorer",
                RecommendedCareerId = defaultCareer.Id,
                BookmarkedPaths = JsonSerializer.Serialize(new List<string> { pathId })
            };
            _context.UserCareerProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return true;
        }

        List<string> bookmarks;
        try
        {
            bookmarks = JsonSerializer.Deserialize<List<string>>(profile.BookmarkedPaths) ?? new List<string>();
        }
        catch
        {
            bookmarks = new List<string>();
        }

        bool bookmarked;
        if (bookmarks.Contains(pathId))
        {
            bookmarks.Remove(pathId);
            bookmarked = false;
        }
        else
        {
            bookmarks.Add(pathId);
            bookmarked = true;
        }

        profile.BookmarkedPaths = JsonSerializer.Serialize(bookmarks);
        _context.UserCareerProfiles.Update(profile);
        await _context.SaveChangesAsync();

        return bookmarked;
    }

    public async Task<List<CareerPathDto>> GetBookmarkedPathsAsync(string userId)
    {
        await VerifyUserExistsAsync(userId);
        var profile = await _context.UserCareerProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return new List<CareerPathDto>();

        List<string> bookmarks;
        try
        {
            bookmarks = JsonSerializer.Deserialize<List<string>>(profile.BookmarkedPaths) ?? new List<string>();
        }
        catch
        {
            return new List<CareerPathDto>();
        }

        var paths = await _context.CareerPaths
            .Where(p => bookmarks.Contains(p.Id))
            .ToListAsync();

        var allPaths = await _context.CareerPaths.ToListAsync();

        return paths.Select(p => MapCareerPathToDto(p, allPaths)).ToList();
    }

    public async Task<SalaryForecastDto> ForecastSalaryAsync(SalaryForecastRequest request)
    {
        var targetId = request.CareerPathId?.Trim() ?? string.Empty;
        var path = await _context.CareerPaths.FindAsync(targetId);
        
        if (path == null && !string.IsNullOrEmpty(targetId))
        {
            var lowerId = targetId.ToLower();
            path = await _context.CareerPaths.FirstOrDefaultAsync(p =>
                p.Id.ToLower() == lowerId ||
                p.Title.ToLower() == lowerId ||
                p.Id.ToLower().Contains(lowerId) ||
                lowerId.Contains(p.Id.ToLower()));
        }

        double startingSalary = 600000;
        double midSalary = 1400000;
        double seniorSalary = 3000000;
        double annualGrowth = 0.12;
        string careerTitle = targetId;

        if (path != null)
        {
            careerTitle = path.Title;
            annualGrowth = path.IndustryGrowth / 100.0;
            if (annualGrowth <= 0.02) annualGrowth = 0.12;

            try
            {
                var salData = JsonSerializer.Deserialize<Dictionary<string, double>>(
                    path.SalaryDataJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                
                if (salData != null && salData.Any())
                {
                    double GetValue(params string[] keys)
                    {
                        foreach (var k in keys)
                        {
                            var match = salData.FirstOrDefault(x => x.Key.Equals(k, StringComparison.OrdinalIgnoreCase));
                            if (!match.Equals(default(KeyValuePair<string, double>))) return match.Value;
                        }
                        return 0.0;
                    }

                    double st = GetValue("Starting", "entry", "Entry", "Junior", "Start");
                    if (st > 0) startingSalary = st;
                    else startingSalary = salData.Values.First();

                    double mid = GetValue("Mid-Career", "mid", "Mid", "Intermediate", "MidCareer");
                    if (mid > 0) midSalary = mid;
                    else midSalary = startingSalary * 2.1;

                    double sn = GetValue("Senior", "senior", "Senior", "Expert", "Lead");
                    if (sn > 0) seniorSalary = sn;
                    else seniorSalary = midSalary * 2.0;
                }
            }
            catch { }
        }
        else if (!string.IsNullOrEmpty(targetId))
        {
            careerTitle = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                targetId.Replace("-", " ").Replace("_", " ")
            );
        }

        // Select accurate baseline compensation based on target skill tier
        double baseSalary = request.SkillLevel switch
        {
            "Beginner" => startingSalary,
            "Intermediate" => midSalary,
            "Expert" => seniorSalary,
            _ => midSalary
        };

        // Geographic multipliers
        double locationMultiplier = request.Location switch
        {
            "Tier 1" => 1.25,
            "Tier 2" => 1.0,
            "Tier 3" => 0.82,
            "Global" => 3.2,
            _ => 1.0
        };

        // Skill mastery performance factor
        double skillPerformanceFactor = request.SkillLevel switch
        {
            "Beginner" => 0.95,
            "Intermediate" => 1.05,
            "Expert" => 1.15,
            _ => 1.0
        };

        int currentYear = DateTime.Now.Year;
        int gradYear = request.ExpectedGraduationYear > 0 ? request.ExpectedGraduationYear : currentYear;
        int yearsDiff = Math.Max(0, request.TargetYear - currentYear);
        int experienceYears = Math.Max(0, request.TargetYear - gradYear);

        // Compounding growth based on market CAGR and career experience progression
        double expGrowthRate = request.SkillLevel == "Beginner" ? 0.09 : 0.06;
        double effectiveRate = annualGrowth + (experienceYears > 0 ? expGrowthRate * 0.5 : 0);

        double predicted = baseSalary * Math.Pow(1 + effectiveRate, yearsDiff) * locationMultiplier * skillPerformanceFactor;
        double minPrediction = predicted * 0.88;
        double maxPrediction = predicted * 1.18;

        // India vs Global averages
        double indiaAvg = baseSalary * locationMultiplier * skillPerformanceFactor;
        double globalAvg = baseSalary * 3.1 * skillPerformanceFactor;

        // Generate trend line points
        var trendData = new List<SalaryTrendPointDto>();
        int endYear = Math.Max(request.TargetYear + 3, currentYear + 5);
        for (int yr = currentYear; yr <= endYear; yr++)
        {
            int diff = yr - currentYear;
            double yrIndia = baseSalary * Math.Pow(1 + annualGrowth, diff) * locationMultiplier * skillPerformanceFactor;
            double yrGlobal = baseSalary * Math.Pow(1 + annualGrowth + 0.015, diff) * 3.1 * skillPerformanceFactor;

            trendData.Add(new SalaryTrendPointDto
            {
                Year = yr,
                SalaryIndia = Math.Round(yrIndia),
                SalaryGlobal = Math.Round(yrGlobal)
            });
        }

        return new SalaryForecastDto
        {
            CareerTitle = careerTitle,
            TargetYear = request.TargetYear,
            PredictedSalary = Math.Round(predicted),
            PredictedSalaryMin = Math.Round(minPrediction),
            PredictedSalaryMax = Math.Round(maxPrediction),
            IndiaSalaryAvg = Math.Round(indiaAvg),
            GlobalSalaryAvg = Math.Round(globalAvg),
            TrendData = trendData
        };
    }


    public async Task<GuidanceAnalyticsDto> GetGuidanceAnalyticsAsync()
    {
        var profiles = await _context.UserCareerProfiles.ToListAsync();
        var allPaths = await _context.CareerPaths.ToListAsync();

        // 1. Compute most chosen careers
        var chosenCounts = profiles
            .GroupBy(p => p.RecommendedCareerId)
            .Select(g => new AnalyticsItemDto
            {
                Name = allPaths.FirstOrDefault(p => p.Id == g.Key)?.Title ?? "Unknown Career",
                Value = g.Count()
            })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToList();

        // Seeding backup data if profiles are scarce so charts render beautifully
        if (chosenCounts.Count < 3)
        {
            chosenCounts = new List<AnalyticsItemDto>
            {
                new() { Name = "AI Engineer", Value = 34 },
                new() { Name = "Full Stack Developer", Value = 28 },
                new() { Name = "DevOps Engineer", Value = 22 },
                new() { Name = "Data Scientist", Value = 18 },
                new() { Name = "Cybersecurity Analyst", Value = 15 }
            };
        }

        // 2. Compute popular streams
        var popularStreams = profiles
            .GroupBy(p => p.SelectedHierarchy)
            .Select(g => new AnalyticsItemDto
            {
                Name = FormatStreamName(g.Key),
                Value = g.Count()
            })
            .ToList();

        if (popularStreams.Count < 2)
        {
            popularStreams = new List<AnalyticsItemDto>
            {
                new() { Name = "After 12th", Value = 45 },
                new() { Name = "After Graduation", Value = 28 },
                new() { Name = "Career Switch", Value = 18 },
                new() { Name = "After 10th", Value = 12 },
                new() { Name = "Skill Enhancement", Value = 9 }
            };
        }

        // 3. User Interest monthly trend
        var trends = new List<AnalyticsTrendPointDto>
        {
            new() { Month = "Jan", TechCount = 12, BusinessCount = 4, ArtsCount = 2 },
            new() { Month = "Feb", TechCount = 18, BusinessCount = 7, ArtsCount = 3 },
            new() { Month = "Mar", TechCount = 24, BusinessCount = 9, ArtsCount = 5 },
            new() { Month = "Apr", TechCount = 35, BusinessCount = 12, ArtsCount = 6 },
            new() { Month = "May", TechCount = 48, BusinessCount = 15, ArtsCount = 9 }
        };

        // 4. Avg confidence
        double avgConfidence = profiles.Any() ? profiles.Average(p => p.ConfidenceScore) : 87.5;

        // 5. Skill gap occurrences
        var skillFrequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in profiles)
        {
            try
            {
                var skills = JsonSerializer.Deserialize<List<string>>(p.SkillGapJson);
                if (skills != null)
                {
                    foreach (var s in skills)
                    {
                        if (skillFrequencies.ContainsKey(s)) skillFrequencies[s]++;
                        else skillFrequencies[s] = 1;
                    }
                }
            }
            catch {}
        }

        var skillGaps = skillFrequencies
            .Select(kv => new AnalyticsItemDto { Name = kv.Key, Value = kv.Value })
            .OrderByDescending(x => x.Value)
            .Take(6)
            .ToList();

        if (skillGaps.Count < 3)
        {
            skillGaps = new List<AnalyticsItemDto>
            {
                new() { Name = "Cloud Computing (AWS/Azure)", Value = 42 },
                new() { Name = "System Design & Architecture", Value = 38 },
                new() { Name = "Machine Learning Models", Value = 31 },
                new() { Name = "Kubernetes & Docker", Value = 27 },
                new() { Name = "Advanced Data Structures", Value = 24 }
            };
        }

        return new GuidanceAnalyticsDto
        {
            MostChosenCareers = chosenCounts,
            PopularStreams = popularStreams,
            UserInterestTrends = trends,
            AveragePredictionConfidence = Math.Round(avgConfidence, 1),
            TotalQuizCompletions = profiles.Count > 0 ? profiles.Count : 108,
            SkillGapTrends = skillGaps
        };
    }

    public async Task<CareerCompareDto> CompareCareersAsync(string pathId1, string pathId2)
    {
        var c1 = await GetCareerPathByIdAsync(pathId1);
        var c2 = await GetCareerPathByIdAsync(pathId2);

        if (c1 == null || c2 == null)
        {
            throw new KeyNotFoundException("One or both career paths not found");
        }

        return new CareerCompareDto
        {
            Career1 = c1,
            Career2 = c2
        };
    }

    // Helper formatting method
    private string FormatStreamName(string raw)
    {
        return raw switch
        {
            "After10th" => "After 10th",
            "After12th" => "After 12th",
            "AfterGraduation" => "After Graduation",
            "CareerSwitch" => "Career Switch",
            "SkillEnhancement" => "Skill Enhancement",
            _ => raw
        };
    }

    // Mapper helpers
    private CareerPathDto MapCareerPathToDto(CareerPath path, List<CareerPath> allPaths)
    {
        List<string> skills;
        try
        {
            skills = JsonSerializer.Deserialize<List<string>>(path.SkillsRequired) ?? new List<string>();
        }
        catch
        {
            skills = path.SkillsRequired
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        Dictionary<string, double> salData;
        try
        {
            salData = JsonSerializer.Deserialize<Dictionary<string, double>>(path.SalaryDataJson) ?? new Dictionary<string, double>();
        }
        catch
        {
            salData = new Dictionary<string, double>();
        }

        List<RoadmapMilestoneDto> roadmap;
        try
        {
            roadmap = JsonSerializer.Deserialize<List<RoadmapMilestoneDto>>(path.RoadmapJson) ?? new List<RoadmapMilestoneDto>();
        }
        catch
        {
            roadmap = new List<RoadmapMilestoneDto>();
        }

        List<string> recommendedSubjects;
        try
        {
            recommendedSubjects = JsonSerializer.Deserialize<List<string>>(path.RecommendedSubjects) ?? new List<string>();
        }
        catch
        {
            recommendedSubjects = new List<string>();
        }

        List<string> topCompanies;
        try
        {
            topCompanies = JsonSerializer.Deserialize<List<string>>(path.TopCompanies) ?? new List<string>();
        }
        catch
        {
            topCompanies = new List<string>();
        }

        List<string> requiredDegrees;
        try
        {
            requiredDegrees = JsonSerializer.Deserialize<List<string>>(path.RequiredDegrees) ?? new List<string>();
        }
        catch
        {
            requiredDegrees = new List<string>();
        }

        List<string> learningResources;
        try
        {
            learningResources = JsonSerializer.Deserialize<List<string>>(path.LearningResources) ?? new List<string>();
        }
        catch
        {
            learningResources = new List<string>();
        }

        List<string> youtubeResources;
        try
        {
            youtubeResources = JsonSerializer.Deserialize<List<string>>(path.YoutubeResources) ?? new List<string>();
        }
        catch
        {
            youtubeResources = new List<string>();
        }

        var dto = new CareerPathDto
        {
            Id = path.Id,
            Title = path.Title,
            Description = path.Description,
            Level = path.Level,
            ParentCareerId = path.ParentCareerId,
            StreamType = path.StreamType,
            Tags = path.Tags,
            SkillsRequired = skills,
            FutureScope = path.FutureScope,
            SalaryData = salData,
            IndustryGrowth = path.IndustryGrowth,
            Roadmap = roadmap,
            Certifications = path.Certifications.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            Colleges = path.Colleges.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            DemandIndex = path.DemandIndex,
            Icon = path.Icon,
            Color = path.Color,
            WhatYouWillStudy = path.WhatYouWillStudy,
            RecommendedSubjects = recommendedSubjects,
            TopCompanies = topCompanies,
            RequiredDegrees = requiredDegrees,
            WorkEnvironment = path.WorkEnvironment,
            RemoteWorkPossibility = path.RemoteWorkPossibility,
            EntrepreneurshipPotential = path.EntrepreneurshipPotential,
            CareerFlexibility = path.CareerFlexibility,
            LearningResources = learningResources,
            YoutubeResources = youtubeResources,
            RoadmapTimeline = path.RoadmapTimeline,
            DifficultyLevel = path.DifficultyLevel,
            PersonalityMatch = path.PersonalityMatch,
            RelatedCareerIds = path.RelatedCareerIds,
            AutomationRisk = path.AutomationRisk
        };

        // Load children recursively
        var children = allPaths.Where(p => p.ParentCareerId == path.Id).ToList();
        foreach (var child in children)
        {
            dto.SubCareers.Add(MapCareerPathToDto(child, allPaths));
        }

        return dto;
    }

    private UserCareerProfileDto MapProfileToDto(UserCareerProfile profile, CareerPath? recommendedCareer)
    {
        Dictionary<string, string> answers;
        try
        {
            answers = JsonSerializer.Deserialize<Dictionary<string, string>>(profile.QuizAnswersJson) ?? new Dictionary<string, string>();
        }
        catch
        {
            answers = new Dictionary<string, string>();
        }

        List<RoadmapMilestoneDto> roadmap;
        try
        {
            roadmap = JsonSerializer.Deserialize<List<RoadmapMilestoneDto>>(profile.RoadmapJson) ?? new List<RoadmapMilestoneDto>();
        }
        catch
        {
            roadmap = new List<RoadmapMilestoneDto>();
        }

        List<string> skillGap;
        try
        {
            skillGap = JsonSerializer.Deserialize<List<string>>(profile.SkillGapJson) ?? new List<string>();
        }
        catch
        {
            skillGap = new List<string>();
        }

        List<string> bookmarks;
        try
        {
            bookmarks = JsonSerializer.Deserialize<List<string>>(profile.BookmarkedPaths) ?? new List<string>();
        }
        catch
        {
            bookmarks = new List<string>();
        }

        List<CareerPathDto> topMatches = new();
        try
        {
            topMatches = JsonSerializer.Deserialize<List<CareerPathDto>>(profile.TopMatchesJson) ?? new List<CareerPathDto>();
        }
        catch { }

        Dictionary<string, List<string>> strengthsWeaknesses = new();
        try
        {
            strengthsWeaknesses = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(profile.StrengthsWeaknessesJson) ?? new Dictionary<string, List<string>>();
        }
        catch { }

        List<SalaryTrendPointDto> careerGrowthGraph = new();
        try
        {
            careerGrowthGraph = JsonSerializer.Deserialize<List<SalaryTrendPointDto>>(profile.CareerGrowthGraphJson) ?? new List<SalaryTrendPointDto>();
        }
        catch { }

        CareerPathDto? careerDto = null;
        if (recommendedCareer != null)
        {
            // Empty list for recursive child mapping to avoid loading entire db recursively for user career profile DTO
            careerDto = MapCareerPathToDto(recommendedCareer, new List<CareerPath>());
        }

        string educationStage = profile.SelectedHierarchy;
        string predictionLevel = "career";
        if (educationStage.Equals("After10th", StringComparison.OrdinalIgnoreCase)) predictionLevel = "stream";
        else if (educationStage.Equals("After12th", StringComparison.OrdinalIgnoreCase)) predictionLevel = "degree_field";

        return new UserCareerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            SelectedHierarchy = profile.SelectedHierarchy,
            EducationStage = educationStage,
            PredictionLevel = predictionLevel,
            QuizAnswers = answers,
            RecommendedCareer = careerDto,
            ConfidenceScore = profile.ConfidenceScore,
            PredictedSalary = profile.PredictedSalary,
            Roadmap = roadmap,
            SkillGap = skillGap,
            BookmarkedPaths = bookmarks,
            TopMatches = topMatches,
            WhyRecommended = profile.WhyRecommended,
            WhyNotRecommended = profile.WhyNotRecommended,
            StrengthsWeaknesses = strengthsWeaknesses,
            CareerGrowthGraph = careerGrowthGraph,
            CreatedAt = profile.CreatedAt
        };
    }

    public async Task<bool> AcceptCareerPathAsync(string userId, string pathId)
    {
        await VerifyUserExistsAsync(userId);
        var path = await _context.CareerPaths.FindAsync(pathId);
        if (path == null) return false;

        var profile = await _context.UserCareerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        bool isNew = profile == null;
        if (profile == null)
        {
            profile = new UserCareerProfile
            {
                UserId = userId,
                BookmarkedPaths = "[]"
            };
        }

        profile.SelectedHierarchy = path.StreamType.Split(',').FirstOrDefault() ?? "DirectAccept";
        profile.RecommendedCareerId = path.Id;
        profile.RoadmapJson = path.RoadmapJson;
        profile.ConfidenceScore = 100.0;
        profile.WhyRecommended = $"You manually selected and accepted {path.Title} as your target career path.";

        double baseSalary = 500000;
        try
        {
            var salData = JsonSerializer.Deserialize<Dictionary<string, double>>(path.SalaryDataJson);
            if (salData != null && salData.Any())
            {
                baseSalary = salData.Values.First();
            }
        }
        catch { }
        profile.PredictedSalary = baseSalary;

        var growthGraph = new List<SalaryTrendPointDto>();
        int startYr = DateTime.Now.Year;
        double currentSal = baseSalary;
        double cagr = path.IndustryGrowth / 100.0;
        if (cagr <= 0) cagr = 0.08;
        for (int i = 0; i < 10; i++)
        {
            growthGraph.Add(new SalaryTrendPointDto
            {
                Year = startYr + i,
                SalaryIndia = Math.Round(currentSal),
                SalaryGlobal = Math.Round(currentSal * 2.3)
            });
            currentSal *= (1 + cagr);
        }
        profile.CareerGrowthGraphJson = JsonSerializer.Serialize(growthGraph);

        var allPathsForMapping = await _context.CareerPaths.ToListAsync();
        var topMatches = new List<CareerPath> { path };
        var topMatchesDtos = topMatches.Select(p => MapCareerPathToDto(p, allPathsForMapping)).ToList();
        profile.TopMatchesJson = JsonSerializer.Serialize(topMatchesDtos);

        if (isNew)
        {
            _context.UserCareerProfiles.Add(profile);
        }
        else
        {
            _context.UserCareerProfiles.Update(profile);
        }

        // Ensure a matching CareerDomain exists to satisfy database FK constraints on UserProgressions
        var domainExists = await _context.CareerDomains.AnyAsync(d => d.Id == path.Id);
        if (!domainExists)
        {
            _context.CareerDomains.Add(new CareerDomain
            {
                Id = path.Id,
                Name = path.Title,
                Description = path.Description ?? $"Custom accepted career path for {path.Title}",
                IsActive = false, // Set to false to avoid displaying in standard domain categories/listings
                Color = path.Color ?? "#4F46E5",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        // Keep legacy career acceptance and the modern roadmap experience in sync.
        // This generates the full phase/module/topic/quiz curriculum and makes it active.
        await _roadmapService.AcceptCareerRoadmapAsync(userId, path.Id, path.Title);
        return true;
    }
}
