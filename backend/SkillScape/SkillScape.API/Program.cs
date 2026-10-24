using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using SkillScape.Application.Interfaces;
using SkillScape.Infrastructure.Data;
using SkillScape.Infrastructure.Services;

using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using SkillScape.Application.Configuration;
using SkillScape.API.Hubs;
using SkillScape.API.Data;
using SkillScape.API.Middleware;
using FluentValidation;
using System.Data.Common;
using System.Diagnostics;
using System.Security.Cryptography;


// Load environment variables from .env file before the configuration is built
void LoadEnvFile(string path)
{
    if (File.Exists(path))
    {
        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            {
                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
                }
            }
        }
    }
}

// Try current directory
LoadEnvFile(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
// Try parent directory (common for solution-level .env)
LoadEnvFile(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "", ".env"));

var builder = WebApplication.CreateBuilder(args);
// Add development secrets to the app's configuration so the JWT setup below
// and options binding both read the same value as `dotnet user-secrets`.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);
}

static async Task EnsureLocalDbInstanceStartedAsync(string connectionString)
{
    if (!OperatingSystem.IsWindows())
    {
        return;
    }

    var connection = new DbConnectionStringBuilder { ConnectionString = connectionString };
    var dataSource = new[] { "Data Source", "Server", "Address", "Addr", "Network Address" }
        .Select(key => connection.TryGetValue(key, out var value) ? value?.ToString() : null)
        .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    const string localDbPrefix = "(localdb)\\";
    if (dataSource is null || !dataSource.StartsWith(localDbPrefix, StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    var instanceName = dataSource[localDbPrefix.Length..].Trim();
    if (string.IsNullOrWhiteSpace(instanceName))
    {
        throw new InvalidOperationException("The LocalDB connection string does not specify an instance name.");
    }

    var startInfo = new ProcessStartInfo("sqllocaldb.exe")
    {
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };
    startInfo.ArgumentList.Add("start");
    startInfo.ArgumentList.Add(instanceName);

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Could not start sqllocaldb.exe.");
    var standardOutput = process.StandardOutput.ReadToEndAsync();
    var standardError = process.StandardError.ReadToEndAsync();
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));

    try
    {
        await process.WaitForExitAsync(timeout.Token);
    }
    catch (OperationCanceledException)
    {
        process.Kill(entireProcessTree: true);
        throw new TimeoutException($"Starting LocalDB instance '{instanceName}' exceeded 60 seconds.");
    }

    var output = $"{await standardOutput}{await standardError}".Trim();
    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException(
            $"Could not start LocalDB instance '{instanceName}'. {output}");
    }
}

