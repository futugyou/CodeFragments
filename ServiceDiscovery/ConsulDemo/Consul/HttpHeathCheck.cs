namespace ConsulDemo.Consul
{
    public class HttpHeathCheck
    {
        public string Host { get; set; }
        public string Path { get; set; } = "/healthcheck";
        public int TimeOunt { get; set; } = 10;
        public int Interval { get; set; } = 10;
    }
}