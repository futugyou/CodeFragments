namespace Actors.Models;

public class RegisterActorTypeOptions
{
    public ActorHostOptions HostOptions { get; set; }
    public List<ActorTypeFactory> Factories { get; set; }
}
