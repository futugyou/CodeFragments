
using AgentStack.Skills;
using AgentStack.ContextProvider;
using AgentStack.Middleware;
using AgentStack.MessageStore;
using AgentStack.Model;
using Microsoft.Extensions.VectorData;

namespace AgentStack.Services;

public class AgentService
{
    private readonly AgentOptions _options;
    private readonly IChatClient _chatClient;
    private readonly VectorStore _vectorStore;
    private readonly AIAgent _jokerAgent;
    private readonly AIAgent _lightAgent;
    private readonly AIAgent _lightApprovalAgent;
    private readonly AIAgent _ragAgent;
    private readonly AIContextProviderFactory _aiContextProviderFactory;
    private static readonly Dictionary<string, string> _threadStore = [];

    public AgentService(
        IOptionsMonitor<AgentOptions> optionsMonitor,
        [FromKeyedServices("AgentVectorStore")] VectorStore vectorStore,
        [FromKeyedServices("joker")] AIAgent jokerAgent,
        [FromKeyedServices("light")] AIAgent lightAgent,
        [FromKeyedServices("light-with-approval")] AIAgent lightApprovalAgent,
        [FromKeyedServices("rag")] AIAgent ragAgent,
        AIContextProviderFactory aiContextProviderFactory
    )
    {
        _aiContextProviderFactory = aiContextProviderFactory;
        _jokerAgent = jokerAgent;
        _lightAgent = lightAgent;
        _lightApprovalAgent = lightApprovalAgent;
        _ragAgent = ragAgent;
        _vectorStore = vectorStore;
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

    public async Task<string> Joker(string message)
    {
        var response = await _jokerAgent.RunAsync(message);
        return response.Text;
    }

    public async IAsyncEnumerable<string> JokerStream(string message)
    {
        await foreach (var update in _jokerAgent.RunStreamingAsync(message))
        {
            yield return update.Text;
        }
    }

    public async Task<string> JokerMessage(
        string message,
        string imageUrl,
        bool hasSystemMessage = false)
    {
        List<ChatMessage> chatMessages = [new(ChatRole.User, [
            new TextContent(message),
            new UriContent(imageUrl, "image/jpeg")
        ])];

        if (hasSystemMessage)
        {
            chatMessages.Insert(0, new(ChatRole.System, "You are good at telling jokes."));
        }

        var response = await _jokerAgent.RunAsync(chatMessages);
        return response.Text;
    }

    public async IAsyncEnumerable<string> Thread()
    {
        var messages = new string[] { "Tell me a joke about a pirate.", "Now add some emojis to the joke and tell it in the voice of a pirate's parrot." };
        AgentSession thread = await _jokerAgent.GetNewSessionAsync();
        foreach (var message in messages)
        {
            var response = await _jokerAgent.RunAsync(message, thread);
            yield return response.Text;
        }

        yield return thread.Serialize(JsonSerializerOptions.Web).GetRawText();
    }

    public async IAsyncEnumerable<string> Function(string message = "Can you tell me the status of all the lights?")
    {
        AgentSession thread = await _lightAgent.GetNewSessionAsync();
        var response = await _lightAgent.RunAsync(message, thread);
        yield return response.Text;
    }

    public async IAsyncEnumerable<string> Approval(string message = "Could you please turn off all the lights?", bool allowChangeState = false)
    {
        AgentSession thread = await _lightApprovalAgent.GetNewSessionAsync();
        var response = await _lightApprovalAgent.RunAsync(message, thread);
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
        response = await _lightApprovalAgent.RunAsync(approvalMessage, thread);
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
            Tools = tools,
            Instructions = "You are good at telling jokes.",
        };

        var message = "Can you tell me the status of all the lights?";
        AIAgent agent = _chatClient.AsAIAgent(new ChatClientAgentOptions()
        {
            Name = "LightAssistant",
            ChatOptions = chatOptions
        });
        AgentSession thread = await agent.GetNewSessionAsync();
        var response = await agent.RunAsync(message, thread);
        Console.WriteLine(response.Text);
        return response.Deserialize<List<LightModel>>(JsonSerializerOptions.Web);
    }

    public async IAsyncEnumerable<string> AgentToTool(string message = "Can you tell me the status of all the lights?")
    {
        AIAgent agent = _chatClient.AsAIAgent(instructions: "You are a helpful assistant who responds in chinese.", tools: [_lightAgent.AsAIFunction()]);
        agent = agent.AsBuilder()
        .UseOpenTelemetry(sourceName: "agent-telemetry-source")
        .Build();

        AgentSession thread = await agent.GetNewSessionAsync();
        var response = await agent.RunAsync(message, thread);
        yield return response.Text;
    }


