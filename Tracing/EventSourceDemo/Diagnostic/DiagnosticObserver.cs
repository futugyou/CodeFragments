using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventSourceDemo.Diagnostic
{
    public sealed class DiagnosticObserver
    {
        public static readonly DiagnosticObserver Instance = new DiagnosticObserver();
        static readonly DiagnosticListener source = new DiagnosticListener("Web");
        private DiagnosticObserver() { }
        public void RegisteDiagnosticObserver()
        {
            var stopwatch = Stopwatch.StartNew();
            if (source.IsEnabled("ReveiveRequest"))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://www.baidu.com");
                source.Write("ReveiveRequest", new { HttpRequest = request, Timestamp = Stopwatch.GetTimestamp() });
            }
            if (source.IsEnabled("SendReply"))
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                source.Write("SendReply", new { HttpResponse = response, Elaped = stopwatch.Elapsed });
            }
        }
    }
}
