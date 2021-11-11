using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspnetcoreEx.HealthCheckExtensions;

public class DemoHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}