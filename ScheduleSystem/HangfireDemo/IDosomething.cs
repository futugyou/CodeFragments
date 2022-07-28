namespace HangfireDemo;

public interface IDosomething
{
    Task ReadMessage(string message);
    Task<int> HaveReturnValue();
    Task Loopwork();
    void Delaywork();
    Task<string> DisplayTime();

    Task<JobParaemter> ChangeAge(JobParaemter jobParaemter);
}
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

    public async Task<int> HaveReturnValue()
    {
        await Task.Delay(1000);
        return 110;
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


public class JobParaemter
{
    public int Age { get; set; }
    public string Name { get; set; }
}