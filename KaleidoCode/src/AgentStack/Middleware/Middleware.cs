
namespace AgentStack.Middleware;

public static class AgentMiddleware
{
    public static async Task<AgentResponse> AgentRunMiddleware(
         IEnumerable<ChatMessage> messages,
         AgentSession? session,
         AgentRunOptions? options,
         AIAgent innerAgent,
         CancellationToken cancellationToken)
    {
        Console.WriteLine($"agent middleware, input count: {messages.Count()}");
        var response = await innerAgent.RunAsync(messages, session, options, cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"agent middleware, output: {response.Messages.Count}");
        return response;
    }

    public static async IAsyncEnumerable<AgentResponseUpdate> AgentRunStreamMiddleware(
          IEnumerable<ChatMessage> messages,
          AgentSession? session,
          AgentRunOptions? options,
          AIAgent innerAgent,
          [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Console.WriteLine($"agent middleware, input count: {messages.Count()}");

        await foreach (var update in innerAgent.RunStreamingAsync(messages, session, options, cancellationToken))
        {
            Console.WriteLine($"agent middleware, output text: {update.Text}");
            yield return update;
        }
    }

    public static async ValueTask<object?> FunctionCallingMiddleware(
           AIAgent agent,
           FunctionInvocationContext context,
           Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
           CancellationToken cancellationToken)
    {
        Console.WriteLine($"function middleware, name: {context!.Function.Name}");
        var result = await next(context, cancellationToken);
        Console.WriteLine($"function middleware, result: {result}");

        return result;
    }

    public static async Task<ChatResponse> ChatClientMiddleware(
          IEnumerable<ChatMessage> messages,
          ChatOptions? options,
          IChatClient innerChatClient,
          CancellationToken cancellationToken)
    {
        Console.WriteLine($"chat middleware, input count: {messages.Count()}");
        var response = await innerChatClient.GetResponseAsync(messages, options, cancellationToken);
        Console.WriteLine($"chat middleware, output count: {response.Messages.Count}");

        return response;
    }

    public static async IAsyncEnumerable<ChatResponseUpdate> ChatClientStreamMiddleware(IEnumerable<ChatMessage> messages, ChatOptions? options, IChatClient innerChatClient, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // https://github.com/microsoft/agent-framework/blob/main/dotnet/src/Microsoft.Agents.AI.Hosting.AGUI.AspNetCore/AGUIEndpointRouteBuilderExtensions.cs#L58
        if (options?.AdditionalProperties is { } properties &&
            properties.TryGetValue("ag_ui_context", out KeyValuePair<string, string>[]? context) &&
            context?.Length > 0)
        {
            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("The following context from the user's application is available:");
            foreach (var item in context)
            {
                contextBuilder.AppendLine($"- {item.Key}: {item.Value}");
            }
            var contextMessage = new ChatMessage(
                ChatRole.System,
                [new TextContent(contextBuilder.ToString())]);
            messages = messages.Append(contextMessage);
        }

        Console.WriteLine($"chat middleware, input count: {messages.Count()}");
        await foreach (var update in innerChatClient.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            Console.WriteLine($"chat middleware, onput text: {update.Text}");
            yield return update;
        }
    }


    public static async IAsyncEnumerable<AgentResponseUpdate> HandleApprovalRequestsMiddleware(
    IEnumerable<ChatMessage> messages,
    AgentSession? session,
    AgentRunOptions? options,
    AIAgent innerAgent,
    JsonSerializerOptions jsonSerializerOptions,
    [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Process messages: Convert approval responses back to agent format
        var modifiedMessages = ConvertApprovalResponsesToFunctionApprovals(messages, jsonSerializerOptions);

        // Invoke inner agent
        await foreach (var update in innerAgent.RunStreamingAsync(
            modifiedMessages, session, options, cancellationToken))
        {
            // Process updates: Convert approval requests to client tool calls
            await foreach (var processedUpdate in ConvertFunctionApprovalsToToolCalls(update, jsonSerializerOptions))
            {
                yield return processedUpdate;
            }
        }

        // Local function: Convert approval responses from client back to FunctionApprovalResponseContent
        static IEnumerable<ChatMessage> ConvertApprovalResponsesToFunctionApprovals(
            IEnumerable<ChatMessage> messages,
            JsonSerializerOptions jsonSerializerOptions)
        {
            // Look for "request_approval" tool calls and their matching results
            Dictionary<string, FunctionCallContent> approvalToolCalls = [];
            FunctionResultContent? approvalResult = null;

            foreach (var message in messages)
            {
                foreach (var content in message.Contents)
                {
                    if (content is FunctionCallContent { Name: "request_approval" } toolCall)
                    {
                        approvalToolCalls[toolCall.CallId] = toolCall;
                    }
                    else if (content is FunctionResultContent result && approvalToolCalls.ContainsKey(result.CallId))
                    {
                        approvalResult = result;
                    }
                }
            }

            // If no approval response found, return messages unchanged
            if (approvalResult == null)
            {
                return messages;
            }

            // Deserialize the approval response
            if ((approvalResult.Result as JsonElement?)?.Deserialize(jsonSerializerOptions.GetTypeInfo(typeof(ApprovalResponse))) is not ApprovalResponse response)
            {
                return messages;
            }

            // Extract the original function call details from the approval request
            var originalToolCall = approvalToolCalls[approvalResult.CallId];

            if (originalToolCall.Arguments?.TryGetValue("request", out var requestObj) is not true || requestObj is not JsonElement request ||
                request.Deserialize(jsonSerializerOptions.GetTypeInfo(typeof(ApprovalRequest))) is not ApprovalRequest approvalRequest)
            {
                return messages;
            }

            // Deserialize the function arguments from JsonElement
            var functionArguments = approvalRequest.FunctionArguments is { } args
                ? (Dictionary<string, object?>?)args.Deserialize(
                    jsonSerializerOptions.GetTypeInfo(typeof(Dictionary<string, object?>)))
                : null;

            var originalFunctionCall = new FunctionCallContent(
                callId: response.ApprovalId,
                name: approvalRequest.FunctionName,
                arguments: functionArguments);

            var functionApprovalResponse = new ToolApprovalResponseContent(
                response.ApprovalId,
                response.Approved,
                originalFunctionCall);

            // Replace/remove the approval-related messages
            List<ChatMessage> newMessages = [];
            foreach (var message in messages)
            {
                bool hasApprovalResult = false;
                bool hasApprovalRequest = false;

                foreach (var content in message.Contents)
                {
                    if (content is FunctionResultContent { CallId: var callId } && callId == approvalResult.CallId)
                    {
                        hasApprovalResult = true;
                        break;
                    }
                    if (content is FunctionCallContent { Name: "request_approval", CallId: var reqCallId } && reqCallId == approvalResult.CallId)
                    {
                        hasApprovalRequest = true;
                        break;
                    }
                }

                if (hasApprovalResult)
                {
                    // Replace tool result with approval response
                    newMessages.Add(new ChatMessage(ChatRole.User, [functionApprovalResponse]));
                }
                else if (hasApprovalRequest)
                {
                    // Skip the request_approval tool call message
                    continue;
                }
                else
                {
                    newMessages.Add(message);
                }
            }

            return newMessages;
        }

        // Local function: Convert ToolApprovalRequestContent to client tool calls
        static async IAsyncEnumerable<AgentResponseUpdate> ConvertFunctionApprovalsToToolCalls(
            AgentResponseUpdate update,
            JsonSerializerOptions jsonSerializerOptions)
        {
            // Check if this update contains a ToolApprovalRequestContent
            ToolApprovalRequestContent? approvalRequestContent = null;
            foreach (var content in update.Contents)
            {
                if (content is ToolApprovalRequestContent request)
                {
                    approvalRequestContent = request;
                    break;
                }
            }

            // If no approval request, yield the update unchanged
            if (approvalRequestContent == null || approvalRequestContent.ToolCall is not FunctionCallContent functionCall)
            {
                yield return update;
                yield break;
            }

            var approvalId = approvalRequestContent.RequestId;

            // Serialize the function arguments as JsonElement
            var argsElement = functionCall.Arguments?.Count > 0
                ? JsonSerializer.SerializeToElement(functionCall.Arguments, jsonSerializerOptions.GetTypeInfo(typeof(IDictionary<string, object?>)))
                : (JsonElement?)null;

            var approvalData = new ApprovalRequest
            {
                ApprovalId = approvalId,
                FunctionName = functionCall.Name,
                FunctionArguments = argsElement,
                Message = $"Approve execution of '{functionCall.Name}'?"
            };

            var approvalJson = JsonSerializer.Serialize(approvalData, jsonSerializerOptions.GetTypeInfo(typeof(ApprovalRequest)));

            // Yield a tool call update that represents the approval request
            yield return new AgentResponseUpdate(ChatRole.Assistant, [
                new FunctionCallContent(
                callId: approvalId,
                name: "request_approval",
                arguments: new Dictionary<string, object?> { ["request"] = approvalJson })
            ]);
        }
    }
}

public sealed class ApprovalRequest
{
    [JsonPropertyName("approval_id")]
    public required string ApprovalId { get; init; }

    [JsonPropertyName("function_name")]
    public required string FunctionName { get; init; }

    [JsonPropertyName("function_arguments")]
    public JsonElement? FunctionArguments { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}

public sealed class ApprovalResponse
{
    [JsonPropertyName("approval_id")]
    public required string ApprovalId { get; init; }

    [JsonPropertyName("approved")]
    public required bool Approved { get; init; }
}
