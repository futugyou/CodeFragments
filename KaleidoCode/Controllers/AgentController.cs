
using KaleidoCode.KernelService;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using KaleidoCode.KernelService.Skills;
using Microsoft.Agents.AI;
using System.Reflection;

namespace KaleidoCode.Controllers;

[Experimental("SKEXP0011")]
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

    [Route("function")]
    [HttpPost]
    public async IAsyncEnumerable<string> Function()
    {
        var lightPlugin = new LightPlugin();
        AITool[] tools =
        [
            .. typeof(LightPlugin)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Select((m) => AIFunctionFactory.Create(m, target: null)), // Get from type static methods
            .. lightPlugin.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Select((m) => AIFunctionFactory.Create(m, target: lightPlugin)) // Get from instance methods
        ];

        var message = "Can you tell me the status of all the lights?";
        AIAgent agent = _chatClient.CreateAIAgent(instructions: "You are a useful assistant.", tools: tools);
        AgentThread thread = agent.GetNewThread();
        var response = await agent.RunAsync(message, thread);
        yield return response.Text;
    }

    [Route("approval")]
    [HttpPost]
    public async IAsyncEnumerable<string> Approval(bool allowChangeState = false)
    {
        var lightPlugin = new LightPlugin();
        AITool[] tools = [
            AIFunctionFactory.Create(lightPlugin.GetLightsAsync),
            new ApprovalRequiredAIFunction(AIFunctionFactory.Create(lightPlugin.ChangeStateAsync))
        ];

        var message = "Could you please turn off all the lights?";
        AIAgent agent = _chatClient.CreateAIAgent(instructions: "You are a useful assistant.", tools: tools);
        AgentThread thread = agent.GetNewThread();
        var response = await agent.RunAsync(message, thread);
        yield return response.Text;

        var functionApprovalRequests = response.Messages
            .SelectMany(x => x.Contents)
            .OfType<FunctionApprovalRequestContent>()
            .ToList();

        if (functionApprovalRequests.Count == 0)
        {
            yield break;
        }

        FunctionApprovalRequestContent requestContent = functionApprovalRequests.First();
        var approvalMessage = new ChatMessage(ChatRole.User, [requestContent.CreateResponse(allowChangeState)]);
        response = await agent.RunAsync(approvalMessage, thread);
        yield return response.Text;
    }

    /// <summary>
    /// This example will not run successfully, regardless of whether an object or a list is used in CreateJsonSchema. 
    /// The reason is that the stupid OpenAI requires JsonSchema to be an object, meaning GetLightsAsync cannot directly return a list; 
    /// it must be contained within an object.
    /// 
    /// The current code prints only one object in WriteLine. 
    /// If it's changed to CreateJsonSchema(typeof(LightModel[])) or CreateJsonSchema(typeof(List<LightModel>)), 
    /// an error will occur during RunAsync, indicating that it must be 'object'.
    /// </summary>
    /// <returns></returns>
    [Route("structured")]
    [HttpPost]
    public async Task<List<LightModel>> Structured()
    {
        var lightPlugin = new LightPlugin();
        AITool[] tools = [
            AIFunctionFactory.Create(lightPlugin.GetLightsAsync),
        ];

        JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(LightModel));
        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "LightModel",
                schemaDescription: "Information about a Light list including their id, name, and is_on"),
            Tools = tools
        };

        var message = "Can you tell me the status of all the lights?";
        AIAgent agent = _chatClient.CreateAIAgent(new ChatClientAgentOptions()
        {
            Name = "LightAssistant",
            Instructions = "You are a helpful assistant.",
            ChatOptions = chatOptions
        });
        AgentThread thread = agent.GetNewThread();
        var response = await agent.RunAsync(message, thread);
        Console.WriteLine(response.Text);
        return response.Deserialize<List<LightModel>>(JsonSerializerOptions.Web);
    }

    [Route("agent-tool")]
    [HttpPost]
    public async IAsyncEnumerable<string> AgentToTool()
    {
        var lightPlugin = new LightPlugin();
        AITool[] tools = [
            AIFunctionFactory.Create(lightPlugin.GetLightsAsync),
            AIFunctionFactory.Create(lightPlugin.ChangeStateAsync)
        ];

        var message = "Can you tell me the status of all the lights?";
        AIAgent toolAgent = _chatClient.CreateAIAgent(
            instructions: "You are a useful light assistant.",
            name: "LightAgent",
            description: "An agent is used to answer your questions about the status of the lights and can help you control the lights on and off.",
            tools: tools);

        AIAgent agent = _chatClient.CreateAIAgent(instructions: "You are a helpful assistant who responds in chinese.", tools: [toolAgent.AsAIFunction()]);
        agent = agent.AsBuilder()
        .UseOpenTelemetry(sourceName: "agent-telemetry-source")
        .Build();

        AgentThread thread = agent.GetNewThread();
        var response = await agent.RunAsync(message, thread);
        yield return response.Text;
    }
}