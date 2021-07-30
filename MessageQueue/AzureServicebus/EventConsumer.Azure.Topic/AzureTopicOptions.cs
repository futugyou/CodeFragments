using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventConsumer.Azure.Topic
{
    public class AzureTopicOptions
    {
        public string SubscriptionName { get; set; }
        public string TopicName { get; set; }
        public string ConnectionString { get; set; }
    }
}
