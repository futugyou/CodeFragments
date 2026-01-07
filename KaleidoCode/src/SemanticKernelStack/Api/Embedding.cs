
using SemanticKernelStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace SemanticKernelStack.Api;

public static class EmbeddingEndpoints
{
    public static void UseEmbeddingEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/embedding")
                .WithName("sk embedding");

        agentGroup.MapPost("/search", EmbeddingSearch).WithName("search");
        agentGroup.MapPost("/create", EmbeddingCreate).WithName("create");
    }

    static IAsyncEnumerable<string> EmbeddingSearch([FromServices] EmbeddingService service, string input)
    {
        return service.EmbeddingSearch(input);
    }

    static IAsyncEnumerable<string> EmbeddingCreate([FromServices] EmbeddingService service)
    {
        return service.EmbeddingCreate();
    }
}
