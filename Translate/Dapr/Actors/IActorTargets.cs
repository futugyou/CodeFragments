using Dapr.Client.Autogen.Grpc.v1;
using Dapr.Proto.Internals.V1;
using Google.Protobuf.WellKnownTypes;

namespace Actors;

public interface IActorTargets
{
    string Key();
    string Type();
    string ID();
    Task<InternalInvokeResponse> InvokeMethodAsync(InternalInvokeRequest request, CancellationToken token);
    Task InvokeReminderAsync(Actors.Models.Reminder reminder, CancellationToken token);
    Task InvokeTimerAsync(Actors.Models.Reminder reminder, CancellationToken token);
    Task InvokeStreamAsync(InternalInvokeRequest request, IAsyncEnumerable<InternalInvokeResponse> stream, CancellationToken token);
    void Deactivate();
}