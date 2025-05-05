namespace Config;

public class ApplicationConfig
{
    [JsonPropertyName("entities")]
    public string[] Entities { get; set; }
    [JsonPropertyName("actorIdleTimeout")]
    public string ActorIdleTimeout { get; set; }
    [JsonPropertyName("drainOngoingCallTimeout")]
    public string DrainOngoingCallTimeout { get; set; }
    [JsonPropertyName("drainRebalancedActors")]
    public bool DrainRebalancedActors { get; set; }
    [JsonPropertyName("reentrancy")]
    public ReentrancyConfig Reentrancy { get; set; }
    [JsonPropertyName("remindersStoragePartitions")]
    public int RemindersStoragePartitions { get; set; }
    [JsonPropertyName("entitiesConfig")]
    public EntityConfig[] EntityConfigs { get; set; }
}