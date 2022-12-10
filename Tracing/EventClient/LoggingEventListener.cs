using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventClient;

public class LoggingEventListener : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "Microsoft-Extensions-Logging")
        {
            EnableEvents(eventSource, EventLevel.LogAlways);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        Console.WriteLine($"Event: {eventData.EventName}");
        var payload = eventData.Payload;
        var payloadNames = eventData.PayloadNames;
        if (payload != null && payloadNames != null)
        {
            for (int index = 0; index<payloadNames.Count;index++)
            {
                var element = payload[index];
                if (element is object[] || element is IDictionary<string, object>)
                {
                    Console.WriteLine($"{payloadNames[index],-16}:{JsonSerializer.Serialize(element)}");
                    continue;
                }
                Console.WriteLine($"{payloadNames[index],-16}:{ payload[index]}");
            }
            Console.WriteLine();
        }
    }
}