
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
        Console.WriteLine($"Input: {messages.Count()}");
        var response = await innerAgent.RunAsync(messages, thread, options, cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"Output: {response.Messages.Count}");
        return response;
    }

    public static async IAsyncEnumerable<AgentRunResponseUpdate> AgentRunStreamMiddleware(
          IEnumerable<ChatMessage> messages,
          AgentThread? thread,
          AgentRunOptions? options,
          AIAgent innerAgent,
          [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Console.WriteLine($"Input: {messages.Count()}");

        await foreach (var update in innerAgent.RunStreamingAsync(messages, thread, options, cancellationToken))
        {
            Console.WriteLine($"Output: {update.Text}");
            yield return update;
        }
    }

    public static async ValueTask<object?> FunctionCallingMiddleware(
           AIAgent agent,
           FunctionInvocationContext context,
           Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
           CancellationToken cancellationToken)
    {
        Console.WriteLine($"Function Name: {context!.Function.Name}");
        var result = await next(context, cancellationToken);
        Console.WriteLine($"Function Call Result: {result}");

        return result;
    }

    public static async Task<ChatResponse> ChatClientMiddleware(
          IEnumerable<ChatMessage> messages,
          ChatOptions? options,
          IChatClient innerChatClient,
          CancellationToken cancellationToken)
    {
        Console.WriteLine($"Input: {messages.Count()}");
        var response = await innerChatClient.GetResponseAsync(messages, options, cancellationToken);
        Console.WriteLine($"Output: {response.Messages.Count}");

        return response;
    }
}