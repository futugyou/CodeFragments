using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace QuartzDemo.Controllers;

[ApiController]
[Route("api/[controller]/")]
public class DemoController : ControllerBase
{

    private readonly ILogger<DemoController> _logger;
    private readonly ISchedulerFactory schedulerFactory;

    public DemoController(ILogger<DemoController> logger, ISchedulerFactory schedulerFactory)
    {
        _logger = logger;
        this.schedulerFactory = schedulerFactory;
    }

    [HttpGet("simple")]
    public async Task<string> Simple()
    {
        var job = JobBuilder.Create<HelloJob>().WithIdentity("job1", "group1").Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity("trigger1", "group1")
            .StartNow()
            // second minute hour / forever forcount
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever())
            .Build();

        var scheduler = await schedulerFactory.GetScheduler();

        await scheduler.ScheduleJob(job, trigger);
        return "";
    }

    [HttpGet("daily")]
    public async Task<string> Daily()
    {
        var job = JobBuilder.Create<HelloJob>().WithIdentity("job1", "group1").Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity("trigger1", "group1")
            .StartNow()
            // start end week
            .WithDailyTimeIntervalSchedule(x => x.WithIntervalInSeconds(10))
            .Build();

        var scheduler = await schedulerFactory.GetScheduler();

        await scheduler.ScheduleJob(job, trigger);
        return "";
    }

    [HttpGet("cron")]
    public async Task<string> Cron()
    {
        var job = JobBuilder.Create<HelloJob>().WithIdentity("job1", "group1").Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity("trigger1", "group1")
            .StartNow()
            .WithPriority(10) // 10 > 5(default) > 1
            .WithCronSchedule("0/15 * * * * ? ")
            .Build();

        var scheduler = await schedulerFactory.GetScheduler();

        await scheduler.ScheduleJob(job, trigger);
        return "";
    }

    [HttpGet("delete")]
    public async Task<string> Delete()
    {
        var scheduler = await schedulerFactory.GetScheduler();
        var jobkey = JobKey.Create("job1", "group1");
        await scheduler.DeleteJob(jobkey);
        return "";
    }

    [HttpGet("clear")]
    public async Task<string> Clear()
    {
        var scheduler = await schedulerFactory.GetScheduler();
        await scheduler.Clear();
        return "";
    }

    [HttpGet("pause")]
    public async Task<string> Pause()
    {
        var scheduler = await schedulerFactory.GetScheduler();
        var jobkey = JobKey.Create("job1", "group1");
        await scheduler.PauseJob(jobkey);
        return "";
    }

    [HttpGet("resume")]
    public async Task<string> Resume()
    {
        var scheduler = await schedulerFactory.GetScheduler();
        var jobkey = JobKey.Create("job1", "group1");
        await scheduler.ResumeJob(jobkey);
        return "";
    }


    [HttpGet("paramerer")]
    public async Task<string> Paramerer()
    {
        var job = JobBuilder.Create<ParameterJob>().WithIdentity("paramerer", "group1").Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity("paramerer", "group1")
            .StartNow()
            .UsingJobData("name", "thisisname")
            .UsingJobData("age", "90")
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever())
            .Build();

        var scheduler = await schedulerFactory.GetScheduler();

        await scheduler.ScheduleJob(job, trigger);
        return "";
    }

}
