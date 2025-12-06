
using SemanticKernelStack;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace KaleidoCode.Controllers;

[Experimental("SKEXP0011")]
[Route("api/af/agent-workflow")]
[ApiController]
public class AgentWorkflowController : ControllerBase
{
    private readonly SemanticKernelOptions _options;
    private readonly IChatClient _chatClient;
    public AgentWorkflowController(IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
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

    [Route("sequential")]
    [HttpPost]
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

    [Route("concurrent")]
    [HttpPost]
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

public sealed class ReverseTextExecutor() : Executor<string, string>("ReverseTextExecutor")
{
    public override ValueTask<string> HandleAsync(string input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Reverse the input text
        return ValueTask.FromResult(new string([.. input.Reverse()]));
    }
}

public sealed class ConcurrentStartExecutor() : Executor<string>("ConcurrentStartExecutor")
{
    public override async ValueTask HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Broadcast the message to all connected agents. Receiving agents will queue
        // the message but will not start processing until they receive a turn token.
        await context.SendMessageAsync(new ChatMessage(ChatRole.User, message), cancellationToken);

        // Broadcast the turn token to kick off the agents.
        await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken);
    }
}

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