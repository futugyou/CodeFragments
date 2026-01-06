
using Microsoft.AspNetCore.Mvc;

namespace OpenSearchStack.Api;

public static class OpenSearchEndpoints
{
    public static void UseOpenSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/es")
                .WithName("Elastic");

        agentGroup.MapPost("/terms", Terms).WithName("terms");
        agentGroup.MapPost("/averageMax", AverageMax).WithName("averageMax");

        agentGroup.MapPost("/analyzer", Analyzer).WithName("analyzer");
        agentGroup.MapPost("/analyzer_testing", AnalyzerTesting).WithName("analyzer_testing");

        agentGroup.MapPost("/mapping", Mapping).WithName("mapping");
        agentGroup.MapPost("/insert", Insert).WithName("insert");
        agentGroup.MapPost("/insert_many", InsertMany).WithName("insert_many");
        agentGroup.MapPost("/get_all", GetAll).WithName("get_all");
        agentGroup.MapPost("/get_page", GetPage).WithName("get_page");
        agentGroup.MapPost("/scroll_get", ScrollGet).WithName("scroll_get");
        agentGroup.MapPost("/search", Search).WithName("search");
        agentGroup.MapPost("/pipeline", Pipeline).WithName("pipeline");
        agentGroup.MapPost("/reindex", Reindex).WithName("reindex");
    }

    static Task<AggregateDictionary> Terms([FromServices] AggregationSerice esService)
    {
        return esService.Terms();
    }

    static Task<AggregateDictionary> AverageMax([FromServices] AggregationSerice esService)
    {
        return esService.AverageMax();
    }

    static Task<CreateIndexResponse> Analyzer([FromServices] AnalyzerService esService, string type = "base")
    {
        return type == "base" ? esService.CreateBaseAnalyzer() : esService.CreateCustomAnalyzer();
    }

    static IAsyncEnumerable<string> AnalyzerTesting([FromServices] TestAnalyzerService esService)
    {
        return esService.TestAnalyzer();
    }

    static Task Mapping([FromServices] BaseElasticService esService)
    {
        return esService.Mapping();
    }

    static async Task<string> Insert([FromServices] BaseElasticService esService)
    {
        var response = await esService.Insert();

        return response.DebugInformation;
    }

    static IAsyncEnumerable<string> InsertMany([FromServices] BaseElasticService esService)
    {
        return esService.InsertMany();
    }

    static Task GetAll([FromServices] BaseElasticService esService)
    {
        return esService.GetAll();
    }

    static Task GetPage([FromServices] BaseElasticService esService)
    {
        return esService.GetPage();
    }

    static Task Search([FromServices] BaseElasticService esService)
    {
        return esService.Search();
    }

    static Task ScrollGet([FromServices] BaseElasticService esService)
    {
        return esService.ScrollGet();
    }

    static Task Pipeline([FromServices] BaseElasticService esService)
    {
        return esService.Pipeline();
    }

    static Task Reindex([FromServices] BaseElasticService esService)
    {
        return esService.Reindex();
    }
}