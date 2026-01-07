
using SemanticKernelStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace SemanticKernelStack.Api;

public static class DeclarativeEndpoints
{
    public static void UseDeclarativeEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/declarative")
                .WithName("sk declarative");

        agentGroup.MapPost("/chat", Chat).WithName("chat");
        agentGroup.MapPost("/function", Function).WithName("function");
        agentGroup.MapPost("/template", Template).WithName("template");
    }

    static IAsyncEnumerable<string> Chat([FromServices] DeclarativeService service, string query = "Cats and Dogs")
    {
        return service.Chat(query);
    }

    static IAsyncEnumerable<string> Function([FromServices] DeclarativeService service, string query = "Can you tell me the status of all the lights?")
    {
        return service.Function(query);
    }

    static IAsyncEnumerable<string> Template([FromServices] DeclarativeService service, string topic = "Dogs", string length = "3")
    {
        return service.Template(topic, length);
    }
}
