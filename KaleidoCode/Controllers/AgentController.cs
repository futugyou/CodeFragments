
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

    [Route("joker-stream")]
    [HttpPost]
    public async IAsyncEnumerable<string> JokerStream(string message = "Tell me a joke about a pirate.")
    {
        AIAgent agent = _chatClient.CreateAIAgent(instructions: "You are good at telling jokes.");
        await foreach (var update in agent.RunStreamingAsync(message))
        {
            yield return update.Text;
        }
    }
    [Route("joker-message")]
    [HttpPost]
    public async Task<string> JokerMessage(
        string message = "Tell me a joke about this image?",
        string imageUrl = "https://upload.wikimedia.org/wikipedia/commons/1/11/Joseph_Grimaldi.jpg",
        bool hasSystemMessage = false)
    {
        AIAgent agent = _chatClient.CreateAIAgent(instructions: hasSystemMessage ? null : "You are good at telling jokes.");

        List<ChatMessage> chatMessages = [new(ChatRole.User, [
            new Microsoft.Extensions.AI.TextContent(message),
            new UriContent(imageUrl, "image/jpeg")
        ])];

        if (hasSystemMessage)
        {
            chatMessages.Insert(0, new(ChatRole.System, "You are good at telling jokes."));
        }

        var response = await agent.RunAsync(chatMessages);
        return response.Text;
    }

    [Route("thread")]
    [HttpPost]
    public async IAsyncEnumerable<string> Thread()
    {
        var messages = new string[] { "Tell me a joke about a pirate.", "Now add some emojis to the joke and tell it in the voice of a pirate's parrot." };
        AIAgent agent = _chatClient.CreateAIAgent(instructions: "You are good at telling jokes.");
        AgentThread thread = agent.GetNewThread();
        foreach (var message in messages)
        {
            var response = await agent.RunAsync(message, thread);
            yield return response.Text;
        }

        yield return thread.Serialize(JsonSerializerOptions.Web).GetRawText();
    }
}