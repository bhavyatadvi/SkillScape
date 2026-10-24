using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using SkillScape.Application.DTOs;
using SkillScape.Application.Interfaces;
using SkillScape.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SkillScape.Infrastructure.Services;

/// <summary>
/// Main chatbot service that orchestrates AI responses with knowledge base
/// Uses Groq as primary provider with OpenAI as fallback for reliability
/// </summary>
public class ChatBotService : IChatBotService
{
    private readonly IOpenAIService _openAIService;
    private readonly IKnowledgeBaseService _knowledgeBaseService;
    private readonly GroqService _groqService;
    private readonly OpenAIService _openAIFallbackService;
    private readonly GeminiService _geminiService;
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatBotService> _logger;
    private readonly Dictionary<string, List<ConversationContext>> _conversationHistory;
    private string _systemPrompt = "";

    public ChatBotService(
        IOpenAIService openAIService,
        IKnowledgeBaseService knowledgeBaseService,
        GroqService groqService,
        OpenAIService openAIFallbackService,
        GeminiService geminiService,
        ApplicationDbContext dbContext,
        IConfiguration configuration,
        ILogger<ChatBotService> logger)
    {
        _openAIService = openAIService;
        _knowledgeBaseService = knowledgeBaseService;
        _groqService = groqService;
        _openAIFallbackService = openAIFallbackService;
        _geminiService = geminiService;
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
        _conversationHistory = new Dictionary<string, List<ConversationContext>>();
        LoadSystemPrompt();
        _logger.LogInformation("ChatBot Service initialized with Personalized Context and Multi-Provider Support");
    }

