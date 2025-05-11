
namespace Actors.Models;

public class LookupActorRequest
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("noCache")]
    public bool NoCache { get; set; }
    public string ActorKey() => $"{ActorType}/{ActorID}";
}
