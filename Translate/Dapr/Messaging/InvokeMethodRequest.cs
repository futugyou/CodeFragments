using System.Text.Json;
using Dapr.Proto.Components.V1;
using Dapr.Proto.Internals.V1;
using Google.Protobuf.WellKnownTypes;
using Grpcv1 = Dapr.Client.Autogen.Grpc.v1;

namespace Messaging;

public class InvokeMethodRequest
{
    private InternalInvokeRequest r;
    private object? dataObject;
    public object? DataObject
    {
        get => dataObject;
        set
        {
            if (value is not null)
            {
                try
                {
                    var json = JsonSerializer.Serialize(value);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                    WithRawDataBytes(bytes);
                }
                catch (Exception)
                {
                }
            }

            dataObject = value;
        }
    }

    public string DataTypeURL
    {
        get => dataTypeURL;
        set => dataTypeURL = value;
    }

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

    public void ResetMessageData()
    {
        if (!HasMessageData())
        {
            return;
        }

        r.Message.Data = new Any();
    }

    public bool HasMessageData()
    {
        var m = r.Message;
        return m.Data != null && m.Data.Value != null && m.Data.Value.Length > 0;
    }

    public InvokeMethodRequest WithRawDataBytes(byte[] data)
    {
        var stream = new MemoryStream(data);
        return WithRawData(stream);
    }

    public InvokeMethodRequest WithRawData(Stream stream)
    {
        dataObject = null;
        ResetMessageData();
        // TODO: use stream
        return this;
    }
}
