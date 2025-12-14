
using AgentStack;
using CompanyReports;
using GraphQLStack;
using KaleidoCode.Extensions;
using KaleidoCode.HttpDiagnosticsExtensions;
using KaleidoCode.HostedService;
using KaleidoCode.MQTT;
using KaleidoCode.OpenTelemetry;
using KaleidoCode.Redis;
using KaleidoCode.RefitClient;
using KaleidoCode.RouteEx;
using KaleidoCode.StaticFileEx;
using OpenSearchStack;
using SemanticKernelStack;
using KernelMemoryStack;

var options = new WebApplicationOptions
{
    Args = args,
    ApplicationName = "Webhost",
    // System.IO.DirectoryNotFoundException: /workspaces/CodeFragments/KaleidoCode/wwwroot/
    ContentRootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), ""),
    EnvironmentName = "Development"
};

var builder = WebApplication.CreateBuilder(options);

builder.AddServiceDefaults();

var configuration = builder.Configuration;

builder.AddKestrelExtensions(configuration);

builder.Services.AddHttpDiagnosticsExtensions(configuration);
builder.Services.AddCustomMetricsSimulation(configuration);

Console.WriteLine(builder.Environment.ApplicationName);
Console.WriteLine(builder.Environment.ContentRootPath);
Console.WriteLine(builder.Environment.WebRootPath);
Console.WriteLine(builder.Environment.EnvironmentName);

builder.Services.AddRouteExtension();
builder.Services.AddElasticClientExtension(configuration);
builder.Services.AddRedisExtension(configuration);
builder.Services.AddControllers();
builder.Services.AddScoped<ResponseCustomMiddleware>();
// Enabling endpoint discovery allows ASP.NET Core to automatically identify Minimal API endpoints and add them to the API explorer.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.CustomSchemaIds(type => type.FullName);
    c.SwaggerDoc("v1", new() { Title = "ResponseBodyFeature", Version = "v1" });
});

builder.Services.AddRefitClientExtension(configuration);

builder.Services.AddGraphQL(configuration, builder.Environment);

builder.Services.AddDIExtension();
// The path must be absolute
builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider("/"));
builder.Services.AddSingleton<IFileSystem, FileSystem>();

builder.Services.AddAgentServices(configuration);
await builder.Services.AddKernelServiceServices(configuration);
builder.Services.AddKernelMemoryServices(configuration);

builder.Services.AddMQTTExtension(configuration);

builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(_ =>
{
    var queueCapacity = 100;
    return new DefaultBackgroundTaskQueue(queueCapacity);
});
builder.Services.AddCompanyReportServices(configuration);

var app = builder.Build();

app.UseCustomRouteRewriter();

app.UseKestrelExtensions();

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
app.MapDefaultEndpoints();
// app.UseMiddleware<ResponseCustomMiddleware>();

app.RoutePatternFactoryExtension();

await app.InitAIData();

app.MapMcp();
app.MapA2AExtensions();

// this will win
// app.Run("http://localhost:5004/");
app.Run();
