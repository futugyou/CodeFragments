namespace Actors;

public interface IActorReminders
{
    Task<Actors.Models.Reminder> GetAsync(GetReminderRequest request, CancellationToken token);
    Task CreateAsync(CreateReminderRequest request, CancellationToken token);
    Task DeleteAsync(DeleteReminderRequest request, CancellationToken token);
}
