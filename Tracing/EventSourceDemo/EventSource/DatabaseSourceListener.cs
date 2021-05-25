using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventSourceDemo
{
    public class DatabaseSourceListener : EventListener
    {
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "System.Threading.Tasks.TplEventSource")
            {
                EnableEvents(eventSource, EventLevel.Informational, (EventKeywords)0x08);
            }
            if (eventSource.Name == "ddd_sqllit_db")
            {
                EnableEvents(eventSource, EventLevel.LogAlways);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventSource.Name == "ddd_sqllit_db")
            {
                var tmpColor = Console.BackgroundColor;
                Console.BackgroundColor = ConsoleColor.Green;
                Console.WriteLine(JsonSerializer.Serialize(eventData));
                Console.BackgroundColor = tmpColor;
            }
        }
    }
}
