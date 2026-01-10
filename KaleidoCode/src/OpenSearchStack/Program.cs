using OpenSearchStack;
using OpenSearchStack.Api;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.AddServiceDefaults();

builder.Services.AddElasticClientExtension(configuration);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Servers = [new OpenApiServer { Url = "/" }];

        return Task.CompletedTask;
    });
});

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

app.UseOpenSearchSearchEndpoints();
app.UseOpenSearchAggregationEndpoints();
app.UseOpenSearchIndexEndpoints();
app.UseOpenSearchAnalyzerEndpoints();
app.UseOpenSearchPipelineEndpoints();

app.Run();
