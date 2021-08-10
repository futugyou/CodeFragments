using System.ComponentModel.DataAnnotations;

namespace ConsulDemo.Consul
{
    public class ConsulOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string ServerName { get; set; }
        public string Token { get; set; }
        public TcpEndpoint TcpEndPoint { get; set; }
        public HttpHeathCheck HttpHeathCheck { get; set; }
        public string[] Tags { get; set; }
    }
}