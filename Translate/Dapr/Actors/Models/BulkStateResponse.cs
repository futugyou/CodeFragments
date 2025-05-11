
namespace Actors.Models;

public class BulkStateResponse
{
    [JsonPropertyName("data")]
    public Dictionary<string, byte[]> Data { get; set; }
}
