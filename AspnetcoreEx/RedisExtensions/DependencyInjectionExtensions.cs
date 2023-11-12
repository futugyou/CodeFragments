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
        // choose one of them
        services.AddOptions<RedisOptions>().Bind(configuration.GetSection("RedisOptions")).Validate(Validate, "not pass");
        services.AddSingleton<IValidateOptions<RedisOptions>, RedisConfigValidation>();
        services.AddSingleton<IRedisClient, RedisClient>();
        services.AddSingleton<RedisProfiler>();
        services.AddHttpContextAccessor();
        return services;
    }

    static bool Validate(RedisOptions o)
    {
        // do validate here!
        return true;
    }
}
