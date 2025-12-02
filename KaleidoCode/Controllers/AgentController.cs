
using KaleidoCode.KernelService;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using Microsoft.Agents.AI;

namespace KaleidoCode.Controllers;

[Route("api/af/agent")]
[ApiController]
public class AgentController : ControllerBase
{
    private readonly SemanticKernelOptions _options;
    private readonly IChatClient _chatClient;
    public AgentController(IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        _options = optionsMonitor.CurrentValue;
        var credential = new ApiKeyCredential(_options.TextCompletion.ApiKey);
        OpenAIClientOptions openAIOptions = new();
        if (!string.IsNullOrEmpty(_options.TextCompletion.Endpoint))
        {
            openAIOptions.Endpoint = new Uri(_options.TextCompletion.Endpoint);
        }

        var ghModelsClient = new OpenAIClient(credential, openAIOptions);
        _chatClient = ghModelsClient.GetChatClient(_options.TextCompletion.ModelId).AsIChatClient();
    }

    [Route("joker")]
    [HttpPost]
    public async Task<string> Joker(string message = "Tell me a joke about a pirate.")
    {
        AIAgent agent = _chatClient.CreateAIAgent(instructions: "You are good at telling jokes.");

        var response = await agent.RunAsync(message);

        return response.Text;
    }
}