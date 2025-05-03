using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using WellKnownTypes = Google.Protobuf.WellKnownTypes;

namespace Actors;


public class Reminder
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("data")]
    public WellKnownTypes.Any Data { get; set; }
    [JsonPropertyName("period")]
    public ReminderPeriod Period { get; set; }
    [JsonPropertyName("registeredTime")]
    public DateTime RegisteredTime { get; set; }
    [JsonPropertyName("dueTime")]
    public string DueTime { get; set; }
    [JsonPropertyName("expirationTime")]
    public DateTime ExpirationTime { get; set; }
    [JsonPropertyName("callback")]
    public string Callback { get; set; }
    [JsonIgnore]
    public bool IsTimer { get; set; }
    [JsonIgnore]
    public bool IsRemote { get; set; }
    [JsonIgnore]
    public bool SkipLock { get; set; }

    public string ActorKey() => $"{ActorType}{Const.DaprSeparator}{ActorID}";
    public string Key() => $"{ActorType}{Const.DaprSeparator}{ActorID}{Const.DaprSeparator}{Name}";

    public (DateTime, bool) NextTick()
    {
        var active = DateTime.MinValue == ExpirationTime || RegisteredTime.CompareTo(ExpirationTime) < 0;
        return (RegisteredTime, active);
    }

    public bool HasRepeats() => Period.HasRepeats();

    public int RepeatsLeft() => Period.Repeats;

    public bool TickExecuted()
    {
        if (Period.Repeats > 0)
        {
            Period.Repeats--;
        }

        if (!HasRepeats())
        {
            return true;
        }

        RegisteredTime = Period.GetFollowing(RegisteredTime);

        return false;
    }

    public DateTime ScheduledTime() => RegisteredTime;

    override public string ToString()
    {
        var hasData = Data != null;
        var dueTime = "null";
        if (DateTime.MinValue != RegisteredTime)
        {
            dueTime = "'" + RegisteredTime.ToString("yyyy-MM-ddTHH:mm:ssZ") + "'";
        }

        var expirationTime = "null";
        if (DateTime.MinValue != ExpirationTime)
        {
            expirationTime = "'" + ExpirationTime.ToString("yyyy-MM-ddTHH:mm:ssZ") + "'";
        }

        var period = Period.ToString();

        if (period == "")
        {
            period = "null";
        }
        else
        {
            period = "'" + period + "'";

        }

        return $"name='{Name}' hasData={hasData} period={period} dueTime={dueTime} expirationTime={expirationTime}";
    }

    public bool RequiresUpdating(Reminder newReminder)
    {
        if (ActorID != newReminder.ActorID || ActorType != newReminder.ActorType || Name != newReminder.Name)
        {
            return false;
        }

        return DueTime != newReminder.DueTime || Period != newReminder.Period ||
        newReminder.ExpirationTime != DateTime.MinValue || (ExpirationTime != DateTime.MinValue && newReminder.ExpirationTime == DateTime.MinValue) ||
       (Data != null && !Data.Equals(newReminder.Data));
    }
}