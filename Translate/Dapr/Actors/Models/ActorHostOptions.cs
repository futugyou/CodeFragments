namespace Actors.Models;

public class ActorHostOptions
{
    public TimeSpan DrainOngoingCallTimeout { get; set; }
    public bool DrainRebalancedActors { get; set; }
    public Dictionary<string, EntityConfig> EntityConfigs { get; set; }
}
