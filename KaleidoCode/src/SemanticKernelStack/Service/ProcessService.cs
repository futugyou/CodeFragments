
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Process.Tools;
using SemanticKernelStack.Internal;

namespace SemanticKernelStack.Services;

public class ProcessService
{
    private readonly Kernel _kernel;
    private readonly SemanticKernelOptions _options;

    public ProcessService(Kernel kernel, IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        _kernel = kernel;
        _options = optionsMonitor.CurrentValue;
    }

    // https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/GettingStartedWithProcesses/Step01
    public async IAsyncEnumerable<string> Sample()
    {
        ProcessBuilder process = new("ChatBot");
        var introStep = process.AddStepFromType<IntroStep>();
        var userInputStep = process.AddStepFromType<ChatUserInputStep>();
        var responseStep = process.AddStepFromType<ChatBotResponseStep>();

        // Define the behavior when the process receives an external event
        process
            .OnInputEvent("StartProcess")
            .SendEventTo(new ProcessFunctionTargetBuilder(introStep));

        // When the intro is complete, notify the userInput step
        introStep
            .OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));

        // When the userInput step emits an exit event, send it to the end step
        userInputStep
            .OnEvent("Exit")
            .StopProcess();

        // When the userInput step emits a user input event, send it to the assistantResponse step
        userInputStep
            .OnEvent("UserInputReceived")
            .SendEventTo(new ProcessFunctionTargetBuilder(responseStep, parameterName: "userMessage"));

        // When the assistantResponse step emits a response, send it to the userInput step
        responseStep
            .OnEvent("AssistantResponseGenerated")
            .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));

        // Build the process to get a handle that can be started
        KernelProcess kernelProcess = process.Build();

        string mermaidGraph = kernelProcess.ToMermaid();
        yield return "mermaid: " + mermaidGraph;

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

public sealed class IntroStep : KernelProcessStep
{
    /// <summary>
    /// Prints an introduction message to the console.
    /// </summary>
    [KernelFunction]
    public void PrintIntroMessage()
    {
        Console.WriteLine("Welcome to Processes in Semantic Kernel.\n");
    }
}

public sealed class ChatBotResponseStep : KernelProcessStep<ChatBotState>
{
    public static class ProcessFunctions
    {
        public const string GetChatResponse = nameof(GetChatResponse);
    }

    /// <summary>
    /// The internal state object for the chat bot response step.
    /// </summary>
    internal ChatBotState? _state;

    /// <summary>
    /// ActivateAsync is the place to initialize the state object for the step.
    /// </summary>
    /// <param name="state">An instance of <see cref="ChatBotState"/></param>
    /// <returns>A <see cref="ValueTask"/></returns>
    public override ValueTask ActivateAsync(KernelProcessStepState<ChatBotState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Generates a response from the chat completion service.
    /// </summary>
    /// <param name="context">The context for the current step and process. <see cref="KernelProcessStepContext"/></param>
    /// <param name="userMessage">The user message from a previous step.</param>
    /// <param name="_kernel">A <see cref="Kernel"/> instance.</param>
    /// <returns></returns>
    [KernelFunction(ProcessFunctions.GetChatResponse)]
    public async Task GetChatResponseAsync(KernelProcessStepContext context, string userMessage, Kernel _kernel)
    {
        _state!.ChatMessages.Add(new(AuthorRole.User, userMessage));
        IChatCompletionService chatService = _kernel.Services.GetRequiredService<IChatCompletionService>();
        ChatMessageContent response = await chatService.GetChatMessageContentAsync(_state.ChatMessages).ConfigureAwait(false) ?? throw new InvalidOperationException("Failed to get a response from the chat completion service.");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"ASSISTANT: {response.Content}");
        Console.ResetColor();

        // Update state with the response
        _state.ChatMessages.Add(response);

        // emit event: assistantResponse
        await context.EmitEventAsync(new KernelProcessEvent { Id = "AssistantResponseGenerated", Data = response });
    }
}

/// <summary>
/// The state object for the <see cref="ChatBotResponseStep"/>.
/// </summary>
public sealed class ChatBotState
{
    internal ChatHistory ChatMessages { get; } = new();
}


public sealed class ChatUserInputStep : ScriptedUserInputStep
{
    public override void PopulateUserInputs(UserInputState state)
    {
        state.UserInputs.Add("How tall is the tallest mountain?");
        state.UserInputs.Add("exit");
        state.UserInputs.Add("This text will be ignored because exit process condition was already met at this point.");
    }

    public override async ValueTask GetUserInputAsync(KernelProcessStepContext context)
    {
        var userMessage = this.GetNextUserMessage();

        if (string.Equals(userMessage, "exit", StringComparison.OrdinalIgnoreCase))
        {
            // exit condition met, emitting exit event
            await context.EmitEventAsync(new() { Id = "Exit", Data = userMessage });
            return;
        }

        // emitting userInputReceived event
        await context.EmitEventAsync(new() { Id = "UserInputReceived", Data = userMessage });
    }
}
