
using OpenSearchStack;
using Microsoft.AspNetCore.Mvc;

namespace OpenSearchStack.Api;

public static class OpenSearchEndpoints
{
    public static void UseOpenSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/es")
                .WithName("Elastic");

        agentGroup.MapPost("/mapping", Mapping).WithName("mapping");
        agentGroup.MapPost("/insert", Insert).WithName("insert");
        agentGroup.MapPost("/insert_many", InsertMany).WithName("insert_many");
        agentGroup.MapPost("/get_all", GetAll).WithName("get_all");
        agentGroup.MapPost("/get_page", GetPage).WithName("get_page");
        agentGroup.MapPost("/scroll_get", ScrollGet).WithName("scroll_get");
        agentGroup.MapPost("/search", Search).WithName("search");
        agentGroup.MapPost("/aggregations", Aggregations).WithName("aggregations");
        agentGroup.MapPost("/pipeline", Pipeline).WithName("pipeline");
        agentGroup.MapPost("/reindex", Reindex).WithName("reindex");
    }

    static void Mapping([FromServices] BaseElasticService esService)
    {
        esService.Mapping();
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

    static void GetAll([FromServices] BaseElasticService esService)
    {
        esService.GetAll();
    }

    static void GetPage([FromServices] BaseElasticService esService)
    {
        esService.GetPage();
    }

    static void Search([FromServices] BaseElasticService esService)
    {
        esService.Search();
    }

    static void ScrollGet([FromServices] BaseElasticService esService)
    {
        esService.ScrollGet();
    }

    static void Aggregations([FromServices] BaseElasticService esService)
    {
        esService.Aggs();
    }

    static void Pipeline([FromServices] BaseElasticService esService)
    {
        esService.Pipeline();
    }

    static void Reindex([FromServices] BaseElasticService esService)
    {
        esService.Reindex();
    }
}