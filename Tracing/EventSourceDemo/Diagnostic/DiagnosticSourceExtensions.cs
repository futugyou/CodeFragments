using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DiagnosticAdapter;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventSourceDemo.Diagnostic
{
    public static class DiagnosticSourceExtensions
    {
        public static IApplicationBuilder UseDiagnosticListener(this IApplicationBuilder builder, IConfiguration configuration = null)
        {
            return builder.UseMiddleware<ApmMiddleware>();
        }

        internal class ApmMiddleware
        {
            private readonly RequestDelegate _next;
            public ApmMiddleware(
                RequestDelegate next)
            {
                RegisteDiagnosticObservable();
                _next = next;
            }

            private static void RegisteDiagnosticObservable()
            {
                DiagnosticListener.AllListeners.Subscribe(listener =>
                {
                    if (listener.Name == "Web")
                    {
                        listener.SubscribeWithAdapter(new DiagnosticCollector());
                    }
                    if (listener.Name == "Microsoft.AspNetCore")
                    {
                        listener.SubscribeWithAdapter(new DiagnosticRequestCollector());
                    }
                });
            }

            public async Task InvokeAsync(HttpContext context)
            {
                await _next.Invoke(context);
            }
        }
    }
}
