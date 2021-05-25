using Microsoft.Extensions.DiagnosticAdapter;
using System;
using System.Net.Http;

namespace EventSourceDemo.Diagnostic
{
    public sealed class DiagnosticCollector
    {
        [DiagnosticName("ReveiveRequest")]
        public void OnReveiveRequest(HttpRequestMessage httpRequest, long timestamp)
        {
            Console.WriteLine($"Reveice request url:{httpRequest.RequestUri} ; Timestamp:{timestamp}");
        }

        [DiagnosticName("SendReply")]
        public void OnSendReply(HttpResponseMessage httpResponse, TimeSpan elaped)
        {
            Console.WriteLine($"send reply status code:{httpResponse.StatusCode} ; Elaped:{elaped}");
        }
    }
}