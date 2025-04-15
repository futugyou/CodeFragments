using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol;

namespace AspnetcoreEx.Controllers;

[ApiController]
[Route("api/mcp")]
public class McpClientController : ControllerBase
{
    [Route("tools")]
    [HttpPost]
    public async Task<string[]> Tools([FromQuery] string? type)
    {
        IClientTransport clientTransport;
        if (type == "sse")
        {
            clientTransport = new SseClientTransport(new()
            {
                Name = "everything",
                Endpoint = new Uri("https://localhost:5000/sse"),
                ConnectionTimeout = TimeSpan.FromSeconds(30),
                MaxReconnectAttempts = 3,
                ReconnectDelay = TimeSpan.FromSeconds(5),
            });
        }
        else
        {
            clientTransport = new StdioClientTransport(new()
            {
                Name = "everything",
                Command = "npx",
                Arguments = ["-y @modelcontextprotocol/server-everything"],
            });
        }

        var client = await McpClientFactory.CreateAsync(clientTransport);

        return [.. (await client.ListToolsAsync()).Select(t => t.Name)];
    }

    [Route("add")]
    [HttpPost]
    public async Task<string> Add()
    {
        var clientTransport = new StdioClientTransport(new()
        {
            Name = "everything",
            Command = "npx",
            Arguments = ["-y @modelcontextprotocol/server-everything"],
        });
        var client = await McpClientFactory.CreateAsync(clientTransport);
        var result = await client.CallToolAsync("add", new Dictionary<string, object?>() { ["a"] = 10, ["b"] = 20 });
        return result.Content.First().Text!;
    }

    [Route("github/tools")]
    [HttpPost]
    public async Task<string[]> GithubTools()
    {
        var clientTransport = new StdioClientTransport(new()
        {
            Name = "everything",
            Command = "npx",
            Arguments = ["-y mcprouter @modelcontextprotocol/github"],
            EnvironmentVariables = new Dictionary<string, string>
            {
                ["SERVER_KEY"] = "*"
            }
        });
        var client = await McpClientFactory.CreateAsync(clientTransport);
        return [.. (await client.ListToolsAsync()).Select(t => t.Name)];
    }


    [Route("github/search_code")]
    [HttpPost]
    public async Task<string> GithubSearchCode()
    {
        var clientTransport = new StdioClientTransport(new()
        {
            Name = "everything",
            Command = "npx",
            Arguments = ["-y mcprouter @modelcontextprotocol/github"],
            EnvironmentVariables = new Dictionary<string, string>
            {
                ["SERVER_KEY"] = "*"
            }
        });
        var client = await McpClientFactory.CreateAsync(clientTransport);
        var result = await client.CallToolAsync("search_code", new Dictionary<string, object?>() { ["q"] = "modelcontextprotocol" });
        return result.Content.First().Text!;
    }
}