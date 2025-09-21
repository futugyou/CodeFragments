
using Microsoft.Extensions.Http.Logging;

namespace KaleidoCode.HttpDiagnosticsExtensions;

public class RequestIdLogger : IHttpClientLogger
{
    private readonly ILogger _log;

    public RequestIdLogger(ILogger<RequestIdLogger> log)
    {
        _log = log;
    }

    private static readonly Action<ILogger, Guid, string?, Exception?> _requestStart =
        LoggerMessage.Define<Guid, string?>(
            LogLevel.Information,
            EventIds.RequestStart,
            "Request Id={RequestId} ({Host}) started");

    private static readonly Action<ILogger, Guid, double, Exception?> _requestStop =
        LoggerMessage.Define<Guid, double>(
            LogLevel.Information,
            EventIds.RequestStop,
            "Request Id={RequestId} succeeded in {elapsed}ms");

    private static readonly Action<ILogger, Guid, Exception?> _requestFailed =
        LoggerMessage.Define<Guid>(
            LogLevel.Error,
            EventIds.RequestFailed,
            "Request Id={RequestId} FAILED");

    public object? LogRequestStart(HttpRequestMessage request)
    {
        var ctx = new Context(Guid.NewGuid());
        _requestStart(_log, ctx.RequestId, request.RequestUri?.Host, null);
        return ctx;
    }

    public void LogRequestStop(object? ctx, HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
    {
        if (ctx == null)
        {
            return;
        }

        _requestStop(_log, ((Context)ctx).RequestId, elapsed.TotalMilliseconds, null);
    }

    public void LogRequestFailed(object? ctx, HttpRequestMessage request, HttpResponseMessage? response, Exception e, TimeSpan elapsed)
    {
        if (ctx == null)
        {
            return;
        }

        _requestFailed(_log, ((Context)ctx!).RequestId, null);
    }

    public static class EventIds
    {
        public static readonly EventId RequestStart = new(1, "RequestStart");
        public static readonly EventId RequestStop = new(2, "RequestStop");
        public static readonly EventId RequestFailed = new(3, "RequestFailed");
    }

    record Context(Guid RequestId);
}