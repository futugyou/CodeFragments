
using System.Text.Json.Serialization;

namespace SemanticKernelStack.Duckduckgo;

public class DuckSearchResult
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("snippet")]
    public string Snippet { get; set; }
    [JsonPropertyName("iconUrl")]
    public string IconUrl { get; set; }
    [JsonPropertyName("displayUrl")]
    public string DisplayUrl { get; set; }
}