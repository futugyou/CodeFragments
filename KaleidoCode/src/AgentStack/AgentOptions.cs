
namespace AgentStack;

public class AgentOptions
{
    public ModelConfig TextCompletion { get; set; } = new();
    public ModelConfig Embedding { get; set; } = new();
}

public class ModelConfig
{
    public string ModelId { get; set; } = "openai/gpt-4.1";
    public string Provider { get; set; } = "openai";// e.g. "openai", "google"
    public string ApiKey { get; set; } = "";
    public string Endpoint { get; set; } = "https://api.openai.com";// e.g. https://api.openai.com, https://models.github.ai/inference, https://generativelanguage.googleapis.com/v1beta/openai, 
    public int Dimensions { get; set; } = 1536; // default for OpenAI
}
