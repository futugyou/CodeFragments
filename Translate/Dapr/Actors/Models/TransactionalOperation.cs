using Dapr.Abstractions;

namespace Actors.Models;

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
