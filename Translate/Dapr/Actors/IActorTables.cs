

namespace Actors;

public interface IActorTables : IDisposable
{
    List<string> Types();
    bool IsActorTypeHosted(string actorType);
    (IActorTargets, bool) HostedTarget(string actorType, string actorKey);
    (IActorTargets, bool) GetOrCreate(string actorType, string actorID);
    void RegisterActorTypes(RegisterActorTypeOptions opts);
    Task UnRegisterActorTypes(params string[] actorTypes);
    Task HaltAll();
    Task Drain(Func<IActorTargets, bool> factory);
    Dictionary<string, int> Len();
    Task DeleteFromTableIn(IActorTargets actor, TimeSpan timeSpan);
    Task RemoveIdler(IActorTargets actor);
    (IAsyncEnumerable<List<string>>, List<string>) SubscribeToTypeUpdates(CancellationToken token);
    Task HaltIdlable(IActorIdlable target, CancellationToken token);
}