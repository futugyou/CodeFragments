using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Hangfire.Tags;

namespace HangfireCapDemo;

public class CapJobHandStateAttribute : JobFilterAttribute, IServerFilter, IElectStateFilter, IApplyStateFilter
{
    private static readonly ILog Logger = LogProvider.For<CapJob>();

    public CapJobHandStateAttribute()
    {
    }

    /// <inheritdoc />
    public void OnStateElection(ElectStateContext context)
    {
        try
        {
            var jobItem = context.BackgroundJob.Job.Args.FirstOrDefault();
            if (jobItem is not CapJobItem capJobItem)
            {
                return;
            }

            var jobResult = context.GetJobParameter<string>("jobErr");
            if (!string.IsNullOrEmpty(jobResult))
            {
                context.SetJobParameter("jobErr", string.Empty);
                context.CandidateState = new CapJobErrorState(jobResult);
                var serverInfo = context.GetJobParameter<string>("serverInfo");
                var startAt = context.GetJobParameter<string>("jobAgentStartAt");
                if (!string.IsNullOrEmpty(serverInfo) && !string.IsNullOrEmpty(startAt))
                {
                    var serverInfoArr = serverInfo.Split(new string[] { "@_@" }, StringSplitOptions.None);
                    if (serverInfoArr.Length == 2)
                    {
                        var startedAt = JobHelper.DeserializeDateTime(startAt);
                        using (var tran = context.Connection.CreateWriteTransaction())
                        {
                            tran.AddJobState(context.BackgroundJob.Id, new CapJobProcessState(serverInfoArr[0], serverInfoArr[1], startedAt));
                            tran.Commit();
                        }
                    }
                }
                context.SetJobParameter("serverInfo", string.Empty);
                return;
            }

            // first, state change to processing
            var processingState = context.CandidateState as ProcessingState;
            if (processingState != null)
            {
                // only in processing
                context.SetJobParameter("serverInfo", processingState.ServerId + "@_@" + processingState.WorkerId);
                return;
            }

            // failed directly
            var failedState = context.CandidateState as FailedState;
            if (failedState != null)
            {
                context.SetJobParameter("serverInfo", string.Empty);
                // This filter accepts only failed job state.
                return;
            }
        }
        catch (Exception)
        {
        }
    }


    /// <inheritdoc />
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        var jobItem = context.BackgroundJob.Job.Args.FirstOrDefault();
        if (jobItem is CapJobItem capJobItem && capJobItem.JobExpirationTimeout > 0)
        {
            context.JobExpirationTimeout = TimeSpan.FromHours(capJobItem.JobExpirationTimeout);
        }
    }

    /// <inheritdoc />
    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        var jobItem = context.BackgroundJob.Job.Args.FirstOrDefault();
        if (jobItem is CapJobItem capJobItem && capJobItem.JobExpirationTimeout > 0)
        {
            context.JobExpirationTimeout = TimeSpan.FromHours(capJobItem.JobExpirationTimeout);
        }
    }

    public void OnPerforming(PerformingContext filterContext)
    {
        var jobItem = filterContext.BackgroundJob.Job.Args.FirstOrDefault();
        if (jobItem is not CapJobItem job)
        {
            return;
        }

        if (!string.IsNullOrEmpty(job.JobName) && DashboardRoutes.Routes.FindDispatcher("/tags/all") != null)
        {
            filterContext.BackgroundJob.Id.AddTags(job.JobName);
        }
    }

    public void OnPerformed(PerformedContext filterContext)
    {

    }
}