    public async IAsyncEnumerable<string> HistoryStorage()
    {
        AgentSession thread = await _jokerAgent.GetNewSessionAsync();

        await _jokerAgent.RunAsync("Tell me a joke about a pirate.", thread);

        var messageStore = thread.GetService<VectorChatMessageStore>()!;
        var invokingContext = new ChatHistoryProvider.InvokingContext([new ChatMessage(ChatRole.User, "Tell me a joke about a pirate.")]);
        var history = await messageStore.InvokingAsync(invokingContext);
        foreach (var item in history)
        {
            yield return item.Text;
        }

        yield return "-------------------";

        JsonElement serializedThread = thread.Serialize();
        yield return JsonSerializer.Serialize(serializedThread, new JsonSerializerOptions { WriteIndented = true });
        yield return "-------------------";
        AgentSession resumedThread = await _jokerAgent.DeserializeSessionAsync(serializedThread);

        await _jokerAgent.RunAsync("Now tell the same joke in the voice of a pirate, and add some emojis to the joke.", resumedThread);

        messageStore = resumedThread.GetService<VectorChatMessageStore>()!;
        yield return $"\nThread is stored in vector store under key: {messageStore.ThreadDbKey}";
        invokingContext = new ChatHistoryProvider.InvokingContext([new ChatMessage(ChatRole.User, "Now tell the same joke in the voice of a pirate, and add some emojis to the joke.")]);
        history = await messageStore.InvokingAsync(invokingContext);
        foreach (var item in history)
        {
            yield return item.Text;
        }
    }

    public async IAsyncEnumerable<string> HistoryMemory(string userId)
    {
        AgentSession thread = await _jokerAgent.GetNewSessionAsync();

        var response = await _jokerAgent.RunAsync("I like jokes about Pirates. Tell me a joke about a pirate.", thread);
        yield return response.Text;

        AgentSession thread2 = await _jokerAgent.GetNewSessionAsync();

        response = await _jokerAgent.RunAsync("Tell me a joke that I might like.", thread2);
        yield return response.Text;
    }

    public async IAsyncEnumerable<string> RAG()
    {
        AgentSession thread = await _ragAgent.GetNewSessionAsync();

        var response = await _ragAgent.RunAsync("what is RAG?", thread);
        yield return response.Text;

        AgentSession thread2 = await _ragAgent.GetNewSessionAsync();

        response = await _ragAgent.RunAsync("what is API?", thread2);
        yield return response.Text;
    }

