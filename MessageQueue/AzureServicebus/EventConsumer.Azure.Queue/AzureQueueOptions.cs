using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageConsumer.Azure.Queue
{
    public class AzureQueueOptions
    {
        public string QueueName { get; set; }
        public string ConnectionString { get; set; }
    }
}
