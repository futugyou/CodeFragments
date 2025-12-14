
namespace KaleidoCode.HealthCheck;

public static class HealthCheckExtension
{
    public static IServiceCollection AddHealthCheckExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecksUI().AddInMemoryStorage();
        services.AddHealthChecks().AddCheck<DemoHealthCheck>("demo-health");

        return services;
    }

}