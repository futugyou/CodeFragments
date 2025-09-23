

namespace KaleidoCode.Extensions;

public static class KestrelExtensions
{
    // include kestrel / hsts / httpsredirection / queuepolicy / stackpolicy config.
    public static WebApplicationBuilder AddKestrelExtensions(this WebApplicationBuilder builder, IConfiguration configuration)
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

        builder.Services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
            options.Preload = true;
        });

        builder.Services.AddQueuePolicy(options =>
        {
            options.MaxConcurrentRequests = 20;
            options.RequestQueueLimit = 20;
        });

        builder.Services.AddStackPolicy(options =>
        {
            options.MaxConcurrentRequests = 20;
            options.RequestQueueLimit = 20;
        });

        builder.Services.AddHttpsRedirection(options => options.HttpsPort = 58176);

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