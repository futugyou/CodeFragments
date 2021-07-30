using CloudNative.CloudEvents;
using EventConsumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consumer
{
    public class FakeEventHandler : IEventHandler
    {
        public Task HandlerEvent(CloudEvent cloudEvent)
        {
            return Task.CompletedTask;
        }
    }
}
