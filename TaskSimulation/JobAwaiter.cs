using System.Runtime.CompilerServices;

namespace TaskSimulation;

public struct JobAwaiter : ICriticalNotifyCompletion
{
    private readonly Job _job;
    public bool IsCompleted => _job.Status == JobStatus.Completed;

    public JobAwaiter(Job job)
    {
        _job = job;
        if (job.Status == JobStatus.Created)
        {
            job.Start();
        }
    }

    public void OnCompleted(Action continuation)
    {
        _job.ContinueWith(_ => continuation());
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        OnCompleted(continuation);
    }

    public void GetResult() { }
}
