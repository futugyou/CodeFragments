using Config;

namespace Actors.Models;

public class RegisterActorTypeOptions
{
    public ActorHostOptions HostOptions { get; set; }
    public List<ActorTypeFactory> Factories { get; set; }
}

public class ActorHostOptions
{
    public TimeSpan DrainOngoingCallTimeout { get; set; }
    public bool DrainRebalancedActors { get; set; }
    public Dictionary<string, EntityConfig> EntityConfigs { get; set; }
}

public class ActorTypeFactory
{
    public string ActorType { get; set; }
    public ReentrancyConfig Reentrancy { get; set; }
    public ActorTargetsFactory Factory { get; set; }
}
