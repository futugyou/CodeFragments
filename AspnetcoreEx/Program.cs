using Microsoft.OpenApi.Models;
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
using GraphQL.Server.Ui.Voyager;
using HotChocolate.Types.Pagination;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Host.ConfigureAppConfiguration(config =>
{
    config.AddJsonFileExtensions("appsettings.json", true, true);
});

builder.Services.AddRedisExtension(configuration);
builder.Services.AddScoped<IUserRepository, UserRepository>();//
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

var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(30000));

builder.Services.AddRefitClient<IGitHubApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.github.com"))
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(timeoutPolicy);

builder.Services
    .AddGraphQLServer()
    .AddFiltering()
    .AddProjections() // AddProjections can get include data like ef.
    .AddSorting()
    .SetPagingOptions(new PagingOptions
    {
        MaxPageSize = 50,
        IncludeTotalCount = true
    })
    .AddQueryType<Query>()
    .AddType<UserConfigure>();

builder.Services.AddHealthChecksUI().AddInMemoryStorage();
builder.Services.AddHealthChecks().AddCheck<DemoHealthCheck>("demo-health");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ResponseBodyFeature v1"));
}

//app.UseHttpsRedirection();

app.UseAuthorization();
// app.UseRouting().UseEndpoints(endpoints =>
// {
//     endpoints.MapGraphQL();
// });
app.MapGraphQL();
app.UseGraphQLVoyager(new VoyagerOptions { GraphQLEndPoint = "/graphql" }, "/graphql-voyager");

app.MapControllers();
app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});
app.MapHealthChecksUI(options => options.UIPath = "/health-ui");
//app.UseMiddleware<ResponseCustomMiddleware>();
app.Run();
