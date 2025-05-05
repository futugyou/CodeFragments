namespace Config;

public class AppConnectionConfig
{
    public string ChannelAddress { get; set; }
    public AppHealthConfig HealthCheck { get; set; }
    public string HealthCheckHTTPPath { get; set; }
    public int MaxConcurrency { get; set; }
    public int Port { get; set; }
    public Protocol Protocol { get; set; }
}
