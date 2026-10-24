using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;
using DomainSkillRequirement = SkillScape.Domain.Entities.SkillRequirement;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Implements placement readiness assessment logic:
/// 1. Scores answers per skill
/// 2. Fetches role requirements from DB
/// 3. Calls ML.NET PlacementReadinessPredictor
/// 4. Generates recommendations + roadmap
/// 5. Persists result
/// </summary>
public class PlacementService : IPlacementService
{
    private readonly ApplicationDbContext _context;
    private readonly PredictionEnginePool<PlacementData, PlacementPrediction>? _readinessPredictionPool;

    // Static metadata for available roles
    private static readonly List<PlacementRoleDto> _availableRoles = new()
    {
        new() { Name = "Software Developer",     Description = "Build robust applications and systems with strong CS fundamentals",
                Icon = "Code2",       KeySkills = new() { "Data Structures", "Algorithms", "OOP", "System Design", "Problem Solving" } },
        new() { Name = "Full Stack Developer",   Description = "End-to-end web development from frontend UI to backend APIs and databases",
                Icon = "Layers",      KeySkills = new() { "JavaScript", "REST APIs", "Databases", "React/Angular", "System Design" } },
        new() { Name = "Data Scientist",         Description = "Extract insights from data using statistical models and machine learning",
                Icon = "BarChart2",   KeySkills = new() { "Python", "Statistics", "Machine Learning", "SQL", "Data Visualization" } },
        new() { Name = "AI Engineer",            Description = "Design and deploy deep learning models and intelligent AI systems",
                Icon = "Brain",       KeySkills = new() { "Python", "Deep Learning", "NLP", "TensorFlow/PyTorch", "MLOps" } },
        new() { Name = "Cybersecurity Analyst",  Description = "Protect systems and networks from digital attacks and vulnerabilities",
                Icon = "Shield",      KeySkills = new() { "Networking", "Linux", "Cryptography", "Penetration Testing", "SIEM" } },
        new() { Name = "Cloud Engineer",         Description = "Architect and manage scalable cloud infrastructure on AWS/Azure/GCP",
                Icon = "Cloud",       KeySkills = new() { "AWS/Azure", "Docker", "Kubernetes", "IaC", "Networking" } },
        new() { Name = "DevOps Engineer",        Description = "Bridge development and operations through automation and CI/CD pipelines",
                Icon = "GitBranch",   KeySkills = new() { "CI/CD", "Docker", "Kubernetes", "Linux", "Monitoring" } },
        new() { Name = "Doctor (MBBS)",          Description = "Diagnose and treat diseases with clinical expertise and medical knowledge",
                Icon = "Heart",       KeySkills = new() { "Anatomy", "Physiology", "Pharmacology", "Clinical Diagnosis", "Patient Care" } },
        new() { Name = "Psychiatrist",           Description = "Assess and treat mental health disorders through therapy and medication",
                Icon = "Brain",       KeySkills = new() { "Psychology", "Psychopathology", "Counseling", "Pharmacotherapy", "Ethics" } },
        new() { Name = "Pharmacist",             Description = "Manage medication therapy and ensure safe drug dispensing and patient care",
                Icon = "Pill",        KeySkills = new() { "Pharmacology", "Drug Interactions", "Chemistry", "Patient Counseling", "Regulations" } },
        new() { Name = "Chartered Accountant",   Description = "Manage financial reporting, auditing, taxation, and business advisory",
                Icon = "Calculator",  KeySkills = new() { "Accounting", "Taxation", "Auditing", "Financial Analysis", "Business Law" } },
        new() { Name = "Graphic Designer",       Description = "Create visual concepts using art and technology to communicate ideas",
                Icon = "Palette",     KeySkills = new() { "Visual Design", "Typography", "Color Theory", "Adobe Suite", "UI/UX" } },
    };

    public PlacementService(
        ApplicationDbContext context,
        PredictionEnginePool<PlacementData, PlacementPrediction>? readinessPredictionPool = null)
    {
        _context = context;
        _readinessPredictionPool = readinessPredictionPool;
    }