static bool IsLocalDbStartupFailure(Exception exception)
{
    for (var current = exception; current != null; current = current.InnerException!)
    {
        if (current.Message.Contains("Local Database Runtime", StringComparison.OrdinalIgnoreCase)
            || current.Message.Contains("LocalDB instance startup", StringComparison.OrdinalIgnoreCase)
            || current.Message.Contains("SQL Server process failed to start", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
    }

    return false;
}

static async Task WaitForLocalDbReadyAsync(ApplicationDbContext context, string connectionString)
{
    if (!connectionString.Contains("(localdb)\\", StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    // Allow only a few retries for a transient LocalDB hand-off. A persistent
    // engine startup failure is environmental and should surface promptly.
    const int maxAttempts = 3;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await context.Database.OpenConnectionAsync();
            await context.Database.CloseConnectionAsync();
            return;
        }
        catch (Exception exception) when (IsLocalDbStartupFailure(exception) && attempt < maxAttempts)
        {
            // LocalDB can report that its instance started before the SQL Server
            // process is ready to accept a connection. Retry only that transient case.
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}

// Add services to the container
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblies(new[] { typeof(Program).Assembly, typeof(SkillScape.Application.DTOs.LoginRequest).Assembly });

// Add Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add MongoDB
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<IMongoClient>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var mongoSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
    mongoSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(1);
    return new MongoClient(mongoSettings);
});
builder.Services.AddSingleton(provider =>
{
    var settings = provider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return provider.GetRequiredService<IMongoClient>().GetDatabase(settings.DatabaseName);
});
builder.Services.AddScoped<MongoDbContext>();

// Add Application Services
builder.Services.Configure<JwtSettings>(options =>
{
    builder.Configuration.GetSection("JwtSettings").Bind(options);
    if (string.IsNullOrWhiteSpace(options.SecretKey))
    {
        options.SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            options.SecretKey = builder.Configuration["JwtSettings:SecretKey"] ?? "";
        }
    }
});
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlacementService, PlacementService>();
builder.Services.AddScoped<IChatBotService, ChatBotService>();
builder.Services.AddScoped<IReverseCareerFinderService, ReverseCareerFinderService>();
builder.Services.AddScoped<IDomainService, DomainService>();
builder.Services.AddScoped<IAiPromptService, AiPromptService>();
builder.Services.AddHttpClient<IRoadmapWebSearchService, RoadmapWebSearchService>();
builder.Services.AddScoped<IRoadmapCurriculumGeneratorService, RoadmapCurriculumGeneratorService>();
builder.Services.AddScoped<IFreeformRoadmapService, FreeformRoadmapService>();
builder.Services.AddScoped<IRoadmapService, RoadmapService>();
builder.Services.AddScoped<IQuestionValidator, QuestionValidator>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IKnowledgeIngestionService, KnowledgeIngestionService>();

// Question Pipeline Services
builder.Services.AddScoped<IQuestionValidationEngine, QuestionValidationEngine>();
builder.Services.AddScoped<IProfileValidationEngine, ProfileValidationEngine>();
builder.Services.AddScoped<IDeterministicQuestionSelector, DeterministicQuestionSelector>();
builder.Services.AddScoped<IQuestionGenerationService, QuestionGenerationService>();
builder.Services.AddScoped<IDynamicLlmQuestionGenerator, LlmQuestionGeneratorService>();
builder.Services.AddScoped<IAssessmentBlueprintService, AssessmentBlueprintService>();
builder.Services.AddScoped<IMultiDimensionalScoringEngine, MultiDimensionalScoringEngine>();
builder.Services.AddScoped<IQuestionQualityValidator, QuestionQualityValidator>();
builder.Services.AddScoped<IQuestionRelevanceValidator, QuestionRelevanceValidator>();
builder.Services.AddScoped<IAgeLevelValidator, AgeLevelValidator>();
builder.Services.AddScoped<IAnswerOptionValidator, AnswerOptionValidator>();
builder.Services.AddScoped<IDuplicateDetector, SemanticDuplicateDetector>();
builder.Services.AddScoped<ISemanticSimilarityService, LocalSemanticSimilarityService>();

// ML Pipeline Services (Parts 25-30)
// NOTE: ModelTrainingCoordinator and PredictionExplainerService removed — SkillScape.ML project
// was excluded to avoid AppLocker-blocked native DLLs. EligibilityEvaluatorService remains (Infrastructure).
builder.Services.AddScoped<IEligibilityService, EligibilityEvaluatorService>();

builder.Services.AddScoped<IProfileParserService, ProfileParserService>();
builder.Services.AddScoped<IOcrIngestionService, ProfileParserService>();
builder.Services.AddScoped<IProfileMapperService, ProfileMapperService>();
builder.Services.AddScoped<IGoalValidationEngine, GoalValidationEngine>();
builder.Services.AddScoped<IQuizMlPredictionService, QuizMlPredictionService>();
builder.Services.AddScoped<IDynamicRoadmapEngine, DynamicRoadmapEngine>();

builder.Services.AddScoped<ICareerPredictionService, CareerPredictionService>();
builder.Services.AddScoped<ICareerGuidanceService, CareerGuidanceService>();
builder.Services.AddScoped<IAdaptiveQuizService, AdaptiveQuizService>();
builder.Services.AddScoped<ISalaryIntelligenceService, SalaryIntelligenceService>();
builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddHttpClient<ITrendService, TrendService>();
builder.Services.AddScoped<IResumeTextExtractor, ResumeTextExtractor>();
builder.Services.AddScoped<ICareerProfileService, CareerProfileService>();

// ─── Career Readiness & Skill Gap Analyzer ──────────────────────────────
// CareerIntelligenceService is a singleton — loads JSON files once at startup
builder.Services.AddSingleton<CareerIntelligenceService>();
builder.Services.AddSingleton<ICareerIntelligenceService>(sp => sp.GetRequiredService<CareerIntelligenceService>());
builder.Services.AddScoped<ISkillGapAnalyzerService, SkillGapAnalyzerService>();

// AI Peer Matching Engine Module & Caching
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ILearningCircleService, LearningCircleService>();

// Add ChatBot Services - Using Groq as Primary, OpenAI as Fallback
builder.Services.AddScoped<IKnowledgeBaseService>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<KnowledgeBaseService>>();
    var knowledgeBasePath = Path.GetFullPath(
        Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "SkillScape.Infrastructure", "KnowledgeBase"));
    logger.LogInformation($"Knowledge Base Path: {knowledgeBasePath}");
    if (!Directory.Exists(knowledgeBasePath))
    {
        logger.LogWarning($"Knowledge Base directory does not exist: {knowledgeBasePath}");
    }
    return new KnowledgeBaseService(logger, knowledgeBasePath);
});

