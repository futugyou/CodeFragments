
using SemanticKernelStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace SemanticKernelStack.Api;

public static class AgentEndpoints
{
    public static void UseAgentEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/agent")
                .WithName("sk agent");

        agentGroup.MapPost("/concurrent", Concurrent).WithName("concurrent");
        agentGroup.MapPost("/joker", Joker).WithName("joker");
        agentGroup.MapPost("/lights", Lights).WithName("lights");
        agentGroup.MapPost("/chat", Chat).WithName("chat");
        agentGroup.MapPost("/di", DI).WithName("di");
        agentGroup.MapPost("/function", Function).WithName("function");
        agentGroup.MapPost("/sequential", Sequential).WithName("sequential");
        agentGroup.MapPost("/group_chat", GroupChat).WithName("group_chat");
        agentGroup.MapPost("/handoff", Handoff).WithName("handoff");
    }
    static IAsyncEnumerable<string> Joker([FromServices] AgentService service)
    {
        return service.Joker();
    }

    static IAsyncEnumerable<string> Lights([FromServices] AgentService service)
    {
        return service.Lights();
    }

    static IAsyncEnumerable<string> Chat([FromServices] AgentService service, int maximumIterations = 4)
    {
        return service.Chat(maximumIterations);
    }

    static IAsyncEnumerable<string> DI([FromServices] AgentService service, int maximumIterations = 4)
    {
        return service.DI(maximumIterations);
    }

    static IAsyncEnumerable<string> Function([FromServices] AgentService service)
    {
        return service.Function();
    }

    static IAsyncEnumerable<string> Concurrent([FromServices] AgentService service, string query = "What is temperature?")
    {
        return service.Concurrent(query);
    }

    static IAsyncEnumerable<string> Sequential([FromServices] AgentService service, string query = "An eco-friendly stainless steel water bottle that keeps drinks cold for 24 hours")
    {
        return service.Sequential(query);
    }

    static IAsyncEnumerable<string> GroupChat([FromServices] AgentService service, string query = "Create a slogan for a new electric SUV that is affordable and fun to drive.")
    {
        return service.GroupChat(query);
    }

    static IAsyncEnumerable<string> Handoff([FromServices] AgentService service, string query = "I am a customer that needs help with my orders")
    {
        return service.Handoff(query);
    }
}
