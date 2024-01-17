
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

    [Route("prompt/one")]
    [HttpPost]
    public async Task<string[]> PromptOne()
    {
        var responseList = new List<string>();
        string request = "I want to send an email to the marketing team celebrating their recent milestone.";
        responseList.Add(request);
        string history = @"<message role=""user"">I hate sending emails, no one ever reads them.</message>
<message role=""assistant"">I'm sorry to hear that. Messages may be a better way to communicate.</message>";

        string prompt = @$"<message role=""system"">Instructions: What is the intent of this request?
If you don't know the intent, don't guess; instead respond with ""Unknown"".
Choices: SendEmail, SendMessage, CompleteTask, CreateDocument, Unknown.
Bonus: You'll get $20 if you get this right.</message>

<message role=""user"">Can you send a very quick approval to the marketing team?</message>
<message role=""system"">Intent:</message>
<message role=""assistant"">SendMessage</message>

<message role=""user"">Can you send the full update to the marketing team?</message>
<message role=""system"">Intent:</message>
<message role=""assistant"">SendEmail</message>

{history}
<message role=""user"">{request}</message>
<message role=""system"">Intent:</message>";

        var result = await _kernel.InvokePromptAsync(prompt);
        responseList.Add(result.ToString());
        return responseList.ToArray();
    }

}
