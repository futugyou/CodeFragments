using System.Diagnostics;

namespace EventClient;

public class EventSourceEx6
{
    public static void ThisEventSource()
    {
        var source = new TraceSource("message", SourceLevels.All);
        source.Listeners.Clear();
        source.Listeners.Add(new DefaultTraceListener());
        source.Listeners.Add(new TextWriterTraceListener());
        source.Listeners.Add(new DelimitedListTraceListener("./de.log"));
        source.Listeners.Add(new ConsoleListener());
        var eventTypes = (TraceEventType[])Enum.GetVlues(typeof(TraceEventType));
        var eventId = 1;
        Array.ForEach(eventTypes, it => source.TraceEvent(it, eventId++, $"this is a {it} message"));
    }

    public static void UseDelimitedListTraceListener()
    {
        var filename = "trace.csv";
        File.AppendAllText(filename, @$"SourceName,EventType,EventId,Message,N/A,ProcessId,LogicalOperationStack,ThreadId,DateTime,Timestamp,{Environment.NewLine}");
        using var fileStream = new FileStream(filename, FileMode.Append);
        TraceOptions options = TraceOptions.Callstack |TraceOptions.DateTime |TraceOptions.LogicalOperationStack |TraceOptions.ProcessId |TraceOptions.ThreadId |TraceOptions.Timestamp;
        var listener = new DelimitedListTraceListener(fileStream)
        {
            TraceOutputOptions = options,
            Delimiter = ","
        };
        var source = new TraceSource("foo", SourceLevels.All);
        source.Listeners.Add(listener);
        var eventTypes = (TraceEventType[])Enum.GetVlues(typeof(TraceEventType));
        for (int i = 0; i < eventTypes.Length; i++)
        {
            var eventType = eventTypes[i];
            var eventId = i+1;
            Trace.CorrelationManager.StartLogicalOperation($"Op{eventId}");
            source.TraceEvent(eventType, eventId, $"this is a {eventType} message");
        }
        source.Flush();
    }
}

public class ConsoleListener : TraceListener
{
    public override void Write(string? message) => Console.Write(message);
    public override void WriteLine(string? message) => Console.WriteLine(message);
}