    public async IAsyncEnumerable<string> ChatReducer(ChatReducerType reducerType = ChatReducerType.MessageCountingChatReducer, ChatReducerTriggerEvent triggerEvent = ChatReducerTriggerEvent.AfterMessagesRetrieval)
    {
        IChatReducer? chatReducer = new MessageCountingChatReducer(3);
        if (reducerType == ChatReducerType.SummarizingChatReducer)
        {
            chatReducer = new SummarizingChatReducer(_chatClient, 3, null);
        }

        AIAgent agent = _chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "Joker",
            ChatOptions = new() { Instructions = "You are good at telling jokes." },
            ChatHistoryProviderFactory = (content, ctx) =>
            {
                // Create a new chat message store for this agent that stores the messages in a vector store.
                return ValueTask.FromResult<ChatHistoryProvider>(new VectorChatMessageStore(
                   _vectorStore,
                   content.SerializedState,
                   content.JsonSerializerOptions,
                   chatReducer,
                   triggerEvent));
            }
        });

        AgentSession thread = await agent.GetNewSessionAsync();

        var response = await agent.RunAsync("Tell me a joke about a pirate.", thread);
        yield return response.Text;
        var messageStore = thread.GetService<VectorChatMessageStore>()!;
        var invokingContext = new ChatHistoryProvider.InvokingContext([new ChatMessage(ChatRole.User, "Tell me a joke about a pirate.")]);
        var chatHistory = await messageStore.InvokingAsync(invokingContext);
        yield return $"\nChat history has {chatHistory?.Count()} messages.\n";

        response = await agent.RunAsync("Tell me a joke about a robot.", thread);
        yield return response.Text;
        invokingContext = new ChatHistoryProvider.InvokingContext([new ChatMessage(ChatRole.User, "Tell me a joke about a robot.")]);
        chatHistory = await messageStore.InvokingAsync(invokingContext);
        yield return $"\nChat history has {chatHistory?.Count()} messages.\n";

        response = await agent.RunAsync("Tell me a joke about a lemur.", thread);
        yield return response.Text;
        invokingContext = new ChatHistoryProvider.InvokingContext([new ChatMessage(ChatRole.User, "Tell me a joke about a lemur.")]);
        chatHistory = await messageStore.InvokingAsync(invokingContext);
        yield return $"\nChat history has {chatHistory?.Count()} messages.\n";

        response = await agent.RunAsync("Tell me the joke about the pirate again, but add emojis and use the voice of a parrot.", thread);
        yield return response.Text;
        invokingContext = new ChatHistoryProvider.InvokingContext([new ChatMessage(ChatRole.User, "Tell me the joke about the pirate again, but add emojis and use the voice of a parrot.")]);
        chatHistory = await messageStore.InvokingAsync(invokingContext);
        yield return $"\nChat history has {chatHistory?.Count()} messages.\n";
    }

    public async IAsyncEnumerable<string> Declarative()
    {
        var text =
    """
    kind: Prompt
    name: Assistant
    description: Light assistant
    instructions: You are a useful light assistant. can tall user the status of the lights and can help user control the lights on and off .
    model:
        options:
            temperature: 0.9
            topP: 0.95
            allowMultipleToolCalls: true
            chatToolMode: auto
    tools:
      - kind: function
        name: GetLightsAsync
        description: Gets a list of lights and their current state.
    """;

        var lightPlugin = new LightPlugin();

        var message = "Can you tell me the status of all the lights?";
        var agentFactory = new ChatClientPromptAgentFactory(_chatClient, [AIFunctionFactory.Create(lightPlugin.GetLightsAsync, "GetLightsAsync")]);
        var agent = await agentFactory.CreateFromYamlAsync(text);

        AgentSession thread = await agent.GetNewSessionAsync();
        var response = await agent.RunAsync(message, thread);
        yield return response.Text;
    }

    public async IAsyncEnumerable<string> Approval2(string? conversation = null, bool allowChangeState = false)
    {
        var lightPlugin = new LightPlugin();
        AITool[] tools = [
            AIFunctionFactory.Create(lightPlugin.GetLightsAsync),
            new ApprovalRequiredAIFunction(AIFunctionFactory.Create(lightPlugin.ChangeStateAsync))
        ];

        conversation ??= Guid.NewGuid().ToString("N");
        _threadStore.TryGetValue(conversation, out var threadString);

        AIAgent agent = _chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "Joker",
            ChatOptions = new()
            {
                Instructions = "You are a useful assistant",
                Tools = tools,
                ConversationId = conversation,
            },

            ChatHistoryProviderFactory = (content, ctx) =>
            {
                return ValueTask.FromResult<ChatHistoryProvider>(new InMemoryChatHistoryProvider(content.SerializedState, content.JsonSerializerOptions));
            }
        });
        agent = agent
            .AsBuilder()
                .Use(AgentMiddleware.FunctionCallingMiddleware)
            .Build();

        AgentSession thread = threadString == null ? await agent.GetNewSessionAsync() : await agent.DeserializeSessionAsync(JsonSerializer.Deserialize<JsonElement>(threadString));
        List<ChatMessage> userMessage = [new ChatMessage(ChatRole.User, "Could you please turn off all the lights?")];

        if (thread is ChatClientAgentSession typedThread && typedThread.ChatHistoryProvider != null)
        {
            var invokingContext = new ChatHistoryProvider.InvokingContext(userMessage);
            var messages = (await typedThread.ChatHistoryProvider.InvokingAsync(invokingContext).ConfigureAwait(false)).ToList() ?? [];
            var approvalMessage = ProcessFunctionApprovals(messages, allowChangeState);
            if (approvalMessage?.Count > 0)
            {
                userMessage = approvalMessage;
            }
        }

        try
        {
            var response = await agent.RunAsync(userMessage, thread);

            _threadStore[conversation] = thread.Serialize().GetRawText();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }


        yield return _threadStore[conversation];
    }

    private static List<ChatMessage> ProcessFunctionApprovals(List<ChatMessage> messages, bool approved = false)
    {
        List<ChatMessage> result = [];
        for (var messageIndex = 0; messageIndex < messages.Count; messageIndex++)
        {
            var message = messages[messageIndex];

            bool hasApprovalResponse = false;
            FunctionApprovalRequestContent? approvalRequestContent = null;
            for (var contentIndex = 0; contentIndex < message.Contents.Count; contentIndex++)
            {
                var content = message.Contents[contentIndex];
                if (content is FunctionApprovalRequestContent approvalRequest)
                {
                    approvalRequestContent = approvalRequest;
                }
                else if (content is FunctionApprovalResponseContent _)
                {
                    hasApprovalResponse = true;
                }
            }

            if (!hasApprovalResponse && approvalRequestContent != null)
            {
                result.Add(new ChatMessage(ChatRole.User, [approvalRequestContent.CreateResponse(approved)]));

                // If don't add the following code, it will get an error `messages with role 'tool' must be a response to a preceeding message with 'tool_calls'`.
                // If add the following code, `thread.Serialize().GetRawText()` will contain a duplicate `"$type": "functionCall"` entry.
                var callMessage = new ChatMessage(ChatRole.Assistant, [approvalRequestContent.FunctionCall])
                {
                    AuthorName = message.AuthorName,
                    CreatedAt = DateTime.UtcNow,
                    MessageId = "chatcmpl-" + Guid.NewGuid().ToString("N"),
                };
                result.Add(callMessage);
            }
        }

        return result;
    }
}

public enum ChatReducerType
{
    SummarizingChatReducer, MessageCountingChatReducer
}
