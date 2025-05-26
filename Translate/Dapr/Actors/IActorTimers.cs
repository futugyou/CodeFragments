
namespace Actors;

public interface IActorTimers
{
    Task CreateAsync(CreateTimerRequest req, CancellationToken token);
    Task DeleteAsync(DeleteTimerRequest req, CancellationToken token);
}