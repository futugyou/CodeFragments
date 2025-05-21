using System.Text.Json;
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

    public APIVersion GetAPIVersion()
    {
        if (r == null)
        {
            throw new InvalidOperationException("InvokeMethodRequest is not initialized");
        }
        return r.Ver;
    }

    public InternalInvokeRequest ProtoWithData()
    {
        if (r == null || r.Message == null)
        {
            throw new InvalidOperationException("InvokeMethodRequest is not initialized");
        }

        // If the data is already in-memory in the object, return the object directly.
        // This doesn't copy the object, and that's fine because receivers are not expected to modify the received object.
        // Only reason for cloning the object below is to make ProtoWithData concurrency-safe.
        if (HasMessageData())
        {
            return r;
        }

        // Clone the object
        var m = r.Clone();
        m.Message.Data = new Any()
        {
            TypeUrl = dataTypeURL,
            Value = m.Message.Data.Value
        };
        return m;
    }
}
