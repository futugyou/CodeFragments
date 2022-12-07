using System.Diagnostics;

namespace EventClient;

public class EventSourceEx6
{
    public static void ThisEventSource()
    {
        var source = new TraceSource("message", SourceLevels.All);
        source.Listeners.Add(new ConsoleListener());
        var eventTypes = (TraceEventType[])Enum.GetVlues(typeof(TraceEventType));
        var eventId = 1;
        Array.ForEach(eventTypes, it => source.TraceEvent(it, eventId++, $"this is a {it} message"));
    }
}

public class ConsoleListener : TraceListener
{
    public override void Write(string? message) => Console.Write(message);
    public override void WriteLine(string? message) => Console.WriteLine(message);
}