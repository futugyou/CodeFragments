
namespace Dapr.Abstractions;

public interface IStateRequest
{
    string GetKey();
    Dictionary<string, string> GetMetadata();
}

public interface ITransactionalStateOperation : IStateRequest
{
    OperationType Operation();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationType
{
    [JsonStringEnumMemberName("upsert")]
    Upsert,
    [JsonStringEnumMemberName("delete")]
    Delete
}

public class SetRequest : ITransactionalStateOperation
{
    [JsonPropertyName("key")]
    public string Key { get; set; }
    [JsonPropertyName("value")]
    public object Value { get; set; }
    [JsonPropertyName("etag")]
    public string ETag { get; set; }
    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; }
    [JsonPropertyName("options")]
    public SetStateOption Options { get; set; }
    [JsonPropertyName("contentType")]
    public string ContentType { get; set; }

    public string GetKey() => Key;
    public Dictionary<string, string> GetMetadata() => Metadata;
    public bool HasETag() => ETag != null && ETag != "";
    public OperationType Operation() => OperationType.Upsert;
}

public class SetStateOption
{
    [JsonPropertyName("concurrency")]
    public string Concurrency { get; set; } // first-write, last-write
    [JsonPropertyName("consistency")]
    public string Consistency { get; set; } // "eventual, strong" 
}

public class DeleteStateOption
{
    [JsonPropertyName("concurrency")]
    public string Concurrency { get; set; } // first-write, last-write
    [JsonPropertyName("consistency")]
    public string Consistency { get; set; } // "eventual, strong" 
}

public class DeleteRequest : ITransactionalStateOperation
{

    [JsonPropertyName("key")]
    public string Key { get; set; }
    [JsonPropertyName("etag")]
    public string ETag { get; set; }
    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; }
    [JsonPropertyName("options")]
    public DeleteStateOption Options { get; set; }

    public string GetKey() => Key;
    public Dictionary<string, string> GetMetadata() => Metadata;
    public bool HasETag() => ETag != null && ETag != "";
    public OperationType Operation() => OperationType.Delete;
}