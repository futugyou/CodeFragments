using System.Collections.Concurrent;

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

public class DedicatedThreadJobScheduler : JobScheduler
{
    private readonly BlockingCollection<Job> _queue = new();
    private readonly Thread[] _threads;

    public DedicatedThreadJobScheduler(int threadCount)
    {
        _threads = new Thread[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            _threads[i] = new Thread(Invoke);
        }
        Array.ForEach(_threads, t => t.Start());

        void Invoke(object? state)
        {
            while (true)
            {
                _queue.Take().Invoke();
            }
        }
    }

    public override void QueueJob(Job job)
    {
        _queue.Add(job);
    }
}
