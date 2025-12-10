
namespace AgentStack.Executor;

public sealed class ReverseTextExecutor() : Executor<string, string>("ReverseTextExecutor")
{
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        string result = string.Concat(message.Reverse());
        Console.WriteLine($"[Reverse] '{message}' → '{result}'");
        return ValueTask.FromResult(result);
    }
}
