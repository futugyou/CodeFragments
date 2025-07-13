using ModelContextProtocol.Client;

namespace AspnetcoreEx.KernelService;

[Experimental("SKEXP0001")]
/// <summary>
/// Extension methods for ModelContextProtocol
/// </summary>
public static class ModelContextProtocolExtensions
{
    /// <summary>
    /// Map the tools exposed on this <see cref="IMcpClient"/> to a collection of <see cref="KernelFunction"/> instances for use with the Semantic Kernel.
    /// <param name="mcpClient">The <see cref="IMcpClient"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// </summary>
    public static async Task<IReadOnlyList<KernelFunction>> MapToFunctionsAsync(this IMcpClient mcpClient, CancellationToken cancellationToken = default)
    {
        var functions = new List<KernelFunction>();
        foreach (var tool in await mcpClient.ListToolsAsync(null, cancellationToken).ConfigureAwait(false))
        {
            // use SemanticKernel AIFunctionExtensions
            functions.Add(tool.AsKernelFunction());
        }

        return functions;
    }
}