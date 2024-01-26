
using Yarp.Telemetry.Consumption;

namespace AspnetcoreEx.HttpDiagnosticsExtensions;

// list all interfaces in Yarp package
public class YarpTelemetryConsumer : IHttpTelemetryConsumer, INameResolutionTelemetryConsumer, INetSecurityTelemetryConsumer, ISocketsTelemetryConsumer, IKestrelTelemetryConsumer
{
    // time-to-process-a-request-in-aspnet-core-running-kestrel
    private static readonly AsyncLocal<DateTime?> _startTimestamp = new();
    private readonly YarpFullTelemetryOption _option;
    private readonly ILogger<YarpTelemetryConsumer> _logger;
    private record RequestInfo(DateTime StartTimestamp, string RequestId, string Path);

    // measure-the-latency-of-a-net-reverse-proxy
    private static readonly AsyncLocal<RequestInfo> _requestInfo = new();

    public YarpTelemetryConsumer(ILogger<YarpTelemetryConsumer> logger, IOptionsMonitor<YarpFullTelemetryOption> option)
    {
        _logger = logger;
        _option = option.CurrentValue;
    }

    // INameResolutionTelemetryConsumer
    public void OnResolutionStart(DateTime timestamp, string hostNameOrAddress)
    {
        // measure-dns-resolutions-for-a-given-endpoint
        if (hostNameOrAddress.Equals(_option.Hostname, StringComparison.OrdinalIgnoreCase))
        {
            _startTimestamp.Value = timestamp;
        }
    }

    public void OnResolutionStop(DateTime timestamp)
    {
        if (_startTimestamp.Value is { } start)
        {
            Console.WriteLine($"DNS resolution for {_option.Hostname} took {(timestamp - start).TotalMilliseconds} ms");
        }
    }

    // IHttpTelemetryConsumer
    public void OnRequestStart(DateTime timestamp, string scheme, string host, int port, string pathAndQuery, int versionMajor, int versionMinor, HttpVersionPolicy versionPolicy) =>
        RequestState.Current.RequestStart = timestamp;

    public void OnRequestStop(DateTime timestamp) =>
        RequestState.Current.RequestStop = timestamp;

    public void OnRequestHeadersStop(DateTime timestamp)
    {
        RequestState.Current.HeadersSent = timestamp;
        if (_requestInfo.Value is { } requestInfo)
        {
            var elapsed = (timestamp - requestInfo.StartTimestamp).TotalMilliseconds;
            _logger.LogInformation("Internal latency for {requestId} to {path} was {duration} ms", requestInfo.RequestId, requestInfo.Path, elapsed);
        }
    }

    public void OnResponseHeadersStop(DateTime timestamp) =>
        RequestState.Current.HeadersReceived = timestamp;

    // IKestrelTelemetryConsumer
    public void OnRequestStart(DateTime timestamp, string connectionId, string requestId, string httpVersion, string path, string method)
    {
        _startTimestamp.Value = timestamp;
        _requestInfo.Value = new RequestInfo(timestamp, requestId, path);
    }

    public void OnRequestStop(DateTime timestamp, string connectionId, string requestId, string httpVersion, string path, string method)
    {
        if (_startTimestamp.Value is { } startTime)
        {
            var elapsed = timestamp - startTime;
            _logger.LogInformation("Request {requestId} to {path} took {elapsedMs} ms", requestId, path, elapsed.TotalMilliseconds);
        }
    }
}

public class YarpFullTelemetryOption
{
    // measure-dns-resolutions-for-a-given-endpoint
    public string Hostname { get; set; }
}

// measure-time-to-headers-when-using-httpclient
public sealed class RequestState
{
    private static readonly AsyncLocal<RequestState> _asyncLocal = new();
    public static RequestState Current => _asyncLocal.Value ??= new();

    public DateTime RequestStart;
    public DateTime HeadersSent;
    public DateTime HeadersReceived;
    public DateTime RequestStop;
}