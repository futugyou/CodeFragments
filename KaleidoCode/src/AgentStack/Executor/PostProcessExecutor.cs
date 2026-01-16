
namespace AgentStack.Executor;

public sealed class PostProcessProtocolExecutor() : ChatProtocolExecutor("PostProcessExecutor", DefaultOptions, declareCrossRunShareable: true)
{
    private static ChatProtocolExecutorOptions DefaultOptions => new()
    {
        StringMessageChatRole = ChatRole.User
    };

    protected override async ValueTask TakeTurnAsync(List<ChatMessage> messages, IWorkflowContext context, bool? emitEvents, CancellationToken cancellationToken = default)
    {
        var result = new List<ChatMessage>();
        foreach (var msg in messages)
        {
            if (msg.Role == ChatRole.User)
            {
                var message = msg.Text;
                string up = $"[FINAL] {message} [END]";
                Console.WriteLine($"[PostProcess] '{message}' → '{up}'");
                result.Add(new ChatMessage(msg.Role, up));
            }
            else
            {
                result.Add(msg);
            }
        }

        await context.SendMessageAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}


public sealed class PostProcessExecutor() : Executor<string, string>("PostProcessExecutor")
{
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        string result = $"[FINAL] {message} [END]";
        Console.WriteLine($"[PostProcess] '{message}' → '{result}'");
        return ValueTask.FromResult(result);
    }
}