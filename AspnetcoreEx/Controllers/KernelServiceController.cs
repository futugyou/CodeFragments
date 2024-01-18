
using AspnetcoreEx.KernelService;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplate.Handlebars;

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

        return [.. responseList];
    }

    [Route("prompt/one")]
    [HttpPost]
    public async Task<string[]> PromptOne()
    {
        var responseList = new List<string>();
        string request = "I want to send an email to the marketing team celebrating their recent milestone.";
        responseList.Add(request);
        string history = @"
        <message role=""user"">I hate sending emails, no one ever reads them.</message>
        <message role=""assistant"">I'm sorry to hear that. Messages may be a better way to communicate.</message>";

        string prompt = @$"
        <message role=""system"">Instructions: What is the intent of this request?
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
        <message role=""system"">Intent:</message>
        ";

        var result = await _kernel.InvokePromptAsync(prompt);
        responseList.Add(result.ToString());
        return [.. responseList];
    }

    [Route("prompt/template")]
    [HttpPost]
    public async Task<string[]> PromptTemplate()
    {
        var responseList = new List<string>();
        ChatHistory history = [];
        string request = "I want to send an email to the marketing team celebrating their recent milestone.";
        responseList.Add(request);
        var chat = _kernel.CreateFunctionFromPrompt(
            @"{{$history}}
            User: {{$request}}
            Assistant: "
        );

        var chatResult = _kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
            chat,
            new() {
                { "request", request },
                { "history", string.Join("\n", history.Select(x => x.Role + ": " + x.Content)) }
            }
        );

        // Stream the response
        string message = "";
        await foreach (var chunk in chatResult)
        {
            if (chunk.Role.HasValue) Console.Write(chunk.Role + " > ");
            message += chunk;
        }

        responseList.Add(message);

        // Append to history
        history.AddUserMessage(request!);
        history.AddAssistantMessage(message);

        return [.. responseList];
    }

    [Route("prompt/handlebars")]
    [HttpPost]
    public async Task<string[]> PromptHandlebars()
    {
        var responseList = new List<string>();
        string request = "I want to send an email to the marketing team celebrating their recent milestone.";
        ChatHistory history = [];
        var getIntent = _kernel.CreateFunctionFromPrompt(
            new()
            {
                Template = @"
                <message role=""system"">Instructions: What is the intent of this request?
                Do not explain the reasoning, just reply back with the intent. If you are unsure, reply with {{choices[0]}}.
                Choices: {{choices}}.</message>

                {{#each fewShotExamples}}
                    {{#each this}}
                        <message role=""{{role}}"">{{content}}</message>
                    {{/each}}
                {{/each}}

                {{#each chatHistory}}
                    <message role=""{{role}}"">{{content}}</message>
                {{/each}}

                {{ConversationSummaryPlugin-SummarizeConversation history}}
                
                <message role=""user"">{{request}}</message>
                <message role=""system"">Intent:</message>",
                TemplateFormat = "handlebars"
            },
            new HandlebarsPromptTemplateFactory()
        );

        // Create choices
        List<string> choices = ["ContinueConversation", "EndConversation"];

        // Create few-shot examples
        List<ChatHistory> fewShotExamples = [
            [
                new ChatMessageContent(AuthorRole.User, "Can you send a very quick approval to the marketing team?"),
                new ChatMessageContent(AuthorRole.System, "Intent:"),
                new ChatMessageContent(AuthorRole.Assistant, "ContinueConversation"),
            ],
            [
                new ChatMessageContent(AuthorRole.User, "Thanks, I'm done for now"),
                new ChatMessageContent(AuthorRole.System, "Intent:"),
                new ChatMessageContent(AuthorRole.Assistant, "EndConversation")
            ]
        ];

        // Invoke prompt
        var intent = await _kernel.InvokeAsync(
            getIntent,
            new() {
                { "request", request },
                { "choices", choices },
                { "history", history },
                { "fewShotExamples", fewShotExamples }
            }
        );

        responseList.Add(intent.ToString());

        request = "i want go home.";
        intent = await _kernel.InvokeAsync(
            getIntent,
            new() {
                { "request", request },
                { "choices", choices },
                { "history", history },
                { "fewShotExamples", fewShotExamples }
            }
        );
        responseList.Add(request);
        responseList.Add(intent.ToString());
        return [.. responseList];
    }


    [Route("prompt/nested")]
    [HttpPost]
    public async Task<string[]> PromptNested()
    {
        var responseList = new List<string>();
        ChatHistory history = [];
        string request = "I want to send an email to the marketing team celebrating their recent milestone.";
        responseList.Add(request);
        var chat = _kernel.CreateFunctionFromPrompt(
            new PromptTemplateConfig()
            {
                Name = "Chat",
                Description = "Chat with the assistant.",
                Template = @"{{ConversationSummaryPlugin.SummarizeConversation $history}}
                User: {{$request}}
                Assistant: ",
                TemplateFormat = "semantic-kernel",
                InputVariables = [
                    new() { Name = "history", Description = "The history of the conversation.", IsRequired = false, Default = "" },
                    new() { Name = "request", Description = "The user's request.", IsRequired = true }
                ],
                ExecutionSettings = {
                    { "default", new OpenAIPromptExecutionSettings() {
                        MaxTokens = 1000,
                        Temperature = 0
                    } },
                    { "gpt-3.5-turbo", new OpenAIPromptExecutionSettings() {
                        ModelId = "gpt-3.5-turbo-0613",
                        MaxTokens = 4000,
                        Temperature = 0.2
                    } },
                    { "gpt-4", new OpenAIPromptExecutionSettings() {
                        ModelId = "gpt-4-1106-preview",
                        MaxTokens = 8000,
                        Temperature = 0.3
                    } }
                }
            }
        );

        var chatResult = _kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
            chat,
            new() {
                { "request", request },
                { "history", string.Join("\n", history.Select(x => x.Role + ": " + x.Content)) }
            }
        );

        // Stream the response
        string message = "";
        await foreach (var chunk in chatResult)
        {
            if (chunk.Role.HasValue) Console.Write(chunk.Role + " > ");
            message += chunk;
        }

        responseList.Add(message);

        // Append to history
        history.AddUserMessage(request!);
        history.AddAssistantMessage(message);

        return [.. responseList];
    }

}
