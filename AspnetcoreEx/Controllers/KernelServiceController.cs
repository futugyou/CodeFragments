
using System.Reflection;
using AspnetcoreEx.KernelService;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplate.Handlebars;
using Microsoft.SemanticKernel.Text;

namespace AspnetcoreEx.Controllers;

[Experimental("SKEXP0011")]
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

    [Route("prompt/file")]
    [HttpPost]
    public async Task<string[]> PromptFile()
    {
        var responseList = new List<string>();
        ChatHistory history = [];
        var prompts = _kernel.CreatePluginFromPromptDirectory("KernelService/Skills");
        string request = "I want to send an email to the marketing team celebrating their recent milestone.";
        responseList.Add(request);

        var chatResult = _kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
            prompts["chat"],
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

    [Route("prompt/yaml")]
    [HttpPost]
    public async Task<string[]> PromptYAML()
    {
        var responseList = new List<string>();
        ChatHistory history = [];
        string request = "I want to send an email to the marketing team celebrating their recent milestone.";
        responseList.Add(request);
        using StreamReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("KernelService.Skills.getIntent.prompt.yaml")!);
        KernelFunction getIntent = _kernel.CreateFunctionFromPromptYaml(
            reader.ReadToEnd(),
            promptTemplateFactory: new HandlebarsPromptTemplateFactory()
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

    [Route("prompt/email")]
    [HttpPost]
    public async Task<string[]> PromptEmail()
    {
        var responseList = new List<string>();
        ChatHistory chatMessages = new ChatHistory("""
            You are a friendly assistant who likes to follow the rules. You will complete required steps
            and request approval before taking any consequential actions. If the user doesn't provide
            enough information for you to complete a task, you will keep asking questions until you have
            enough information to complete the task.
            """);

        var user_inputs = new string[] {
            "Can you help me write an email for my boss?" ,
            """
            I want to give her an update on last months sales. We broke a bunch of records that
            I want to share with her, but we did have a challenge selling the X4321 model.
            """,
            "Sure! It's sarah@contoso.com",
            "Yes please!",
            "Please sign it with Stephen and then you can go ahead and send it to Sarah",
        };

        foreach (var request in user_inputs)
        {
            chatMessages.AddUserMessage(request);
            responseList.Add(request);

            // Get the chat completions
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };
            var result = _chatCompletionService.GetStreamingChatMessageContentsAsync(
                chatMessages,
                executionSettings: openAIPromptExecutionSettings,
                kernel: _kernel);

            // Stream the results
            string fullMessage = "";
            await foreach (var content in result)
            {
                if (content.Role.HasValue)
                {
                    Console.Write("Assistant > ");
                }
                Console.Write(content.Content);
                fullMessage += content.Content;
            }
            Console.WriteLine();

            // Add the message from the agent to the chat history
            chatMessages.AddAssistantMessage(fullMessage);
            responseList.Add(fullMessage);

        }
        return [.. responseList];
    }

    [Route("prompt/math")]
    [HttpPost]
    public async Task<string> PromptMath()
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
        var first = true;
        await foreach (var content in result)
        {
            if (content.Role.HasValue && first)
            {
                Console.Write("Assistant > ");
                first = false;
            }
            Console.Write(content.Content);
            fullMessage += content.Content;
        }

        return fullMessage;
    }

    [Route("token")]
    [HttpPost]
    public List<string> Token()
    {
        var text = @"You are a friendly assistant who likes to follow the rules. You will complete required steps
            and request approval before taking any consequential actions. If the user doesn't provide
            enough information for you to complete a task, you will keep asking questions until you have
            enough information to complete the task.";

        // 1. no TokenCounter
        var lines = TextChunker.SplitPlainTextLines(text, 40);
        Console.WriteLine(lines);
        var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 120);
        Console.WriteLine(paragraphs);

        // 2. SharpToken
        lines = TextChunker.SplitPlainTextLines(text, 40, SharpTokenTokenCounter);
        Console.WriteLine(lines);
        paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 120, tokenCounter: SharpTokenTokenCounter);
        Console.WriteLine(paragraphs);

        // 3. Microsoft.ML.Tokenizers
        lines = TextChunker.SplitPlainTextLines(text, 40, MicrosoftMLTokenCounter);
        Console.WriteLine(lines);
        paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 120, tokenCounter: MicrosoftMLTokenCounter);
        Console.WriteLine(paragraphs);

        // 4. Microsoft.ML.Robert
        lines = TextChunker.SplitPlainTextLines(text, 40, MicrosoftMLRobertaTokenCounter);
        Console.WriteLine(lines);
        paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 120, tokenCounter: MicrosoftMLRobertaTokenCounter);
        Console.WriteLine(paragraphs);


        return paragraphs;
    }

    // SharpToken token counter
    private static TextChunker.TokenCounter SharpTokenTokenCounter => (string input) =>
    {
        var encoding = SharpToken.GptEncoding.GetEncoding("cl100k_base");
        var tokens = encoding.Encode(input);

        return tokens.Count;
    };

    // Microsoft.ML.Tokenizers token counter
    private static TextChunker.TokenCounter MicrosoftMLTokenCounter => (string input) =>
    {
        Microsoft.ML.Tokenizers.Tokenizer tokenizer = new(new Microsoft.ML.Tokenizers.Bpe());
        var tokens = tokenizer.Encode(input).Tokens;

        return tokens.Count;
    };

    // Microsoft.ML.Robert token counter
    private static TextChunker.TokenCounter MicrosoftMLRobertaTokenCounter => (string input) =>
    {
        Assembly assembly = typeof(KernelServiceController).Assembly;

        Microsoft.ML.Tokenizers.EnglishRoberta model = new(
                                   assembly.GetManifestResourceStream("encoder.json")!,
                                   assembly.GetManifestResourceStream("vocab.bpe")!,
                                   assembly.GetManifestResourceStream("dict.txt")!);
        model.AddMaskSymbol();
        Microsoft.ML.Tokenizers.Tokenizer tokenizer = new(model, new Microsoft.ML.Tokenizers.RobertaPreTokenizer());
        var tokens = tokenizer.Encode(input).Tokens;

        return tokens.Count;
    };
}
