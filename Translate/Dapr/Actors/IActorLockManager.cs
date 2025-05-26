using Dapr.Proto.Internals.V1;

namespace Actors;

public interface IActorLockManager
{
    (CancellationTokenSource cancelSource, bool success) Lock(string actorType, string actorID);
    (CancellationTokenSource cancelSource, bool success) LockRequest(InternalInvokeRequest req);
    void Close(string actorKey);
    void CloseUntil(string actorKey, TimeSpan duration);
}
