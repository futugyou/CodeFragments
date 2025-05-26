

namespace Actors;

public interface IActorPlacement
{
    Task RunAsync(CancellationToken token);
    bool Ready();
    (CancellationTokenSource cancelSource, bool success) LockAsync(CancellationToken token);
    Task<LookupActorResponse> LookupActorAsync(LookupActorRequest req, CancellationToken token);
}