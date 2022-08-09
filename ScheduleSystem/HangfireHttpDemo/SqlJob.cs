namespace HangfireHttpDemo;

public class SqlJob
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpireAt { get; set; }
    public DateTime? FetchedAt { get; set; }
    public string StateName { get; set; } = "";
    public string StateReason { get; set; } = "";
    public DateTime? StateChanged { get; set; }
    public string Arguments { get; set; } = "";
    public string InvocationData { get; set; } = "";
}
