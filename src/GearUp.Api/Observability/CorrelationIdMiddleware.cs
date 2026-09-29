using System.Diagnostics;

namespace GearUp.Api.Observability;

internal sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ILogger<CorrelationIdMiddleware> logger)
{
    private const int MaxCorrelationIdLength = 128;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ObterCorrelationId(context);

        context.Items[ObservabilityConstants.CorrelationIdHeader] = correlationId;
        Activity.Current?.SetTag("gearup.correlation_id", correlationId);
        context.Response.Headers[ObservabilityConstants.CorrelationIdHeader] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await next(context);
        }
    }

    private static string ObterCorrelationId(HttpContext context)
    {
        var recebido = context.Request.Headers[ObservabilityConstants.CorrelationIdHeader]
            .FirstOrDefault();

        if (CorrelationIdValido(recebido))
            return recebido!;

        return Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
    }

    private static bool CorrelationIdValido(string? correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId) ||
            correlationId.Length > MaxCorrelationIdLength)
        {
            return false;
        }

        return correlationId.All(character =>
            char.IsLetterOrDigit(character) ||
            character is '-' or '_' or '.');
    }
}
