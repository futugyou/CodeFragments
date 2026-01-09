
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

        app.MapAGUI("/joker", sp =>
        {
            var chatClient = sp.GetRequiredKeyedService<IChatClient>("AgentChatClient");
            return chatClient.CreateAIAgent(instructions: "You are good at telling jokes.");
        });

        return app;
    }

    public static IEndpointConventionBuilder MapAGUI(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("route")] string pattern,
        Func<IServiceProvider, AIAgent> factory)
    {
        var agent = factory(endpoints.ServiceProvider);
        return endpoints.MapAGUI(pattern, agent); ;
    }
}