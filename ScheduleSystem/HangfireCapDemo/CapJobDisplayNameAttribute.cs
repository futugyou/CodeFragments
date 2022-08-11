using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;

namespace HangfireCapDemo;

public class CapJobDisplayNameAttribute : JobDisplayNameAttribute
{

    public CapJobDisplayNameAttribute(string displayName) : base(displayName)
    {
    }

    public override string Format(DashboardContext context, Job job)
    {
        if (job.Args.FirstOrDefault() is not CapJobItem data)
        {
            return job.Method.Name;
        }

        try
        {
            if (string.IsNullOrEmpty(data.RecurringJobIdentifier))
            {
                data.RecurringJobIdentifier = data.JobName;
            }

            return "JobName: " + data.JobName + " | EventName: " + data.CapEventName;
        }
        catch (Exception)
        {
            return data.JobName;
        }
    }
}