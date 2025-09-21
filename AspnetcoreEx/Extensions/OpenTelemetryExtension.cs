
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AspnetcoreEx.Extensions;

public static class OpenTelemetryExtension
{
    public static IServiceCollection AddOpenTelemetryExtension(this IServiceCollection services, IConfiguration configuration)
    {
        var otelConfig = configuration.GetSection("OpenTelemetry");
        var exporters = otelConfig.GetSection("Exporters");
        var serviceName = otelConfig.GetValue<string>("ServiceName") ?? "net-service";

        var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName);

        AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

        // Tracing + Metrics
        services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName))
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddSource("Microsoft.SemanticKernel*")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(opt =>
                    {
                        configuration.GetSection("OpenTelemetry:Exporter").Bind(opt);
                    })
                    .AddConsoleExporter();
            })
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .AddMeter("Microsoft.SemanticKernel*")
                    .AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(opt =>
                    {
                        configuration.GetSection("OpenTelemetry:Exporter").Bind(opt);
                    })
                    .AddConsoleExporter();
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
                options.AddOtlpExporter(opt =>
                {
                    configuration.GetSection("OpenTelemetry:Exporter").Bind(opt);
                });
                options.AddConsoleExporter();
            });
            loggingBuilder.SetMinimumLevel(LogLevel.Information);
        });

        return services;
    }
}