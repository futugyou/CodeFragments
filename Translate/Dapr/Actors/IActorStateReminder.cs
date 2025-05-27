namespace Actors;


public interface IActorStateReminder : IDisposable
{
    Task<Reminder> GetAsync(GetReminderRequest request, CancellationToken token);
    Task CreateAsync(CreateReminderRequest request, CancellationToken token);
    Task DeleteAsync(DeleteReminderRequest request, CancellationToken token);
    Task<List<Reminder>> ListAsync(ListRemindersRequest request, CancellationToken token);
    void DrainRebalancedReminders();
    Task OnPlacementTablesUpdated(Func<LookupActorRequest, CancellationToken, Task<bool>> lookupFn, CancellationToken token);
}
