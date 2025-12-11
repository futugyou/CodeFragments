
namespace AgentStack.Executor;

public static class FlowStateHelpers
{
    public static async Task<FlowState> ReadFlowStateAsync(IWorkflowContext context)
    {
        FlowState? state = await context.ReadStateAsync<FlowState>(FlowStateShared.Key, scopeName: FlowStateShared.Scope);
        return state ?? new FlowState();
    }

    public static ValueTask SaveFlowStateAsync(IWorkflowContext context, FlowState state)
        => context.QueueStateUpdateAsync(FlowStateShared.Key, state, scopeName: FlowStateShared.Scope);
}

public sealed class FlowState
{
    public int Iteration { get; set; } = 1;
    public List<ChatMessage> History { get; } = [];
}

public static class FlowStateShared
{
    public const string Scope = "FlowStateScope";
    public const string Key = "singleton";
}