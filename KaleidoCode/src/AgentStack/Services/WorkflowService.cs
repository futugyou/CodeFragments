
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

    public async IAsyncEnumerable<string> Sequential()
    {
        Func<string, string> uppercaseFunc = s => s.ToUpperInvariant();
        var uppercase = uppercaseFunc.BindAsExecutor("UppercaseExecutor");
        ReverseTextExecutor reverse = new();
        WorkflowBuilder builder = new(uppercase);
        builder.AddEdge(uppercase, reverse).WithOutputFrom(reverse);
        var workflow = builder.Build();
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
            if (evt is WorkflowOutputEvent output)
            {
                yield return $"Workflow completed with results:\n{output.Data}";
                break;
            }
        }
    }
}
