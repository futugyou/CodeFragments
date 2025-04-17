using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol;
using AspnetcoreEx.KernelService;

namespace AspnetcoreEx.Controllers;

[ApiController]
[Route("api/mcp")]
public class McpClientController : ControllerBase
{
    public McpClientController(IOptionsMonitor<SemanticKernelOptions> optionsMonitor, ILogger<McpClientController> logger)
    {
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    private readonly IOptionsMonitor<SemanticKernelOptions> _optionsMonitor;
    private readonly ILogger<McpClientController> _logger;

    private async Task<IMcpClient> GetMcpClient(string serverType)
    {
        IClientTransport clientTransport;
        var mcpServer = _optionsMonitor.CurrentValue.McpServers[serverType] ?? throw new ArgumentException($"Server {serverType} not found");

        if (!string.IsNullOrEmpty(mcpServer.Url))
        {
            clientTransport = new SseClientTransport(new()
            {
                Name = serverType,
                Endpoint = new Uri(mcpServer.Url),
                ConnectionTimeout = TimeSpan.FromSeconds(30),
            });
        }
        else
        {
            clientTransport = new StdioClientTransport(new()
            {
                Name = serverType,
                Command = mcpServer.Command,
                Arguments = mcpServer.Args,
                EnvironmentVariables = mcpServer.Env,
            });
        }

        var client = await McpClientFactory.CreateAsync(clientTransport);
        return client;
    }

    [Route("tools")]
    [HttpPost]
    public async Task<string[]> Tools([FromQuery] string? server)
    {
        server ??= "everything";
        var client = await GetMcpClient(server);
        return [.. (await client.ListToolsAsync()).Select(t => t.Name)];
    }


    [Route("add")]
    [HttpPost]
    public async Task<string> Add()
    {
        var client = await GetMcpClient("everything");
        var result = await client.CallToolAsync("add", new Dictionary<string, object?>() { ["a"] = 10, ["b"] = 20 });
        return result.Content.First().Text!;
    }

    [Route("github/tools")]
    [HttpPost]
    public async Task<string[]> GithubTools()
    {
        var client = await GetMcpClient("github");
        return [.. (await client.ListToolsAsync()).Select(t => t.Name)];
    }


    [Route("github/search_code")]
    [HttpPost]
    public async Task<string> GithubSearchCode()
    {
        var client = await GetMcpClient("github");
        var result = await client.CallToolAsync("search_code", new Dictionary<string, object?>() { ["q"] = "modelcontextprotocol" });
        return result.Content.First().Text!;
    }
}