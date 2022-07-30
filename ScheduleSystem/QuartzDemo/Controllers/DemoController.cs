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
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever())
            .Build();

        var scheduler = await schedulerFactory.GetScheduler();

        await scheduler.ScheduleJob(job, trigger);
        return "";
    }
}
