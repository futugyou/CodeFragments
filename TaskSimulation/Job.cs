namespace TaskSimulation;

public class Job
{
    private readonly Action _work;
    private Job? _continue;
    public JobStatus Status { get; internal set; }

    public Job(Action work)
    {
        _work = work;
    }

    internal protected virtual void Invoke()
    {
        Status = JobStatus.Running;
        _work();
        Status = JobStatus.Completed;
        _continue?.Start();
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

    public Job ContinueWith(Action<Job> continueJob)
    {
        if (_continue == null)
        {
            var job = new Job(() => continueJob(this));
            _continue = job;
        }
        else
        {
            _continue.ContinueWith(continueJob);
        }
        return this;
    }
}

public enum JobStatus
{
    Created,
    Scheduled,
    Running,
    Completed
}
