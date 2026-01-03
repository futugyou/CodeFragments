
namespace AgentStack.Executor;

public sealed class PrefixExecutor(string prefix) : Executor<string, string>("PrefixExecutor")
{
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        string result = prefix + message;
        Console.WriteLine($"[Prefix] '{message}' → '{result}'");
        return ValueTask.FromResult(result);
    }
}