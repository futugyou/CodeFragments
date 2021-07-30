using CloudNative.CloudEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageConsumer
{
    public interface IEventHandler
    {
        Task HandlerEvent(CloudEvent cloudEvent);
    }
}
