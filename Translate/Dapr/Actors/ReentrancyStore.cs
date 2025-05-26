using Config;

namespace Actors;


public class ReentrancyStore
{
    private ConcurrentDictionary<string, ReentrancyConfig> reentrancys { get; set; } = new();

    public void Store(string actorType, ReentrancyConfig cfg)
    {
        reentrancys.TryAdd(actorType, cfg);
    }
    public void Delete(string actorType)
    {
        reentrancys.Remove(actorType, out _);
    }

    public (ReentrancyConfig?, bool) Load(string actorType)
    {
        return reentrancys.TryGetValue(actorType, out var cfg) ? (cfg, true) : (null, false);
    }
}