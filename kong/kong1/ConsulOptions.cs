using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace kong1
{
    public class ConsulOptions
    {
        /// <summary>consul服务期，eg:http://127.0.0.1:8500"</summary>
        [Required]
        public string HttpEndPoint { get; set; }

        /// <summary>consul token</summary>
        public string Token { get; set; }

        /// <summary>暂未启用，兼容配置</summary>
        public TcpEndpoint TcpEndPoint { get; set; }

        /// <summary>心跳检查</summary>
        public HttpHeathCheck HttpHeathCheck { get; set; }

        /// <summary>服务tag</summary>
        public string[] Tags { get; set; }
    }
}
