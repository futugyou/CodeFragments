
namespace KaleidoCode.OpenTelemetry;

public static class OpenTelemetryExtension
{
    public static IServiceCollection AddCustomMetricsSimulation(this IServiceCollection services, IConfiguration configuration)
    {
        var counter = new MetricsCollector();
        services.AddHostedService<PerformanceMetricsCollector>();
        services.AddSingleton<IProcessorMetricsCollector>(counter);
        services.AddSingleton<IMemoryMetricsCollector>(counter);
        services.AddSingleton<INetworkMetricsCollector>(counter);
        services.AddSingleton<IMetricsDeliver, MetricsDeliver>();

        return services;
    }
 }