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


public class ParameterJob : IJob
{
    private readonly ILogger<ParameterJob> logger;

    public ParameterJob(ILogger<ParameterJob> logger)
    {
        this.logger = logger;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        var data = context.MergedJobDataMap;
        var name = data.GetString("name");
        var age = data.GetInt("age");
        logger.LogInformation($"name: {name}, age: {age}.");
        await Task.Delay(3000);
    }
}