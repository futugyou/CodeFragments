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

    class FoobarEventListener : EventListener{}
    public static void TraceSourceLog()
    {
        var listener = new FoobarEventListener();
        listener.EventSourceCreated += (sender, args) =>
        {
            if (args.EventSource?.Name == "Microsoft-Extensions-Logging")
            {
                listener.EnableEvents(args.EventSource, EventLevel.LogAlways);
            }
        };
        listener.EventWritten += (sender, args) =>
        {
            var payload = args.Payload;
            var payloadNames = args.PayloadNames;
            if (args.EventName == "FotmattedMessage" && payload != null && payloadNames != null)
            {
                var level = payloadNames.IndexOf("Level");
                var category = payloadNames.IndexOf("LoggerName");
                var eventid = payloadNames.IndexOf("EventId");
                var message = payloadNames.IndexOf("FormattedMessage");
                Console.WriteLine($"{level},{category},{eventid},{message}");
            }
        };
        var logger = new ServiceCollection()
        .AddLogging(builder => 
            builder.AddTraceSource(new SourceSwitch("default","All"), new DefaultTraceListener{LogFileName = "trace.log"})
            .AddEventSourceLogger()
        )
        .BuildServiceProvider()
        .GetRequiredService<ILogger<Program>>();
        var levels = (LogLevel[])Enum.GetValues(typeof(LogLevel));
        levels = levels.Where(it => it != LogLevel.None).ToArray();
        var eventid = 1;
        Array.ForEach(levels, level => logger.Log(level, eventid++, $"this is a {level} message"));
    }
}

public class ConsoleListener : TraceListener
{
    public override void Write(string? message) => Console.Write(message);
    public override void WriteLine(string? message) => Console.WriteLine(message);
}