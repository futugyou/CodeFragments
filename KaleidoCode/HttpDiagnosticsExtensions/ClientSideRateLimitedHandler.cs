using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Http.Resilience;

namespace KaleidoCode.HttpDiagnosticsExtensions;

public static class HttpExtensions
{
    internal static IServiceCollection AddClientSideRateLimited(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new TokenBucketRateLimiterOptions
        {
            TokenLimit = 8,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 3,
            ReplenishmentPeriod = TimeSpan.FromMilliseconds(1),
            TokensPerPeriod = 2,
            AutoReplenishment = true
        };

        var tokenLiniter = new TokenBucketRateLimiter(options);

        var options1 = new ConcurrencyLimiterOptions
        {
            PermitLimit = 8,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 3,
        };

        var limiter1 = new ConcurrencyLimiter(options1);

        // FixedWindowRateLimiter
        // PartitionedRateLimiter
        // SlidingWindowRateLimiter

        services
        .AddHttpClient("ClientSideRate", c => { })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new ClientSideRateLimitedHandler(limiter: tokenLiniter);
        })
        .AddResilienceHandler("AdvancedPipeline", static (builder, context) =>
        {
            // Enable reloads whenever the named options change
            context.EnableReloads<HttpRetryStrategyOptions>("RetryOptions");

            // Retrieve the named options
            var retryOptions = context.GetOptions<HttpRetryStrategyOptions>("RetryOptions");

            // Add retries using the resolved options
            builder.AddRetry(retryOptions);
        });

        return services;
    }
}

public sealed class ClientSideRateLimitedHandler(RateLimiter limiter) : DelegatingHandler(new HttpClientHandler()), IAsyncDisposable
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using RateLimitLease lease = await limiter.AcquireAsync(
            permitCount: 1, cancellationToken);

        if (lease.IsAcquired)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        if (lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            response.Headers.Add("Retry-After", ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
        }

        return response;
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await limiter.DisposeAsync().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            limiter.Dispose();
        }
    }
}