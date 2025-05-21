
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Config;
using Messaging;
using static Dapr.AppCallback.Autogen.Grpc.v1.AppCallbackAlpha;
using static Dapr.AppCallback.Autogen.Grpc.v1.AppCallback;
using static Dapr.AppCallback.Autogen.Grpc.v1.AppCallbackHealthCheck;
using Microsoft.Extensions.Options;
using Dapr.Proto.Internals.V1;
using Grpc.Core;

namespace AppChannel;

public class GrpcChannel(AppCallbackClient appCallbackClient, AppCallbackAlphaClient appCallbackAlphaClient, AppCallbackHealthCheckClient appCallbackHealthCheckClient, IOptionsMonitor<TracingSpec> tracingSpec)
 : IAppChannel, IHealthCheck
{
    private readonly TracingSpec tracingSpec = tracingSpec.CurrentValue;
    private int maxRequestBodySize;
    private string appMetadataToken;
    private static readonly Google.Protobuf.WellKnownTypes.Empty _empty = new();

    public Task<ApplicationConfig> GetAppConfigAsync(string appID, CancellationToken token)
    {
        return Task.FromResult(new ApplicationConfig());
    }

    public Task<InvokeMethodResponse> InvokeMethodAsync(InvokeMethodRequest req, string appID, CancellationToken token)
    {
        return req.GetAPIVersion() switch
        {
            APIVersion.V1 => InvokeMethodV1(req, appID, token),
            _ => throw new NotImplementedException($"Unsupported spec version: {req.GetAPIVersion()}"),
        };
    }

    private async Task<InvokeMethodResponse> InvokeMethodV1(InvokeMethodRequest req, string appID, CancellationToken token)
    {
        var pd = req.ProtoWithData();
        var md = Util.InternalMetadataToGrpcMetadata(pd.Metadata, true, token);


        if (!string.IsNullOrEmpty(appMetadataToken))
        {
            md.Add("dapr-api-token", appMetadataToken);
        }

        InvokeMethodResponse rsp;
        try
        {
            var resp = await appCallbackClient.OnInvokeAsync(pd.Message, headers: md, cancellationToken: token);
            rsp = new InvokeMethodResponse(200, "", []);
        }
        catch (RpcException ex)
        {
            rsp = new InvokeMethodResponse((int)ex.StatusCode, ex.Message, []);
        }
        rsp.WithHeaders(md);

        return rsp;
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