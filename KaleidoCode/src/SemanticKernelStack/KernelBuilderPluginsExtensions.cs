
using System.Collections.Concurrent;
using  SemanticKernelStack.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;

namespace SemanticKernelStack;

[Experimental("SKEXP0010")]
public static class KernelBuilderPluginsExtensions
{
    private static readonly ConcurrentDictionary<string, IKernelBuilderPlugins> SseMap = new();

    /// <summary>
    /// Creates a Model Content Protocol plugin from an SSE server that contains the specified MCP functions and adds it into the plugin collection.
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="serverName"></param>
    /// <param name="cancellationToken">The optional <see cref="CancellationToken"/>.</param>
    /// <param name="plugins"></param>
    /// <returns>A <see cref="KernelPlugin"/> containing the functions.</returns>
    public static async Task<IKernelBuilderPlugins> AddMcpFunctionsFromSseServerAsync(this IKernelBuilderPlugins plugins,
        string name, McpServerConfig server, CancellationToken cancellationToken = default)
    {
        var key = PluginNameSanitizer.ToSafePluginName(name);

        if (SseMap.TryGetValue(key, out var sseKernelPlugin))
        {
            return sseKernelPlugin;
        }

        var mcpClient = await GetClientAsync(name, server, cancellationToken).ConfigureAwait(false);
        var functions = await mcpClient.MapToFunctionsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        cancellationToken.Register(() => mcpClient.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult());

        sseKernelPlugin = plugins.AddFromFunctions(key, functions);
        return SseMap[key] = sseKernelPlugin;
    }

    private static async Task<McpClient> GetClientAsync(string name, McpServerConfig mcpServer, CancellationToken cancellationToken)
    {
        IClientTransport clientTransport;
        if (!string.IsNullOrEmpty(mcpServer.Url))
        {
            clientTransport = new HttpClientTransport(new()
            {
                Name = name,
                Endpoint = new Uri(mcpServer.Url),
                ConnectionTimeout = TimeSpan.FromSeconds(30),
            });
        }
        else
        {
            clientTransport = new StdioClientTransport(new()
            {
                Name = name,
                Command = mcpServer.Command,
                Arguments = mcpServer.Args,
                EnvironmentVariables = mcpServer.Env,
            });
        }

        var transportType = clientTransport.GetType().Name;
        McpClientOptions options = new()
        {
            ClientInfo = new()
            {
                Name = $"{name} {transportType}Client",
                Version = "1.0.0"
            }
        };

        return await McpClient.CreateAsync(clientTransport, options, NullLoggerFactory.Instance, cancellationToken);
    }
}