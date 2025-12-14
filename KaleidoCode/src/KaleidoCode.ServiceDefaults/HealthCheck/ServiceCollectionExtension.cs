
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class HealthCheckServiceCollectionExtension
{
    public static IServiceCollection AddHealthCheckExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecksUI().AddInMemoryStorage();
        services.AddHealthChecks().AddCheck<DefaultHealthCheck>("self", tags: ["live"]);

        return services;
    }

}