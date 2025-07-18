namespace AspnetcoreEx.KernelService;

public class SemanticKernelOptions
{
    public ModelConfig TextCompletion { get; set; } = new();
    public ModelConfig Embedding { get; set; } = new();
    public ModelConfig Image { get; set; } = new();
    public JiraConfig Jira { get; set; } = new();
    public KernelMemoryConfig KernelMemory { get; set; } = new();
    public WebSearchConfig WebSearch { get; set; } = new();
    public bool UseMemoryVectorStore { get; set; } = true;
    // for mcp server
    public Dictionary<string, McpServer> McpServers { get; set; } = [];
}

public class WebSearchConfig
{
    public string BingApiKey { get; set; } = "";
    public string GoogleApiKey { get; set; } = "";
    public string GoogleSearchEngineId { get; set; } = "";
}

public class ModelConfig
{
    public string ModelId { get; set; } = "openai/gpt-4.1";
    public string Provider { get; set; } = "openai";// e.g. "openai", "google", "azure"
    public string ApiKey { get; set; } = "";
    public string Endpoint { get; set; } = "https://api.openai.com";// e.g. https://api.openai.com, https://models.github.ai/inference, https://generativelanguage.googleapis.com/v1beta/openai, 
    public int Dimensions { get; set; } = 1536; // default for OpenAI
}

public class JiraConfig
{
    public string JiraAddress { get; set; }
    public string JiraEmailAddress { get; set; }
    public string JiraApiKey { get; set; }
}

public class KernelMemoryConfig
{
    public string Endpoint { get; set; }
    public string ApiKey { get; set; }
    public string VectorStoreName { get; set; } = "default-vector-store";
}

public class McpServer
{
    public string Command { get; set; }
    public string Url { get; set; }
    public string[] Args { get; set; } = [];
    public Dictionary<string, string?> Env { get; set; } = new();
}
