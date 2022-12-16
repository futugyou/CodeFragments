using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DiagnosticAdapter;
using System;

namespace EventSourceDemo.Diagnostic
{
    public sealed class DiagnosticRequestCollector
    {
        [DiagnosticName("Microsoft.AspNetCore.Hosting.BeginRequest")]
        public void OnRequestStart(HttpContext httpContext, long timestamp)
        {
            var request = httpContext.Request;
            Console.WriteLine($"Request starting :{request.Protocol} {request.Method} {request.Scheme}://{request.Host}{request.PathBase}{request.Path}; Timestamp:{timestamp}");
            httpContext.Items["StartingTimestamp"] = timestamp;
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.EndRequest")]
        public void OnRequestEnd(HttpContext httpContext, long currentTimestamp)
        {   
            var startTimestamp = long.Parse(httpContext.Items["StartingTimestamp"]!.ToString());
            var timestamoToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
            var elapsed = new TimeSpan((long)(timestamoToTicks * (currentTimestamp - startTimestamp)));
            Console.WriteLine($"send reply status code:{httpContext.Response.StatusCode} ; Request finished:{elapsed.TotalMilliseconds} ms");
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.UnhandledException")]
        public void OnException(HttpContext httpContext, long timestamp, Exception exception)
        {
            OnRequestEnd(httpContext, timestamp);
            Console.WriteLine($"{exception.Message} Type:{exception.GetType()} Stacktrace: {exception.StackTrace}");
        }
    }
}