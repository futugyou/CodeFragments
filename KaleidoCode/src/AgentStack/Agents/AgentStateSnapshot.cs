
namespace AgentStack.Agents;

public class AgentStateSnapshot
{
    [JsonPropertyName("searches")]
    public List<SearchInfo> Searches { get; set; } = [];
}

public class SearchInfo
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;
    [JsonPropertyName("done")]
    public bool Done { get; set; }
}
