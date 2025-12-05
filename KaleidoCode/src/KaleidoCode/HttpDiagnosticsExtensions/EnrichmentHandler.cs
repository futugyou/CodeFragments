using System.Net.Http.Metrics;

namespace KaleidoCode.HttpDiagnosticsExtensions;

public sealed class EnrichmentHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpMetricsEnrichmentContext.AddCallback(request, static context =>
        {
            if (context.Response is not null) // Response is null when an exception occurs.
            {
                // Use any information available on the request or the response to emit custom tags.
                string? value = context.Response.Headers.GetValues("Enrichment-Value").FirstOrDefault();
                if (value != null)
                {
                    context.AddCustomTag("enrichment_value", value);
                }
            }
        });
        return base.SendAsync(request, cancellationToken);
    }
}