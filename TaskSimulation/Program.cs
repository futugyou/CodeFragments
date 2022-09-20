// See https://aka.ms/new-console-template for more information
using TaskSimulation;

Console.WriteLine("Hello, World!");

ThreadPoolDemo();

void ThreadPoolDemo()
{
    _ = Job.Run(() => Console.WriteLine($"job1 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
    _ = Job.Run(() => Console.WriteLine($"job2 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
    _ = Job.Run(() => Console.WriteLine($"job3 is excuted in thead {Thread.CurrentThread.ManagedThreadId}"));
}

Console.ReadLine();