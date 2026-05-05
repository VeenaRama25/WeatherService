using System.Net;
using System.Text.Json;

namespace WeatherMicroservice.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Upstream API error");
            await WriteErrorAsync(context, HttpStatusCode.BadGateway,
                "Upstream weather API is unavailable. Please try again shortly.");
        }
        catch (OperationCanceledException)
        {
            await WriteErrorAsync(context, HttpStatusCode.RequestTimeout, "Request timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext ctx, HttpStatusCode code, string message)
    {
        ctx.Response.StatusCode = (int)code;
        ctx.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { error = message, status = (int)code });
        await ctx.Response.WriteAsync(body);
    }
}