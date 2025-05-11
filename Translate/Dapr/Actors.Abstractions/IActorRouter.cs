using Dapr.Client.Autogen.Grpc.v1;
using Dapr.Proto.Internals.V1;
using Google.Protobuf.WellKnownTypes;

namespace Actors.Abstractions;

public interface IActorRouter
{
    Task<InternalInvokeResponse> CallAsync(InternalInvokeRequest request, CancellationToken token);
    Task CallReminderAsync(Reminder reminder, CancellationToken token);

    Task CallStreamAsync(InternalInvokeRequest request, IAsyncEnumerable<InternalInvokeResponse> stream, CancellationToken token);
}
