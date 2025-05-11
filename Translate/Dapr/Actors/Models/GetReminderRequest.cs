
namespace Actors.Models;

public class GetReminderRequest
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
