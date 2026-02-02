
namespace AgentStack.Agents;

public sealed class StateStreamingAgent : DelegatingAIAgent
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    public StateStreamingAgent(AIAgent innerAgent, JsonSerializerOptions jsonSerializerOptions)
        : base(innerAgent)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? thread = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Get current state from options if provided
        // https://github.com/microsoft/agent-framework/blob/main/dotnet/src/Microsoft.Agents.AI.Hosting.AGUI.AspNetCore/AGUIEndpointRouteBuilderExtensions.cs#L57
        JsonElement currentState = default;
        if (options is ChatClientAgentRunOptions { ChatOptions.AdditionalProperties: { } properties } &&
            properties.TryGetValue("ag_ui_state", out object? stateObj) && stateObj is JsonElement state)
        {
            currentState = state;
        }
        // Create options with JSON schema for structured state output
        ChatClientAgentRunOptions stateOptions = new ChatClientAgentRunOptions
        {
            ChatOptions = new ChatOptions
            {
                ResponseFormat = ChatResponseFormat.ForJsonSchema<AgentStateSnapshot>(
                    schemaName: "AgentStateSnapshot",
                    schemaDescription: "Research progress state")
            }
        };
        // Add system message with current state
        var stateMessage = new ChatMessage(ChatRole.System,
            $"Current state: {(currentState.ValueKind != JsonValueKind.Undefined ? currentState.GetRawText() : "{}")}");
        var messagesWithState = messages.Append(stateMessage);
        // Collect all updates
        var allUpdates = new List<AgentResponseUpdate>();
        await foreach (var update in InnerAgent.RunStreamingAsync(messagesWithState, thread, stateOptions, cancellationToken))
        {
            allUpdates.Add(update);
            // Stream non-text updates immediately
            if (update.Contents.Any(c => c is not TextContent))
            {
                yield return update;
            }
        }
        // Deserialize state snapshot from response
        var response = allUpdates.ToAgentResponse();
        if (response.TryDeserialize(_jsonSerializerOptions, out JsonElement stateSnapshot))
        {
            byte[] stateBytes = JsonSerializer.SerializeToUtf8Bytes(
                stateSnapshot,
                _jsonSerializerOptions.GetTypeInfo(typeof(JsonElement)));
            // Emit state snapshot as DataContent
            yield return new AgentResponseUpdate
            {
                Contents = [new DataContent(stateBytes, "application/json")]
            };
        }
        // Stream text summary
        var summaryMessage = new ChatMessage(ChatRole.System, "Provide a brief summary of your progress.");
        await foreach (var update in InnerAgent.RunStreamingAsync(
            messages.Concat(response.Messages).Append(summaryMessage), thread, options, cancellationToken))
        {
            yield return update;
        }
    }
}