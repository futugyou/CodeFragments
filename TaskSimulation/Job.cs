namespace TaskSimulation;

public class Job
{
    private readonly Action _work;
    public Job(Action work)
    {
        _work = work;
    }
    public JobStatus Status { get; internal set; }

    internal protected virtual void Invoke()
    {
        Status = JobStatus.Running;
        _work();
        Status = JobStatus.Completed;
    }

    public void Start(JobScheduler? jobScheduler = null)
    {
        (jobScheduler ?? JobScheduler.Current).QueueJob(this);
    }

    public static Job Run(Action work)
    {
        var job = new Job(work);
        job.Start();
        return job;
    }
}

public enum JobStatus
{
    Created,
    Scheduled,
    Running,
    Completed
}
