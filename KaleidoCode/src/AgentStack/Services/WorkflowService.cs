
using System.Text;
using AgentStack.Executor;

namespace AgentStack.Services;

public class WorkflowService
{
    private readonly AgentOptions _options;
    private readonly IChatClient _chatClient;
    public WorkflowService(IOptionsMonitor<AgentOptions> optionsMonitor)
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
    }

    public async IAsyncEnumerable<string> Sequential(bool streaming = false)
    {
        Func<string, string> uppercaseFunc = s => s.ToUpperInvariant();
        var uppercase = uppercaseFunc.BindAsExecutor("UppercaseExecutor");
        // UppercaseExecutor uppercase = new(); 
        ReverseTextExecutor reverse = new();
        WorkflowBuilder builder = new(uppercase);
        builder.AddEdge(uppercase, reverse).WithOutputFrom(reverse);
        var workflow = builder.Build();
        if (streaming)
        {
            await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, input: "Hello, World!");
            await foreach (WorkflowEvent evt in run.WatchStreamAsync())
            {
                if (evt is ExecutorCompletedEvent executorCompleted)
                {
                    yield return $"Completed: {executorCompleted.ExecutorId}: {executorCompleted.Data}";
                }
                // this work have no agent, so we can not use AgentRunUpdateEvent
                if (evt is AgentRunUpdateEvent executorComplete)
                {
                    yield return $"RunUpdate: {executorComplete.ExecutorId}: {executorComplete.Data}";
                }
            }
        }
        else
        {
            await using Run run = await InProcessExecution.RunAsync(workflow, "Hello, World!");
            foreach (WorkflowEvent evt in run.NewEvents)
            {
                switch (evt)
                {
                    case ExecutorCompletedEvent executorComplete:
                        yield return $"{executorComplete.ExecutorId}: {executorComplete.Data}";
                        break;
                }
            }
        }
    }

    public async IAsyncEnumerable<string> Concurrent()
    {
        // Create the AI agents with specialized expertise
        AIAgent physicist = _chatClient.CreateAIAgent(
            name: "Physicist",
            instructions: "You are an expert in physics. You answer questions from a physics perspective."
        );

        AIAgent chemist = _chatClient.CreateAIAgent(
            name: "Chemist",
            instructions: "You are an expert in chemistry. You answer questions from a chemistry perspective."
        );

        var startExecutor = new ConcurrentStartExecutor();
        var aggregationExecutor = new ConcurrentAggregationExecutor();

        // Build the workflow by adding executors and connecting them
        var workflow = new WorkflowBuilder(startExecutor)
            .AddFanOutEdge(startExecutor, [physicist, chemist])
            .AddFanInEdge([physicist, chemist], aggregationExecutor)
            .WithOutputFrom(aggregationExecutor)
            .Build();
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, "What is temperature?");
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            switch (evt)
            {
                case AgentRunUpdateEvent output:
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
        ChatClientAgent historyTutor = new(_chatClient,
                   "You provide assistance with historical queries. Explain important events and context clearly. Only respond about history.",
                   "history_tutor",
                   "Specialist agent for historical questions");
        ChatClientAgent mathTutor = new(_chatClient,
            "You provide help with math problems. Explain your reasoning at each step and include examples. Only respond about math.",
            "math_tutor",
            "Specialist agent for math questions");
        ChatClientAgent triageAgent = new(_chatClient,
            "You determine which agent to use based on the user's homework question. ALWAYS handoff to another agent.",
            "triage_agent",
            "Routes messages to the appropriate specialist agent");
        var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(triageAgent)
            .WithHandoffs(triageAgent, [mathTutor, historyTutor])
            .WithHandoffs([mathTutor, historyTutor], triageAgent)
            .Build();
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
        string? lastExecutorId = null;
        StringBuilder sb = new();
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            switch (evt)
            {
                case AgentRunUpdateEvent output:
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
                // WorkflowOutputEvent is enough，no need to use AgentRunUpdateEvent/ExecutorCompletedEvent
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
        static ChatClientAgent GetTranslationAgent(string targetLanguage, IChatClient chatClient) =>
         new(chatClient,
             $"You are a translation assistant who only responds in {targetLanguage}. Respond to any " +
             $"input by outputting the name of the input language and then translating the input to {targetLanguage}.");

        List<ChatMessage> messages = [new(ChatRole.User, input)];
        var workflow = AgentWorkflowBuilder.CreateGroupChatBuilderWith(agents => new RoundRobinGroupChatManager(agents) { MaximumIterationCount = 3 })
                        .AddParticipants(from lang in (string[])["French", "Spanish", "English"] select GetTranslationAgent(lang, _chatClient))
                        .Build();
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
        string? lastExecutorId = null;
        StringBuilder sb = new();
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            switch (evt)
            {
                case AgentRunUpdateEvent output:
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
                // WorkflowOutputEvent is enough，no need to use AgentRunUpdateEvent/ExecutorCompletedEvent
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
        UppercaseExecutor uppercase = new();
        ReverseTextExecutor reverse = new();
        AppendSuffixExecutor append = new(" [PROCESSED]");

        var subWorkflow = new WorkflowBuilder(uppercase)
            .AddEdge(uppercase, reverse)
            .AddEdge(reverse, append)
            .WithOutputFrom(append)
            .Build();

        ExecutorBinding subWorkflowExecutor = subWorkflow.BindAsExecutor("TextProcessingSubWorkflow");

        PrefixExecutor prefix = new("INPUT: ");
        PostProcessExecutor postProcess = new();

        var mainWorkflow = new WorkflowBuilder(prefix)
            .AddEdge(prefix, subWorkflowExecutor)
            .AddEdge(subWorkflowExecutor, postProcess)
            .WithOutputFrom(postProcess)
            .Build();

        await using Run run = await InProcessExecution.RunAsync(mainWorkflow, input);

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

}
