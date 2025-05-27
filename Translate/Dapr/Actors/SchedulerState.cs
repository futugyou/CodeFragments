
using Dapr.Proto.Scheduler.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Tools;
using static Dapr.Proto.Scheduler.V1.Scheduler;

namespace Actors;

public class SchedulerState(SchedulerClient client, string @namespace, string appID, IActorTables actorTables, IActorStateReminder stateReminder) : IActorStateReminder
{
    public async Task CreateAsync(CreateReminderRequest request, CancellationToken token)
    {
        var (years, months, days, period, repeats, error) = TimeUtil.ParseDuration(request.Period);
        if (error != null)
        {
            throw error;
        }
        var schedule = "@every " + period.ToString("c");
        FailurePolicy? failurePolicy = null;
        if (request.IsOneShot)
        {
            failurePolicy = new FailurePolicy
            {
                Constant = new FailurePolicyConstant
                {
                    Interval = Duration.FromTimeSpan(TimeSpan.FromSeconds(1)),
                }
            };
        }

        var internalScheduleJobReq = new ScheduleJobRequest
        {
            Name = request.Name,
            Job = new Job
            {
                Schedule = schedule,
                Repeats = (uint)repeats,
                DueTime = request.DueTime.ToString(),
                Ttl = request.TTL.ToString(),
                Data = request.Data,
                FailurePolicy = failurePolicy,
            },
            Metadata = new JobMetadata
            {
                AppId = appID,
                Namespace = @namespace,
                Target = new JobTargetMetadata
                {
                    Actor = new TargetActorReminder
                    {
                        Id = request.ActorID,
                        Type = request.ActorType,
                    },
                },
            },
        };
        await client.ScheduleJobAsync(internalScheduleJobReq, cancellationToken: token);
    }

    public async Task DeleteAsync(DeleteReminderRequest request, CancellationToken token)
    {
        var deleteJobRequest = new DeleteJobRequest
        {
            Name = request.Name,
            Metadata = new JobMetadata
            {
                Namespace = @namespace,
                AppId = appID,
                Target = new JobTargetMetadata
                {
                    Actor = new TargetActorReminder
                    {
                        Type = request.ActorType,
                    }
                },
            }
        };
        await client.DeleteJobAsync(deleteJobRequest, cancellationToken: token);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void DrainRebalancedReminders()
    {
    }

    public async Task<Reminder> GetAsync(GetReminderRequest request, CancellationToken token)
    {
        var getJobRequest = new GetJobRequest
        {
            Name = request.Name,
            Metadata = new JobMetadata
            {
                Namespace = @namespace,
                AppId = appID,
                Target = new JobTargetMetadata
                {
                    Actor = new TargetActorReminder
                    {
                        Type = request.ActorType,
                    }
                },
            }
        };
        try
        {
            var job = await client.GetJobAsync(getJobRequest, cancellationToken: token);
            var reminder = new Reminder
            {
                ActorID = request.ActorID,
                ActorType = request.ActorType,
                DueTime = job.Job.DueTime,
                Period = new ReminderPeriod(job.Job.Schedule, (int)job.Job.Repeats),
                Data = job.Job.Data,
            };

            return reminder;
        }
        catch (RpcException)
        {
            throw;
        }
    }

    public async Task<List<Reminder>> ListAsync(ListRemindersRequest request, CancellationToken token)
    {
        var listJobsRequest = new ListJobsRequest
        {
            Metadata = new JobMetadata
            {
                Namespace = @namespace,
                AppId = appID,
                Target = new JobTargetMetadata
                {
                    Actor = new TargetActorReminder
                    {
                        Type = request.ActorType,
                    }
                },
            }
        };
        var response = await client.ListJobsAsync(listJobsRequest, cancellationToken: token);
        var reminders = new List<Reminder>();
        foreach (var job in response.Jobs)
        {
            var actor = job.Metadata?.Target?.Actor;
            if (actor == null)
            {
                continue; // Skip jobs that do not match the requested actor type
            }

            var reminder = new Reminder
            {
                Name = job.Name,
                ActorID = actor.Id,
                ActorType = actor.Type,
                DueTime = job.Job.DueTime,
                Period = new ReminderPeriod(job.Job.Schedule, (int)job.Job.Repeats),
                Data = job.Job.Data,
            };
            reminders.Add(reminder);
        }

        return reminders;
    }

    public Task OnPlacementTablesUpdated(Func<LookupActorRequest, CancellationToken, Task<bool>> lookupFn, CancellationToken token)
    {
        var opts = new ToSchedulerOptions
        (
            actorTables: actorTables,
            stateReminders: stateReminder,
            schedulerReminders: this,
            lookupFn: lookupFn
        );
        return ToSchedulerOptions.ToScheduler(opts, token);
    }

}