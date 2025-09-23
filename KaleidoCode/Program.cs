
using KaleidoCode.Auth;
using KaleidoCode.Elasticsearch;
using KaleidoCode.Extensions;
using KaleidoCode.GraphQL;
using KaleidoCode.HealthCheckExtensions;
using KaleidoCode.Redis;
using KaleidoCode.RouteEx;
using KaleidoCode.StaticFileEx;
using KaleidoCode.HttpDiagnosticsExtensions;
using KaleidoCode.KernelService;
using KaleidoCode.HostedService;
using KaleidoCode.OpenTelemetry;
using Microsoft.Extensions.Http.Resilience;
using KaleidoCode.RefitClient;
using Microsoft.Extensions.ServiceDiscovery;
using KaleidoCode.MQTT;

var options = new WebApplicationOptions
{
    Args = args,
    ApplicationName = "KaleidoCode",
    // System.IO.DirectoryNotFoundException: /workspaces/CodeFragments/KaleidoCode/wwwroot/
    ContentRootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), ""),
    EnvironmentName = "Development"
};

var builder = WebApplication.CreateBuilder(options);

var configuration = builder.Configuration;
configuration.AddJsonFileExtensions("ok.json", true, true);

//  Overriding address(es) 'https://localhost:58176, http://localhost:58177'. Binding to endpoints defined via IConfiguration and/or UseKestrel() instead.
//builder.WebHost.UseKestrel(kestrel =>
//{
//    kestrel.Listen(IPAddress.Any, 58177);
//    kestrel.Listen(IPAddress.Any, 58176, listener =>
//    {
//        listener.UseHttps(https =>
//        {
//            https.ServerCertificateSelector = SelectServerCertificate;
//        });
//    });
//});

builder.Services.AddHttpsRedirection(options => options.HttpsPort = 58176);

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

builder.Services.AddSingleton<SimpleConsoleLogger>();
builder.Services.AddSingleton<RequestIdLogger>();
builder.Services.AddTransient<TestAuthHandler>();
builder.Services.AddTransient<EnrichmentHandler>();
builder.Services.ConfigureHttpClientDefaults(static http =>
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

// consume-events-in-process
builder.Services.AddTelemetryConsumer<YarpTelemetryConsumer>();

builder.Services.AddOpenTelemetryExtension(configuration);
builder.Services.AddCustomMetricsSimulation(configuration);
builder.Services.AddAuthExtension(configuration);


Console.WriteLine(builder.Environment.ApplicationName);
Console.WriteLine(builder.Environment.ContentRootPath);
Console.WriteLine(builder.Environment.WebRootPath);
Console.WriteLine(builder.Environment.EnvironmentName);

// builder.Services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILogger<Program>>());

builder.Services.AddRouteExtension();
builder.Services.AddElasticClientExtension(configuration);
builder.Services.AddRedisExtension(configuration);
builder.Services.AddControllers();
builder.Services.AddScoped<ResponseCustomMiddleware>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ResponseBodyFeature", Version = "v1" });
});

builder.Services.AddRefitClientExtension(configuration);

var section = builder.Configuration.GetSection("RetryOptions");
builder.Services.Configure<HttpRetryStrategyOptions>(section);

builder.Services.AddGraphQL(configuration, builder.Environment);

builder.Services.AddHealthChecksUI().AddInMemoryStorage();
builder.Services.AddHealthChecks().AddCheck<DemoHealthCheck>("demo-health");
builder.Services.AddDIExtension();
// The path must be absolute
builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider("/"));
builder.Services.AddSingleton<IFileSystem, FileSystem>();



await builder.Services.AddKernelServiceServices(configuration);
builder.Services.AddKernelMemoryServices(configuration);

// builder.Services.AddMQTTExtension(configuration);

builder.Services.Configure<KaleidoCode.Controllers.TestOption>(configuration);
configuration.AddAwsParameterStore();

// this will use aspire, not base asp.net core
// builder.Services.AddPassThroughServiceEndPointResolver();
// builder.Services.AddConfigurationServiceEndPointResolver(options =>
// {
//     options.SectionName = "Services";

//     // Configure the logic for applying host name metadata
//     options.ApplyHostNameMetadata = static endpoint =>
//     {
//         // Your custom logic here. For example:
//         return endpoint.EndPoint is DnsEndPoint dnsEp && dnsEp.Host.StartsWith("internal");
//     };
// });
// builder.Services.Configure<ConfigurationServiceEndPointResolverOptions>(
//     static options =>
//     {
//         options.SectionName = "MyServiceEndpoints";

//         // Configure the logic for applying host name metadata
//         options.ApplyHostNameMetadata = static endpoint =>
//         {
//             // Your custom logic here. For example:
//             return endpoint.EndPoint is DnsEndPoint dnsEp
//                 && dnsEp.Host.StartsWith("internal");
//         };
//     });

builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(_ =>
{
    var queueCapacity = 100;
    return new DefaultBackgroundTaskQueue(queueCapacity);
});

var app = builder.Build();

var rewriteOptions = new RewriteOptions()
    // client redirect
    .AddRedirect("^text/(.*)", "bar/$1")
    // server rewrite
    .AddRewrite(regex: "^text/(.*)", replacement: "bar/$1", skipRemainingRules: true)
    .AddIISUrlRewrite(fileProvider: app.Environment.ContentRootFileProvider, filePath: "rewrite.xml")
    .AddApacheModRewrite(fileProvider: app.Environment.ContentRootFileProvider, filePath: "rewrite.config");
app.UseRewriter(rewriteOptions);

// app.UseHttpsRedirection().UseHsts();

// app.Urls.Add("http://localhost:5003/");
var environment = app.Environment;
Console.WriteLine(environment.ApplicationName);
Console.WriteLine(environment.ContentRootPath);
Console.WriteLine(environment.WebRootPath);
Console.WriteLine(environment.EnvironmentName);

app.StaticFileComposite();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ResponseBodyFeature v1"));
}

if (app.Environment.IsDevelopment())
{
    app.MapSwagger("/openapi/{documentName}.json");
    app.MapScalarApiReference("/docs");
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
app.UseGraphQLCustom();

app.MapControllers();
app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});
app.MapHealthChecksUI(options => options.UIPath = "/health-ui");
// app.UseMiddleware<ResponseCustomMiddleware>();

app.RoutePatternFactoryExtension();

await app.InitAIData();
// this will win
// app.Run("http://localhost:5004/");
app.MapMcp();

app.Run();

static X509Certificate2? SelectServerCertificate(ConnectionContext? context, string? domain)
{
    return domain?.ToLowerInvariant() switch
    {
        "dome.com" => CertificateLoader.LoadFromStoreCert("dome.com", "my", StoreLocation.CurrentUser, true),
        _ => null,
    };
}

static Task ConcurrencyRejectAsync(HttpContext httpContext)
{
    var request = httpContext.Request;
    if (!request.Query.ContainsKey("reject"))
    {
        var response = httpContext.Response;
        response.StatusCode = 307;
        var queryString = request.QueryString.Add("reject", "add");
        var newUrl = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path, queryString);
        response.Headers.Location = newUrl;
    }

    return Task.CompletedTask;
}