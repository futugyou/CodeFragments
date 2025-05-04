namespace Actors;

public class StateOperationOpts
{
    public Dictionary<string, string> Metadata { get; set; }
    public string ContentType { get; set; }
    public bool StateTTLEnabled { get; set; }
}

public enum OperationType
{
    Upsert,
    Delete
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
}


public class TransactionalDelete
{
    [JsonPropertyName("key")]
    public string Key { get; set; }
    [JsonPropertyName("etag")]
    public string ETag { get; set; }
}