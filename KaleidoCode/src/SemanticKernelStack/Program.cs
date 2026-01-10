using SemanticKernelStack;
using SemanticKernelStack.Api;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.AddServiceDefaults();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Servers = [new OpenApiServer { Url = "/" }];

        return Task.CompletedTask;
    });
});

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
