
namespace Actors.Models;

public class TransactionalRequest
{
    [JsonPropertyName("operations")]
    public List<TransactionalOperation> Operations { get; set; }
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }

    public string ActorKey() => $"{ActorType}{Const.DaprSeparator}{ActorID}";
}
