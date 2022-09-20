// See https://aka.ms/new-console-template for more information
using TaskSimulation;

Console.WriteLine("Hello, World!");

ThreadPoolDemo();
DedicatedThreaDemo();

Console.ReadLine();


void ThreadPoolDemo()
{
    _ = Job.Run(() => Console.WriteLine($"job1 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
    _ = Job.Run(() => Console.WriteLine($"job2 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
    _ = Job.Run(() => Console.WriteLine($"job3 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
    _ = Job.Run(() => Console.WriteLine($"job4 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
    _ = Job.Run(() => Console.WriteLine($"job5 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
    _ = Job.Run(() => Console.WriteLine($"job6 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
}


void DedicatedThreaDemo()
{
    JobScheduler.Current = new DedicatedThreadJobScheduler(2);
    _ = Job.Run(() => Console.WriteLine($"job11 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
    _ = Job.Run(() => Console.WriteLine($"job12 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
    _ = Job.Run(() => Console.WriteLine($"job13 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
    _ = Job.Run(() => Console.WriteLine($"job14 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
    _ = Job.Run(() => Console.WriteLine($"job15 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
    _ = Job.Run(() => Console.WriteLine($"job16 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
}
