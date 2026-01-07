
using SemanticKernelStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace SemanticKernelStack.Api;

public static class A2AEndpoints
{
    public static void UseA2AEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/a2a")
                .WithName("sk a2a");

        agentGroup.MapPost("/concurrent", Concurrent).WithName("concurrent");
    }

    static IAsyncEnumerable<string> Concurrent([FromServices] A2AService service, string text = "Give the fruit with a unit price greater than 10 yuan!")
    {
        return service.Concurrent(text);
    }

}
