
namespace AgentStack.Executor;

public sealed class PostProcessExecutor() : Executor<string, string>("PostProcessExecutor")
{
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        string result = $"[FINAL] {message} [END]";
        Console.WriteLine($"[PostProcess] '{message}' → '{result}'");
        return ValueTask.FromResult(result);
    }
}