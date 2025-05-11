namespace Actors.Models;

public class GetBulkStateRequest
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("keys")]
    public string[] Keys { get; set; }

    public string ActorKey() => $"{ActorType}{Const.DaprSeparator}{ActorID}";
}
