namespace Hangfire.Cap;

public class CapJobItem
{
    public int RetryTimes { get; set; }
    public string Cron { get; set; }
    public string RecurringJobIdentifier { get; set; }
    public string JobName { get; set; }
    public string RetryDelaysInSeconds { get; set; }
    public string QueueName { get; set; } = "default";
    public string CapEventName { get; set; }
    public object Data { get; set; }
    // hour
    public int JobExpirationTimeout { get; set; }
}