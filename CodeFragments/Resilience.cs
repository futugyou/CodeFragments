using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;

namespace CodeFragments;

public class Resilience
{
    public static async Task BaseAsync()
    {
        var services = new ServiceCollection();

        const string key = "Retry-Timeout";
        services.AddResilienceEnricher();
        services.AddResiliencePipeline(key, static builder =>
        {
            // See: https://www.pollydocs.org/strategies/retry.html
            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<TimeoutRejectedException>()
            });

            // See: https://www.pollydocs.org/strategies/timeout.html
            builder.AddTimeout(TimeSpan.FromSeconds(1.5));
            Polly.CircuitBreaker.CircuitBreakerStrategyOptions options = new()
            {
                BreakDuration = TimeSpan.FromSeconds(5.0),
                SamplingDuration = TimeSpan.FromSeconds(30.0),
            };
            builder.AddCircuitBreaker(options);
        });

        using ServiceProvider provider = services.BuildServiceProvider();

        ResiliencePipelineProvider<string> pipelineProvider = provider.GetRequiredService<ResiliencePipelineProvider<string>>();

        ResiliencePipeline pipeline = pipelineProvider.GetPipeline(key);

        await pipeline.ExecuteAsync(static cancellationToken =>
        {
            // Code that could potentially fail.

            return ValueTask.CompletedTask;
        });
    }
}