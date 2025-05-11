
namespace Actors.Models;

public class StateResponse
{
    [JsonPropertyName("data")]
    public byte[] Data { get; set; }
    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; }
}
