
namespace Actors;

public interface IActorState
{
    Task<StateResponse> GetAsync(GetStateRequest request, CancellationToken token);
    Task<BulkStateResponse> GetBulkAsync(GetBulkStateRequest request, CancellationToken token);
    Task TransactionalStateOperationAsync(bool ignoreHosted, TransactionalRequest request, CancellationToken token);
}
