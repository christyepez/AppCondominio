using System.Diagnostics;

namespace AppCondominio.Api;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.TraceIdentifier = correlationId;
        Activity.Current?.SetTag("app.correlation_id", correlationId);
        context.Response.Headers[HeaderName] = correlationId;
        await next(context);
    }
}
