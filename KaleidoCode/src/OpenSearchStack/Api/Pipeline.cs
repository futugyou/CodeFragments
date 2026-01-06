
using Microsoft.AspNetCore.Mvc;

namespace OpenSearchStack.Api;

public static class OpenSearchPipelineEndpoints
{
    public static void UseOpenSearchPipelineEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/pipeline")
                .WithName("Elastic");
        agentGroup.MapPost("/create", Create).WithName("create");
        agentGroup.MapPost("/insert", Insert).WithName("insert");
    }

    static Task<bool> Create([FromServices] PipelineService esService)
    {
        return esService.CreatePipeline();
    }

    static Task<IndexResponse> Insert([FromServices] PipelineService esService)
    {
        return esService.InsertDataWithPipline();
    }

}