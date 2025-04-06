using AspnetcoreEx.Elasticsearch;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;

namespace AspnetcoreEx.Controllers;

[ApiController]
[Route("api/mcp")]
[Route("[controller]")]
public class McpClientController : ControllerBase
{
    [Route("tools")]
    [HttpPost]
    public async Task<string[]> Tools()
    {
        //  var client = await McpClientFactory.CreateAsync(new()
        // {
        //     Id = "everything",
        //     Name = "Everything",
        //     TransportType = TransportTypes.Sse,
        //     Location = "https://localhost:5000/sse"
        // });
        var client = await McpClientFactory.CreateAsync(new()
        {
            Id = "everything",
            Name = "Everything",
            TransportType = TransportTypes.StdIo,
            TransportOptions = new()
            {
                ["command"] = "npx",
                ["arguments"] = "-y @modelcontextprotocol/server-everything",
            }
        });

        return (await client.ListToolsAsync()).Select(t => t.Name).ToArray();
    }

    [Route("add")]
    [HttpPost]
    public async Task<string> Add()
    {
        var client = await McpClientFactory.CreateAsync(new()
        {
            Id = "everything",
            Name = "Everything",
            TransportType = TransportTypes.StdIo,
            TransportOptions = new()
            {
                ["command"] = "npx",
                ["arguments"] = "-y @modelcontextprotocol/server-everything",
            }
        });
        var result = await client.CallToolAsync("add", new Dictionary<string, object?>() { ["a"] = 10, ["b"] = 20 });
        return result.Content.First().Text!;
    }
}