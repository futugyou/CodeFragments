
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace KaleidoCode.OpenTelemetry;

public static class OpenTelemetryExtension
{
    public static IServiceCollection AddOpenTelemetryExtension(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenTelemetryOptions>(configuration.GetSection("OpenTelemetry"));
        var sp = services.BuildServiceProvider();
        var config = sp.GetRequiredService<IOptionsMonitor<OpenTelemetryOptions>>()!.CurrentValue;

        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(config.ServiceName);

        AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

        // Tracing + Metrics
        services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService(config.ServiceName))
        .WithTracing(tracerProviderBuilder =>
        {
            tracerProviderBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddHotChocolateInstrumentation()
                .AddSource("Microsoft.SemanticKernel*")
                .AddConfiguredExporters(config.Exporters);
        })
        .WithMetrics(meterProviderBuilder =>
        {
            meterProviderBuilder
                .AddMeter("Microsoft.SemanticKernel*")
                .AddConfiguredExporters(config.Exporters);
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

        if (exporters.Jaeger.Enabled)
        {
            builder.AddJaegerExporter(opt =>
            {
                opt.AgentHost = exporters.Jaeger.AgentHost;
                opt.AgentPort = exporters.Jaeger.AgentPort;
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

        if (exporters.AllowAspNetCoreInstrumentation)
        {
            builder.AddAspNetCoreInstrumentation();
        }

        if (exporters.AllowRuntimeInstrumentation)
        {
            builder.AddRuntimeInstrumentation();
        }

        if (exporters.AllowHttpClientInstrumentation)
        {
            builder.AddHttpClientInstrumentation();
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