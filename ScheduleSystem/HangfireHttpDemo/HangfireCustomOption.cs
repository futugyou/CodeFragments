namespace HangfireHttpDemo;

public class HangfireCustomOption
{
    public static readonly string ConfigSection = "HangfireOption";

    public string ConnectionString { get; set; } = "";
    public int JobExpirationTimeoutDay { get; set; } = 30;
    public string DefaultQueueName { get; set; } = "default";
    public string[] AllQueues { get; set; } = new string[] { "default" };
    public string ServiceName { get; set; } = "ScheduleCenter";
    public int WorkerCount { get; set; } = 20;
}
