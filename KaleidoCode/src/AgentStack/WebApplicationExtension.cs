
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;

namespace AgentStack;


public static class WebApplicationExtension
{
    public static WebApplication MapAguiExtensions(this WebApplication app, IWebHostEnvironment environment)
    {
        app.MapOpenAIResponses();
        app.MapOpenAIConversations();

        if (environment.IsDevelopment())
        {
            app.MapDevUI();
        }

        // app.MapAGUI("/joker", sp =>
        // {
        //     var chatClient = sp.GetRequiredKeyedService<IChatClient>("AgentChatClient");
        //     return chatClient.AsAIAgent(instructions: "You are good at telling jokes.");
        // });

        // // it also works
        // app.MapAGUI("/light", sp =>
        // {
        //     return sp.GetRequiredKeyedService<AIAgent>("light");
        // });

        app.MapAGUI();
        app.MapA2A();

        return app;
    }

    // public static IEndpointConventionBuilder MapAGUI(
    //     this IEndpointRouteBuilder endpoints,
    //     [StringSyntax("route")] string pattern,
    //     Func<IServiceProvider, AIAgent> factory)
    // {
    //     return endpoints.MapAGUI(pattern, factory);
    // }

    public static IEndpointRouteBuilder MapA2A(this IEndpointRouteBuilder endpoints)
    {
        var registeredAIAgents = GetRegisteredEntities<AIAgent>(endpoints.ServiceProvider);
        var registeredWorkflows = GetRegisteredEntities<Workflow>(endpoints.ServiceProvider);

        foreach (var agent in registeredAIAgents)
        {
            if (agent.Name != null)
            {
                endpoints.MapA2AHttpJson(agent, $"/a2a/{agent.Name}" );
            }
        }

        foreach (var workflow in registeredWorkflows)
        {
            if (workflow.Name != null)
            {
                endpoints.MapA2AHttpJson(workflow.AsAIAgent(name: workflow.Name), $"/a2a/{workflow.Name}" );
            }
        }

        return endpoints;
    }

    public static IEndpointRouteBuilder MapAGUI(this IEndpointRouteBuilder endpoints)
    {
        var registeredAIAgents = GetRegisteredEntities<AIAgent>(endpoints.ServiceProvider);
        var registeredWorkflows = GetRegisteredEntities<Workflow>(endpoints.ServiceProvider);

        foreach (var agent in registeredAIAgents)
        {
            if (agent.Name != null)
            {
                endpoints.MapAGUIServer(agent.Name, agent);
            }
        }

        foreach (var workflow in registeredWorkflows)
        {
            if (workflow.Name != null)
            {
                endpoints.MapAGUIServer(workflow.Name!, workflow.AsAIAgent());
            }
        }

        return endpoints;
    }

    private static IEnumerable<T> GetRegisteredEntities<T>(IServiceProvider serviceProvider)
    {
        var keyedEntities = serviceProvider.GetKeyedServices<T>(KeyedService.AnyKey);
        var defaultEntities = serviceProvider.GetServices<T>() ?? [];

        return keyedEntities
            .Concat(defaultEntities)
            .Where(entity => entity is not null);
    }
}