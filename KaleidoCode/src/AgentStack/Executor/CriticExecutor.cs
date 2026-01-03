
namespace AgentStack.Executor;

public sealed class CriticExecutor : Executor<ChatMessage, CriticDecision>
{
    private readonly AIAgent _agent;

    private readonly int _maxIterations;

    public CriticExecutor(IChatClient chatClient,int  maxIterations) : base("Critic")
    {
        _maxIterations = maxIterations;
        _agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
        {
            Name = "Critic",
            ChatOptions = new()
            {
                Instructions = """
                    You are a constructive critic. Review the content and provide specific feedback.
                    Always try to provide actionable suggestions for improvement and strive to identify improvement points.
                    Only approve if the content is high quality, clear, and meets the original requirements and you see no improvement points.
                
                    Provide your decision as structured output with:
                    - approved: true if content is good, false if revisions needed
                    - feedback: specific improvements needed (empty if approved)
                
                    Be concise but specific in your feedback.
                    """,
                ResponseFormat = ChatResponseFormat.ForJsonSchema<CriticDecision>()
            }
        });
    }

    public override async ValueTask<CriticDecision> HandleAsync(
        ChatMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        FlowState state = await FlowStateHelpers.ReadFlowStateAsync(context);

        // Use RunStreamingAsync to get streaming updates, then deserialize at the end
        IAsyncEnumerable<AgentRunResponseUpdate> updates = _agent.RunStreamingAsync(message, cancellationToken: cancellationToken);

        // Convert the stream to a response and deserialize the structured output
        AgentRunResponse response = await updates.ToAgentRunResponseAsync(cancellationToken);
        CriticDecision decision = response.Deserialize<CriticDecision>(JsonSerializerOptions.Web);

        // Safety: approve if max iterations reached
        if (!decision.Approved && state.Iteration >= _maxIterations)
        {
            decision.Approved = true;
            decision.Feedback = "";
        }

        // Increment iteration ONLY if rejecting (will loop back to Writer)
        if (!decision.Approved)
        {
            state.Iteration++;
        }

        // Store the decision in history
        state.History.Add(new ChatMessage(ChatRole.Assistant,
            $"[Decision: {(decision.Approved ? "Approved" : "Needs Revision")}] {decision.Feedback}"));
        await FlowStateHelpers.SaveFlowStateAsync(context, state);

        // Populate workflow-specific fields
        decision.Content = message.Text ?? "";
        decision.Iteration = state.Iteration;

        return decision;
    }
}