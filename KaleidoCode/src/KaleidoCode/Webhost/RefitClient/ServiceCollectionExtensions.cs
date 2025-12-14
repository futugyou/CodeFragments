
namespace KaleidoCode.RefitClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRefitClientExtension(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // 类型“TimeoutRejectedException”同时存在于“Polly.Core, Version=8.0.0.0, Culture=neutral, PublicKeyToken=c8a3ffc3f8f825cc”和
        // “Polly, Version=7.0.0.0, Culture=neutral, PublicKeyToken=c8a3ffc3f8f825cc”中
        // Polly.Timeout.TimeoutRejectedException fullname does not work.
        // var retryPolicy = HttpPolicyExtensions
        //     .HandleTransientHttpError()
        //     .Or<Polly.Timeout.TimeoutRejectedException>()
        //     .WaitAndRetryAsync(10, _ => TimeSpan.FromMilliseconds(5000));

        var timeoutPolicy = Polly.Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(30000));

        services.AddRefitClient<IGitHubApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.github.com"))
            // .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(timeoutPolicy);

        return services;
    }
}
