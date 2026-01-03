
namespace AgentStack.Executor;

public sealed class ReverseTextExecutor() : Executor<string, string>("ReverseTextExecutor")
{
    private const string StateKey = "ReverseTextExecutorState";

    private List<string> messages = [];

    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        messages.Add(message);
        string result = string.Concat(message.Reverse());
        Console.WriteLine($"[Reverse] '{message}' → '{result}'");
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
