using Dapr.Client.Autogen.Grpc.v1;
using Dapr.Proto.Internals.V1;
using Google.Protobuf.WellKnownTypes;


namespace Messaging;

public class InvokeMethodResponse
{
    private InternalInvokeResponse r;
    public string DataTypeURL
    {
        get => dataTypeURL;
        set => dataTypeURL = value;
    }

    private string dataTypeURL;

    public InvokeMethodResponse(int statusCode, string statusMessage, Any[] statusDetails)
    {
        var status = new Status { Code = statusCode, Message = statusMessage };
        status.Details.AddRange(statusDetails);
        r = new InternalInvokeResponse()
        {
            Status = status,
            Message = new InvokeResponse { },
        };
    }

    public InvokeMethodResponse(InternalInvokeResponse request)
    {
        r = request;
    }

    public InvokeMethodResponse WithMessage(InvokeResponse pb)
    {
        r.Message = pb;
        return this;
    }

    public InvokeMethodResponse WithContentType(string contentType)
    {
        r.Message.ContentType = contentType;
        return this;
    }

    public bool IsHTTPResponse()
    {
        if (r == null)
        {
            return false;
        }

        return r.Status.Code >= 100;
    }

    public Dictionary<string, ListStringValue>? Trailers()
    {
        if (r == null || r.Trailers == null)
        {
            return null;
        }

        return r.Trailers.ToDictionary(x => x.Key, x => x.Value);
    }

    public InvokeMethodResponse WithHeaders(Grpc.Core.Metadata headers)
    {
        foreach (var item in Util.MetadataToInternalMetadata(headers))
        {
            r.Headers.Add(item.Key, item.Value);
        }

        return this;
    }

    public InvokeMethodResponse WithTrailers(Grpc.Core.Metadata trailers)
    {
        foreach (var item in Util.MetadataToInternalMetadata(trailers))
        {
            r.Trailers.Add(item.Key, item.Value);
        }

        return this;
    }

}
