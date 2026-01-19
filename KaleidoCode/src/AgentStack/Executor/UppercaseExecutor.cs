
namespace AgentStack.Executor;

public class UppercaseChatProtocolExecutor() : ChatProtocolExecutor("UppercaseExecutor", DefaultOptions, declareCrossRunShareable: true)
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
                var up = m.ToUpperInvariant();
                Console.WriteLine($"[Uppercase] '{m}' → '{up}'");
                result.Add(new ChatMessage(msg.Role, up));
                await context.AddEventAsync(new ExecutorEvent("UppercaseExecutor", $"[Uppercase] '{m}' → '{up}'"), cancellationToken);
                await context.QueueStateUpdateAsync<string>("Uppercase", up, scopeName: "TextExecutor", cancellationToken);
            }
            else
            {
                result.Add(msg);
            }
        }

        await context.SendMessageAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}

public sealed class UppercaseExecutor() : Executor<string, string>("UppercaseExecutor")
{
    private const string StateKey = "UppercaseExecutorState";

    private List<string> messages = [];

    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        messages.Add(message);
        string result = message.ToUpperInvariant();
        Console.WriteLine($"[Uppercase] '{message}' → '{result}'");
        return ValueTask.FromResult(result);
    }

    protected override ValueTask OnCheckpointingAsync(IWorkflowContext context, CancellationToken cancellation = default)
    {
        return context.QueueStateUpdateAsync(StateKey, messages, cancellation);
    }

    protected override async ValueTask OnCheckpointRestoredAsync(IWorkflowContext context, CancellationToken cancellation = default)
    {
        messages = await context.ReadStateAsync<List<string>>(StateKey, cancellation).ConfigureAwait(false) ?? [];
    }
}