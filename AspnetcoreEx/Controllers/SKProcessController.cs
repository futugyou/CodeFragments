
using AspnetcoreEx.KernelService;
using Microsoft.SemanticKernel;
using AspnetcoreEx.KernelService.Skills;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AspnetcoreEx.Controllers;

[Experimental("SKEXP0011")]
[Route("api/sk/process")]
[ApiController]
public class SKProcessController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly SemanticKernelOptions _options;

    public SKProcessController(Kernel kernel, IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        _kernel = kernel;
        _options = optionsMonitor.CurrentValue;
    }

    [Route("sample")]
    [HttpPost]
    public async IAsyncEnumerable<string> Sample()
    {
        ProcessBuilder process = new("ChatBot");
        var startStep = process.AddStepFromType<StartStep>();
        var lastStep = process.AddStepFromType<EndStep>();

        // Define the process flow
        process
            .OnInputEvent("StartProcess")
            .SendEventTo(new ProcessFunctionTargetBuilder(startStep));

        startStep
            .OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(lastStep));

        lastStep
            .OnFunctionResult()
            .StopProcess();

        // Build the process to get a handle that can be started
        KernelProcess kernelProcess = process.Build();

        // Start the process with an initial external event
        await using var runningProcess = await kernelProcess.StartAsync(
            _kernel,
                new KernelProcessEvent()
                {
                    Id = "StartProcess",
                    Data = null
                });
        KernelProcess state = await runningProcess.GetStateAsync();
        foreach (var step in state.Steps)
        {
            yield return step.State.Name;
        }

    }
}


[Experimental("SKEXP0011")]
public sealed class StartStep : KernelProcessStep
{
    [KernelFunction]
    public async ValueTask ExecuteAsync(KernelProcessStepContext context)
    {
        Console.WriteLine("Start\n");
    }
}
[Experimental("SKEXP0011")]
public sealed class EndStep : KernelProcessStep
{
    [KernelFunction]
    public async ValueTask ExecuteAsync(KernelProcessStepContext context)
    {
        Console.WriteLine("This is the Final Step...\n");
    }
}