// Register Groq as primary AI service (faster inference)
builder.Services.AddHttpClient<GroqService>();
builder.Services.AddScoped<IOpenAIService>(provider =>
    provider.GetRequiredService<GroqService>());

// Register OpenAI service for fallback
builder.Services.AddHttpClient<OpenAIService>();

// Register Gemini service
builder.Services.AddHttpClient<GeminiService>();

// Register Dedicated Resume Groq Service (GROQ_API_RESUME + model openai/gpt-oss-120b)
builder.Services.AddHttpClient<ResumeGroqService>();
builder.Services.AddScoped<IResumeGroqService, ResumeGroqService>();

// Configure SignalR (with optional Redis Backplane for Load Balancing)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddSignalR().AddStackExchangeRedis(redisConnectionString);
}
else
{
    builder.Services.AddSignalR();
}

// ML.NET Prediction Engine pools removed — models are not deployed in this environment.
// PlacementService uses deterministic fallback scoring (CalculateFallbackScore) instead.

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        if (allowedOrigins is null || allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one allowed frontend origin.");
        }

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
if (string.IsNullOrWhiteSpace(secretKey))
{
    secretKey = builder.Configuration["JwtSettings:SecretKey"];
}
const string insecureDefaultJwtSecret = "your-super-secret-key-change-this-in-production-at-least-32-characters-long";

static bool IsValidJwtSecret(string? value) =>
    !string.IsNullOrWhiteSpace(value) &&
    value != insecureDefaultJwtSecret &&
    value.Length >= 32;

if (!IsValidJwtSecret(secretKey) && builder.Environment.IsDevelopment())
{
    // User Secrets are scoped to a Windows account. Keep a stable development
    // key in that user's local profile as a fallback so `dotnet run` works even
    // when the shell uses a different account than the one that configured secrets.
    var localSecretPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SkillScape",
        "jwt-secret");

    if (File.Exists(localSecretPath))
    {
        secretKey = File.ReadAllText(localSecretPath).Trim();
    }

    if (!IsValidJwtSecret(secretKey))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(localSecretPath)!);
        secretKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        File.WriteAllText(localSecretPath, secretKey);
    }
}

if (!IsValidJwtSecret(secretKey))
{
    throw new InvalidOperationException(
        "JWT SecretKey must be a non-default secret with at least 32 characters.");
}

// Ensure the options binding below uses the exact validated signing key.
builder.Configuration["JwtSettings:SecretKey"] = secretKey;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && 
                    (path.StartsWithSegments("/hubs/chat") || path.StartsWithSegments("/hubs/peer-rooms")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Custom middleware
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map SignalR Hubs
app.MapHub<ChatHub>("/hubs/chat");

// Database initialization
await EnsureLocalDbInstanceStartedAsync(connectionString);
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    await WaitForLocalDbReadyAsync(context, connectionString);
    context.Database.Migrate();
    await DatabaseSeeder.DeactivateOrphanedLearningCirclesAsync(context);
    
    // Disable heavy seeders on every startup to prevent 40+ minute delays.
    // They can be run manually or triggered via a specific CLI arg if needed.
    // await DatabaseSeeder.SeedAsync(context);
    await V2DataSeeder.SeedAsync(context);
    await DatabaseSeeder.SeedAdminAnalyticsSampleResultsAsync(context);
    
    // KnowledgeGraphSeeder removed — SkillScape.DataPipeline project excluded to avoid AppLocker-blocked DLLs.
    // await KnowledgeGraphSeeder.SeedMockGovernmentTaxonomyAsync(services);
    
    // ML.NET Model Training disabled — SkillScape.ML project removed to avoid AppLocker block.
}

app.Run();
