
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

public class LookupActorResponse
{
    [JsonPropertyName("address")]
    public string Address { get; set; }
    [JsonPropertyName("appID")]
    public string AppID { get; set; }
    [JsonPropertyName("local")]
    public bool Local { get; set; }
}
