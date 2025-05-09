
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Config;
using Messaging;

namespace AppChannel;

public class GrpcChannel : IAppChannel, IHealthCheck
{
    public Task<ApplicationConfig> GetAppConfigAsync(string appID, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<InvokeMethodResponse> InvokeMethodAsync(InvokeMethodRequest req, string appID, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<InvokeMethodResponse> TriggerJobAsync(string name, object data)
    {
        throw new NotImplementedException();
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}