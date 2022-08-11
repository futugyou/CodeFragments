using Hangfire.Common;
using Hangfire.States;
using Newtonsoft.Json;

namespace HangfireCapDemo;

public class CapJobErrorState : IState
{
    public static readonly string StateName = "Failed";
    private readonly string _title;

    public CapJobErrorState(string err, string title = "CapJob")
    {
        FailedAt = DateTime.UtcNow;
        Reason = err;
        _title = string.IsNullOrEmpty(title) ? "CapJob" : title;
    }

    [JsonIgnore]
    public DateTime FailedAt { get; set; }

    [JsonIgnore]
    public string Name => StateName;

    public Dictionary<string, string> SerializeData()
    {
        return new Dictionary<string, string>
            {
                { "FailedAt", JobHelper.SerializeDateTime(FailedAt) },
                { "ExceptionType", _title },
                { "ExceptionMessage", Reason },
                { "ExceptionDetails", ""}
            };
    }

    public string Reason { get; set; }


    [JsonIgnore]
    public bool IsFinal => false;
    [JsonIgnore]
    public bool IgnoreJobLoadException => false;
}