
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Config;
using Messaging;

namespace AppChannel;

public class HttpChannel : IAppChannel, IHealthCheck
{
    public Task<ApplicationConfig> GetAppConfigAsync(string appID, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<InvokeMethodResponse> InvokeMethodAsync(InvokeMethodRequest req, string appID, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<InvokeMethodResponse> TriggerJobAsync(string name, Google.Protobuf.WellKnownTypes.Any data, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}