
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Actors;

public class PlacementHealthState
{
    private volatile bool _isHealthy = false;

    public void ReportHealthy() => _isHealthy = true;
    public void ReportUnhealthy() => _isHealthy = false;
    public bool IsHealthy => _isHealthy;
}

public class PlacementHealthCheck : IHealthCheck
{
    private readonly PlacementHealthState _state;

    public PlacementHealthCheck(PlacementHealthState state)
    {
        _state = state;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_state.IsHealthy
            ? HealthCheckResult.Healthy("Placement connection healthy.")
            : HealthCheckResult.Unhealthy("Placement connection unhealthy."));
    }
}