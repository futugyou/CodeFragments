using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DiagnosticAdapter;
using System;

namespace EventSourceDemo.Diagnostic
{
    public sealed class DiagnosticRequestCollector
    {
        [DiagnosticName("Microsoft.AspNetCore.Hosting.BeginRequest")]
        public void OnReveiveRequest(HttpContext httpContext, long timestamp)
        {
            Console.WriteLine($"Reveice request url:{httpContext.Request.ToString()} ; Timestamp:{timestamp}");
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.EndRequest")]
        public void OnSendReply(HttpContext httpContext, long currentTimestamp)
        {
            Console.WriteLine($"send reply status code:{httpContext.Response.StatusCode} ; Timestamp:{currentTimestamp}");
        }
    }
}