
namespace Config;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Feature
{
    [JsonStringEnumMemberName("ActorStateTTL")]
    ActorStateTTL,
    [JsonStringEnumMemberName("HotReload")]
    HotReload,
    [JsonStringEnumMemberName("SchedulerReminders")]
    SchedulerReminders,
}
