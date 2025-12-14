
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public static class OpenTelemetryServiceCollectionExtension
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static IServiceCollection AddOpenTelemetryExtension(this IServiceCollection services, IConfiguration configuration, string appName)
    {
        services.Configure<OpenTelemetryOptions>(configuration.GetSection("OpenTelemetry"));
        var config = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryOptions>() ?? new();

        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(config.ServiceName);

        AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

        // Tracing + Metrics
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(config.ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(appName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        tracing.Filter = context => !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath))
                    .AddHttpClientInstrumentation()
                    .AddSource("HotChocolate.Diagnostics")
                    .AddSource("Microsoft.SemanticKernel*")
                    .AddSource("agent-telemetry-source")
                    .AddConfiguredExporters(config.Exporters);
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter("Microsoft.SemanticKernel*")
                    .AddMeter("agent-telemetry-source");

                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                metrics.AddConfiguredExporters(config.Exporters);
            });

        // Logging
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.AddConfiguredExporters(config.Exporters);
            });
            loggingBuilder.SetMinimumLevel(LogLevel.Information);
        });

        return services;
    }

    public static TracerProviderBuilder AddConfiguredExporters(
        this TracerProviderBuilder builder, ExportersSettings exporters)
    {
        if (exporters.Otlp.Enabled)
        {
            builder.AddOtlpExporter(opt =>
            {
                if (!string.IsNullOrEmpty(exporters.Otlp.Endpoint))
                    opt.Endpoint = new Uri(exporters.Otlp.Endpoint);

                if (!string.IsNullOrEmpty(exporters.Otlp.Protocol) &&
                    Enum.TryParse<OtlpExportProtocol>(exporters.Otlp.Protocol, true, out var protocol))
                {
                    opt.Protocol = protocol;
                }

                if (!string.IsNullOrEmpty(exporters.Otlp.Headers))
                    opt.Headers = exporters.Otlp.Headers;
            });
        }

        if (exporters.Console.Enabled)
        {
            builder.AddConsoleExporter();
        }


        return builder;
    }

    public static MeterProviderBuilder AddConfiguredExporters(
        this MeterProviderBuilder builder, ExportersSettings exporters)
    {
        if (exporters.Otlp.Enabled)
        {
            builder.AddOtlpExporter(opt =>
            {
                if (!string.IsNullOrEmpty(exporters.Otlp.Endpoint))
                    opt.Endpoint = new Uri(exporters.Otlp.Endpoint);

                if (!string.IsNullOrEmpty(exporters.Otlp.Protocol) &&
                    Enum.TryParse<OtlpExportProtocol>(exporters.Otlp.Protocol, true, out var protocol))
                {
                    opt.Protocol = protocol;
                }

                if (!string.IsNullOrEmpty(exporters.Otlp.Headers))
                    opt.Headers = exporters.Otlp.Headers;
            });
        }

        if (exporters.Console.Enabled)
        {
            builder.AddConsoleExporter();
        }

        return builder;
    }

    public static OpenTelemetryLoggerOptions AddConfiguredExporters(
        this OpenTelemetryLoggerOptions options, ExportersSettings exporters)
    {
        if (exporters.Otlp.Enabled)
        {
            options.AddOtlpExporter(opt =>
            {
                if (!string.IsNullOrEmpty(exporters.Otlp.Endpoint))
                    opt.Endpoint = new Uri(exporters.Otlp.Endpoint);

                if (!string.IsNullOrEmpty(exporters.Otlp.Protocol) &&
                    Enum.TryParse<OtlpExportProtocol>(exporters.Otlp.Protocol, true, out var protocol))
                {
                    opt.Protocol = protocol;
                }

                if (!string.IsNullOrEmpty(exporters.Otlp.Headers))
                    opt.Headers = exporters.Otlp.Headers;
            });
        }

        if (exporters.Console.Enabled)
        {
            options.AddConsoleExporter();
        }

        return options;
    }
}