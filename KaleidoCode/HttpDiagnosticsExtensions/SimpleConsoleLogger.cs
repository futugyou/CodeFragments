
using Microsoft.Extensions.Http.Logging;

namespace KaleidoCode.HttpDiagnosticsExtensions;
// outputs one line per request to console
public class SimpleConsoleLogger : IHttpClientLogger
{
    public object? LogRequestStart(HttpRequestMessage request) => null;

    public void LogRequestStop(object? ctx, HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
=> Console.WriteLine($"{request.Method} {request.RequestUri?.AbsoluteUri} - {(int)response.StatusCode} {response.StatusCode} in {elapsed.TotalMilliseconds}ms");

    public void LogRequestFailed(object? ctx, HttpRequestMessage request, HttpResponseMessage? response, Exception e, TimeSpan elapsed)
=> Console.WriteLine($"{request.Method} {request.RequestUri?.AbsoluteUri} - Exception {e.GetType().FullName}: {e.Message}");
}
