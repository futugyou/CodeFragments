
using Microsoft.AspNetCore.Mvc;

namespace OpenSearchStack.Api;

public static class OpenSearchBaseEndpoints
{
    public static void UseOpenSearchBaseEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/es")
                .WithName("Elastic"); 

        agentGroup.MapPost("/analyzer", Analyzer).WithName("analyzer");
        agentGroup.MapPost("/analyzer_testing", AnalyzerTesting).WithName("analyzer_testing");

        agentGroup.MapPost("/insert", Insert).WithName("insert");
        agentGroup.MapPost("/insert_many", InsertMany).WithName("insert_many");
        agentGroup.MapPost("/get_all", GetAll).WithName("get_all");
        agentGroup.MapPost("/get_page", GetPage).WithName("get_page");
        agentGroup.MapPost("/scroll_get", ScrollGet).WithName("scroll_get");
        agentGroup.MapPost("/search", Search).WithName("search");
        agentGroup.MapPost("/pipeline", Pipeline).WithName("pipeline");
    }
 
    static Task<CreateIndexResponse> Analyzer([FromServices] AnalyzerService esService, string type = "base")
    {
        return type == "base" ? esService.CreateBaseAnalyzer() : esService.CreateCustomAnalyzer();
    }

    static IAsyncEnumerable<string> AnalyzerTesting([FromServices] TestAnalyzerService esService)
    {
        return esService.TestAnalyzer();
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
 
}