using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KaleidoCode.HealthCheckExtensions;

public class DemoHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}