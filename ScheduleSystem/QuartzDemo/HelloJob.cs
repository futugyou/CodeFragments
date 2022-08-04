using Quartz;

namespace QuartzDemo;

public class HelloJob : IJob
{
    private readonly ILogger<HelloJob> logger;

    public HelloJob(ILogger<HelloJob> logger)
    {
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobkey = context.JobDetail.Key;
        var triggerkey = context.Trigger.Key;
        logger.LogInformation($"jobkey: {jobkey}, triggerkey: {triggerkey}.");
        await Console.Out.WriteLineAsync($"Greetings from HelloJob!{jobkey} job executing, triggered by {triggerkey}");
    }
}

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class ParameterJob : IJob
{
    public const string Name = "name";
    public const string Age = "age";

    private readonly ILogger<ParameterJob> logger;
    private int counter = 1;
    public ParameterJob(ILogger<ParameterJob> logger)
    {
        this.logger = logger;
    }
    public virtual async Task Execute(IJobExecutionContext context)
    {
        var jdata = context.JobDetail.JobDataMap;
        var tdata = context.Trigger.JobDataMap;
        var data = context.MergedJobDataMap;
        var jname = jdata.GetString(Name);
        var jage = jdata.GetInt(Age);
        var tname = tdata.GetString(Name);
        var tage = tdata.GetInt(Age);
        var name = data.GetString(Name);
        var age = data.GetInt(Age);
        jdata.Put(Age, jage + 2);
        tdata.Put(Age, tage + 3);
        data.Put(Age, age + 5);
        counter++;
        logger.LogInformation($"jname: {jname}, tname: {tname}, name: {name}, jage: {jage}, tage: {tage}, age: {age}, counter: {counter}.");
        await Task.Delay(3000);
    }
}

[PersistJobDataAfterExecution]
[DisallowConcurrentExecution]
public class MisfireJob : IJob
{
    private readonly ILogger<MisfireJob> logger;

    public MisfireJob(ILogger<MisfireJob> logger)
    {
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await Task.Delay(10000);
        logger.LogInformation("fireat: {0:r}", DateTime.UtcNow);
    }
}


public class ErrorJob : IJob
{
    private readonly ILogger<ErrorJob> logger;

    public ErrorJob(ILogger<ErrorJob> logger)
    {
        this.logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobkey = context.JobDetail.Key;
        var triggerkey = context.Trigger.Key;
        logger.LogInformation($"jobkey: {jobkey}, triggerkey: {triggerkey}.");
        await Console.Out.WriteLineAsync($"Greetings from HelloJob!{jobkey} job executing, triggered by {triggerkey}");
        //throw new NotImplementedException();
    }
}