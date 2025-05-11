
namespace Actors.Models;

public class ListRemindersRequest
{
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
}
