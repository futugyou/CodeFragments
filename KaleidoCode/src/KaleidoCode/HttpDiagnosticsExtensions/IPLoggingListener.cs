using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;

namespace KaleidoCode.HttpDiagnosticsExtensions;

public sealed class IPLoggingListener : EventListener
{
    public static readonly IPLoggingListener IPLoggingListenerInstance = new();
    private static readonly ConcurrentDictionary<long, string> s_connection2Endpoint = new ConcurrentDictionary<long, string>();

    // EventId corresponds to [Event(eventId)] attribute argument and the payload indices correspond to the event method argument order.

    // See: https://github.com/dotnet/runtime/blob/a6e4834d53ac591a4b3d4a213a8928ad685f7ad8/src/libraries/System.Net.Http/src/System/Net/Http/HttpTelemetry.cs#L100-L101
    private const int ConnectionEstablished_EventId = 4;
    private const int ConnectionEstablished_ConnectionIdIndex = 2;
    private const int ConnectionEstablished_RemoteAddressIndex = 6;

    // See: https://github.com/dotnet/runtime/blob/a6e4834d53ac591a4b3d4a213a8928ad685f7ad8/src/libraries/System.Net.Http/src/System/Net/Http/HttpTelemetry.cs#L106-L107
    private const int ConnectionClosed_EventId = 5;
    private const int ConnectionClosed_ConnectionIdIndex = 2;

    // See: https://github.com/dotnet/runtime/blob/a6e4834d53ac591a4b3d4a213a8928ad685f7ad8/src/libraries/System.Net.Http/src/System/Net/Http/HttpTelemetry.cs#L118-L119
    private const int RequestHeadersStart_EventId = 7;
    private const int RequestHeadersStart_ConnectionIdIndex = 0;

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "System.Net.Http")
        {
            EnableEvents(eventSource, EventLevel.LogAlways);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        ReadOnlyCollection<object?>? payload = eventData.Payload;
        if (payload == null) return;

        switch (eventData.EventId)
        {
            case ConnectionEstablished_EventId:
                // Remember the connection data.
                long connectionId = (long)payload[ConnectionEstablished_ConnectionIdIndex]!;
                string? remoteAddress = (string?)payload[ConnectionEstablished_RemoteAddressIndex];
                if (remoteAddress != null)
                {
                    Console.WriteLine($"Connection {connectionId} established to {remoteAddress}");
                    s_connection2Endpoint.TryAdd(connectionId, remoteAddress);
                }
                break;
            case ConnectionClosed_EventId:
                connectionId = (long)payload[ConnectionClosed_ConnectionIdIndex]!;
                s_connection2Endpoint.TryRemove(connectionId, out _);
                break;
            case RequestHeadersStart_EventId:
                // Populate the async local RequestInfo with data from "ConnectionEstablished" event.
                connectionId = (long)payload[RequestHeadersStart_ConnectionIdIndex]!;
                if (s_connection2Endpoint.TryGetValue(connectionId, out remoteAddress))
                {
                    RequestInfo.Current.RemoteAddress = remoteAddress;
                    RequestInfo.Current.ConnectionId = connectionId;
                }
                break;
        }
    }
}