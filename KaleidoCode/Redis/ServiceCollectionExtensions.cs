namespace KaleidoCode.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisExtension(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        return services.AddRedisExtension(configuration);
    }

    public static IServiceCollection AddRedisExtension(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

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
