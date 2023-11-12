using AspnetcoreEx;
using AspnetcoreEx.Elasticsearch;
using AspnetcoreEx.Extensions;
using AspnetcoreEx.GraphQL;
using AspnetcoreEx.HealthCheckExtensions;
using AspnetcoreEx.RedisExtensions;
using AspnetcoreEx.RouteEx;
using AspnetcoreEx.SemanticKernel;
using AspnetcoreEx.StaticFileEx;

// MiniExtensions.StartMiniAspnetCore();

var options = new WebApplicationOptions
{
    Args = args,
    ApplicationName = "AspnetcoreEx",
    // System.IO.DirectoryNotFoundException: /workspaces/CodeFragments/AspnetcoreEx/wwwroot/
    ContentRootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), ""),
    EnvironmentName = "Development"
};

var builder = WebApplication.CreateBuilder(options);

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


// Concurrency Limiter middleware has been deprecated
// builder.Services.Configure<ConcurrencyLimiterOptions>(options =>
// {
//     options.OnRejected = ConcurrencyRejectAsync;
// });

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

var configuration = builder.Configuration;

//configuration.AddJsonFileExtensions("appsettings.json", true, true);

Console.WriteLine(builder.Environment.ApplicationName);
Console.WriteLine(builder.Environment.ContentRootPath);
Console.WriteLine(builder.Environment.WebRootPath);
Console.WriteLine(builder.Environment.EnvironmentName);
builder.Services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILogger<Program>>());

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

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .Or<TimeoutRejectedException>()
    .WaitAndRetryAsync(10, _ => TimeSpan.FromMilliseconds(5000));

var timeoutPolicy = Polly.Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(30000));

builder.Services.AddRefitClient<IGitHubApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.github.com"))
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(timeoutPolicy);

builder.Services.AddGraphQL(configuration, builder.Environment);

builder.Services.AddHealthChecksUI().AddInMemoryStorage();
builder.Services.AddHealthChecks().AddCheck<DemoHealthCheck>("demo-health");
builder.Services.AddDIExtension();
// The path must be absolute
builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider("/"));
builder.Services.AddSingleton<IFileSystem, FileSystem>();

var counter = new MetricsCollector();
builder.Services.AddHostedService<PerformanceMetricsCollector>();
builder.Services.AddSingleton<IProcessorMetricsCollector>(counter);
builder.Services.AddSingleton<IMemoryMetricsCollector>(counter);
builder.Services.AddSingleton<INetworkMetricsCollector>(counter);
builder.Services.AddSingleton<IMetricsDeliver, MetricsDeliver>();

builder.Services.AddSemanticKernelServices(configuration);

var app = builder.Build();

// 'ConcurrencyLimiterExtensions.UseConcurrencyLimiter(IApplicationBuilder)' is obsolete
// app.UseConcurrencyLimiter();

var rewriteOptions = new RewriteOptions()
    // client redirect
    .AddRedirect("^text/(.*)", "bar/$1")
    // server rewrite
    .AddRewrite(regex: "^text/(.*)", replacement: "bar/$1", skipRemainingRules: true)
    .AddIISUrlRewrite(fileProvider: app.Environment.ContentRootFileProvider, filePath: "rewrite.xml")
    .AddApacheModRewrite(fileProvider: app.Environment.ContentRootFileProvider, filePath: "rewrite.config");
app.UseRewriter(rewriteOptions);
app.UseHttpsRedirection().UseHsts();
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
app.RoutePatternFactoryExtension();
app.MapHealthChecksUI(options => options.UIPath = "/health-ui");
// app.UseMiddleware<ResponseCustomMiddleware>();

// this will win
// app.Run("http://localhost:5004/");
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