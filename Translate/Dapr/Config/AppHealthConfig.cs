namespace Config;

public class AppHealthConfig
{
    public TimeSpan ProbeInterval { get; set; }
    public TimeSpan ProbeTimeout { get; set; }
    public bool ProbeOnly { get; set; }
    public int Threshold { get; set; }
}