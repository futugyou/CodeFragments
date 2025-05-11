using Dapr.Abstractions;

namespace Actors.Models;

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
