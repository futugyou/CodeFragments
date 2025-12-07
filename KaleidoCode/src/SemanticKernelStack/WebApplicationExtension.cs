
using A2A;
using A2A.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.A2A;
using SemanticKernelStack.Skills;

namespace SemanticKernelStack;


public static class WebApplicationExtension
{
    public static WebApplication MapA2AExtensions(this WebApplication app)
    {
        var kernel = app.Services.GetRequiredService<Kernel>();
        A2AHostAgent shopHostAgent = CreateChatCompletionHostAgent(
                    kernel,
                    "ShopAgent",
                    """
                    You specialize in handling requests related to store inventory management.
                    """);
        var taskManager = shopHostAgent.TaskManager!;

        app.MapA2A(taskManager, "/a2a/shop");
        // this need `A2A.AspNetCore` 0.3.1-preview
        // app.MapWellKnownAgentCard(taskManager, "/a2a/shop");
        app.MapHttpA2A(taskManager, "/a2a/shop");
        return app;
    }

    static A2AHostAgent CreateChatCompletionHostAgent(Kernel kernel, string name, string instructions)
    {
        kernel = kernel.Clone();
        kernel.Plugins.Clear();
        kernel.ImportPluginFromType<StoreSystemPlugin>("StoreSystemPlugin");

        var agent = new ChatCompletionAgent()
        {
            Kernel = kernel,
            Name = name,
            Instructions = instructions,
            Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
        };
        var agentCard = GetShopAgentCard();
        return new A2AHostAgent(agent, agentCard);
    }

    static AgentCard GetShopAgentCard()
    {
        var capabilities = new AgentCapabilities()
        {
            Streaming = false,
            PushNotifications = false,
        };

        var invoiceQuery = new AgentSkill()
        {
            Id = "id_shop_agent",
            Name = "ShopAgent",
            Description = "Handles requests related to store purchases, sales, and inventory.",
            Tags = ["Fruit", "semantic-kernel"],
            Examples = ["Query fruit by name (Name),"],
        };

        return new()
        {
            Name = "ShopAgent",
            Description = "Handles requests related to store purchases, sales, and inventory.",
            Version = "1.0.0",
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [invoiceQuery],
        };
    }
}