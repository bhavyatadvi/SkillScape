namespace SkillScape.API.Middleware;

/// <summary>
/// Request/Response logging middleware
/// </summary>
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        _logger.LogInformation($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {request.Method} {request.Path}");

        var originalBodyStream = context.Response.Body;

        try
        {
            await _next(context);
        }
        finally
        {
            var statusCode = context.Response.StatusCode;
            _logger.LogInformation($"Response: {statusCode}");
            context.Response.Body = originalBodyStream;
        }
    }
}
