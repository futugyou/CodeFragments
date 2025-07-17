
using AspnetcoreEx.KernelService.Duckduckgo;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Document;
using Microsoft.SemanticKernel.Plugins.Document.FileSystem;
using Microsoft.SemanticKernel.Plugins.Document.OpenXml;

namespace AspnetcoreEx.Controllers;

[Experimental("SKEXP0011")]
[Route("api/sk/plugins")]
[ApiController]
public class SKPluginsController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    public SKPluginsController(Kernel kernel)
    {
        _kernel = kernel;
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
    }

    [Route("duck")]
    [HttpPost]
    public async Task<IAsyncEnumerable<string>> Dock(string input)
    {
        // Cannot be used, will trigger duck's verification code
        var search = new DuckduckgoTextSearch();
        var results = await search.SearchAsync(input);
        return results.Results;
    }

    [Route("search")]
    [HttpPost]
    public async Task<string[]> WebSearch(string input)
    {
        ChatHistory history = [];
        history.AddAssistantMessage("""
        You are a search engine assistant that can help users find content of interest from search engines such as Google/Bing/DuckDuckGo.
        Don't generate answers directly, but use tools to get answers.
        """);
        history.AddUserMessage(input);
        var result = await _chatCompletionService.GetChatMessageContentAsync(history, executionSettings: openAIPromptExecutionSettings, kernel: _kernel);
        history.AddMessage(result.Role, result.Content ?? "");
        return [.. history.Select(x => x.Role + " > " + x.Content)];
    }

    [Route("infr-project-platforms-count")]
    [HttpPost]
    public async Task<string[]> InfrProjectPlatformsCount()
    {
        ChatHistory history = [];
        history.AddUserMessage("How many platforms is InfrastructureProject currently connected to?");
        var result = await _chatCompletionService.GetChatMessageContentAsync(history, executionSettings: openAIPromptExecutionSettings, kernel: _kernel);
        history.AddMessage(result.Role, result.Content ?? "");
        return [.. history.Select(x => x.Role + " > " + x.Content)];
    }

    [Route("data-generator")]
    [HttpPost]
    public async Task<string[]> DataGenerator(string input)
    {
        ChatHistory history = [];
        history.AddSystemMessage("""
        You are a data generation assistant. 
        You will receive a class name or a JSON Schema of the class or C# class definition string. 
        Please use this as a basis to generate at least 3 demo data.
        """);
        await CallLightFunction(input, history);
        return [.. history.Select(x => x.Role + " > " + x.Content)];
    }

    [Route("light-controller")]
    [HttpPost]
    public async Task<string[]> LightController()
    {
        ChatHistory history = [];
        await CallLightFunction("Hello", history);
        await CallLightFunction("Can you turn on the lights", history);
        await CallLightFunction("Light name is `Chandelier`", history);
        await CallLightFunction("Ok, turn off it", history);
        return [.. history.Select(x => x.Role + " > " + x.Content)];
    }

    private async Task CallLightFunction(string input, ChatHistory history)
    {
        history.AddUserMessage(input);
        var result = await _chatCompletionService.GetChatMessageContentAsync(history, executionSettings: openAIPromptExecutionSettings, kernel: _kernel);
        history.AddMessage(result.Role, result.Content ?? "");
    }

    [Route("email-sender")]
    [HttpPost]
    public async Task<string[]> EmailSender()
    {
        ChatHistory chatMessages = new ChatHistory("""
            You are a mail assistant who helps users send mail to the specified address.
            """);

        // Get the chat completions
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
        chatMessages.AddUserMessage("""
            The marketing team has made considerable progress, 
            I want to send an email to the marketing team celebrating their recent milestone.
            Once the email is written, it can be sent directly without confirming it with me.
            """);
        // `Once the email is written...` will send the email directly without confirmation, but It will use a random email address and signature. 
        // I can add the signature in the prompt, but if I add the `email` address it will report an `error`.
        // The response was filtered due to the prompt triggering Azure OpenAI's content management policy. Please modify your prompt and retry. To learn more about our content filtering policies please read our documentation: https://go.microsoft.com/fwlink/?linkid=2198766
        var result = _chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatMessages,
            executionSettings: openAIPromptExecutionSettings,
            kernel: _kernel);

        // Stream the results
        string fullMessage = "";
        await foreach (var content in result)
        {
            fullMessage += content.Content;
        }

        // Add the message from the agent to the chat history
        chatMessages.AddAssistantMessage(fullMessage);

        return [.. chatMessages.Select(x => x.Role + " > " + x.Content)];
    }

    [Route("math-executor")]
    [HttpPost]
    public async Task<string[]> MathExecutor()
    {
        // double answer = await _kernel.InvokeAsync<double>(
        // "MathExPlugin", "Sqrt",
        //     new() {
        //         { "number1", 12 }
        //     }
        // );
        // return answer.ToString();
        // Create chat history
        ChatHistory history = [];
        history.AddUserMessage("Take the square root of 12");

        // Enable auto function calling
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        // Get the response from the AI
        var result = _chatCompletionService.GetStreamingChatMessageContentsAsync(
            history,
            executionSettings: openAIPromptExecutionSettings,
            kernel: _kernel);

        // Stream the results
        string fullMessage = "";
        await foreach (var content in result)
        {
            fullMessage += content.Content;
        }

        // Add the message from the agent to the chat history
        history.AddAssistantMessage(fullMessage);

        return [.. history.Select(x => x.Role + " > " + x.Content)];
    }

    [Route("word-reader")]
    [HttpPost]
    public async Task<string> WordReader(string filePath)
    {
        DocumentPlugin documentPlugin = new(new WordDocumentConnector(), new LocalFileSystemConnector());
        string text = await documentPlugin.ReadTextAsync(filePath);
        return text;
    }

    [Route("file")]
    [HttpPost]
    public async Task<string[]> CallFilePlugin()
    {
        ChatHistory history = [];
        history.AddUserMessage("Hello");
        history.AddAssistantMessage("Hello, it's a pleasure to serve you. How can I assist you today?");
        history.AddUserMessage("Recently, the marketing team has made considerable progress.");
        history.AddAssistantMessage("Yes, I already understand this situation. How can I help you with it?");

        var prompts = _kernel.CreatePluginFromPromptDirectory("KernelService/Skills");
        string request = "I want to send an email to the marketing team celebrating their recent milestone.";

        var chatResult = _kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
            prompts["chat"], // "Skills","chat",           
            new() {
                { "request", request },
                { "history", string.Join("\n", history.Select(x => x.Role + ": " + x.Content)) }
            }
        );

        // Stream the response
        string message = "";
        await foreach (var chunk in chatResult)
        {
            message += chunk;
        }

        // Append to history
        history.AddUserMessage(request!);
        history.AddAssistantMessage(message);

        return [.. history.Select(x => x.Role + " > " + x.Content)];
    }

    [Route("summary")]
    [HttpPost]
    public async Task<string[]> Summary()
    {
        ChatHistory history = [];
        history.AddUserMessage("Hello");
        history.AddAssistantMessage("Hello, it's a pleasure to serve you. How can I assist you today?");
        history.AddUserMessage("Recently, the marketing team has made considerable progress.");
        history.AddAssistantMessage("Yes, I already understand this situation. How can I help you with it?");

        var summaryPlugin = _kernel.Plugins.GetFunction("ConversationSummaryPlugin", "SummarizeConversation");
        FunctionResult summary = await _kernel.InvokeAsync(summaryPlugin, new() { ["input"] = string.Join("\n", history.Select(x => x.Role + ": " + x.Content)) });

        return [summary.GetValue<string>() ?? "No summary available."];
    }
}
