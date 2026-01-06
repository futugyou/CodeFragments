using OpenSearchStack;
using OpenSearchStack.Api;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.AddServiceDefaults();

builder.Services.AddElasticClientExtension(configuration);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
