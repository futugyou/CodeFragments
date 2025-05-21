namespace Config;

public class ReentrancyConfig
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    [JsonPropertyName("maxStackDepth")]
    public int MaxStackDepth { get; set; }
}
