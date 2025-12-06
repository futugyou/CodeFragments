
using Microsoft.SemanticKernel.Agents;

namespace SemanticKernelStack.Internal;

public class CopyWriterAgentFactory
{
    private readonly Kernel _kernel;
    public CopyWriterAgentFactory(Kernel kernel)
    {
        _kernel = kernel;
    }
    public ChatCompletionAgent Create()
    {
        return new()
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
    }
}