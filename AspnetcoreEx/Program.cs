using AspnetcoreEx;
using AspnetcoreEx.Extensions;
using AspnetcoreEx.RedisExtensions;
using Polly.Timeout;
using Polly;
using Polly.Extensions.Http;
using Refit;
using AspnetcoreEx.GraphQL;
using AspnetcoreEx.HealthCheckExtensions;
using HealthChecks.UI.Client;
using AspnetcoreEx.Elasticsearch;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Configuration.AddJsonFileExtensions("appsettings.json", true, true);

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
var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ResponseBodyFeature v1"));
}

//app.UseHttpsRedirection();

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
//app.UseMiddleware<ResponseCustomMiddleware>();

app.Run();
