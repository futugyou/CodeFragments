using Dapr.Abstractions;

namespace Actors.Models;

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
