using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventConsumer.Azure.Queue
{
    internal static class Constants
    {
        public static string PropertyKeyPrefix { get; } = "cloudEvents:";

        public static string SpecVersionPropertyKey { get; } = PropertyKeyPrefix + "specversion";
    }
}
