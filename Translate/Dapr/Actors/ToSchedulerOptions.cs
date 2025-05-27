namespace Actors;

public class ToSchedulerOptions(IActorTables actorTables,
    IActorStateReminder stateReminders,
    IActorStateReminder schedulerReminders,
    Func<LookupActorRequest, CancellationToken, Task<bool>> lookupFn)
{
    private readonly IActorTables actorTables = actorTables;
    private readonly IActorStateReminder stateReminders = stateReminders;
    private readonly IActorStateReminder schedulerReminders = schedulerReminders;
    private readonly Func<LookupActorRequest, CancellationToken, Task<bool>> lookupFn = lookupFn;

    public static async Task ToScheduler(ToSchedulerOptions opts, CancellationToken token)
    {
        var stateReminders = new Dictionary<string, List<Reminder>>();
        var schedulerReminders = new Dictionary<string, List<Reminder>>();
        var actorTypes = opts.actorTables.Types();
        foreach (var actorType in actorTypes)
        {
            var stateR = await opts.stateReminders.ListAsync(new ListRemindersRequest
            {
                ActorType = actorType,
            }, token);

            foreach (var reminder in stateR)
            {
                var find = await opts.lookupFn(new LookupActorRequest
                {
                    ActorType = reminder.ActorType,
                    ActorID = reminder.ActorID,
                }, token);
                if (find)
                {
                    if (!stateReminders.TryGetValue(actorType, out List<Reminder>? value))
                    {
                        value = [];
                        stateReminders[actorType] = value;
                    }

                    value.Add(reminder);
                }
            }
            var schedR = await opts.schedulerReminders.ListAsync(new ListRemindersRequest
            {
                ActorType = actorType,
            }, token);

            schedulerReminders[actorType] = schedR;
        }

        var missingReminders = new List<Reminder>();
        foreach (var actorType in actorTypes)
        {
            foreach (var stateReminder in stateReminders[actorType])
            {
                var exists = false;
                foreach (var schedulerReminder in schedulerReminders[actorType])
                {
                    if (stateReminder.ActorID == schedulerReminder.ActorID && stateReminder.Name == schedulerReminder.Name)
                    {
                        exists = stateReminder.DueTime == schedulerReminder.DueTime &&
                            stateReminder.Data.Equals(schedulerReminder.Data) &&
                            Math.Abs((stateReminder.ExpirationTime - schedulerReminder.ExpirationTime).TotalMinutes) < 1;

                        break;
                    }
                }

                if (!exists)
                {
                    Console.WriteLine($"Found missing scheduler reminder {stateReminder.Key()}");
                    missingReminders.Add(stateReminder);
                }
            }
        }

        var taskList = new List<Task>();
        foreach (var reminder in missingReminders)
        {
            taskList.Add(opts.schedulerReminders.CreateAsync(new CreateReminderRequest
            {
                ActorType = reminder.ActorType,
                ActorID = reminder.ActorID,
                Name = reminder.Name,
                DueTime = reminder.DueTime,
                Period = reminder.Period.ToString(),
                Data = reminder.Data,
            }, token));
        }

        await Task.WhenAll(taskList);
    }
}
