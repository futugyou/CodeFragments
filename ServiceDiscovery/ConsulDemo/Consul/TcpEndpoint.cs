using System.Net;

namespace ConsulDemo.Consul
{
    public class TcpEndpoint
    {
        public string Address { get; set; }

        public int Port { get; set; }

        public IPEndPoint ToIPEndPoint() => new IPEndPoint(IPAddress.Parse(this.Address), this.Port);
    }
}