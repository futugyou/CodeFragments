using Config;

namespace Actors.Models;

public class EntityConfig
{
    public string[] Entities { get; set; }
    public TimeSpan ActorIdleTimeout { get; set; }
    public TimeSpan DrainOngoingCallTimeout { get; set; }
    public bool DrainRebalancedActors { get; set; }
    public ReentrancyConfig ReentrancyConfig { get; set; }
    public int RemindersStoragePartitions { get; set; } = 1;

    public EntityConfig(Config.EntityConfig config)
    {
        Entities = config.Entities;
        ActorIdleTimeout = Const.DefaultIdleTimeout;
        DrainOngoingCallTimeout = Const.DefaultOngoingCallTimeout;
        DrainRebalancedActors = config.DrainRebalancedActors;
        ReentrancyConfig = config.Reentrancy;
        RemindersStoragePartitions = config.RemindersStoragePartitions;

        if (config.ActorIdleTimeout != null)
        {
            ActorIdleTimeout = TimeSpan.Parse(config.ActorIdleTimeout);
        }

        if (config.Reentrancy.MaxStackDepth != 0)
        {
            ReentrancyConfig.MaxStackDepth = config.Reentrancy.MaxStackDepth;
        }
    }
}