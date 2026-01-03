
using AgentStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgentStack.Api;

public static class AgentEndpoints
{
    public static void UseAgentEndpoints(this IEndpointRouteBuilder app)
    {
        var vehicleGroup = app.MapGroup("/api/af/agent")
                .WithName("maf agent");

        vehicleGroup.MapPost("/joker", Joker).WithName("joker");
    }

    static async Task<string> Joker([FromServices] AgentService agentService, string message = "Tell me a joke about a pirate.")
    {
        return await agentService.Joker(message);
    }
}