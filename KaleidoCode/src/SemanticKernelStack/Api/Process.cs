
using SemanticKernelStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace SemanticKernelStack.Api;

public static class ProcessEndpoints
{
    public static void UseProcessEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/process")
                .WithName("sk process");

        agentGroup.MapPost("/sample", Sample).WithName("sample");
    }

    static IAsyncEnumerable<string> Sample([FromServices] ProcessService service)
    {
        return service.Sample();
    }

}
