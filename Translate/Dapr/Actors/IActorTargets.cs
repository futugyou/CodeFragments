using Dapr.Proto.Internals.V1;

namespace Actors;

public interface IActorTargets
{
    string Key();
    string Type();
    string ID();
    Task<InternalInvokeResponse> InvokeMethodAsync(InternalInvokeRequest request, CancellationToken token);
    Task InvokeReminderAsync(Models.Reminder reminder, CancellationToken token);
    Task InvokeTimerAsync(Models.Reminder reminder, CancellationToken token);
    Task InvokeStreamAsync(InternalInvokeRequest request, IAsyncEnumerable<InternalInvokeResponse> stream, CancellationToken token);
    void Deactivate();
}

public interface IActorIdlable : IActorTargets
{
    DateTime ScheduledTime();
}

public delegate IActorTargets ActorTargetsFactory(string actorID);
