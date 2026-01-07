using SemanticKernelStack;
using SemanticKernelStack.Api;
using SemanticKernelStack.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

await builder.Services.AddKernelServiceServices(configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();

await app.InitAIData();

app.MapMcp();
app.MapA2AExtensions();

app.UseA2AEndpoints();
app.UseAgentEndpoints();
app.UseDeclarativeEndpoints();
app.UseEmbeddingEndpoints();
app.UsePluginsEndpoints();
app.UseProcessEndpoints();
app.UsePromptEndpoints();
app.UseTokenCounterEndpoints();
app.UseMcpEndpoints();

app.Run();
