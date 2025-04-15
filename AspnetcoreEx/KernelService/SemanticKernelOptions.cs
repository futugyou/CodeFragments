namespace AspnetcoreEx.KernelService;

public class SemanticKernelOptions
{
    public string Key { get; set; }
    public string GoogleApikey { get; set; }
    public string GoogleEngine { get; set; }
    public string GithubToken { get; set; }
    public string JiraAddress { get; set; }
    public string JiraEmailAddress { get; set; }
    public string JiraApiKey { get; set; }
    public string QdrantHost { get; set; }
    public string QdrantKey { get; set; }
    public int QdrantPort { get; set; }
    public int QdrantVectorSize { get; set; }
    public string Embedding { get; set; }
    public string TextCompletion { get; set; }
    public string Image { get; set; }
    public string ChatModel { get; set; }
    // for azure
    public string Endpoint { get; set; }
    public string KernelMemoryEndpoint { get; set; }
    public string KernelMemoryApiKey { get; set; }
    // for mcp server
    public Dictionary<string, McpServer> McpServers { get; set; } = [];
}

public class McpServer
{
    public string Command { get; set; }
    public string Url { get; set; }
    public string[] Args { get; set; } = [];
    public Dictionary<string, string> Env { get; set; } = new();
}
