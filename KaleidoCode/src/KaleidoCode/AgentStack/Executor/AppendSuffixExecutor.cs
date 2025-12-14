
namespace AgentStack.Executor;

public sealed class AppendSuffixExecutor(string suffix) : Executor<string, string>("AppendSuffixExecutor")
{
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        string result = message + suffix;
        Console.WriteLine($"[AppendSuffix] '{message}' → '{result}'");
        return ValueTask.FromResult(result);
    }
}