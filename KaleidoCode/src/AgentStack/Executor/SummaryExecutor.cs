
namespace AgentStack.Executor;

public sealed class SummaryExecutor : Executor<CriticDecision, ChatMessage>
{
    private readonly AIAgent _agent;

    public SummaryExecutor(IChatClient chatClient) : base("Summary")
    {
        _agent = new ChatClientAgent(
            chatClient,
            name: "Summary",
            instructions: """
                You present the final approved content to the user.
                Simply output the polished content - no additional commentary needed.
                """
        );
    }

    public override async ValueTask<ChatMessage> HandleAsync(
        CriticDecision message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        string prompt = $"Present this approved content:\n\n{message.Content}";

        StringBuilder sb = new();
        await foreach (AgentResponseUpdate update in _agent.RunStreamingAsync(new ChatMessage(ChatRole.User, prompt), cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                sb.Append(update.Text);
            }
        }

        ChatMessage result = new(ChatRole.Assistant, sb.ToString());
        await context.YieldOutputAsync(result, cancellationToken);
        return result;
    }
}