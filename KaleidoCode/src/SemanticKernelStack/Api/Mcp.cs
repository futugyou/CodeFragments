using SemanticKernelStack;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using SemanticKernelStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace SemanticKernelStack.Api;

public static class McpEndpoints
{
    private static async Task<McpClient> GetMcpClient(IOptionsMonitor<SemanticKernelOptions> optionsMonitor, string serverType)
    {
        IClientTransport clientTransport;
        var mcpServer = optionsMonitor.CurrentValue.McpServers[serverType] ?? throw new ArgumentException($"Server {serverType} not found");

        if (!string.IsNullOrEmpty(mcpServer.Url))
        {
            clientTransport = new HttpClientTransport(new()
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

        var client = await McpClient.CreateAsync(clientTransport);
        return client;
    }

    public static void UseMcpEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/mcp")
                .WithName("sk mcp");

        agentGroup.MapPost("/tools", Tools).WithName("tools");
        agentGroup.MapPost("/add", Add).WithName("add");
        agentGroup.MapPost("/github/tools", GithubTools).WithName("github_tools");
        agentGroup.MapPost("/github/search_code", GithubSearchCode).WithName("github_search_code");
    }

    static async Task<string[]> Tools([FromServices] IOptionsMonitor<SemanticKernelOptions> optionsMonitor, [FromQuery] string? server)
    {
        server ??= "everything";
        var client = await GetMcpClient(optionsMonitor, server);
        return [.. (await client.ListToolsAsync()).Select(t => t.Name)];
    }

    static async Task<string> Add([FromServices] IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        var client = await GetMcpClient(optionsMonitor, "everything");
        var result = await client.CallToolAsync("add", new Dictionary<string, object?>() { ["a"] = 10, ["b"] = 20 });
        return ((TextContentBlock)result.Content[0]).Text;
    }

    static async Task<string[]> GithubTools([FromServices] IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        var client = await GetMcpClient(optionsMonitor, "github");
        return [.. (await client.ListToolsAsync()).Select(t => t.Name)];
    }

    static async Task<string> GithubSearchCode([FromServices] IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        var client = await GetMcpClient(optionsMonitor, "github");
        var result = await client.CallToolAsync("search_code", new Dictionary<string, object?>() { ["q"] = "modelcontextprotocol" });
        return ((TextContentBlock)result.Content[0]).Text;
    }
}