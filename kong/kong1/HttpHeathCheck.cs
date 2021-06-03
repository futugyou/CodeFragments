using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kong1
{
    public class HttpHeathCheck
    {
        /// <summary>eg:/healthcheck default:/healthcheck</summary>
        public string Path { get; set; } = "/healthcheck";

        /// <summary>
        /// 10秒超时 eg:10(Second) default:10 （TimeOunt是 DeregisterCriticalServiceAfter时间）
        /// </summary>
        public int TimeOunt { get; set; } = 10;

        /// <summary>检测间隔 eg:10(Second) default:10</summary>
        public int Interval { get; set; } = 10;
    }
}
