
namespace AgentStack.Executor;

public sealed class ConcurrentAggregationExecutor() : Executor<List<ChatMessage>>("ConcurrentAggregationExecutor")
{
    private readonly List<ChatMessage> _messages = [];
    public override async ValueTask HandleAsync(List<ChatMessage> message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        _messages.AddRange(message);

        if (_messages.Count == 2)
        {
            var formattedMessages = string.Join(Environment.NewLine, _messages.Select(m => $"{m.AuthorName}: {m.Text}"));
            await context.YieldOutputAsync(formattedMessages, cancellationToken);
        }
    }
}
