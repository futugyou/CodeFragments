
using AgentStack.Middleware; 

namespace AgentStack.Agents;

public sealed class FunctionApprovalAgent : DelegatingAIAgent
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public FunctionApprovalAgent(AIAgent innerAgent, JsonSerializerOptions jsonSerializerOptions)
        : base(innerAgent)
    {
        this._jsonSerializerOptions = jsonSerializerOptions;
    }

    protected override Task<AgentResponse> RunCoreAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return this.RunCoreStreamingAsync(messages, session, options, cancellationToken)
            .ToAgentResponseAsync(cancellationToken);
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Process and transform incoming approval responses from client, creating a new message list
        var processedMessages = ProcessIncomingFunctionApprovals(messages.ToList(), this._jsonSerializerOptions);

        // Run the inner agent and intercept any approval requests
        await foreach (var update in this.InnerAgent.RunStreamingAsync(
            processedMessages, session, options, cancellationToken).ConfigureAwait(false))
        {
            yield return ProcessOutgoingApprovalRequests(update, this._jsonSerializerOptions);
        }
    }

#pragma warning disable MEAI001 // Type is for evaluation purposes only
    private static Microsoft.Extensions.AI.ToolApprovalRequestContent ConvertToolCallToApprovalRequest(FunctionCallContent toolCall, JsonSerializerOptions jsonSerializerOptions)
    {
        if (toolCall.Name != "request_approval" || toolCall.Arguments == null)
        {
            throw new InvalidOperationException("Invalid request_approval tool call");
        }

        var request = toolCall.Arguments.TryGetValue("request", out var reqObj) &&
            reqObj is JsonElement argsElement &&
            argsElement.Deserialize(jsonSerializerOptions.GetTypeInfo(typeof(ApprovalRequest))) is ApprovalRequest approvalRequest &&
            approvalRequest != null ? approvalRequest : null;

        if (request == null)
        {
            throw new InvalidOperationException("Failed to deserialize approval request from tool call");
        }


        var functionArguments = request.FunctionArguments is { } args
                        ? (Dictionary<string, object?>?)args.Deserialize(
                            jsonSerializerOptions.GetTypeInfo(typeof(Dictionary<string, object?>)))
                        : null;

        return new ToolApprovalRequestContent(
            requestId: request.ApprovalId,
            new FunctionCallContent(
                callId: request.ApprovalId,
                name: request.FunctionName,
                arguments: functionArguments));
    }

    private static ToolApprovalResponseContent ConvertToolResultToApprovalResponse(FunctionResultContent result, ToolApprovalRequestContent approval, JsonSerializerOptions jsonSerializerOptions)
    {
        var approvalResponse = result.Result is JsonElement je ?
            (ApprovalResponse?)je.Deserialize(jsonSerializerOptions.GetTypeInfo(typeof(ApprovalResponse))) :
            result.Result is string str ?
                (ApprovalResponse?)JsonSerializer.Deserialize(str, jsonSerializerOptions.GetTypeInfo(typeof(ApprovalResponse))) :
                result.Result as ApprovalResponse;

        if (approvalResponse == null)
        {
            throw new InvalidOperationException("Failed to deserialize approval response from tool result");
        }

        return approval.CreateResponse(approvalResponse.Approved);
    }
