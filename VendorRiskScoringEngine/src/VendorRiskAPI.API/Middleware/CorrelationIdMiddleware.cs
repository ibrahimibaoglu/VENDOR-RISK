using Serilog.Context;

namespace VendorRiskAPI.API.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Add to response headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers.Add(CorrelationIdHeader, correlationId);
            }
            return Task.CompletedTask;
        });

        // Add to Serilog context
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        // Try to get from request headers
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) 
            && !string.IsNullOrEmpty(correlationId))
        {
            return correlationId!;
        }

        // Generate new one
        return Guid.NewGuid().ToString();
    }
}
