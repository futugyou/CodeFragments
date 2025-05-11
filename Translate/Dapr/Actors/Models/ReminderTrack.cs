namespace Actors.Models;

public class ReminderTrack
{
    [JsonPropertyName("lastFiredTime")]
    public DateTime LastFiredTime { get; set; }
    [JsonPropertyName("repetitionLeft")]
    public int RepetitionLeft { get; set; }
    [JsonPropertyName("ETag")]
    public string ETag { get; set; }
}