#pragma warning restore MEAI001

    private static List<ChatMessage> CopyMessagesUpToIndex(List<ChatMessage> messages, int index)
    {
        var result = new List<ChatMessage>(index);
        for (int i = 0; i < index; i++)
        {
            result.Add(messages[i]);
        }
        return result;
    }

    private static List<AIContent> CopyContentsUpToIndex(IList<AIContent> contents, int index)
    {
        var result = new List<AIContent>(index);
        for (int i = 0; i < index; i++)
        {
            result.Add(contents[i]);
        }
        return result;
    }

    private static List<ChatMessage> ProcessIncomingFunctionApprovals(
        List<ChatMessage> messages,
        JsonSerializerOptions jsonSerializerOptions)
    {
        List<ChatMessage>? result = null;

        // Track approval ID to original call ID mapping
        _ = new Dictionary<string, string>();
#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        Dictionary<string, ToolApprovalRequestContent> trackedRequestApprovalToolCalls = new(); // Remote approvals
        for (int messageIndex = 0; messageIndex < messages.Count; messageIndex++)
        {
            var message = messages[messageIndex];
            List<AIContent>? transformedContents = null;
            for (int j = 0; j < message.Contents.Count; j++)
            {
                var content = message.Contents[j];
                if (content is FunctionCallContent { Name: "request_approval" } toolCall)
                {
                    result ??= CopyMessagesUpToIndex(messages, messageIndex);
                    transformedContents ??= CopyContentsUpToIndex(message.Contents, j);
                    var approvalRequest = ConvertToolCallToApprovalRequest(toolCall, jsonSerializerOptions);
                    transformedContents.Add(approvalRequest);
                    trackedRequestApprovalToolCalls[toolCall.CallId] = approvalRequest;
                    result.Add(new ChatMessage(message.Role, transformedContents)
                    {
                        AuthorName = message.AuthorName,
                        MessageId = message.MessageId,
                        CreatedAt = message.CreatedAt,
                        RawRepresentation = message.RawRepresentation,
                        AdditionalProperties = message.AdditionalProperties
                    });
                }
                else if (content is FunctionResultContent toolResult &&
                    trackedRequestApprovalToolCalls.TryGetValue(toolResult.CallId, out var approval) == true)
                {
                    result ??= CopyMessagesUpToIndex(messages, messageIndex);
                    transformedContents ??= CopyContentsUpToIndex(message.Contents, j);
                    var approvalResponse = ConvertToolResultToApprovalResponse(toolResult, approval, jsonSerializerOptions);
                    transformedContents.Add(approvalResponse);
                    result.Add(new ChatMessage(message.Role, transformedContents)
                    {
                        AuthorName = message.AuthorName,
                        MessageId = message.MessageId,
                        CreatedAt = message.CreatedAt,
                        RawRepresentation = message.RawRepresentation,
                        AdditionalProperties = message.AdditionalProperties
                    });
                }
                else if (result != null)
                {
                    result.Add(message);
                }
            }
        }
#pragma warning restore MEAI001

        return result ?? messages;
    }

    private static AgentResponseUpdate ProcessOutgoingApprovalRequests(
        AgentResponseUpdate update,
        JsonSerializerOptions jsonSerializerOptions)
    {
        IList<AIContent>? updatedContents = null;
        for (var i = 0; i < update.Contents.Count; i++)
        {
            var content = update.Contents[i];
#pragma warning disable MEAI001 // Type is for evaluation purposes only
            if (content is ToolApprovalRequestContent request && request.ToolCall is FunctionCallContent functionCall)
            {
                updatedContents ??= [.. update.Contents];
                var approvalId = request.RequestId;

                JsonElement arg = JsonSerializer.SerializeToElement(functionCall.Arguments);

                var approvalData = new ApprovalRequest
                {
                    ApprovalId = approvalId,
                    FunctionName = functionCall.Name,
                    FunctionArguments = arg,
                    Message = $"Approve execution of '{functionCall.Name}'?"
                };

                updatedContents[i] = new FunctionCallContent(
                    callId: approvalId,
                    name: "request_approval",
                    arguments: new Dictionary<string, object?> { ["request"] = approvalData });
            }
#pragma warning restore MEAI001
        }

        if (updatedContents is not null)
        {
            var chatUpdate = update.AsChatResponseUpdate();
            // Yield a tool call update that represents the approval request
            return new AgentResponseUpdate(new ChatResponseUpdate()
            {
                Role = chatUpdate.Role,
                Contents = updatedContents,
                MessageId = chatUpdate.MessageId,
                AuthorName = chatUpdate.AuthorName,
                CreatedAt = chatUpdate.CreatedAt,
                RawRepresentation = chatUpdate.RawRepresentation,
                ResponseId = chatUpdate.ResponseId,
                AdditionalProperties = chatUpdate.AdditionalProperties
            })
            {
                AgentId = update.AgentId,
                ContinuationToken = update.ContinuationToken
            };
        }

        return update;
    }
}