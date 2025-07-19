

using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AspnetcoreEx.KernelService.Internal;

[Experimental("SKEXP0011")]
public class ArtReviewerAgentChat(Kernel kernel, [FromKeyedServices("CopyWriter")] ChatCompletionAgent copyWriterAgent, [FromKeyedServices("ArtDirector")] ChatCompletionAgent artDirectorAgent)
{
    public readonly AgentGroupChat Chat =
            new(copyWriterAgent, artDirectorAgent)
            {
                ExecutionSettings =
                    new()
                    {
                        TerminationStrategy = CreateTerminationStrategy(kernel, artDirectorAgent),
                        SelectionStrategy = CreateSelectionStrategy(kernel, copyWriterAgent),
                    }
            };

    public void UpdateMaximumIterations(int maximumIterations)
    {
        Chat.ExecutionSettings.TerminationStrategy.MaximumIterations = maximumIterations;
    }

    public IAsyncEnumerable<ChatMessageContent> RunAsync(ChatMessageContent input, CancellationToken cancellationToken = default)
    {
        Chat.AddChatMessage(input);

        return Chat.InvokeAsync(cancellationToken);
    }

    #region private methods
    private static KernelFunctionTerminationStrategy CreateTerminationStrategy(Kernel kernel, ChatCompletionAgent agent)
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
            MaximumIterations = 6,
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