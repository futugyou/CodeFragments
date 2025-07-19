
using AspnetcoreEx.KernelService;
using AspnetcoreEx.KernelService.Internal;
using AspnetcoreEx.KernelService.Skills;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
namespace AspnetcoreEx.Controllers;

[Experimental("SKEXP0011")]
[Route("api/sk/agent")]
[ApiController]
public class SKAgentController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly SemanticKernelOptions _options;
    private readonly ArtReviewerAgentChat _artReviewerAgent;

    public SKAgentController(Kernel kernel, IOptionsMonitor<SemanticKernelOptions> optionsMonitor, ArtReviewerAgentChat artReviewerAgent)
    {
        _kernel = kernel;
        _options = optionsMonitor.CurrentValue;
        _artReviewerAgent = artReviewerAgent;
    }

    [Route("joker")]
    [HttpPost]
    /// <summary>
    /// base sk agent demo
    /// </summary>
    /// <returns></returns>
    public async IAsyncEnumerable<string> Joker()
    {
        ChatCompletionAgent agent =
            new()
            {
                Name = "Joker",
                Instructions = "You are good at telling jokes.",
                Kernel = _kernel,
            };

        AgentThread? thread = new ChatHistoryAgentThread(
            [
                new ChatMessageContent(AuthorRole.User, "Tell me a joke about a pirate."),
                new ChatMessageContent(AuthorRole.Assistant, "Why did the pirate go to school? Because he wanted to improve his \"arrrrrrrrrticulation\""),
            ]);

        // Respond to user input
        await foreach (var message in InvokeAgentAsync("Now add some emojis to the joke."))
        {
            yield return message;
        }
        await foreach (var message in InvokeAgentAsync("Now make the joke sillier."))
        {
            yield return message;
        }

        // Local function to invoke agent and display the conversation messages.
        async IAsyncEnumerable<string> InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            yield return $"Role: {message.Role}, Content: {message.Content}";
            await foreach (AgentResponseItem<ChatMessageContent> response in agent.InvokeAsync(message, thread))
            {
                yield return $"Role: {response.Message.Role}, Content: {response.Message.Content}";
            }
        }
    }

    [Route("lights")]
    [HttpPost]
    /// <summary>
    /// sk agent demo with plugin
    /// </summary>
    /// <returns></returns>
    public async IAsyncEnumerable<string> Lights()
    {
        ChatCompletionAgent agent =
            new()
            {
                Name = "lights",
                Instructions = "You are good at check the light status and control the light switch.",
                Kernel = _kernel,
                Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
            };
        // System.ArgumentException: An item with the same key has already been added. Key: Lights
        // It has been registered in the kernel during setup, so it cannot be registered again here.
        // agent.Kernel.Plugins.AddFromType<LightPlugin>("Lights");

        ChatHistoryAgentThread thread = new();

        await foreach (var message in InvokeAgentAsync("Can you tell me the status of all the lights?"))
        {
            yield return message;
        }

        // Local function to invoke agent and display the conversation messages.
        async IAsyncEnumerable<string> InvokeAgentAsync(string input)
        {
            ChatMessageContent message = new(AuthorRole.User, input);
            yield return $"Role: {message.Role}, Content: {message.Content}";
            await foreach (AgentResponseItem<ChatMessageContent> response in agent.InvokeAsync(message, thread))
            {
                yield return $"Role: {response.Message.Role}, Content: {response.Message.Content}";
            }
        }
    }

    [Route("chat")]
    [HttpPost]
    /// <summary>
    /// sk agent demo for chat
    /// </summary>
    /// <returns></returns>
    public async IAsyncEnumerable<string> Chat(int maximumIterations = 4)
    {
        ChatCompletionAgent agentReviewer =
        new()
        {
            Instructions = """
        You are an art director who has opinions about copywriting born of a love for David Ogilvy.
        The goal is to determine if the given copy is acceptable to print.
        If so, state that it is approved.
        If not, provide insight on how to refine suggested copy without example.
        """,
            Name = "ArtDirector",
            Kernel = _kernel,
        };

        ChatCompletionAgent agentWriter =
            new()
            {
                Instructions = """
        You are a copywriter with ten years of experience and are known for brevity and a dry humor.
        The goal is to refine and decide on the single best copy as an expert in the field.
        Only provide a single proposal per response.
        You're laser focused on the goal at hand.
        Don't waste time with chit chat.
        Consider suggestions when refining an idea.
        """,
                Name = "CopyWriter",
                Kernel = _kernel,
            };

        // Create a chat for agent interaction.
        AgentGroupChat chat =
            new(agentWriter, agentReviewer)
            {
                ExecutionSettings =
                    new()
                    {
                        // Here a TerminationStrategy subclass is used that will terminate when
                        // an assistant message contains the term "approve".
                        // TerminationStrategy =
                        //     new ApprovalTerminationStrategy()
                        //     {
                        //         // Only the art-director may approve.
                        //         Agents = [agentReviewer],
                        //         // Limit total number of turns
                        //         MaximumIterations = maximumIterations,
                        //     }
                        TerminationStrategy = CreateTerminationStrategy(_kernel, agentReviewer, maximumIterations),
                        SelectionStrategy = CreateSelectionStrategy(_kernel, agentWriter),
                    }
            };

        // Invoke chat and display messages.
        ChatMessageContent input = new(AuthorRole.User, "concept: maps made out of egg cartons.");
        chat.AddChatMessage(input);
        yield return $"#{input.Role}: {input.Content}";

        await foreach (ChatMessageContent response in chat.InvokeAsync())
        {
            yield return $"#{response.Role} - {response.AuthorName}: {response.Content}";
        }

        yield return $"[IS COMPLETED: {chat.IsComplete}]";
    }

    [Route("di")]
    [HttpPost]
    /// <summary>
    /// sk agent demo for chat
    /// </summary>
    /// <returns></returns>
    public async IAsyncEnumerable<string> DI(int maximumIterations = 4)
    {
        ChatMessageContent input = new(AuthorRole.User, "concept: maps made out of egg cartons.");

        yield return $"#{input.Role}: {input.Content}";
        _artReviewerAgent.UpdateMaximumIterations(maximumIterations);

        await foreach (ChatMessageContent response in _artReviewerAgent.RunAsync(input))
        {
            yield return $"#{response.Role} - {response.AuthorName}: {response.Content}";
        }

        yield return $"[IS COMPLETED: {_artReviewerAgent.Chat.IsComplete}]";

    }
    #region private methods
    private static KernelFunctionTerminationStrategy CreateTerminationStrategy(Kernel kernel, ChatCompletionAgent agent, int maximumIterations = 4)
    {
        KernelFunction terminationFunction =
            AgentGroupChat.CreatePromptFunctionForStrategy(
                """
                Determine if the copy has been approved.  If so, respond with a single word: yes

                History:
                {{$history}}
                """,
                safeParameterNames: "history");
        ChatHistoryTruncationReducer strategyReducer = new(1);

        return new KernelFunctionTerminationStrategy(terminationFunction, kernel)
        {
            // Only the art-director may approve.
            Agents = [agent],
            // Customer result parser to determine if the response is "yes"
            ResultParser = (result) => result.GetValue<string>()?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
            // The prompt variable name for the history argument.
            HistoryVariableName = "history",
            // Limit total number of turns
            MaximumIterations = maximumIterations,
            // Save tokens by not including the entire history in the prompt
            HistoryReducer = strategyReducer,
        };
    }

    private static KernelFunctionSelectionStrategy CreateSelectionStrategy(Kernel kernel, ChatCompletionAgent agent)
    {
        KernelFunction selectionFunction =
            AgentGroupChat.CreatePromptFunctionForStrategy(
                $$$"""
                Determine which participant takes the next turn in a conversation based on the the most recent participant.
                State only the name of the participant to take the next turn.
                No participant should take more than one turn in a row.

                Choose only from these participants:
                - ArtDirector
                - CopyWriter

                Always follow these rules when selecting the next participant:
                - After CopyWriter, it is ArtDirector's turn.
                - After ArtDirector, it is CopyWriter's turn.

                History:
                {{$history}}
                """,
                safeParameterNames: "history");
        ChatHistoryTruncationReducer strategyReducer = new(1);

        return new KernelFunctionSelectionStrategy(selectionFunction, kernel)
        {
            // Always start with the writer agent.
            InitialAgent = agent,
            // Returns the entire result value as a string.
            ResultParser = (result) => result.GetValue<string>() ?? "CopyWriter",
            // The prompt variable name for the history argument.
            HistoryVariableName = "history",
            // Save tokens by not including the entire history in the prompt
            HistoryReducer = strategyReducer,
            // Only include the agent names and not the message content
            EvaluateNameOnly = true,
        };
    }

    #endregion
}

[Experimental("SKEXP0011")]
sealed class ApprovalTerminationStrategy : TerminationStrategy
{
    // Terminate when the final message contains the term "approve"
    // The `This copy is not approved` also case `approve`.
    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
}