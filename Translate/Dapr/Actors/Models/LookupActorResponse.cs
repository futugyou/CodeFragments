
namespace Actors.Models;

public class LookupActorResponse
{
    [JsonPropertyName("address")]
    public string Address { get; set; }
    [JsonPropertyName("appID")]
    public string AppID { get; set; }
    [JsonPropertyName("local")]
    public bool Local { get; set; }
}
