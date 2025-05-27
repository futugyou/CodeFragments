
namespace Actors;

public class StoreState : IActorStateReminder
{
    public Task CreateAsync(CreateReminderRequest request, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(DeleteReminderRequest request, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void DrainRebalancedReminders()
    {
        throw new NotImplementedException();
    }

    public Task<Reminder> GetAsync(GetReminderRequest request, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task<List<Reminder>> ListAsync(ListRemindersRequest request, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public Task OnPlacementTablesUpdated(Func<LookupActorRequest, CancellationToken, Task<bool>> lookupFn, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}