    // ─────────────────────────────────────────
    public Task<List<PlacementRoleDto>> GetAvailableRolesAsync()
        => Task.FromResult(_availableRoles);

    // ─────────────────────────────────────────
    public async Task<List<PlacementQuestionDto>> GetAssessmentQuestionsAsync(string role)
    {
        // Fetch questions that list this role in ApplicableRoles (comma-separated)
        var questions = await _context.PlacementAssessmentQuestions

            .Where(q => q.IsActive && q.ApplicableRoles.Contains(role))
            .OrderBy(q => q.Category)
            .ThenBy(q => q.DisplayOrder)
            .ToListAsync();

        // Fallback: get general questions if no role-specific ones found
        if (!questions.Any())
        {
            questions = await _context.PlacementAssessmentQuestions
                .Where(q => q.IsActive)
                .OrderBy(q => q.Category)
                .ThenBy(q => q.DisplayOrder)
                .Take(20)
                .ToListAsync();
        }

        return questions.Select(q => new PlacementQuestionDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            Options = JsonSerializer.Deserialize<List<string>>(q.OptionsJson) ?? new(),
            SkillMapped = q.SkillMapped,
            Category = q.Category,
            Difficulty = q.Difficulty,
            DisplayOrder = q.DisplayOrder
        }).ToList();
    }

    // ─────────────────────────────────────────
    public async Task<PlacementResultDto> SubmitAssessmentAsync(string userId, SubmitPlacementRequest request)
    {
        _ = await _context.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found");

        // 1. Fetch all questions for this assessment (including correct answers for scoring)
        var questionIds = request.Answers.Select(a => a.QuestionId).ToList();
        var questions = await _context.PlacementAssessmentQuestions
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync();

        // 2. Calculate raw skill scores (% correct per skill)
        var skillCorrect = new Dictionary<string, (int Correct, int Total)>();
        foreach (var answer in request.Answers)
        {
            var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null) continue;

            var skill = question.SkillMapped;
            if (!skillCorrect.ContainsKey(skill))
                skillCorrect[skill] = (0, 0);

            var (correct, total) = skillCorrect[skill];
            total++;
            if (answer.SelectedAnswer.Trim().Equals(question.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                correct++;

            skillCorrect[skill] = (correct, total);
        }

        // Convert to 0-100 score per skill
        var skillScores = skillCorrect.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Total > 0 ? (double)kvp.Value.Correct / kvp.Value.Total * 100 : 0.0
        );

        // 3. Fetch role skill requirements
        var roleProfile = await _context.RoleSkillProfiles
            .FirstOrDefaultAsync(r => r.RoleName == request.TargetRole);

        var requirements = roleProfile != null
            ? JsonSerializer.Deserialize<List<DomainSkillRequirement>>(roleProfile.SkillsJson) ?? new()
            : new List<DomainSkillRequirement>();

        // 4. Map scores to 7 standard ML feature buckets
        var mlInput = MapToMlFeatures(skillScores, request.TargetRole);

        // 5. Call ML.NET PlacementReadinessPredictor
        double readinessScore = 75.0; // safe default
        try
        {
            var prediction = _readinessPredictionPool?.Predict(
                modelName: "PlacementReadinessPredictor",
                example: mlInput);

            if (prediction != null)
                readinessScore = Math.Max(0, Math.Min(100, prediction.ReadinessScore));
            else
                readinessScore = CalculateFallbackScore(skillScores, requirements);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ML-ERROR] Placement Readiness Prediction failed: {ex.Message}");
            // Fallback: weighted average of skill vs required
            readinessScore = CalculateFallbackScore(skillScores, requirements);
        }

        // 6. Build skill score DTOs + identify strong/weak
        var skillScoreDtos = BuildSkillScoreDtos(skillScores, requirements);
        var strongSkills = skillScoreDtos.Where(s => s.IsMet).Select(s => s.SkillName).ToList();
        var weakSkills = skillScoreDtos.Where(s => !s.IsMet).Select(s => s.SkillName).ToList();

        // 7. Generate recommendations + roadmap
        var recommendations = GenerateRecommendations(weakSkills, request.TargetRole);
        var roadmap = GenerateRoadmap(weakSkills, strongSkills, request.TargetRole);

        // 8. Persist to DB
        var assessment = new PlacementAssessment
        {
            UserId = userId,
            TargetRole = request.TargetRole,
            SkillScoresJson = JsonSerializer.Serialize(skillScores),
            ReadinessScore = readinessScore,
            StrongSkillsJson = JsonSerializer.Serialize(strongSkills),
            WeakSkillsJson = JsonSerializer.Serialize(weakSkills),
            RecommendationsJson = JsonSerializer.Serialize(recommendations),
            RoadmapJson = JsonSerializer.Serialize(roadmap),
            CreatedAt = DateTime.UtcNow
        };

        _context.PlacementAssessments.Add(assessment);
        await _context.SaveChangesAsync();

        return BuildResultDto(assessment, skillScoreDtos, strongSkills, weakSkills, recommendations, roadmap);
    }

    // ─────────────────────────────────────────
    public async Task<List<PlacementHistoryDto>> GetMyAssessmentHistoryAsync(string userId)
    {
        var assessments = await _context.PlacementAssessments
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return assessments.Select(a => new PlacementHistoryDto
            {
                AssessmentId = a.Id,
                TargetRole = a.TargetRole,
                ReadinessScore = a.ReadinessScore,
                ReadinessLabel = GetReadinessLabel(a.ReadinessScore),
                StrongSkillCount = string.IsNullOrEmpty(a.StrongSkillsJson) ? 0 : a.StrongSkillsJson.Split(',').Count(s => !string.IsNullOrWhiteSpace(s.Trim('[', ']', '"'))),
                WeakSkillCount = string.IsNullOrEmpty(a.WeakSkillsJson) ? 0 : a.WeakSkillsJson.Split(',').Count(s => !string.IsNullOrWhiteSpace(s.Trim('[', ']', '"'))),
                CompletedAt = a.CreatedAt
            })
            .ToList();
    }

    // ─────────────────────────────────────────
    public async Task<PlacementResultDto> GetAssessmentByIdAsync(string userId, string assessmentId)
    {
        var assessment = await _context.PlacementAssessments
            .FirstOrDefaultAsync(a => a.Id == assessmentId && a.UserId == userId)
            ?? throw new InvalidOperationException("Assessment not found");

        var skillScores = JsonSerializer.Deserialize<Dictionary<string, double>>(assessment.SkillScoresJson) ?? new();
        var roleProfile = await _context.RoleSkillProfiles
            .FirstOrDefaultAsync(r => r.RoleName == assessment.TargetRole);
        var requirements = roleProfile != null
            ? JsonSerializer.Deserialize<List<DomainSkillRequirement>>(roleProfile.SkillsJson) ?? new()
            : new List<DomainSkillRequirement>();

        var skillScoreDtos = BuildSkillScoreDtos(skillScores, requirements);
        var strongSkills = JsonSerializer.Deserialize<List<string>>(assessment.StrongSkillsJson) ?? new();
        var weakSkills = JsonSerializer.Deserialize<List<string>>(assessment.WeakSkillsJson) ?? new();
        var recommendations = JsonSerializer.Deserialize<List<string>>(assessment.RecommendationsJson) ?? new();
        var roadmap = JsonSerializer.Deserialize<List<RoadmapWeekDto>>(assessment.RoadmapJson) ?? new();

        return BuildResultDto(assessment, skillScoreDtos, strongSkills, weakSkills, recommendations, roadmap);
    }

    // ─────────────────────────────────────────────────────
    // Private Helpers
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// Maps raw skill scores to the 7 ML feature buckets.
    /// Each role emphasizes different skills; unmapped features default to 50.
    /// </summary>
    private static PlacementData MapToMlFeatures(Dictionary<string, double> skillScores, string role)
    {
        double GetScore(params string[] possibleKeys)
        {
            foreach (var key in possibleKeys)
            {
                var match = skillScores.FirstOrDefault(kvp =>
                    kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (!match.Equals(default(KeyValuePair<string, double>)))
                    return match.Value;
            }
            // Average all scores if no exact match (reasonable default)
            return skillScores.Any() ? skillScores.Values.Average() : 50.0;
        }

        // Map domain-specific skills into the 7 ML feature buckets.
        // Non-tech roles map their domain knowledge areas into these slots.
        return new PlacementData
        {
            DsaScore = (float)GetScore("Data Structures", "DSA", "Algorithms", "Anatomy", "Psychology", "Pharmacology", "Accounting", "Visual Design"),
            ProgrammingScore = (float)GetScore("Programming", "Java", "Python", "C++", "JavaScript", "Physiology", "Psychopathology", "Drug Interactions", "Taxation", "Typography"),
            ProblemSolvingScore = (float)GetScore("Problem Solving", "Logical Reasoning", "Clinical Diagnosis", "Counseling", "Patient Counseling", "Auditing", "Color Theory"),
            AptitudeScore = (float)GetScore("Aptitude", "Mathematics", "Quantitative", "Medical Aptitude", "Clinical Aptitude"),
            SoftSkillsScore = (float)GetScore("Soft Skills", "Communication", "Teamwork", "Patient Care", "Ethics", "Regulations", "Business Law", "UI/UX"),
            SystemDesignScore = (float)GetScore("System Design", "Architecture", "Design Patterns", "OOP", "Pharmacotherapy", "Financial Analysis", "Adobe Suite"),
            DatabasesScore = (float)GetScore("Database", "SQL", "Databases", "MongoDB", "Chemistry", "Statistics")
        };
    }

    private static double CalculateFallbackScore(Dictionary<string, double> skillScores, List<DomainSkillRequirement> requirements)
    {
        if (!requirements.Any() || !skillScores.Any())
            return skillScores.Any() ? skillScores.Values.Average() : 50.0;

        double total = 0;
        double totalWeight = 0;

        foreach (var req in requirements)
        {
            var userScore = skillScores.TryGetValue(req.SkillName, out var s) ? s : 0;
            var contribution = (userScore / req.RequiredLevel) * req.Weightage;
            total += Math.Min(contribution, req.Weightage); // cap at requirement
            totalWeight += req.Weightage;
        }

        return totalWeight > 0 ? Math.Min(100, (total / totalWeight) * 100) : 50.0;
    }

    private static List<SkillScoreDto> BuildSkillScoreDtos(
        Dictionary<string, double> skillScores,
        List<DomainSkillRequirement> requirements)
    {
        var dtos = new List<SkillScoreDto>();

        // Add scores for skills with requirements
        foreach (var req in requirements)
        {
            var userScore = skillScores.TryGetValue(req.SkillName, out var s) ? s : 0;
            var gap = Math.Max(0, req.RequiredLevel - userScore);

            dtos.Add(new SkillScoreDto
            {
                SkillName = req.SkillName,
                Category = req.Category,
                UserScore = Math.Round(userScore, 1),
                RequiredLevel = req.RequiredLevel,
                GapPercentage = Math.Round(gap, 1),
                IsMet = userScore >= req.RequiredLevel,
                Weightage = req.Weightage
            });
        }

        // Add any assessed skills not in requirements
        foreach (var kvp in skillScores)
        {
            if (!requirements.Any(r => r.SkillName == kvp.Key))
            {
                dtos.Add(new SkillScoreDto
                {
                    SkillName = kvp.Key,
                    Category = "Technical",
                    UserScore = Math.Round(kvp.Value, 1),
                    RequiredLevel = 70,
                    GapPercentage = Math.Max(0, Math.Round(70 - kvp.Value, 1)),
                    IsMet = kvp.Value >= 70,
                    Weightage = 0
                });
            }
        }

        return dtos.OrderByDescending(d => d.UserScore).ToList();
    }

    private static List<string> GenerateRecommendations(List<string> weakSkills, string role)
    {
        var recommendations = new List<string>();

        var skillRecs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Data Structures"] = "Practice Arrays, Linked Lists, Trees, and Graphs. Solve 50+ problems on LeetCode.",
            ["Algorithms"] = "Study sorting algorithms, dynamic programming, and greedy approaches. Aim for 30+ algorithm problems.",
            ["DSA"] = "Practice Arrays, Linked Lists, Trees, and Graphs. Solve 50+ problems on LeetCode.",
            ["Problem Solving"] = "Take part in competitive programming on CodeForces or HackerRank weekly.",
            ["OOP"] = "Build 2 projects applying SOLID principles — a library system and an e-commerce backend.",
            ["System Design"] = "Study 'Designing Data-Intensive Applications'. Practice designing Uber, Twitter, and YouTube clones.",
            ["Java"] = "Complete Java 11 certification. Build a Spring Boot REST API with JPA integration.",
            ["Python"] = "Complete Python for Data Science course. Build 3 data analysis projects using Pandas and NumPy.",
            ["SQL"] = "Practice complex SQL queries: joins, window functions, CTEs. Complete SQLZoo exercises.",
            ["Database"] = "Practice SQL joins, indexes, normalization. Build a relational DB schema for a real app.",
            ["Databases"] = "Practice SQL joins, indexes, normalization. Build a relational DB schema for a real app.",
            ["Machine Learning"] = "Complete Andrew Ng's ML course. Implement 5 algorithms from scratch in Python.",
            ["Deep Learning"] = "Study Neural Networks, CNNs, RNNs. Build an image classifier using TensorFlow/PyTorch.",
            ["NLP"] = "Study transformers and BERT. Build a text classifier or chatbot using Hugging Face.",
            ["Networking"] = "Study TCP/IP, DNS, HTTP/HTTPS. Complete a networking fundamentals course (CompTIA Network+).",
            ["Linux"] = "Practice Linux commands daily. Complete a Linux administration certification.",
            ["Cryptography"] = "Study symmetric/asymmetric encryption, hashing. Implement basic crypto algorithms.",
            ["Aptitude"] = "Practice quantitative aptitude daily using IndiaBIX. Focus on time-speed-distance and probability.",
            ["Soft Skills"] = "Practice mock interviews. Work on communication through technical blog writing.",
            ["Communication"] = "Practice mock interviews. Record and review your verbal explanations of code.",
            ["Docker"] = "Build a multi-container app using Docker Compose. Learn container networking basics.",
            ["Kubernetes"] = "Deploy a microservice app to a local Kubernetes cluster using minikube.",
            ["AWS"] = "Obtain AWS Cloud Practitioner certification. Deploy a full-stack app to AWS.",
            ["Statistics"] = "Review probability, hypothesis testing, and regression. Complete a statistics course on Coursera.",
            ["Data Visualization"] = "Build 5 dashboards using matplotlib, seaborn, and Power BI.",
            ["REST APIs"] = "Build 3 REST APIs using Node.js/Express or .NET. Focus on proper status codes and auth.",
            ["React"] = "Build a full CRUD app in React. Study hooks, context API, and component optimization.",
            ["JavaScript"] = "Master ES6+, async/await, closures. Build 3 projects without frameworks.",
            ["CSS"] = "Study flexbox, grid, and responsive design. Replicate 5 real website UIs.",
        };

        foreach (var skill in weakSkills.Take(5))
        {
            if (skillRecs.TryGetValue(skill, out var rec))
                recommendations.Add($"[{skill}] {rec}");
            else
                recommendations.Add($"[{skill}] Deepen your understanding of {skill} through structured study and hands-on projects.");
        }

        if (!recommendations.Any())
            recommendations.Add($"Continue strengthening your skills for the {role} role through project-based learning and mock interviews.");

        return recommendations;
    }

    private static List<RoadmapWeekDto> GenerateRoadmap(List<string> weakSkills, List<string> strongSkills, string role)
    {
        var roadmap = new List<RoadmapWeekDto>();

        // Week 1-2: Core weak skills
        if (weakSkills.Any())
        {
            var coreWeakSkills = weakSkills.Take(3).ToList();
            roadmap.Add(new RoadmapWeekDto
            {
                Week = "Week 1–2",
                Focus = $"Core Foundations: {string.Join(", ", coreWeakSkills)}",
                Tasks = coreWeakSkills.SelectMany(skill => GenerateWeekTasks(skill)).Take(5).ToList()
            });
        }

        // Week 3: Problem solving & practice
        roadmap.Add(new RoadmapWeekDto
        {
            Week = "Week 3",
            Focus = "Applied Problem Solving",
            Tasks = new()
            {
                "Solve 15 LeetCode Easy/Medium problems daily",
                "Participate in 1 online coding contest",
                "Implement 2 data structures from scratch",
                "Review solutions and write explanations",
                "Time yourself: solve problems within 20 min each"
            }
        });

        // Week 4: Advanced weak skills
        if (weakSkills.Count > 3)
        {
            var advancedWeakSkills = weakSkills.Skip(3).Take(2).ToList();
            roadmap.Add(new RoadmapWeekDto
            {
                Week = "Week 4",
                Focus = $"Advanced Concepts: {string.Join(", ", advancedWeakSkills)}",
                Tasks = advancedWeakSkills.SelectMany(skill => GenerateWeekTasks(skill)).Take(5).ToList()
            });
        }

        // Week 5-6: Role-specific project
        roadmap.Add(new RoadmapWeekDto
        {
            Week = "Week 5–6",
            Focus = $"{role} Capstone Project",
            Tasks = GenerateProjectTasks(role)
        });

        // Week 7-8: Interview prep
        roadmap.Add(new RoadmapWeekDto
        {
            Week = "Week 7–8",
            Focus = "Interview Preparation",
            Tasks = new()
            {
                "Complete 3 mock technical interviews (Pramp/InterviewBit)",
                $"Practice explaining {role} concepts out loud",
                "Prepare 5 STAR-format behavioral stories",
                "Review system design questions for your role",
                "Research target companies and their tech stacks"
            }
        });

        return roadmap;
    }

    private static List<string> GenerateWeekTasks(string skill)
    {
        var taskMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Data Structures"] = new() { "Study Arrays, Stacks, Queues", "Learn Linked Lists with implementation", "Study Trees (BST, AVL)", "Practice Graph traversal (BFS, DFS)", "Solve 10 DSA problems" },
            ["Algorithms"] = new() { "Study sorting: Quick, Merge, Heap", "Learn Dynamic Programming with DP patterns", "Study Greedy algorithms", "Practice divide and conquer", "Solve 10 algorithm problems" },
            ["DSA"] = new() { "Study Arrays, Stacks, Queues", "Learn Linked Lists with implementation", "Study Trees (BST, AVL)", "Practice Graph traversal (BFS, DFS)", "Solve 10 DSA problems" },
            ["System Design"] = new() { "Study scalability fundamentals", "Learn load balancing and caching", "Design a URL shortener (Tiny URL)", "Design a chat system", "Review database sharding concepts" },
            ["Python"] = new() { "Study Python OOP and decorators", "Practice Pandas and NumPy", "Build a web scraper", "Study file I/O and error handling", "Implement 3 Python projects" },
            ["Java"] = new() { "Study Java collections framework", "Learn multithreading and concurrency", "Build a Spring Boot REST API", "Study JPA and Hibernate basics", "Implement design patterns in Java" },
            ["Machine Learning"] = new() { "Study Linear/Logistic Regression", "Learn Decision Trees and Random Forests", "Implement a classifier from scratch", "Study model evaluation metrics", "Build a predictive model project" },
            ["SQL"] = new() { "Practice complex JOINs", "Study window functions", "Learn query optimization with indexes", "Write 20 SQL queries from exercises", "Design a normalized database schema" },
            ["Networking"] = new() { "Study OSI model layers", "Learn TCP/IP and UDP", "Study DNS, HTTP, HTTPS", "Practice subnetting exercises", "Use Wireshark for packet analysis" },
            ["Linux"] = new() { "Master basic shell commands", "Study file permissions and users", "Learn bash scripting basics", "Practice networking commands", "Set up a Linux server (VirtualBox)" },
            ["Aptitude"] = new() { "Practice 20 aptitude questions daily", "Study number theory and probability", "Time yourself on each question type", "Review common exam patterns", "Take 2 full practice tests" },
            ["Soft Skills"] = new() { "Record a 2-minute technical explanation video", "Write 1 blog post about a tech topic", "Practice peer-to-peer code reviews", "Join a toastmasters or debate group", "Prepare elevator pitch for resume" },
            ["Docker"] = new() { "Build and run a Docker container", "Write a Dockerfile for your app", "Learn Docker Compose basics", "Study container networking", "Deploy multi-container app" },
            ["Deep Learning"] = new() { "Study neural network fundamentals", "Implement a neural net from scratch", "Build an image classifier using CNN", "Study RNNs and LSTM", "Train a model using PyTorch" },
        };

        if (taskMap.TryGetValue(skill, out var tasks))
            return tasks;

        return new() { $"Study {skill} fundamentals", $"Practice {skill} through exercises", $"Build a small project using {skill}", $"Read documentation and best practices for {skill}", $"Complete an online course module on {skill}" };
    }

    private static List<string> GenerateProjectTasks(string role)
    {
        return role switch
        {
            "Software Developer" => new() { "Build a CLI task manager in Java/C++", "Implement a simple OS scheduler simulation", "Create a RESTful API with authentication", "Write unit tests for your project (>80% coverage)", "Document with Swagger/OpenAPI" },
            "Full Stack Developer" => new() { "Build a full CRUD web app (React + Node.js + MongoDB)", "Implement JWT auth with refresh tokens", "Deploy to Vercel (frontend) and Railway (backend)", "Add real-time features using WebSockets", "Write end-to-end tests with Playwright" },
            "Data Scientist" => new() { "Build an EDA notebook on a Kaggle dataset", "Train and compare 3 ML models", "Build an interactive dashboard with Plotly/Dash", "Write a data pipeline using Pandas", "Document findings in a professional report" },
            "AI Engineer" => new() { "Fine-tune a pre-trained transformer model", "Build an NLP chatbot with Hugging Face", "Deploy model as REST API using FastAPI", "Implement model monitoring and logging", "Document model cards and performance metrics" },
            "Cybersecurity Analyst" => new() { "Complete a TryHackMe beginner room", "Set up a home lab with Kali Linux", "Practice OWASP Top 10 vulnerabilities", "Write a vulnerability assessment report", "Learn Metasploit for ethical penetration testing" },
            "Cloud Engineer" => new() { "Deploy a 3-tier architecture on AWS", "Use Terraform to provision infrastructure (IaC)", "Set up auto-scaling and load balancing", "Implement CloudWatch monitoring", "Create a CI/CD pipeline using AWS CodePipeline" },
            "DevOps Engineer" => new() { "Set up a full CI/CD pipeline with GitHub Actions", "Containerize an app with Docker", "Deploy to Kubernetes (minikube locally)", "Configure Prometheus + Grafana monitoring", "Automate infrastructure with Ansible" },
            _ => new() { $"Build a capstone project for {role}", "Write comprehensive documentation", "Present project with a 5-minute demo video", "Deploy to a cloud platform", "Get peer code review feedback" }
        };
    }

    private static PlacementResultDto BuildResultDto(
        PlacementAssessment assessment,
        List<SkillScoreDto> skillScoreDtos,
        List<string> strongSkills,
        List<string> weakSkills,
        List<string> recommendations,
        List<RoadmapWeekDto> roadmap)
    {
        return new PlacementResultDto
        {
            AssessmentId = assessment.Id,
            TargetRole = assessment.TargetRole,
            ReadinessScore = Math.Round(assessment.ReadinessScore, 1),
            ReadinessLabel = GetReadinessLabel(assessment.ReadinessScore),
            SkillScores = skillScoreDtos,
            StrongSkills = strongSkills,
            WeakSkills = weakSkills,
            Recommendations = recommendations,
            Roadmap = roadmap,
            CompletedAt = assessment.CreatedAt
        };
    }

    private static string GetReadinessLabel(double score) => score switch
    {
        >= 85 => "Placement Ready",
        >= 70 => "Almost Ready",
        >= 50 => "Needs Improvement",
        _ => "Significant Gaps"
    };
}
