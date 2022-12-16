using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventSourceDemo;

public class HostEventSourceListener : EventListener
    {
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "Microsoft.AspNetCore.Hosting")
            {
                EnableEvents(eventSource, EventLevel.LogAlways);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventSource.Name == "Microsoft.AspNetCore.Hosting")
            {
                Console.WriteLine(eventData.EventName);
                for (int i = 0; i < eventData.PayloadNames?.Count; i++)
                {
                    Console.WriteLine($"\t{eventData.PayloadNames[i]} = {eventData.Payload?[i]}");
                }
            }
        }
    }