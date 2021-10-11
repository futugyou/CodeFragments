namespace AspnetcoreEx.RedisExtensions;

public class RedisOptions
{
    public string Host { get; set; }
    public int DatabaseNumber { get; set; }
    public int MaxStreamReadCount { get; set; }
}