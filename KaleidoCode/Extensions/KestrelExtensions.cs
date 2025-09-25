
namespace KaleidoCode.Extensions;

public class KestrelOptions
{
    public bool AllowKestrelConfig { get; set; }
    public bool AllowHstsConfig { get; set; }
    public bool AllowQueuePolicyConfig { get; set; }
    public bool AllowStackPolicyConfig { get; set; }
    public bool AllowHttpsRedirectionConfig { get; set; }
    public int HstsMaxAge { get; set; } = 365;
    public bool HstsIncludeSubDomains { get; set; } = true;
    public bool HstsPreload { get; set; } = true;
    public int QueueMaxConcurrentRequests { get; set; } = 20;
    public int QueueRequestQueueLimit { get; set; } = 20;
    public int StackMaxConcurrentRequests { get; set; } = 20;
    public int StackRequestQueueLimit { get; set; } = 20;
    public int HttpsRedirectionPort { get; set; } = 58176;

}

public static class KestrelExtensions
{
    // include kestrel / hsts / httpsredirection / queuepolicy / stackpolicy config.
    public static WebApplicationBuilder AddKestrelExtensions(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.Configure<KestrelOptions>(configuration.GetSection("KestrelOptions"));
        var sp = builder.Services.BuildServiceProvider();
        var config = sp.GetRequiredService<IOptionsMonitor<KestrelOptions>>()!.CurrentValue;

        if (config.AllowKestrelConfig)
        {
            //  Overriding address(es) 'https://localhost:58176, http://localhost:58177'. Binding to endpoints defined via IConfiguration and/or UseKestrel() instead.
            builder.WebHost.UseKestrel(kestrel =>
            {
                kestrel.Listen(IPAddress.Any, 58177);
                kestrel.Listen(IPAddress.Any, 58176, listener =>
                {
                    listener.UseHttps(https =>
                    {
                        https.ServerCertificateSelector = SelectServerCertificate;
                    });
                });
            });
        }

        if (config.AllowQueuePolicyConfig)
        {
            builder.Services.AddQueuePolicy(options =>
            {
                options.MaxConcurrentRequests = config.QueueMaxConcurrentRequests;
                options.RequestQueueLimit = config.QueueRequestQueueLimit;
            });
        }

        if (config.AllowStackPolicyConfig)
        {
            builder.Services.AddStackPolicy(options =>
            {
                options.MaxConcurrentRequests = config.StackMaxConcurrentRequests;
                options.RequestQueueLimit = config.StackRequestQueueLimit;
            });
        }

        if (config.AllowHttpsRedirectionConfig)
        {
            builder.Services.AddHttpsRedirection(options => options.HttpsPort = config.HttpsRedirectionPort);
        }

        if (config.AllowHstsConfig)
        {
            builder.Services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(config.HstsMaxAge);
                options.IncludeSubDomains = config.HstsIncludeSubDomains;
                options.Preload = config.HstsPreload;
            });
        }

        return builder;
    }

    static X509Certificate2? SelectServerCertificate(ConnectionContext? context, string? domain)
    {
        return domain?.ToLowerInvariant() switch
        {
            "dome.com" => CertificateLoader.LoadFromStoreCert("dome.com", "my", StoreLocation.CurrentUser, true),
            _ => null,
        };
    }
}

public static class WebApplicationExtension
{
    public static WebApplication UseKestrelExtensions(this WebApplication app)
    {
        var config = app.Services.GetRequiredService<IOptionsMonitor<KestrelOptions>>()!.CurrentValue;

        if (config.AllowHttpsRedirectionConfig)
        {
            app.UseHttpsRedirection();
        }

        if (config.AllowHstsConfig)
        {
            app.UseHsts();
        }

        return app;
    }

}