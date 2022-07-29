namespace HangfireDemo.Service;

public class Dosomething : IDosomething
{
    private readonly ILogger<Dosomething> logger;

    public Dosomething(ILogger<Dosomething> logger)
    {
        this.logger = logger;
    }

    public Task<JobParaemter> ChangeAge(JobParaemter jobParaemter)
    {
        jobParaemter.Age += 100;
        Console.WriteLine("now age is " + jobParaemter.Age);
        return Task.FromResult(jobParaemter);
    }

    public void Delaywork()
    {
        Console.WriteLine("this work complete at " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
    }

    public Task<string> DisplayTime()
    {
        var time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        Console.WriteLine(time);
        return Task.FromResult(time);
    }

    public Task ErrorMethod()
    {
        throw new NotImplementedException();
    }

    public Task ErrorMethodWithLimit()
    {
        throw new NotImplementedException();
    }

    public async Task<int> HaveReturnValue()
    {
        await Task.Delay(1000);
        return 110;
    }

    public async Task LongRunningJob()
    {
        await Task.Delay(TimeSpan.FromSeconds(30));
    }

    public Task Loopwork()
    {
        var time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        Console.WriteLine(time);
        return Task.CompletedTask;
    }

    public Task ReadMessage(string message)
    {
        Console.WriteLine(message);
        logger.LogInformation(message);
        return Task.FromResult(0);
    }
}