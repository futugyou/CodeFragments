
namespace Actors.Models;

public class GetStateRequest
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("key")]
    public string Key { get; set; }

    public string ActorKey() => $"{ActorType}{Const.DaprSeparator}{ActorID}";
}
