
namespace AgentStack.Middleware;

public static class AgentMiddleware
{
    public static async Task<AgentRunResponse> AgentRunMiddleware(
         IEnumerable<ChatMessage> messages,
         AgentThread? thread,
         AgentRunOptions? options,
         AIAgent innerAgent,
         CancellationToken cancellationToken)
    {
        Console.WriteLine($"agent middleware, input count: {messages.Count()}");
        var response = await innerAgent.RunAsync(messages, thread, options, cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"agent middleware, output: {response.Messages.Count}");
        return response;
    }

    public static async IAsyncEnumerable<AgentRunResponseUpdate> AgentRunStreamMiddleware(
          IEnumerable<ChatMessage> messages,
          AgentThread? thread,
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

    public static async IAsyncEnumerable<ChatResponseUpdate> ChatClientStreamMiddleware(IEnumerable<ChatMessage> messages, ChatOptions? options, IChatClient innerChatClient, [EnumeratorCancellation]CancellationToken cancellationToken)
    {
        Console.WriteLine($"chat middleware, input count: {messages.Count()}");
        await foreach (var update in innerChatClient.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            Console.WriteLine($"chat middleware, onput text: {update.Text}");
            yield return update;
        }
    }
}