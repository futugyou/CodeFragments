using System.Text.Json;
using System.Text.Json.Serialization;

namespace Config;

public class EntityConfig
{
    [JsonPropertyName("entities")]
    public string[] Entities { get; set; }
    // Duration. example: "1h".
    [JsonPropertyName("actorIdleTimeout")]
    public string ActorIdleTimeout { get; set; }
    // Duration. example: "30s".
    [JsonPropertyName("drainOngoingCallTimeout")]
    public string DrainOngoingCallTimeout { get; set; }
    [JsonPropertyName("drainRebalancedActors")]
    public bool DrainRebalancedActors { get; set; }
    [JsonPropertyName("reentrancy")]
    public ReentrancyConfig Reentrancy { get; set; }
    [JsonPropertyName("remindersStoragePartitions")]
    public int RemindersStoragePartitions { get; set; }
}
