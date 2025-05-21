
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Config;
using Messaging;
using static Dapr.AppCallback.Autogen.Grpc.v1.AppCallbackAlpha;
using static Dapr.AppCallback.Autogen.Grpc.v1.AppCallback;
using static Dapr.AppCallback.Autogen.Grpc.v1.AppCallbackHealthCheck;
using Microsoft.Extensions.Options;
using Dapr.Proto.Internals.V1;
using Grpc.Core;
using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client.Autogen.Grpc.v1;

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
        Metadata trailers;

        try
        {
            var call = appCallbackClient.OnInvokeAsync(pd.Message, headers: md, cancellationToken: token);
            md = await call.ResponseHeadersAsync;
            var response = await call.ResponseAsync;
            trailers = call.GetTrailers();
            rsp = new InvokeMethodResponse((int)StatusCode.OK, "", []);
        }
        catch (RpcException ex)
        {
            trailers = ex.Trailers;
            rsp = new InvokeMethodResponse((int)ex.StatusCode, ex.Status.Detail, []);
        }

        rsp.WithHeaders(md).WithTrailers(trailers);

        return rsp;
    }

    public async Task<InvokeMethodResponse> TriggerJobAsync(string name, Google.Protobuf.WellKnownTypes.Any data, CancellationToken token)
    {
        var request = new JobEventRequest
        {
            Name = name,
            Data = data,
            Method = "job/" + name,
            ContentType = data.TypeUrl,
            HttpExtension = new HTTPExtension { Verb = HTTPExtension.Types.Verb.Post }
        };

        Metadata? responseHeaders = null;
        Metadata trailers;
        InvokeMethodResponse rsp;
        try
        {
            var call = appCallbackAlphaClient.OnJobEventAlpha1Async(request, cancellationToken: token);
            responseHeaders = await call.ResponseHeadersAsync;
            var response = await call.ResponseAsync;
            trailers = call.GetTrailers();
            rsp = new InvokeMethodResponse((int)StatusCode.OK, "", []);
        }
        catch (RpcException ex)
        {
            trailers = ex.Trailers;
            rsp = new InvokeMethodResponse((int)ex.StatusCode, ex.Status.Detail, []);
        }

        if (responseHeaders != null)
        {
            rsp.WithHeaders(responseHeaders).WithTrailers(trailers);
        }

        return rsp;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}