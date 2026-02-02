
using System.Text;
using AgentStack.Executor;

namespace AgentStack.Services;

public class WorkflowService
{
    private readonly AgentOptions _options;
    private readonly IChatClient _chatClient;
    private readonly Workflow _seqWorkflow;
    private readonly Workflow _conWorkflow;
    private readonly Workflow _handoffWorkflow;
    private readonly Workflow _groupChatWorkflow;
    private readonly Workflow _subWorkflow;

    public WorkflowService(
        IOptionsMonitor<AgentOptions> optionsMonitor,
        [FromKeyedServices("sequential")] Workflow seqWorkflow,
        [FromKeyedServices("concurrent")] Workflow conWorkflow,
        [FromKeyedServices("handoff")] Workflow handoffWorkflow,
        [FromKeyedServices("groupChat")] Workflow groupChatWorkflow,
        [FromKeyedServices("sub-workflow")] Workflow subWorkflow)
    {
        _options = optionsMonitor.CurrentValue;
        var credential = new ApiKeyCredential(_options.TextCompletion.ApiKey);
        OpenAIClientOptions openAIOptions = new();
        if (!string.IsNullOrEmpty(_options.TextCompletion.Endpoint))
        {
            openAIOptions.Endpoint = new Uri(_options.TextCompletion.Endpoint);
        }

        var ghModelsClient = new OpenAIClient(credential, openAIOptions);
        _chatClient = ghModelsClient.GetChatClient(_options.TextCompletion.ModelId).AsIChatClient();
        _seqWorkflow = seqWorkflow;
        _conWorkflow = conWorkflow;
        _handoffWorkflow = handoffWorkflow;
        _groupChatWorkflow = groupChatWorkflow;
        _subWorkflow = subWorkflow;
    }

    static async IAsyncEnumerable<string> GetEventData(string type, string id, object? data)
    {
        var items = data switch
        {
            string str => [str],
            ChatMessage msg => [msg.Text],
            IEnumerable<ChatMessage> m => m.Select(x => x.Text),
            _ => []
        };

        foreach (var item in items)
        {
            yield return type + ": " + id + ": " + item;
        }
    }

