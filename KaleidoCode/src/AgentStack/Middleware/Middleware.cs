
namespace AgentStack.Middleware;

public static class AgentMiddleware
{
    public static async Task<AgentResponse> AgentRunMiddleware(
         IEnumerable<ChatMessage> messages,
         AgentSession? thread,
         AgentRunOptions? options,
         AIAgent innerAgent,
         CancellationToken cancellationToken)
    {
        Console.WriteLine($"agent middleware, input count: {messages.Count()}");
        var response = await innerAgent.RunAsync(messages, thread, options, cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"agent middleware, output: {response.Messages.Count}");
        return response;
    }

    public static async IAsyncEnumerable<AgentResponseUpdate> AgentRunStreamMiddleware(
          IEnumerable<ChatMessage> messages,
          AgentSession? thread,
          AgentRunOptions? options,
          AIAgent innerAgent,
          [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Console.WriteLine($"agent middleware, input count: {messages.Count()}");

        await foreach (var update in innerAgent.RunStreamingAsync(messages, thread, options, cancellationToken))
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
}