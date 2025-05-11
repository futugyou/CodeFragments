using Config;

namespace Actors.Models;

public class ActorTypeFactory
{
    public string ActorType { get; set; }
    public ReentrancyConfig Reentrancy { get; set; }
    public ActorTargetsFactory Factory { get; set; }
}
