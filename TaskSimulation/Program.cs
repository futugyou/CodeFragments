// See https://aka.ms/new-console-template for more information
using TaskSimulation;

Console.WriteLine("Hello, World!");

//ThreadPoolDemo();
//DedicatedThreaDemo();
//ContinueJob();

await Foo();
await Bar();
await Baz();

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

void ContinueJob()
{
    Job.Run(() =>
    {
        Thread.Sleep(1000);
        Console.WriteLine("Foo1");
    }).ContinueWith(_ =>
    {
        Thread.Sleep(100);
        Console.WriteLine("Bar1");
    }).ContinueWith(_ =>
    {
        Thread.Sleep(100);
        Console.WriteLine("Baz1");
    });

    Job.Run(() =>
    {
        Thread.Sleep(100);
        Console.WriteLine("Foo2");
    }).ContinueWith(_ =>
    {
        Thread.Sleep(10);
        Console.WriteLine("Bar2");
    }).ContinueWith(_ =>
    {
        Thread.Sleep(10);
        Console.WriteLine("Baz2");
    });
}

static Job Foo() => new Job(() =>
{
    Thread.Sleep(1000);
    Console.WriteLine("Foo");
});

static Job Bar() => new Job(() =>
{
    Thread.Sleep(100);
    Console.WriteLine("Bar");
});

static Job Baz() => new Job(() =>
{
    Thread.Sleep(10);
    Console.WriteLine("Baz");
});