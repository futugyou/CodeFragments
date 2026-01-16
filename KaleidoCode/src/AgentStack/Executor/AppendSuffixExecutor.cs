
namespace AgentStack.Executor;

public sealed class AppendSuffixChatProtocolExecutor(string suffix) : ChatProtocolExecutor("AppendSuffixExecutor", DefaultOptions, declareCrossRunShareable: true)
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
                var up = m + suffix;
                Console.WriteLine($"[AppendSuffix] '{m}' → '{up}'");
                result.Add(new ChatMessage(msg.Role, up));
                await context.AddEventAsync(new ExecutorEvent("AppendSuffixExecutor", $"[AppendSuffix] '{m}' → '{up}'"), cancellationToken);
            }
            else
            {
                result.Add(msg);
            }
        }

        await context.SendMessageAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}

public sealed class AppendSuffixExecutor(string suffix) : Executor<string, string>("AppendSuffixExecutor")
{
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        string result = message + suffix;
        Console.WriteLine($"[AppendSuffix] '{message}' → '{result}'");
        return ValueTask.FromResult(result);
    }
}