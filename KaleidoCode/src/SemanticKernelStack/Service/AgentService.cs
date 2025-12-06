
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelStack.Internal;
using AngleSharp.Common;

namespace SemanticKernelStack.Services;

public class AgentService
{
    private readonly Kernel _kernel;
    private readonly SemanticKernelOptions _options;
    private readonly ArtReviewerAgentChat _artReviewerAgent;
    private readonly ChatCompletionAgent jokerAgent;
    private readonly ChatCompletionAgent lightsAgent;
    public AgentService(Kernel kernel, IOptionsMonitor<SemanticKernelOptions> optionsMonitor, ArtReviewerAgentChat artReviewerAgent)
    {
        _kernel = kernel;
        _options = optionsMonitor.CurrentValue;
        _artReviewerAgent = artReviewerAgent;
        jokerAgent = new ChatCompletionAgent()
        {
            Instructions = "You are good at telling jokes.",
            Name = "Joker",
            Kernel = _kernel,
        };
        lightsAgent = new ChatCompletionAgent()
        {
            Name = "lights",
            Instructions = "You are good at check the light status and control the light switch.",
            Kernel = _kernel,
            Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
        };
    }
    public async IAsyncEnumerable<string> Joker()
    {
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
            await foreach (AgentResponseItem<ChatMessageContent> response in jokerAgent.InvokeAsync(message, thread))
            {
                yield return $"Role: {response.Message.Role}, Content: {response.Message.Content}";
            }
        }
    }

    public async IAsyncEnumerable<string> Lights()
    {
        // System.ArgumentException: An item with the same key has already been added. Key: Lights
        // It has been registered in the kernel during setup, so it cannot be registered again here.
        // lightsAgent.Kernel.Plugins.AddFromType<LightPlugin>("Lights");

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
            await foreach (AgentResponseItem<ChatMessageContent> response in lightsAgent.InvokeAsync(message, thread))
            {
                yield return $"Role: {response.Message.Role}, Content: {response.Message.Content}";
            }
        }
    }
    public async IAsyncEnumerable<string> Chat(int maximumIterations = 4)
    {
        var agentReviewer = new ChatCompletionAgent()
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

        var agentWriter = new ChatCompletionAgent()
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
        AgentGroupChat chat = new(agentWriter, agentReviewer)
        {
            ExecutionSettings = new()
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

    public async IAsyncEnumerable<string> DI(int maximumIterations)
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

    public async IAsyncEnumerable<string> Function()
    {
        var kernel = _kernel.Clone();
        var agentPlugin = KernelPluginFactory.CreateFromFunctions("AgentPlugin",
           [
               AgentKernelFunctionFactory.CreateFromAgent(jokerAgent),
                AgentKernelFunctionFactory.CreateFromAgent(lightsAgent),
            ]);
        kernel.Plugins.Add(agentPlugin);

        // Define the agent
        ChatCompletionAgent agent =
            new()
            {
                Name = "ToolAssistant",
                Instructions = "You are a tool assistant. Delegate to the provided agents to help the user with tell jokes and control lights.",
                Kernel = kernel,
                Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
            };

        // Invoke the agent and display the responses
        string[] messages =
            [
                "Tell me a joke about a pirate..",
                "Can you tell me the status of all the lights?."
            ];

        AgentThread? agentThread = null;
        foreach (var message in messages)
        {
            var input = new ChatMessageContent(AuthorRole.User, message);
            yield return $"#{input.Role}: {input.Content}";

            await foreach (var response in agent.InvokeAsync(input, agentThread))
            {
                agentThread = response.Thread;

                yield return $"#{response.Message.Role} - {response.Message.AuthorName}: {response.Message.Content}";
            }
        }
    }

    public async IAsyncEnumerable<string> Concurrent(string query = "What is temperature?")
    {
        ChatCompletionAgent physicist = new()
        {
            Name = "PhysicsExpert",
            Instructions = "You are an expert in physics. You answer questions from a physics perspective.",
            Kernel = _kernel.Clone(),
        };

        ChatCompletionAgent chemist = new()
        {
            Name = "ChemistryExpert",
            Instructions = "You are an expert in chemistry. You answer questions from a chemistry perspective.",
            Kernel = _kernel.Clone(),
        };

        ConcurrentOrchestration orchestration = new(physicist, chemist);
        InProcessRuntime runtime = new();
        var result = await orchestration.InvokeAsync(query, runtime);
        await runtime.StartAsync();

        string[] output = await result.GetValueAsync();
        yield return $"# RESULT:\n{string.Join("\n\n", output.Select(text => $"{text}"))}";
        await runtime.RunUntilIdleAsync();
    }

    public async IAsyncEnumerable<string> Sequential(string query)
    {
        ChatCompletionAgent analystAgent = new()
        {
            Name = "Analyst",
            Instructions = "You are a marketing analyst. Given a product description, identify:\n- Key features\n- Target audience\n- Unique selling points",
            Kernel = _kernel.Clone(),
        };

        ChatCompletionAgent writerAgent = new()
        {
            Name = "Copywriter",
            Instructions = "You are a marketing copywriter. Given a block of text describing features, audience, and USPs, compose a compelling marketing copy (like a newsletter section) that highlights these points. Output should be short (around 150 words), output just the copy as a single text block.",
            Kernel = _kernel.Clone(),
        };

        ChatCompletionAgent editorAgent = new()
        {
            Name = "Editor",
            Instructions = "You are an editor. Given the draft copy, correct grammar, improve clarity, ensure consistent tone, give format and make it polished. Output the final improved copy as a single text block.",
            Kernel = _kernel.Clone(),
        };
        ChatHistory history = [];

        ValueTask responseCallback(ChatMessageContent response)
        {
            history.Add(response);
            return ValueTask.CompletedTask;
        }

        SequentialOrchestration orchestration = new(analystAgent, writerAgent, editorAgent)
        {
            ResponseCallback = responseCallback,
        };

        InProcessRuntime runtime = new();
        var result = await orchestration.InvokeAsync(query, runtime);
        await runtime.StartAsync();

        string output = await result.GetValueAsync(TimeSpan.FromSeconds(20));
        yield return $"\n# RESULT: {output}";

        foreach (ChatMessageContent message in history)
        {
            yield return $"#{message.Role} - {message.AuthorName}: {message.Content}";
        }

        await runtime.RunUntilIdleAsync();
    }

    public async IAsyncEnumerable<string> GroupChat(string query)
    {
        ChatCompletionAgent writer = new()
        {
            Name = "CopyWriter",
            Description = "A copy writer",
            Instructions = "You are a copywriter with ten years of experience and are known for brevity and a dry humor. The goal is to refine and decide on the single best copy as an expert in the field. Only provide a single proposal per response. You're laser focused on the goal at hand. Don't waste time with chit chat. Consider suggestions when refining an idea.",
            Kernel = _kernel.Clone(),
        };

        ChatCompletionAgent editor = new()
        {
            Name = "Reviewer",
            Description = "An editor.",
            Instructions = "You are an art director who has opinions about copywriting born of a love for David Ogilvy. The goal is to determine if the given copy is acceptable to print. If so, state that it is approved. If not, provide insight on how to refine suggested copy without example.",
            Kernel = _kernel.Clone(),
        };

        ChatHistory history = [];

        ValueTask responseCallback(ChatMessageContent response)
        {
            history.Add(response);
            return ValueTask.CompletedTask;
        }

        GroupChatOrchestration orchestration = new(
             new CustomGroupChatManager { MaximumInvocationCount = 5 },
             writer,
             editor)
        {
            ResponseCallback = responseCallback,
        };

        InProcessRuntime runtime = new();
        var result = await orchestration.InvokeAsync(query, runtime);
        await runtime.StartAsync();

        string output = await result.GetValueAsync(TimeSpan.FromSeconds(20));
        yield return $"\n# RESULT: {output}";

        foreach (ChatMessageContent message in history)
        {
            yield return $"#{message.Role} - {message.AuthorName}: {message.Content}";
        }

        await runtime.RunUntilIdleAsync();
    }

    public async IAsyncEnumerable<string> Handoff(string query)
    {
        ChatCompletionAgent triageAgent = new()
        {
            Name = "TriageAgent",
            Description = "Handle customer requests.",
            Instructions = "A customer support agent that triages issues.",
            Kernel = _kernel.Clone(),
        };

        ChatCompletionAgent statusAgent = new()
        {
            Name = "OrderStatusAgent",
            Description = "A customer support agent that checks order status.",
            Instructions = "Handle order status requests.",
            Kernel = _kernel.Clone(),
        };
        statusAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(new OrderStatusPlugin()));

        ChatCompletionAgent returnAgent = new()
        {
            Name = "OrderReturnAgent",
            Description = "A customer support agent that handles order returns.",
            Instructions = "Handle order return requests.",
            Kernel = _kernel.Clone(),
        };
        returnAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(new OrderReturnPlugin()));

        ChatCompletionAgent refundAgent = new()
        {
            Name = "OrderRefundAgent",
            Description = "A customer support agent that handles order refund.",
            Instructions = "Handle order refund requests.",
            Kernel = _kernel.Clone(),
        };
        refundAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(new OrderRefundPlugin()));

        var handoffs = OrchestrationHandoffs
            .StartWith(triageAgent)
            .Add(triageAgent, statusAgent, returnAgent, refundAgent)
            .Add(statusAgent, triageAgent, "Transfer to this agent if the issue is not status related")
            .Add(returnAgent, triageAgent, "Transfer to this agent if the issue is not return related")
            .Add(refundAgent, triageAgent, "Transfer to this agent if the issue is not refund related");

        ChatHistory history = [];

        ValueTask responseCallback(ChatMessageContent response)
        {
            history.Add(response);
            return ValueTask.CompletedTask;
        }

        // Simulate user input with a queue
        Queue<string> responses = new();
        responses.Enqueue("I'd like to track the status of my order");
        responses.Enqueue("My order ID is 123");
        responses.Enqueue("I want to return another order of mine");
        responses.Enqueue("Order ID 321");
        responses.Enqueue("Broken item");
        responses.Enqueue("No, bye");

        ValueTask<ChatMessageContent> interactiveCallback()
        {
            string input = responses.Dequeue();
            Console.WriteLine($"\n# INPUT: {input}\n");
            return ValueTask.FromResult(new ChatMessageContent(AuthorRole.User, input));
        }

        HandoffOrchestration orchestration = new(
            handoffs,
            triageAgent,
            statusAgent,
            returnAgent,
            refundAgent)
        {
            InteractiveCallback = interactiveCallback,
            ResponseCallback = responseCallback,
        };

        InProcessRuntime runtime = new();
        var result = await orchestration.InvokeAsync(query, runtime);
        await runtime.StartAsync();

        string output = await result.GetValueAsync(TimeSpan.FromSeconds(20));
        yield return $"\n# RESULT: {output}";

        foreach (ChatMessageContent message in history)
        {
            yield return $"#{message.Role} - {message.AuthorName}: {message.Content}";
        }

        await runtime.RunUntilIdleAsync();
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

