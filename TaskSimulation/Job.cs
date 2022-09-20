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
}

public enum JobStatus
{
    Created,
    Scheduled,
    Running,
    Completed
}
