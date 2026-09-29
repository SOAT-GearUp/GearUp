using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Routing;

namespace GearUp.Api.Observability;

internal sealed class ApiMetrics : IDisposable
{
    private readonly Meter meter = new(ObservabilityConstants.MeterName);
    private readonly Counter<long> errors;

    public ApiMetrics()
    {
        errors = meter.CreateCounter<long>(
            "gearup.api.erros",
            description: "Quantidade de erros tratados pela API.");
    }

    public void RegistrarFalha(
        HttpContext httpContext,
        Exception exception,
        string code,
        int statusCode)
    {
        var route = (httpContext.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText
            ?? "rota_desconhecida";

        var tags = new TagList
        {
            { "http.route", route },
            { "http.response.status_code", statusCode },
            { "error.type", exception.GetType().Name },
            { "gearup.error.code", code }
        };

        errors.Add(1, tags);

        if (statusCode >= StatusCodes.Status500InternalServerError)
            Activity.Current?.SetStatus(ActivityStatusCode.Error, code);

        Activity.Current?.AddEvent(new ActivityEvent(
            "gearup.api.erro",
            tags: new ActivityTagsCollection
            {
                { "error.type", exception.GetType().Name },
                { "gearup.error.code", code },
                { "http.response.status_code", statusCode }
            }));
    }

    public void Dispose() => meter.Dispose();
}
