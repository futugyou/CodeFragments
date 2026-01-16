
namespace AgentStack.Executor;

public sealed class PrefixChatProtocolExecutor(string prefix) : ChatProtocolExecutor("PrefixExecutor", DefaultOptions, declareCrossRunShareable: true)
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
                var m = msg.Text;
                var up = prefix + m;
                Console.WriteLine($"[Prefix] '{m}' → '{up}'");
                result.Add(new ChatMessage(msg.Role, up));
                await context.AddEventAsync(new ExecutorEvent("PrefixExecutor", $"[Prefix] '{m}' → '{up}'"), cancellationToken);
            }
            else
            {
                result.Add(msg);
            }
        }

        await context.SendMessageAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}

public sealed class PrefixExecutor(string prefix) : Executor<string, string>("PrefixExecutor")
{
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        string result = prefix + message;
        Console.WriteLine($"[Prefix] '{message}' → '{result}'");
        return ValueTask.FromResult(result);
    }
}