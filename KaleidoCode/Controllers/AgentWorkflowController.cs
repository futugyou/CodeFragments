
using KaleidoCode.KernelService;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using Microsoft.Agents.AI;
using System;
using System.Linq;
using System.Threading.Tasks;
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
}

public sealed class ReverseTextExecutor() : Executor<string, string>("ReverseTextExecutor")
{
    public override ValueTask<string> HandleAsync(string input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Reverse the input text
        return ValueTask.FromResult(new string([.. input.Reverse()]));
    }
}