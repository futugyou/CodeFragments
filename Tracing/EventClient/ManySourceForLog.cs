using System.Diagnostics;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

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
        var eventTypes = (TraceEventType[])Enum.GetValues(typeof(TraceEventType));
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
        var eventTypes = (TraceEventType[])Enum.GetValues(typeof(TraceEventType));
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
            builder
            .AddFilter(TraceFilter)
            .AddTraceSource(new SourceSwitch("default","All"), new DefaultTraceListener{LogFileName = "trace.log"})
            .AddEventSourceLogger()
        )
        .BuildServiceProvider()
        .GetRequiredService<ILogger<Program>>();
        var levels = (LogLevel[])Enum.GetValues(typeof(LogLevel));
        levels = levels.Where(it => it != LogLevel.None).ToArray();
        var eventid = 1;
        Array.ForEach(levels, level => logger.Log(level, eventid++, $"this is a {level} message"));
    }

    static bool TraceFilter(string category, LogLevel level)
    {
        return category switch
        {
            // we only have this category
            "Program" => level >= LogLevel.Debug,
            _ => level >= LogLevel.Information,
        };
    }

    public static void LoggerMessageUsecase()
    {
        var template = "this is log {one}, time {two}, message {three}";
        var log = LoggerMessage.Define<string,DateTime,string>(
            logLevel: LogLevel.Trace,
            eventId: 123,
            formatString: template
        );
        var logger = new ServiceCollection()
        .AddLogging(builder => 
            builder
            .SetMinimumLevel(LogLevel.Trace)
            .AddConsole()
        )
        .BuildServiceProvider()
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Program");
        log(logger,"thisisone",DateTime.Now,"thisismessage",null);
    }

    public static void ActivityUseCase()
    {
        var source = new ActivitySource("demo");
        Debug.Assert(source.CreateActivity("bar", ActivityKind.Internal) == null);
        var listener1 = new ActivityListener { ShouldListenTo = MatchAll , Sample = SampleNone };
        ActivitySource.AddActivityListener(listener1);
        Debug.Assert(source.CreateActivity("bar", ActivityKind.Internal) == null);
        
        var listener2 = new ActivityListener { ShouldListenTo = MatchAll , Sample = SamplePropagationData }; 
        ActivitySource.AddActivityListener(listener2);
        var activity = source.CreateActivity("bar", ActivityKind.Internal);
        Debug.Assert(activity?.IsAllDataRequested == false);

        var listener3 = new ActivityListener { ShouldListenTo = MatchAll , Sample = SampleAllData }; 
        ActivitySource.AddActivityListener(listener3);
        activity = source.CreateActivity("bar", ActivityKind.Internal);
        Debug.Assert(activity?.IsAllDataRequested == true);
    }

    static ActivitySamplingResult SampleNone(ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.None;
    static ActivitySamplingResult SamplePropagationData(ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.PropagationData;
    static ActivitySamplingResult SampleAllData(ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData;
    static ActivitySamplingResult Sample(ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData;

    static bool MatchAll(ActivitySource activitySource) => true;

    public static void ActivityUseCase2()
    {
        var logger = new ServiceCollection()
            .AddLogging(builder => builder
                .Configure(options => options.ActivityTrackingOptions = 
                    ActivityTrackingOptions.TraceId 
                    | ActivityTrackingOptions.SpanId 
                    | ActivityTrackingOptions.ParentId)
                .AddConsole()
                .AddSimpleConsole(options => options.IncludeScopes = true))
            .BuildServiceProvider()
            .GetRequiredService<ILogger<Program>>();
        ActivitySource.AddActivityListener(new ActivityListener{ShouldListenTo = _=>true,Sample = Sample});
        var source = new ActivitySource("App");
        using (source.StartActivity("foo"))
        {
            logger.Log(LogLevel.Information, "this is a log written by scope foo");
            using(source.StartActivity("bar"))
            {
                logger.Log(LogLevel.Information, "this is a log written by scope bar");
                using(source.StartActivity("baz"))
                {
                    logger.Log(LogLevel.Information, "this is a log written by scope baz");
                }
            }
        }
        Console.Read();
    }
}

public class ConsoleListener : TraceListener
{
    public override void Write(string? message) => Console.Write(message);
    public override void WriteLine(string? message) => Console.WriteLine(message);
}