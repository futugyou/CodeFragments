namespace TaskSimulation;

public abstract class JobScheduler
{
    public abstract void QueueJob(Job job);
    public static JobScheduler Current { get; set; } = new ThreadPoolJobScheduler();
}

public class ThreadPoolJobScheduler : JobScheduler
{
    public override void QueueJob(Job job)
    {
        job.Status = JobStatus.Scheduled;
        var context = ExecutionContext.Capture();
        ThreadPool.QueueUserWorkItem(_ =>
        {
            ExecutionContext.Run(context!, _ => job.Invoke(), null);
        });
    }
}