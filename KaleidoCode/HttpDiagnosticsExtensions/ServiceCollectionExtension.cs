
namespace KaleidoCode.HttpDiagnosticsExtensions;

public static class HttpExtension
{
    public static IServiceCollection AddHttpDiagnosticsExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HttpDiagnosticsOptions>(configuration.GetSection("HttpDiagnostics"));
        var sp = services.BuildServiceProvider();
        var config = sp.GetRequiredService<IOptionsMonitor<HttpDiagnosticsOptions>>()!.CurrentValue;

        if (config.AllowConfigureHttpClient)
        {
            services.AddSingleton<SimpleConsoleLogger>();
            services.AddSingleton<RequestIdLogger>();
            services.AddTransient<TestAuthHandler>();
            services.AddTransient<EnrichmentHandler>();
            services.ConfigureHttpClientDefaults(static http =>
            {
                // http.AddLogger<SimpleConsoleLogger>();
                // http.AddLogger<RequestIdLogger>();
                // http.ConfigureHttpClient(c => c.DefaultRequestHeaders.UserAgent.ParseAdd("HttpClient/8.0"));
                // b.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseCookies = false });
                // http.AddHttpMessageHandler<TestAuthHandler>();
                // http.AddHttpMessageHandler<EnrichmentHandler>();
                // http.AddHttpMessageHandler<ClientSideRateLimitedHandler>();
                // it will remove all log, see read,md in HttpDiagnosticsExtensions
                // b.RemoveAllLoggers();
                // http.AddServiceDiscovery();
            });
        }

        // consume-events-in-process
        services.AddTelemetryConsumer<YarpTelemetryConsumer>();

        return services;
    }

}