using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspnetcoreEx.RedisExtensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddRedisExtension(this IServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        return services.AddRedisExtension(configuration);
    }

    public static IServiceCollection AddRedisExtension(this IServiceCollection services, IConfiguration configuration)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        services.Configure<RedisOptions>(configuration.GetSection("RedisOptions"));
        services.AddSingleton<IRedisClient, RedisClient>();
        services.AddSingleton<RedisProfiler>();
        services.AddHttpContextAccessor();
        return services;
    }
}