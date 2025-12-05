
using Microsoft.SemanticKernel.Agents;

namespace KaleidoCode.KernelService.Internal;

public class ArtDirectorAgentFactory
{
    private readonly Kernel _kernel;
    public ArtDirectorAgentFactory(Kernel kernel)
    {
        _kernel = kernel;
    }
    public ChatCompletionAgent Create()
    {
        return new()
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
    }
}