using Dapr.Proto.Components.V1;
using Dapr.Proto.Internals.V1;
using Grpcv1 = Dapr.Client.Autogen.Grpc.v1;

namespace Messaging;

public class InvokeMethodRequest
{
    private InternalInvokeRequest r;
    private object dataObject;
    private string dataTypeURL;

    public InvokeMethodRequest(string methodName)
    {
        r = new InternalInvokeRequest()
        {
            Ver = APIVersion.V1,
            Message = new Grpcv1.InvokeRequest()
            {
                Method = methodName,
            }
        };
    }

    public InvokeMethodRequest(Grpcv1.InvokeRequest request)
    {
        r = new InternalInvokeRequest()
        {
            Ver = APIVersion.V1,
            Message = request,
        };
    }

    public InvokeMethodRequest(InternalInvokeRequest request)
    {
        r = request;
        ArgumentNullException.ThrowIfNull(request.Message, nameof(request.Message));
    }

    public InvokeMethodRequest WithActor(string actorType, string actorID)
    {
        r.Actor = new Actor() { ActorType = actorType, ActorId = actorID };
        return this;
    }

    public InvokeMethodRequest WithMetadata(Dictionary<string, string[]> md)
    {
        foreach (var kvp in md)
        {
            var listStringValue = new ListStringValue();
            listStringValue.Values.Add(kvp.Value);
            r.Metadata.Add(kvp.Key, listStringValue);
        }

        return this;
    }

}
