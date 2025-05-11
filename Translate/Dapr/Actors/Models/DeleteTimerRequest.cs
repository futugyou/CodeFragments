
namespace Actors.Models;

public class DeleteTimerRequest
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }

    public string ActorKey() => $"{ActorType}{Const.DaprSeparator}{ActorID}";
    public string Key() => $"{ActorType}{Const.DaprSeparator}{ActorID}{Const.DaprSeparator}{Name}";
}
