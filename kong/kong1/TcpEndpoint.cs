using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
namespace kong1
{
    public class TcpEndpoint
    {
        public string Address { get; set; }

        public int Port { get; set; }

        public IPEndPoint ToIPEndPoint() => new IPEndPoint(IPAddress.Parse(this.Address), this.Port);
    }
}
