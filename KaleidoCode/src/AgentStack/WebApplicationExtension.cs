
using Microsoft.AspNetCore.Builder;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;

namespace AgentStack;


public static class WebApplicationExtension
{
    public static WebApplication MapAguiExtensions(this WebApplication app)
    {
        var _options = app.Services.GetRequiredService<IOptionsMonitor<AgentOptions>>()!.CurrentValue;

        var credential = new ApiKeyCredential(_options.TextCompletion.ApiKey);
        OpenAIClientOptions openAIOptions = new();
        if (!string.IsNullOrEmpty(_options.TextCompletion.Endpoint))
        {
            openAIOptions.Endpoint = new Uri(_options.TextCompletion.Endpoint);
        }

        var ghModelsClient = new OpenAIClient(credential, openAIOptions);

        var _chatClient = ghModelsClient.GetChatClient(_options.TextCompletion.ModelId).AsIChatClient();

        var chatClient = _chatClient
        .AsBuilder()
        .Build();

        var agent = chatClient.CreateAIAgent(instructions: "You are good at telling jokes.");
        app.MapAGUI("/agui", agent);

        return app;
    }
}