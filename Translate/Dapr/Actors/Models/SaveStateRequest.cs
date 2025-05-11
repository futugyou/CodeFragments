using Google.Protobuf;
using WellKnownTypes = Google.Protobuf.WellKnownTypes;

namespace Actors.Models;

public class SaveStateRequest
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("keys")]
    public string[] Keys { get; set; }
    [JsonPropertyName("value")]
    public WellKnownTypes.Any Value { get; set; }
}