// Plugin implementations
public sealed class OrderStatusPlugin
{
    [KernelFunction]
    public static string CheckOrderStatus(string orderId) => $"Order {orderId} is shipped and will arrive in 2-3 days.";
}

public sealed class OrderReturnPlugin
{
    [KernelFunction]
    public static string ProcessReturn(string orderId, string reason) => $"Return for order {orderId} has been processed successfully.";
}

public sealed class OrderRefundPlugin
{
    [KernelFunction]
    public static string ProcessReturn(string orderId, string reason) => $"Refund for order {orderId} has been processed successfully.";
}

[Experimental("SKEXP0011")]
sealed class ApprovalTerminationStrategy : TerminationStrategy
{
    // Terminate when the final message contains the term "approve"
    // The `This copy is not approved` also case `approve`.
    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
}

public class CustomGroupChatManager : GroupChatManager
{
    public override ValueTask<GroupChatManagerResult<string>> FilterResults(ChatHistory history, CancellationToken cancellationToken = default)
    {
        // Custom logic to filter or summarize chat results
        return ValueTask.FromResult(new GroupChatManagerResult<string>("Summary") { Reason = "Custom summary logic." });
    }

    public override ValueTask<GroupChatManagerResult<string>> SelectNextAgent(ChatHistory history, GroupChatTeam team, CancellationToken cancellationToken = default)
    {
        // Randomly select an agent from the team
        var random = new Random();
        int index = random.Next(team.Count);
        string nextAgent = team.GetItemByIndex(index).Key;
        return ValueTask.FromResult(new GroupChatManagerResult<string>(nextAgent) { Reason = "Custom selection logic." });
    }

    public override ValueTask<GroupChatManagerResult<bool>> ShouldRequestUserInput(ChatHistory history, CancellationToken cancellationToken = default)
    {
        // Custom logic to decide if user input is needed
        return ValueTask.FromResult(new GroupChatManagerResult<bool>(false) { Reason = "No user input required." });
    }

    public override async ValueTask<GroupChatManagerResult<bool>> ShouldTerminate(ChatHistory history, CancellationToken cancellationToken = default)
    {
        // Optionally call the base implementation to check for default termination logic
        var baseResult = await base.ShouldTerminate(history, cancellationToken);
        if (baseResult.Value)
        {
            // If the base logic says to terminate, respect it
            return baseResult;
        }

        // Custom logic to determine if the chat should terminate
        bool shouldEnd = history.Count > 10; // Example: end after 10 messages
        return new GroupChatManagerResult<bool>(shouldEnd) { Reason = "Custom termination logic." };
    }
}