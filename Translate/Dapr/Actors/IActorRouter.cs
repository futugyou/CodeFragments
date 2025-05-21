using Dapr.Proto.Internals.V1;

namespace Actors;

public interface IActorRouter
{
    Task<InternalInvokeResponse> CallAsync(InternalInvokeRequest request, CancellationToken token);
    Task CallReminderAsync(Models.Reminder reminder, CancellationToken token);

    Task CallStreamAsync(InternalInvokeRequest request, IAsyncEnumerable<InternalInvokeResponse> stream, CancellationToken token);
}
