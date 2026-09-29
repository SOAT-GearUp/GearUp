using System.Diagnostics;
using GearUp.Application.Common.Observability;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace GearUp.Api.Observability;

internal static class DependencyInjection
{
    public static ILoggingBuilder AddGearUpStructuredLogging(
        this ILoggingBuilder logging,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        logging.ClearProviders();
        logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.UseUtcTimestamp = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        });

        logging.Services.Configure<LoggerFactoryOptions>(options =>
        {
            options.ActivityTrackingOptions =
                ActivityTrackingOptions.TraceId |
                ActivityTrackingOptions.SpanId |
                ActivityTrackingOptions.ParentId;
        });

        var endpoint = ObterOtlpEndpoint(configuration);
        if (endpoint is null)
            return logging;

        //envia os dados ao OpenTelemetry Collector
        logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.SetResourceBuilder(CriarResourceBuilder(configuration, environment));
            options.AddOtlpExporter(exporter => exporter.Endpoint = endpoint);
        });

        return logging;
    }

    public static IServiceCollection AddGearUpObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var endpoint = ObterOtlpEndpoint(configuration);

        services.AddSingleton<ApiMetrics>();
        services.Replace(ServiceDescriptor.Singleton<IOrdemServicoMetrics, OrdemServicoMetrics>());

        services.AddOpenTelemetry()
            .ConfigureResource(resource => ConfigurarResource(resource, configuration, environment))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (endpoint is not null)
                    tracing.AddOtlpExporter(exporter => exporter.Endpoint = endpoint);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(ObservabilityConstants.MeterName);

                if (endpoint is not null)
                    metrics.AddOtlpExporter(exporter => exporter.Endpoint = endpoint);
            });

        return services;
    }

    private static ResourceBuilder CriarResourceBuilder(
        IConfiguration configuration,
        IHostEnvironment environment) =>
        ConfigurarResource(ResourceBuilder.CreateDefault(), configuration, environment);

    private static ResourceBuilder ConfigurarResource(
        ResourceBuilder resource,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "gearup-api";
        var serviceNamespace = configuration["OpenTelemetry:ServiceNamespace"] ?? "gearup";
        var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString();

        return resource
            .AddService(serviceName, serviceNamespace, serviceVersion)
            .AddAttributes([
                new KeyValuePair<string, object>("deployment.environment.name", environment.EnvironmentName)
            ]);
    }

    private static Uri? ObterOtlpEndpoint(IConfiguration configuration)
    {
        var valor = configuration["OpenTelemetry:Otlp:Endpoint"];
        if (string.IsNullOrWhiteSpace(valor))
            valor = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return Uri.TryCreate(valor, UriKind.Absolute, out var endpoint)
            ? endpoint
            : throw new InvalidOperationException("OpenTelemetry:Otlp:Endpoint deve ser uma URI absoluta.");
    }
}
