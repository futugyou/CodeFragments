
using AspnetcoreEx.KernelService;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AspnetcoreEx.Controllers;

[Route("api/sk")]
[ApiController]
public class KernelServiceController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly SemanticKernelOptions _options;
    private readonly IChatCompletionService _chatCompletionService;
    private OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    public KernelServiceController(Kernel kernel, IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        _kernel = kernel;
        _options = optionsMonitor.CurrentValue;
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    }

    [Route("plugin/light")]
    [HttpPost]
    public async Task<string[]> PluginLight()
    {
        var responseList = new List<string>();
        ChatHistory history = [];
        history.AddUserMessage("Hello");
        responseList.Add("User > Hello");
        var result = await _chatCompletionService.GetChatMessageContentAsync(history, executionSettings: openAIPromptExecutionSettings, kernel: _kernel);
        history.AddMessage(result.Role, result.Content ?? "");
        responseList.Add("Assistant > " + result);

        history.AddUserMessage("Can you turn on the lights");
        responseList.Add("User > Can you turn on the lights");
        result = await _chatCompletionService.GetChatMessageContentAsync(history, executionSettings: openAIPromptExecutionSettings, kernel: _kernel);
        history.AddMessage(result.Role, result.Content ?? "");
        responseList.Add("Assistant > " + result);

        return responseList.ToArray();
    }

}
