using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace kong1
{
    public class ServiceDiscoveryOptions
    {
        /// <summary>true=禁用</summary>
        public bool Disable { get; set; }

        /// <summary>服务名称，大小写字母+下划线，其他的字符没测试，避免使用（.试过有问题）</summary>
        [Required]
        public string ServiceName { get; set; }

        [Required]
        public ConsulOptions Consul { get; set; }
    }
}
