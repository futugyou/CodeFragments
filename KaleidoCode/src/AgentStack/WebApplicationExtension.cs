
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.Agents.AI.DevUI;

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
        //     return chatClient.CreateAIAgent(instructions: "You are good at telling jokes.");
        // });

        // // it also works
        // app.MapAGUI("/light", sp =>
        // {
        //     return sp.GetRequiredKeyedService<AIAgent>("light");
        // });

        app.MapAGUI();

        return app;
    }

    public static IEndpointConventionBuilder MapAGUI(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("route")] string pattern,
        Func<IServiceProvider, AIAgent> factory)
    {
        var agent = factory(endpoints.ServiceProvider);
        return endpoints.MapAGUI(pattern, agent);
    }

    public static IEndpointRouteBuilder MapAGUI(this IEndpointRouteBuilder endpoints)
    {
        var registeredAIAgents = GetRegisteredEntities<AIAgent>(endpoints.ServiceProvider);
        var registeredWorkflows = GetRegisteredEntities<Workflow>(endpoints.ServiceProvider);

        foreach (var agent in registeredAIAgents)
        {
            if (agent.Name != null)
            {
                endpoints.MapAGUI(agent.Name, agent);
            }
        }

        foreach (var workflow in registeredWorkflows)
        {
            if (workflow.Name != null)
            {
                endpoints.MapAGUI(workflow.Name!, workflow.AsAgent());
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