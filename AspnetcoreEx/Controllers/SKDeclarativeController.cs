
using AspnetcoreEx.KernelService;
using Microsoft.SemanticKernel;
using AspnetcoreEx.KernelService.Skills;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AspnetcoreEx.Controllers;

[Experimental("SKEXP0011")]
[Route("api/sk/declarative")]
[ApiController]
public class SKDeclarativeController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly SemanticKernelOptions _options;

    public SKDeclarativeController(Kernel kernel, IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        _kernel = kernel;
        _options = optionsMonitor.CurrentValue;
    }

    [Route("chat")]
    [HttpPost]
    public async IAsyncEnumerable<string> Chat(string query = "Cats and Dogs")
    {
        var text =
             """
            type: chat_completion_agent
            name: StoryAgent
            description: Story Telling Agent
            instructions: Tell a story suitable for children about the topic provided by the user.
            """;
        var agentFactory = new ChatCompletionAgentFactory();

        var agent = await agentFactory.CreateAgentFromYamlAsync(text, new() { Kernel = _kernel });

        await foreach (ChatMessageContent response in agent!.InvokeAsync(query))
        {
            yield return $"Role: {response.Role}, Content: {response.Content}";
        }
    }

    [Route("func")]
    [HttpPost]
    public async IAsyncEnumerable<string> Function(string query = "Can you tell me the status of all the lights?")
    {
        _kernel.Plugins.Clear();
        _kernel.Plugins.AddFromType<LightPlugin>("Lights");
        var text =
            """
            type: chat_completion_agent
            name: FunctionCallingAgent
            instructions: Use the provided functions to answer questions about the lights.
            description: This agent uses the provided functions to answer questions about the lights.
            model:
              options:
                temperature: 0.4
            tools:
              - id: Lights.get_lights
                type: function
              - id: Lights.change_state
                type: function
            """;
        var agentFactory = new ChatCompletionAgentFactory();

        var agent = await agentFactory.CreateAgentFromYamlAsync(text, new() { Kernel = _kernel });

        await foreach (ChatMessageContent response in agent!.InvokeAsync(new ChatMessageContent(AuthorRole.User, query)))
        {
            yield return $"Role: {response.Role}, Content: {response.Content}";
        }
    }

    [Route("template")]
    [HttpPost]
    public async IAsyncEnumerable<string> Template(string topic = "Dogs", string length = "3")
    {
        var text =
            """
            type: chat_completion_agent
            name: StoryAgent
            description: A agent that generates a story about a topic.
            instructions: Tell a story about {{$topic}} that is {{$length}} sentences long.
            inputs:
                topic:
                    description: The topic of the story.
                    required: true
                    default: Cats
                length:
                    description: The number of sentences in the story.
                    required: true
                    default: 2
            outputs:
                output1:
                    description: output1 description
            template:
                format: semantic-kernel
            """;
        var agentFactory = new ChatCompletionAgentFactory();
        var promptTemplateFactory = new KernelPromptTemplateFactory();

        var agent = await agentFactory.CreateAgentFromYamlAsync(text, new() { Kernel = _kernel, PromptTemplateFactory = promptTemplateFactory });
        var options = new AgentInvokeOptions()
        {
            KernelArguments = new()
            {
                { "topic", topic },
                { "length", length },
            }
        };

        await foreach (ChatMessageContent response in agent!.InvokeAsync(Array.Empty<ChatMessageContent>(), options: options))
        {
            yield return $"Role: {response.Role}, Content: {response.Content}";
        }
    }
}