    /// <summary>
    /// Process a user message and generate an AI response
    /// </summary>
    public async Task<ChatResponse> ProcessMessageAsync(ChatRequest chatRequest, string userId)
    {
        try
        {
            if (!_conversationHistory.ContainsKey(userId))
            {
                _conversationHistory[userId] = new List<ConversationContext>();
            }

            var response = new ChatResponse { Message = "" };

            // Special handling for greetings
            if (IsGreeting(chatRequest.Message))
            {
                return new ChatResponse 
                { 
                    Message = "Hii! I'm Skill Navigator, your AI career assistant. What can I help you with today? I can help you discover careers, learn new skills, or create a roadmap!",
                    ConfidenceScore = 1.0
                };
            }

            // Determine intent and route to appropriate handler
            var intent = DetermineIntent(chatRequest.Message);

            switch (intent)
            {
                case "career_guidance":
                    response = await HandleCareerGuidanceAsync(chatRequest, userId);
                    break;
                case "skill_recommendation":
                    response = await HandleSkillRecommendationAsync(chatRequest, userId);
                    break;
                case "roadmap_request":
                    response = await HandleRoadmapRequestAsync(chatRequest, userId);
                    break;
                default:
                    response = await HandleGeneralQueryAsync(chatRequest, userId);
                    break;
            }

            // Add both user message and assistant response to history at once
            _conversationHistory[userId].Add(new ConversationContext
            {
                Role = "user",
                Content = chatRequest.Message
            });

            _conversationHistory[userId].Add(new ConversationContext
            {
                Role = "assistant",
                Content = response.Message
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"ChatBot Error: {ex.Message}");
            
            // DATA-DRIVEN FALLBACK v2.0
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var userName = user?.FullName ?? "there";
            
            var query = chatRequest.Message.ToLower();
            var allCareers = await _knowledgeBaseService.GetRelevantCareersAsync(new List<string> { "Technology", "Data", "Security" }, limit: 50);
            
            var bestMatch = allCareers
                .Where(c => query.Contains(c.Title.ToLower()) || c.Title.ToLower().Split(' ').Any(w => query.Contains(w) && w.Length > 3))
                .OrderByDescending(c => c.Title.Split(' ').Count(w => query.Contains(w.ToLower())))
                .FirstOrDefault();

            var careerText = "";
            var careerResults = new List<CareerSuggestion>();

            if (bestMatch != null)
            {
                careerResults.Add(bestMatch);
                careerText = $"\n\n### 📋 Responsibilities of {bestMatch.Title}:\n- {string.Join("\n- ", bestMatch.KeyResponsibilities)}\n\n**Average Salary:** {bestMatch.AverageSalary}\n**Job Outlook:** {bestMatch.JobOutlook}";
            }
            else
            {
                careerResults = await _knowledgeBaseService.GetRelevantCareersAsync(new List<string> { query }, limit: 1);
                if (careerResults.Any())
                {
                    var career = careerResults.First();
                    careerText = $"\n\n### 📋 Responsibilities of {career.Title}:\n- {string.Join("\n- ", career.KeyResponsibilities.Take(5))}";
                }
            }

            var fallbackMessage = bestMatch != null || careerResults.Any()
                ? $"Hi {userName}! I couldn't reach an AI provider just now, so I can only share the matching information available in Skill Navigator.{careerText}\n\nPlease try again in a moment for a complete answer."
                : $"Hi {userName}! I couldn't reach an AI provider just now, and I don't have enough verified local information to answer that accurately. Please try again in a moment.";

            return new ChatResponse
            {
                Message = fallbackMessage,
                ConfidenceScore = careerResults.Any() ? 0.5 : 0.0,
                Careers = careerResults
            };
        }
    }

    /// <summary>
    /// Get conversation history for a user
    /// </summary>
    public Task<List<ConversationContext>> GetConversationHistoryAsync(string userId, int limit = 10)
    {
        if (_conversationHistory.TryGetValue(userId, out var history))
        {
            return Task.FromResult(history.TakeLast(limit).ToList());
        }

        return Task.FromResult(new List<ConversationContext>());
    }

    /// <summary>
    /// Clear conversation history for a user
    /// </summary>
    public Task ClearConversationHistoryAsync(string userId)
    {
        if (_conversationHistory.ContainsKey(userId))
        {
            _conversationHistory.Remove(userId);
        }

        return Task.CompletedTask;
    }

    // Private helper methods

    private void LoadSystemPrompt()
    {
        _systemPrompt = @"You are Skill Scapy, a Voice-Enabled AI career assistant who is friendly, encouraging, and genuinely excited to help people discover their ideal career path.

Your Personality & Language:
- You are Skill Scapy!
- Conversational and warm, like talking to a knowledgeable friend.
- Enthusiastic about career development and skill building.
- Patient and non-judgmental.
- Always positive and encouraging.
- **CRITICAL:** You will often receive messages in regional languages (like Hindi, Gujarati, Marathi, Tamil, etc.). You MUST reply in the EXACT SAME language the user uses. If they speak in Hindi, reply in Hindi. If Gujarati, reply in Gujarati. Maintain your helpful career-guide persona in whatever language you use.

Your Core Functions:
1. **Career Discovery** - Help users find careers that match their interests and skills
2. **Skill Development** - Recommend specific skills to learn and provide learning paths
3. **Career Planning** - Create actionable roadmaps with milestones and timelines
4. **Industry Insights** - Share information about job market trends and opportunities

Key Guidelines:
1. **Be Conversational** - Use natural language, ask follow-up questions, and show genuine interest
2. **Be Specific** - Provide concrete next steps and actionable advice, not generic platitudes
3. **Be Encouraging** - Always emphasize that career growth is possible and achievable
4. **Be Practical** - Balance idealism with realistic timelines and effort requirements
5. **Admit Limitations Gracefully** - If you don't have specific data, offer alternative help
6. **Ask Clarifying Questions** - When users are vague, ask questions to understand their real needs
7. **Celebrate Progress** - Acknowledge and encourage any steps they're taking toward their goals

Topics You Can Discuss:
- Career options across all industries
- Skills required for different roles
- Learning resources and training programs
- Salary expectations and career progression
- Job market trends
- Work-life balance considerations
- Career transitions and pivots
- Mentorship and professional development
- Education pathways (bootcamps, degrees, certifications)

Things to Remember:
- Career paths are unique to each person
- Multiple paths can lead to the same destination
- It's never too late to change careers
- Small consistent progress is better than waiting for the perfect moment
- Curiosity and willingness to learn matter more than initial experience

When Someone Asks for a Roadmap:
- Ask which specific field they're interested in
- Break down the journey into clear phases
- Mention realistic timeframes
- Include both technical and soft skills
- Suggest resources and learning options
- Celebrate their interest in planning ahead

Response Format:
- Answer the user's actual question first; do not give unrelated career lists.
- Use concise Markdown headings, short paragraphs, and bullet or numbered lists.
- For a roadmap, use the headings Goal, Starting Point, Phases, Practice Projects, and Next Step when relevant.
- Keep every phase specific to the requested career and distinguish prerequisites from optional specializations.
- Never invent a qualification, salary, regulation, or job guarantee. Clearly label estimates and suggest verifying time-sensitive details.
- Do not return JSON, XML, or Markdown code fences for an ordinary conversational answer.

Start conversations warmly, listen carefully, and help them take their first step toward their dream career!";
    }

    /// <summary>
    /// Build a specialized prompt for career guidance that includes knowledge base data and user personalization
    /// </summary>
    private async Task<string> BuildCareerGuidancePromptAsync(ChatRequest chatRequest, List<CareerSuggestion> careers, string userId)
    {
        var userContext = await GetUserPersonalizationContextAsync(userId);
        var careerContext = "";
        if (careers.Any())
        {
            careerContext = "Based on our database, here are some matching careers:\n";
            foreach (var career in careers.Take(3))
            {
                careerContext += $"\n- **{career.Title}**: {career.Description ?? "A rewarding career path"} (Match: {(career.MatchScore * 100):F0}%)";
                if (!string.IsNullOrEmpty(career.AverageSalary))
                {
                    careerContext += $" - Avg. Salary: {career.AverageSalary}";
                }
            }
            careerContext += "\n\nUse this information to give personalized recommendations about these careers.";
        }

        return $@"You are Skill Navigator, helping someone explore career options.
        
[USER PROFILE CONTEXT]
{userContext}

{careerContext}

The user is asking: {chatRequest.Message}

Respond in a friendly, conversational way. Use the user's name and reference their existing skills and interests to make the advice feel personal. If you mentioned careers from the database above, explain why they might be a good fit based on what you know about the user. Ask follow-up questions to understand their goals better.";
    }

    private async Task<string> GetUserPersonalizationContextAsync(string userId)
    {
        try
        {
            var user = await _dbContext.Users
                .Include(u => u.UserSkills).ThenInclude(us => us.Skill)

                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return "User profile not found.";

            var skills = string.Join(", ", user.UserSkills.Select(s => s.Skill.Name));

            return $@"Name: {user.FullName}
Current Skills: {skills}
Level: {user.Level}
Total XP: {user.TotalXP}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch user personalization context");
            return "Unable to fetch user profile details.";
        }
    }

    /// <summary>
    /// Build a specialized prompt for skill recommendations that includes knowledge base data
    /// </summary>
    private string BuildSkillRecommendationPrompt(ChatRequest chatRequest, List<SkillRequirement> skills)
    {
        var skillContext = "";
        if (skills.Any())
        {
            skillContext = "Based on our knowledge base, here are the recommended skills to develop:\n";
            foreach (var skill in skills.Take(5))
            {
                skillContext += $"\n- **{skill.Name}**: {skill.Description ?? "Important skill for career growth"}";
                if (skill.EstimatedMonths > 0)
                {
                    skillContext += $" (Estimated: {skill.EstimatedMonths} months to learn)";
                }
                if (skill.Resources.Any())
                {
                    skillContext += $" - Resources available";
                }
            }
            skillContext += "\n\nReference these skills in your response and create a learning plan.";
        }

        return $@"You are Skill Navigator, helping someone develop their skills.

{skillContext}

The user is asking: {chatRequest.Message}

Respond in a friendly, conversational way. Give them specific, actionable steps to learn these skills. Mention realistic timeframes and break it down into manageable phases. Be encouraging and explain why these skills matter for their career growth.";
    }

    /// <summary>
    /// Build a specialized prompt for roadmap requests that includes knowledge base data
    /// </summary>
    private string BuildRoadmapPrompt(ChatRequest chatRequest, Roadmap? roadmap, List<CareerSuggestion> careers)
    {
        var roadmapContext = "";
        
        if (roadmap != null)
        {
            roadmapContext = $"Based on our knowledge base, here's a structured roadmap:\n";
            roadmapContext += $"- **Goal**: {roadmap.Title}\n";
            roadmapContext += $"- **Total Duration**: {roadmap.TotalDurationMonths} months\n";
            if (roadmap.Phases != null && roadmap.Phases.Any())
            {
                roadmapContext += $"\nLearning Phases:\n";
                foreach (var p in roadmap.Phases.Take(4))
                {
                    roadmapContext += $"\n**{p.Name}** ({p.DurationMonths} months)\n";
                    if (p.Topics.Any())
                    {
                        roadmapContext += $"  Topics: {string.Join(", ", p.Topics.Take(3))}\n";
                    }
                    if (p.Milestones.Any())
                    {
                        roadmapContext += $"  Milestones: {string.Join(", ", p.Milestones.Take(2))}\n";
                    }
                }
            }
            roadmapContext += "\nUse this roadmap structure in your response.";
        }
        else if (careers.Any())
        {
            roadmapContext = $"These careers match their interest: {string.Join(", ", careers.Select(c => c.Title))}\nCreate a roadmap for developing in one of these directions.";
        }

        return $@"You are Skill Navigator, helping someone create a learning roadmap.

{roadmapContext}

The user is asking: {chatRequest.Message}

Create a friendly, step-by-step roadmap. Break it into clear phases (Foundation, Intermediate, Advanced). Mention realistic timelines. Include specific skills to learn in each phase. Be encouraging and make it feel achievable. Ask clarifying questions if needed.";
    }

    private string DetermineIntent(string message)
    {
        var lowerMessage = message.ToLower();

        // Roadmap and path planning - check first as it's most specific
        if (lowerMessage.Contains("roadmap") || lowerMessage.Contains("path") || 
            lowerMessage.Contains("plan") || lowerMessage.Contains("timeline") ||
            lowerMessage.Contains("learning path") || lowerMessage.Contains("how to start") ||
            lowerMessage.Contains("step by step") || lowerMessage.Contains("progression"))
        {
            return "roadmap_request";
        }

        // Skills and learning
        if (lowerMessage.Contains("skill") || lowerMessage.Contains("learn") || 
            lowerMessage.Contains("train") || lowerMessage.Contains("course") ||
            lowerMessage.Contains("certification") || lowerMessage.Contains("prerequisite") ||
            lowerMessage.Contains("what do i need") || lowerMessage.Contains("how to learn"))
        {
            return "skill_recommendation";
        }

        // Career guidance
        if (lowerMessage.Contains("career") || lowerMessage.Contains("job") || 
            lowerMessage.Contains("profession") || lowerMessage.Contains("work") ||
            lowerMessage.Contains("field") || lowerMessage.Contains("industry") ||
            lowerMessage.Contains("which career") || lowerMessage.Contains("what career") ||
            lowerMessage.Contains("what job") || lowerMessage.Contains("switch career") ||
            lowerMessage.Contains("salary") || lowerMessage.Contains("opportunities"))
        {
            return "career_guidance";
        }

        return "general";
    }

    private bool IsGreeting(string message)
    {
        var lower = message.ToLower().Trim().TrimEnd('!', '.', '?');
        var greetings = new[] { "hello", "hi", "hey", "hii", "greetings", "yo", "sup" };
        return greetings.Contains(lower);
    }

    /// <summary>
    /// Get AI response with fallback logic - respects AI_PROVIDER configuration
    /// </summary>
    private async Task<string> GetAIResponseWithFallbackAsync(string systemPrompt, string userMessage, List<ConversationContext> conversationHistory)
    {
        var primaryProvider = (Environment.GetEnvironmentVariable("AI_PROVIDER")
            ?? _configuration["AI_PROVIDER"]
            ?? "groq").Trim().ToLowerInvariant();
        _logger.LogInformation($"Using primary AI provider: {primaryProvider}");

        // Define the sequence of providers based on selection
        var providers = new List<(string Name, Func<Task<string>> Call)>();

        if (primaryProvider == "groq")
        {
            providers.Add(("Groq", () => _groqService.GetResponseAsync(systemPrompt, userMessage, conversationHistory)));
            providers.Add(("Gemini", () => _geminiService.GetResponseAsync(systemPrompt, userMessage, conversationHistory)));
            providers.Add(("OpenAI", () => _openAIFallbackService.GetResponseAsync(systemPrompt, userMessage, conversationHistory)));
        }
        else if (primaryProvider == "openai")
        {
            providers.Add(("OpenAI", () => _openAIFallbackService.GetResponseAsync(systemPrompt, userMessage, conversationHistory)));
            providers.Add(("Gemini", () => _geminiService.GetResponseAsync(systemPrompt, userMessage, conversationHistory)));
            providers.Add(("Groq", () => _groqService.GetResponseAsync(systemPrompt, userMessage, conversationHistory)));
        }
        else // default to gemini
        {
            providers.Add(("Gemini", () => _geminiService.GetResponseAsync(systemPrompt, userMessage, conversationHistory)));
            providers.Add(("Groq", () => _groqService.GetResponseAsync(systemPrompt, userMessage, conversationHistory)));
            providers.Add(("OpenAI", () => _openAIFallbackService.GetResponseAsync(systemPrompt, userMessage, conversationHistory)));
        }

        List<Exception> exceptions = new List<Exception>();

        foreach (var (name, call) in providers)
        {
            try
            {
                _logger.LogInformation($"Attempting to get response from {name}");
                var response = await call();
                if (!string.IsNullOrWhiteSpace(response))
                {
                    _logger.LogInformation($"Successfully received response from {name}");
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"{name} service failed, trying next fallback...");
                exceptions.Add(ex);
            }
        }

        _logger.LogError("All AI providers failed.");
        throw new InvalidOperationException("All AI providers (Gemini, Groq, OpenAI) failed. Please check your API keys and internet connection.", 
            new AggregateException(exceptions));
    }


    private async Task<ChatResponse> HandleCareerGuidanceAsync(ChatRequest chatRequest, string userId)
    {
        var response = new ChatResponse { Message = "" };

        try
        {
            // Get relevant careers based on interests
            var careers = await _knowledgeBaseService.GetRelevantCareersAsync(
                chatRequest.Interests.Any() ? chatRequest.Interests : new List<string> { "Technology" },
                limit: 3
            );

            var conversationHistory = await GetConversationHistoryAsync(userId, limit: 5);

            // Build specialized prompt that INCLUDES the knowledge base data and user personalization
            string careerGuidancePrompt = await BuildCareerGuidancePromptAsync(chatRequest, careers, userId);

            // Get AI response with fallback
            var aiResponse = await GetAIResponseWithFallbackAsync(
                careerGuidancePrompt,  // Use specialized prompt, not generic
                chatRequest.Message,
                conversationHistory
            );

            response.Message = aiResponse;
            response.Careers = careers;
            response.ConfidenceScore = 0.9;

            // Get skills for top career if available
            if (careers.Any())
            {
                var topCareer = careers.First();
                try
                {
                    response.Skills = await _knowledgeBaseService.GetCareerSkillsAsync(
                        topCareer.Title,
                        chatRequest.SkillLevel ?? "Beginner"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get skills for career");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in career guidance, re-throwing to main handler");
            throw; // Re-throw to be caught by the personalized handler in ProcessMessageAsync
        }

        return response;
    }

    private async Task<ChatResponse> HandleSkillRecommendationAsync(ChatRequest chatRequest, string userId)
    {
        var response = new ChatResponse { Message = "" };

        try
        {
            // Get relevant skills information
            var skills = new List<SkillRequirement>();
            var careers = await _knowledgeBaseService.GetRelevantCareersAsync(
                chatRequest.Interests.Any() ? chatRequest.Interests : new List<string> { "Technology" },
                limit: 1
            );

            if (careers.Any())
            {
                skills = await _knowledgeBaseService.GetCareerSkillsAsync(
                    careers.First().Title,
                    chatRequest.SkillLevel ?? "Beginner"
                );
            }

            var conversationHistory = await GetConversationHistoryAsync(userId, limit: 5);

            // Build specialized prompt that INCLUDES the skills data
            string skillPrompt = BuildSkillRecommendationPrompt(chatRequest, skills);

            // Get AI response with specialized prompt
            var aiResponse = await GetAIResponseWithFallbackAsync(
                skillPrompt,
                chatRequest.Message,
                conversationHistory
            );

            response.Message = aiResponse;
            response.Skills = skills;
            response.ConfidenceScore = 0.85;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in skill recommendation, re-throwing to main handler");
            throw;
        }

        return response;
    }

    private async Task<ChatResponse> HandleRoadmapRequestAsync(ChatRequest chatRequest, string userId)
    {
        var response = new ChatResponse { Message = "" };

        try
        {
            // Check if user specified a field/career interest
            if (!chatRequest.Interests.Any())
            {
                // No specific field mentioned - ask user to choose
                var availableCareers = await _knowledgeBaseService.GetRelevantCareersAsync(
                    new List<string> { "Technology", "Business", "Healthcare", "Creative", "Engineering" },
                    limit: 5
                );

                var careerList = string.Join(", ", availableCareers.Select(c => c.Title));
                var conversationHistory = await GetConversationHistoryAsync(userId, limit: 5);
                
                var clarificationPrompt = $@"The user asked for a learning roadmap or career path guidance. They haven't specified which field they're interested in.
                
Available career fields include: {careerList}

Help them choose by asking which field interests them most. Be enthusiastic and explain what each field involves in simple terms. Make it conversational like you're a friend helping them explore options.";

                var aiResponse = await GetAIResponseWithFallbackAsync(
                    _systemPrompt,
                    clarificationPrompt,
                    conversationHistory
                );

                response.Message = aiResponse;
                response.Careers = availableCareers;
                response.ConfidenceScore = 0.75;
                return response;
            }

            // User specified a field - get roadmap
            var careersByInterest = await _knowledgeBaseService.GetRelevantCareersAsync(
                chatRequest.Interests,
                limit: 1
            );

            var conversationHistoryFull = await GetConversationHistoryAsync(userId, limit: 5);
            Roadmap? roadmapData = null;
            
            // Try to get roadmap data from knowledge base
            if (careersByInterest.Any())
            {
                try
                {
                    roadmapData = await _knowledgeBaseService.GetCareerRoadmapAsync(
                        careersByInterest.First().Title,
                        chatRequest.SkillLevel ?? "Beginner"
                    );
                    response.Careers = careersByInterest;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get roadmap data");
                }
            }

            // Build specialized prompt that INCLUDES roadmap data
            string roadmapPrompt = BuildRoadmapPrompt(chatRequest, roadmapData, careersByInterest);

            // Get AI response with specialized prompt
            var aiResponseFull = await GetAIResponseWithFallbackAsync(
                roadmapPrompt,
                chatRequest.Message,
                conversationHistoryFull
            );

            response.Message = aiResponseFull;
            response.Roadmap = roadmapData;
            response.ConfidenceScore = 0.88;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in roadmap request, re-throwing to main handler");
            throw;
        }

        return response;
    }

    private async Task<ChatResponse> HandleGeneralQueryAsync(ChatRequest chatRequest, string userId)
    {
        var response = new ChatResponse { Message = "" };

        try
        {
            // Get AI response for general query with fallback
            var conversationHistory = await GetConversationHistoryAsync(userId, limit: 5);
            var aiResponse = await GetAIResponseWithFallbackAsync(
                _systemPrompt,
                chatRequest.Message,
                conversationHistory
            );

            response.Message = aiResponse;
            response.ConfidenceScore = 0.75;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in general query, re-throwing to main handler");
            throw;
        }

        return response;
    }
}

