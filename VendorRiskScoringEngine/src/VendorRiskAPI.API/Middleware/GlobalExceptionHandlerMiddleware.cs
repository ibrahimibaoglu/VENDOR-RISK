using System.Net;
using System.Text.Json;

namespace VendorRiskAPI.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "An error occurred while processing your request.",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                response.Status = (int)HttpStatusCode.BadRequest;
                response.Title = "Bad Request";
                response.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                break;

            case KeyNotFoundException:
                response.Status = (int)HttpStatusCode.NotFound;
                response.Title = "Resource Not Found";
                response.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                break;

            case UnauthorizedAccessException:
                response.Status = (int)HttpStatusCode.Unauthorized;
                response.Title = "Unauthorized";
                response.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                break;

            default:
                response.Status = (int)HttpStatusCode.InternalServerError;
                break;
        }

        context.Response.StatusCode = response.Status;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}

public class ErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
