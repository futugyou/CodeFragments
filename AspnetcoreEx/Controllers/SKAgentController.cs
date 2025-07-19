
using AspnetcoreEx.KernelService;
using AspnetcoreEx.KernelService.Skills;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
namespace AspnetcoreEx.Controllers;

[Experimental("SKEXP0011")]
[Route("api/sk/agent")]
[ApiController]
public class SKAgentController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly SemanticKernelOptions _options;

    public SKAgentController(Kernel kernel, IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        _kernel = kernel;
        _options = optionsMonitor.CurrentValue;
    }

    [Route("joker")]
    [HttpPost]
    /// <summary>
    /// base sk agent demo
    /// </summary>
    /// <returns></returns>
    public async IAsyncEnumerable<string> Joker()
    {
        ChatCompletionAgent agent =
            new()
            {
                Name = "Joker",
                Instructions = "You are good at telling jokes.",
                Kernel = _kernel,
            };

        AgentThread? thread = new ChatHistoryAgentThread(
            [
                new ChatMessageContent(AuthorRole.User, "Tell me a joke about a pirate."),
                new ChatMessageContent(AuthorRole.Assistant, "Why did the pirate go to school? Because he wanted to improve his \"arrrrrrrrrticulation\""),
            ]);

        // Respond to user input
        await foreach (var message in InvokeAgentAsync("Now add some emojis to the joke."))
        {
            yield return message;
        }
        await foreach (var message in InvokeAgentAsync("Now make the joke sillier."))
        {
            yield return message;
        }

        // Local function to invoke agent and display the conversation messages.
        async IAsyncEnumerable<string> InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            yield return $"Role: {message.Role}, Content: {message.Content}";
            await foreach (AgentResponseItem<ChatMessageContent> response in agent.InvokeAsync(message, thread))
            {
                yield return $"Role: {response.Message.Role}, Content: {response.Message.Content}";
            }
        }
    }

    [Route("lights")]
    [HttpPost]
    /// <summary>
    /// sk agent demo with plugin
    /// </summary>
    /// <returns></returns>
    public async IAsyncEnumerable<string> Lights()
    {
        ChatCompletionAgent agent =
            new()
            {
                Name = "lights",
                Instructions = "You are good at check the light status and control the light switch.",
                Kernel = _kernel,
                Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
            };
        // System.ArgumentException: An item with the same key has already been added. Key: Lights
        // It has been registered in the kernel during setup, so it cannot be registered again here.
        // agent.Kernel.Plugins.AddFromType<LightPlugin>("Lights");

        ChatHistoryAgentThread thread = new();

        await foreach (var message in InvokeAgentAsync("Can you tell me the status of all the lights?"))
        {
            yield return message;
        }

        // Local function to invoke agent and display the conversation messages.
        async IAsyncEnumerable<string> InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            yield return $"Role: {message.Role}, Content: {message.Content}";
            await foreach (AgentResponseItem<ChatMessageContent> response in agent.InvokeAsync(message, thread))
            {
                yield return $"Role: {response.Message.Role}, Content: {response.Message.Content}";
            }
        }
    }

}
