
using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace KaleidoCode.KernelService;

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"hello {message}";

    [McpServerTool, Description("Get the current time for a city")]
    public static string GetCurrentTime(string city) =>
            $"It is {DateTime.Now.Hour}:{DateTime.Now.Minute} in {city}.";

    [McpServerTool(Name = "SummarizeContentFromUrl"), Description("Summarizes content downloaded from a specific URI")]
    public static async Task<string> SummarizeDownloadedContent(
        McpServer thisServer,
        HttpClient httpClient,
        [Description("The url from which to download the content to summarize")] string url,
        CancellationToken cancellationToken)
    {
        string content = await httpClient.GetStringAsync(url, cancellationToken);

        ChatMessage[] messages =
        [
            new(ChatRole.User, "Briefly summarize the following downloaded content:"),
        new(ChatRole.User, content),
    ];

        ChatOptions options = new()
        {
            MaxOutputTokens = 256,
            Temperature = 0.3f,
        };

        var client = thisServer.AsSamplingChatClient();

        return $"Summary: {await client.GetResponseAsync(messages, options, cancellationToken)}";
    }
}

[McpServerPromptType]
public static class MyPrompts
{
    [McpServerPrompt, Description("Creates a prompt to summarize the provided message.")]
    public static ChatMessage Summarize([Description("The content to summarize")] string content) =>
        new(ChatRole.User, $"Please summarize this content into a single sentence: {content}");
}

[McpServerToolType]
public static class WeatherTools
{
    [McpServerTool, Description("Get weather alerts for a US state.")]
    public static async Task<string> GetAlerts(
        HttpClient client,
        [Description("The US state to get alerts for.")] string state)
    {
        var jsonElement = await client.GetFromJsonAsync<JsonElement>($"/alerts/active/area/{state}");
        var alerts = jsonElement.GetProperty("features").EnumerateArray();
        if (!alerts.Any())
        {
            return "No active alerts for this state.";
        }
        return string.Join("\n-\n", alerts.Select(alert =>
        {
            JsonElement properties = alert.GetProperty("properties");
            return $"""
                Event: {properties.GetProperty("event").GetString()}
                """;
        }));
    }
}