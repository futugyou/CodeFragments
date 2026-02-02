
using AgentStack.Skills;
using AgentStack.Services;
using AgentStack.ChatHistory;
using Microsoft.AspNetCore.Mvc;

namespace AgentStack.Api;

public static class AgentEndpoints
{
    public static void UseAgentEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/agent")
                .WithName("maf agent");

        agentGroup.MapPost("/joker", Joker).WithName("joker");
        agentGroup.MapPost("/joker-stream", JokerStream).WithName("joker-stream");
        agentGroup.MapPost("/joker-message", JokerMessage).WithName("joker-message");
        agentGroup.MapPost("/session", Session).WithName("session");
        agentGroup.MapPost("/function", Function).WithName("function");
        agentGroup.MapPost("/approval", Approval).WithName("approval");
        agentGroup.MapPost("/approval2", Approval2).WithName("approval2");
        agentGroup.MapPost("/structured", Structured).WithName("structured");
        agentGroup.MapPost("/agent-tool", AgentToTool).WithName("agent-tool");
        agentGroup.MapPost("/history-storage", HistoryStorage).WithName("history-storage");
        agentGroup.MapPost("/history-memory", HistoryMemory).WithName("history-memory");
        agentGroup.MapPost("/rag", RAG).WithName("rag");
        agentGroup.MapPost("/chat-reducer", ChatReducer).WithName("chat-reducer");
        agentGroup.MapPost("/declarative", Declarative).WithName("declarative");
    }

    static async Task<string> Joker([FromServices] AgentService agentService, [FromHeader] string UserId, [FromHeader] string SessionId, string message = "Tell me a joke about a pirate.")
    {
        return await agentService.Joker(message);
    }

    static IAsyncEnumerable<string> JokerStream([FromServices] AgentService agentService, string message = "Tell me a joke about a pirate.")
    {
        return agentService.JokerStream(message);
    }

    static async Task<string> JokerMessage([FromServices] AgentService agentService,
        string message = "Tell me a joke about this image?",
        string imageUrl = "https://upload.wikimedia.org/wikipedia/commons/1/11/Joseph_Grimaldi.jpg",
        bool hasSystemMessage = false)
    {
        return await agentService.JokerMessage(message, imageUrl, hasSystemMessage);
    }

    static IAsyncEnumerable<string> Session([FromServices] AgentService agentService)
    {
        return agentService.Session();
    }

    static IAsyncEnumerable<string> Function([FromServices] AgentService agentService, string message = "Can you tell me the status of all the lights?")
    {
        return agentService.Function(message);
    }

    static IAsyncEnumerable<string> Approval([FromServices] AgentService agentService, string message = "Could you please turn off all the lights?", bool allowChangeState = false)
    {
        return agentService.Approval(message, allowChangeState);
    }

    static IAsyncEnumerable<string> Approval2([FromServices] AgentService agentService, string? conversation = null, bool allowChangeState = false)
    {
        return agentService.Approval2(conversation, allowChangeState);
    }

    /// <summary>
    /// This example will not run successfully, regardless of whether an object or a list is used in CreateJsonSchema. 
    /// The reason is that the stupid OpenAI requires JsonSchema to be an object, meaning GetLightsAsync cannot directly return a list; 
    /// it must be contained within an object.
    /// 
    /// The current code prints only one object in WriteLine. 
    /// If it's changed to CreateJsonSchema(typeof(LightModel[])) or CreateJsonSchema(typeof(List<LightModel>)), 
    /// an error will occur during RunAsync, indicating that it must be 'object'.
    /// </summary>
    /// <returns></returns> 
    static async Task<List<LightModel>> Structured([FromServices] AgentService agentService)
    {
        return await agentService.Structured();
    }

    static IAsyncEnumerable<string> AgentToTool([FromServices] AgentService agentService, string message = "Can you tell me the status of all the lights?")
    {
        return agentService.AgentToTool(message);
    }

    static IAsyncEnumerable<string> HistoryStorage([FromServices] AgentService agentService)
    {
        return agentService.HistoryStorage();
    }

    static IAsyncEnumerable<string> HistoryMemory([FromServices] AgentService agentService, string useid)
    {
        return agentService.HistoryMemory(useid);
    }

    static IAsyncEnumerable<string> RAG([FromServices] AgentService agentService)
    {
        return agentService.RAG();
    }

    static IAsyncEnumerable<string> ChatReducer([FromServices] AgentService agentService, ChatReducerType reducerType, ChatReducerTriggerEvent triggerEvent)
    {
        return agentService.ChatReducer(reducerType, triggerEvent);
    }

    static IAsyncEnumerable<string> Declarative([FromServices] AgentService agentService)
    {
        return agentService.Declarative();
    }
}