    public async IAsyncEnumerable<string> Sequential(bool streaming = false)
    {
        var workflow = _seqWorkflow;
        var msg = new ChatMessage(ChatRole.User, "Hello, World!");
        if (streaming)
        {
            await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, msg);
            await foreach (WorkflowEvent evt in run.WatchStreamAsync())
            {
                // Why didn't streaming trigger an ExecutorEvent?
                Console.WriteLine(evt.GetType().Name);
                if (evt is SuperStepStartedEvent superStepStartedEvent)
                {
                    yield return $"SuperStepStartedEvent: {superStepStartedEvent.StepNumber}: {superStepStartedEvent.Data}";
                }
                if (evt is ExecutorInvokedEvent executorInvokedEvent)
                {
                    yield return $"ExecutorInvokedEvent: {executorInvokedEvent.ExecutorId}: {executorInvokedEvent.Data}";
                }
                if (evt is ExecutorCompletedEvent executorCompletedEvent)
                {
                    yield return $"ExecutorCompletedEvent: {executorCompletedEvent.ExecutorId}: {executorCompletedEvent.Data}";
                }
                if (evt is SuperStepCompletedEvent superStepCompletedEvent)
                {
                    yield return $"SuperStepCompletedEvent: {superStepCompletedEvent.StepNumber}: {superStepCompletedEvent.Data}";
                }
            }
        }
        else
        {
            await using Run run = await InProcessExecution.RunAsync(workflow, msg);
            foreach (WorkflowEvent evt in run.NewEvents)
            {
                switch (evt)
                {
                    case SuperStepStartedEvent superStepStartedEvent:
                        await foreach (var item in GetEventData(evt.GetType().Name, superStepStartedEvent.StepNumber.ToString(), superStepStartedEvent.Data))
                        {
                            yield return item;
                        }
                        break;
                    case ExecutorInvokedEvent executorInvokedEvent:
                        await foreach (var item in GetEventData(evt.GetType().Name, executorInvokedEvent.ExecutorId, executorInvokedEvent.Data))
                        {
                            yield return item;
                        }
                        break;
                    case ExecutorCompletedEvent executorCompletedEvent:
                        await foreach (var item in GetEventData(evt.GetType().Name, executorCompletedEvent.ExecutorId, executorCompletedEvent.Data))
                        {
                            yield return item;
                        }
                        break;
                    case SuperStepCompletedEvent superStepCompletedEvent:
                        await foreach (var item in GetEventData(evt.GetType().Name, superStepCompletedEvent.StepNumber.ToString(), superStepCompletedEvent.Data))
                        {
                            yield return item;
                        }
                        break;
                    case ExecutorEvent executorEvent:
                        await foreach (var item in GetEventData(evt.GetType().Name, executorEvent.ExecutorId, executorEvent.Data))
                        {
                            yield return item;
                        }
                        break;
                }
            }
        }
    }

    public async IAsyncEnumerable<string> Concurrent()
    {
        var workflow = _conWorkflow;
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, "What is temperature?");
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            switch (evt)
            {
                case AgentResponseUpdateEvent output:
                    yield return $"Workflow RunUpdate with results:\n{output.Data}";
                    break;
                case ExecutorCompletedEvent output:
                    yield return $"Completed: {output.ExecutorId}: {output.Data}";
                    break;
                case WorkflowOutputEvent output:
                    yield return $"Workflow Output:{output.SourceId}:  {output.Data}";
                    break;
            }
        }
    }

    public async IAsyncEnumerable<string> Handoffs(string question)
    {
        List<ChatMessage> messages = [new(ChatRole.User, question)];
        var workflow = _handoffWorkflow;
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
        string? lastExecutorId = null;
        StringBuilder sb = new();
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            switch (evt)
            {
                case AgentResponseUpdateEvent output:
                    if (output.ExecutorId != lastExecutorId)
                    {
                        if (sb.Length > 0)
                        {
                            yield return sb.ToString();
                        }
                        sb = new();
                        lastExecutorId = output.ExecutorId;
                        sb.Append($"Workflow RunUpdate: {output.ExecutorId} ");
                    }

                    sb.Append($"{output.Update.Text}");
                    break;
                case ExecutorCompletedEvent output:
                    yield return $"Completed: {output.ExecutorId}: {output.Data}";
                    break;
                // WorkflowOutputEvent is enough，no need to use AgentResponseUpdateEvent/ExecutorCompletedEvent
                case WorkflowOutputEvent output:
                    var msgs = (List<ChatMessage>)output.Data!;
                    foreach (var msg in msgs)
                    {
                        yield return $"Workflow Output: {output.SourceId} {msg.AuthorName}:  {msg.Text}";
                    }
                    break;
            }
        }

        if (sb.Length > 0)
        {
            yield return sb.ToString();
        }
    }

    public async IAsyncEnumerable<string> Groupchat(string input)
    {
        List<ChatMessage> messages = [new(ChatRole.User, input)];
        var workflow = _groupChatWorkflow;
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
        string? lastExecutorId = null;
        StringBuilder sb = new();
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            switch (evt)
            {
                case AgentResponseUpdateEvent output:
                    if (output.ExecutorId != lastExecutorId)
                    {
                        if (sb.Length > 0)
                        {
                            yield return sb.ToString();
                        }
                        sb = new();
                        lastExecutorId = output.ExecutorId;
                        sb.Append($"Workflow RunUpdate! ExecutorId: {output.ExecutorId}, Text: ");
                    }

                    sb.Append($"{output.Update.Text}");
                    break;
                case ExecutorCompletedEvent output:
                    yield return $"Completed! ExecutorId: {output.ExecutorId}, Data: {output.Data}";
                    break;
                // WorkflowOutputEvent is enough，no need to use AgentResponseUpdateEvent/ExecutorCompletedEvent
                case WorkflowOutputEvent output:
                    var msgs = (List<ChatMessage>)output.Data!;
                    foreach (var msg in msgs)
                    {
                        yield return $"Workflow Output! SourceId: {output.SourceId}, AuthorName: {msg.AuthorName}, Text: {msg.Text}";
                    }
                    break;
            }
        }

        if (sb.Length > 0)
        {
            yield return sb.ToString();
        }
    }

    public async IAsyncEnumerable<string> SubWorkflow(string input)
    {
        var mainWorkflow = _subWorkflow;
        List<ChatMessage> messages = [new(ChatRole.User, input)];
        await using Run run = await InProcessExecution.RunAsync(mainWorkflow, messages);

        foreach (WorkflowEvent evt in run.NewEvents)
        {
            if (evt is ExecutorCompletedEvent executorComplete && executorComplete.Data is not null)
            {
                yield return $"[{executorComplete.ExecutorId}] {executorComplete.Data}";
            }
            else if (evt is WorkflowOutputEvent output)
            {
                yield return $"Final Output: {output.Data}";
            }
        }
    }

    public async IAsyncEnumerable<string> SwitchRoutes(string input, int maxIterations = 3)
    {
        WriterExecutor writer = new(_chatClient);
        CriticExecutor critic = new(_chatClient, maxIterations);
        SummaryExecutor summary = new(_chatClient);

        // Build the workflow with conditional routing based on critic's decision
        WorkflowBuilder workflowBuilder = new WorkflowBuilder(writer)
            .AddEdge(writer, critic)
            .AddSwitch(critic, sw => sw
                .AddCase<CriticDecision>(cd => cd?.Approved == true, summary)
                .AddCase<CriticDecision>(cd => cd?.Approved == false, writer))
            .WithOutputFrom(summary);

        Workflow workflow = workflowBuilder.Build();
        await using Run run = await InProcessExecution.RunAsync(workflow, input);

        foreach (WorkflowEvent evt in run.NewEvents)
        {
            if (evt is ExecutorCompletedEvent executorComplete && executorComplete.Data is not null)
            {
                yield return $"[{executorComplete.ExecutorId}] {executorComplete.Data}";
            }
            else if (evt is WorkflowOutputEvent output)
            {
                yield return $"Final Output: {output.Data}";
            }
        }
    }

    public async IAsyncEnumerable<string> Checkpoint()
    {
        var checkpointManager = CheckpointManager.Default;
        var checkpoints = new List<CheckpointInfo>();

        UppercaseExecutor uppercase = new();
        ReverseTextExecutor reverse = new();

        WorkflowBuilder builder = new(uppercase);
        builder.AddEdge(uppercase, reverse).WithOutputFrom(reverse);
        var workflow = builder.Build();

        await using var run = await InProcessExecution.StreamAsync(workflow, input: "Hello, World!", checkpointManager);

        await foreach (WorkflowEvent evt in run.Run.WatchStreamAsync())
        {
            if (evt is ExecutorCompletedEvent executorCompleted)
            {
                yield return $"Completed: {executorCompleted.ExecutorId}: {executorCompleted.Data}";
            }

            // this work have no agent, so we can not use AgentResponseUpdateEvent
            if (evt is AgentResponseUpdateEvent executorComplete)
            {
                yield return $"RunUpdate: {executorComplete.ExecutorId}: {executorComplete.Data}";
            }

            if (evt is SuperStepCompletedEvent superStepCompletedEvt)
            {
                CheckpointInfo? checkpoint = superStepCompletedEvt.CompletionInfo!.Checkpoint;
                if (checkpoint is not null)
                {
                    yield return $"CheckpointInfo: {checkpoint.CheckpointId}: {checkpoint.RunId}";
                    checkpoints.Add(checkpoint);
                }
            }
        }

        yield return "";

        CheckpointInfo savedCheckpoint = checkpoints[0];
        await run.RestoreCheckpointAsync(savedCheckpoint, CancellationToken.None);
        await foreach (WorkflowEvent evt in run.Run.WatchStreamAsync())
        {
            switch (evt)
            {
                case RequestInfoEvent requestInputEvt:
                    yield return $"RequestInfoEvent: {requestInputEvt.Request.Data}";
                    break;
                case ExecutorCompletedEvent executorCompletedEvt:
                    yield return $"Completed: {executorCompletedEvt.ExecutorId}: {executorCompletedEvt.Data}";
                    break;
                case WorkflowOutputEvent workflowOutputEvt:
                    yield return $"Workflow completed with result: {workflowOutputEvt.Data}";
                    break;
            }
        }
    }

}
