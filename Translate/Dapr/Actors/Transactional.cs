using Dapr.Abstractions;

namespace Actors;

public class StateOperationOpts
{
    public Dictionary<string, string> Metadata { get; set; }
    public string ContentType { get; set; }
    public bool StateTTLEnabled { get; set; }
}


public class TransactionalRequest
{
    [JsonPropertyName("operations")]
    public List<TransactionalOperation> Operations { get; set; }
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }

    public string ActorKey() => $"{ActorType}{Const.DaprSeparator}{ActorID}";
}

public class TransactionalOperation
{
    [JsonPropertyName("operation")]
    public OperationType Operation { get; set; }
    [JsonPropertyName("request")]
    public object Request { get; set; }

    public ITransactionalStateOperation StateOperation(string baseKey, StateOperationOpts opts)
    {
        ArgumentNullException.ThrowIfNull(Request, nameof(Request));

        switch (Request)
        {
            case TransactionalUpsert upsert when Operation == OperationType.Upsert:
                return upsert.StateOperation(baseKey, opts);
            case TransactionalDelete delete when Operation == OperationType.Delete:
                return delete.StateOperation(baseKey, opts);
            default:
                // Fallback to use AutoMapper or System.TexJson for dynamic mapping
                if (Operation == OperationType.Upsert)
                {
                    // TODO: need JsonSerializerOptions
                    var upsert = JsonSerializer.Deserialize<TransactionalUpsert>(JsonSerializer.Serialize(Request)) ?? throw new InvalidOperationException("upsert request is null");
                    return upsert.StateOperation(baseKey, opts);
                }
                else if (Operation == OperationType.Delete)
                {
                    // TODO: need JsonSerializerOptions
                    var delete = JsonSerializer.Deserialize<TransactionalDelete>(JsonSerializer.Serialize(Request)) ?? throw new InvalidOperationException("delete request is null");
                    return delete.StateOperation(baseKey, opts);
                }
                else
                {
                    throw new InvalidOperationException($"operation type {Operation} not supported");
                }
        }
    }
}

public class TransactionalUpsert
{
    [JsonPropertyName("key")]
    public string Key { get; set; }
    [JsonPropertyName("value")]
    public string Value { get; set; }
    [JsonPropertyName("etag")]
    public string ETag { get; set; }
    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; }

    public ITransactionalStateOperation StateOperation(string baseKey, StateOperationOpts opts)
    {
        ArgumentNullException.ThrowIfNull(Key, nameof(Key));

        if (Metadata == null || Metadata.Count == 0)
        {
            Metadata = opts.Metadata;

        }
        else
        {
            foreach (var d in opts.Metadata)
            {
                Metadata[d.Key] = d.Value;
            }
        }

        if (!opts.StateTTLEnabled && Metadata.ContainsKey("ttlInSeconds"))
        {
            throw new InvalidOperationException("ttlInSeconds is not supported without the 'ActorStateTTL' feature enabled");
        }

        return new SetRequest
        {
            Key = baseKey + Key,
            Value = Value,
            Metadata = Metadata,
            ETag = ETag,
            ContentType = opts.ContentType,
        };
    }
}


public class TransactionalDelete
{
    [JsonPropertyName("key")]
    public string Key { get; set; }
    [JsonPropertyName("etag")]
    public string ETag { get; set; }

    public ITransactionalStateOperation StateOperation(string baseKey, StateOperationOpts opts)
    {
        ArgumentNullException.ThrowIfNull(Key, nameof(Key));
        return new DeleteRequest
        {
            Key = baseKey + Key,
            Metadata = opts.Metadata,
            ETag = ETag,
        };
    }
}