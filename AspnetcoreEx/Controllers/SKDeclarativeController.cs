
using AspnetcoreEx.KernelService;
using Microsoft.SemanticKernel;
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
    public async IAsyncEnumerable<string> Chat(string query="Cats and Dogs")
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
}
