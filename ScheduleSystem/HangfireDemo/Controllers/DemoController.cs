using Hangfire;
using HangfireDemo.Service;
using Microsoft.AspNetCore.Mvc;

namespace HangfireDemo.Controllers;

[ApiController]
[Route("api/[controller]/")]
public class DemoController : ControllerBase
{
    private readonly ILogger<DemoController> _logger;
    private readonly IBackgroundJobClient _backgroundJob;

    public DemoController(ILogger<DemoController> logger, IBackgroundJobClient backgroundJob)
    {
        _logger = logger;
        _backgroundJob = backgroundJob;
    }

    /// <summary>
    /// fire and forgot
    /// </summary>
    /// <returns></returns>
    [HttpGet("fire1")]
    public string FireAndForgot1()
    {
        // static method
        // var jobid1 = _backgroundJob.Enqueue(() => Console.WriteLine("Hello world from Hangfire!"));
        var jobname = _backgroundJob.Enqueue("static_method", () => Console.WriteLine("Hello world from Hangfire!"));
        Console.WriteLine(jobname);
        return jobname;
    }

    /// <summary>
    /// fire and forgot
    /// </summary>
    /// <returns></returns>
    [HttpGet("fire2")]
    public string FireAndForgot2()
    {
        // instance method 
        var jobname = _backgroundJob.Enqueue<IDosomething>(x => x.HaveReturnValue());
        Console.WriteLine(jobname);
        return jobname;
    }
    /// <summary>
    /// fire and forgot
    /// </summary>
    /// <returns></returns>
    [HttpGet("fire3")]
    public string FireAndForgot3()
    {
        //  instance method with parameter
        var jobname = _backgroundJob.Enqueue<IDosomething>(x => x.ReadMessage("this is message"));
        Console.WriteLine(jobname);
        return jobname;
    }

    [HttpGet("schedule")]
    public string Schedule()
    {
        // schedule
        var jobid1 = _backgroundJob.Schedule<IDosomething>(x => x.Delaywork(), TimeSpan.FromSeconds(10));
        return jobid1;
    }

    [HttpGet("cron1")]
    public string Cronjob1()
    {
        RecurringJob.AddOrUpdate<IDosomething>("thisiscronjob", x => x.DisplayTime(), Cron.Minutely);
        return "thisiscronjob";
    }

    [HttpGet("cron2")]
    public string Cronjob2()
    {
        var p = new JobParaemter { Name = "tee", Age = 10 };
        RecurringJob.AddOrUpdate<IDosomething>("jobwithcron", x => x.ChangeAge(p), "0/15 * * * * ? ");
        return "jobwithcron";
    }

    [HttpGet("trigger")]
    public void Triggerjob()
    {
        RecurringJob.Trigger("thisiscronjob");
    }

    [HttpGet("delete")]
    public void Deletejob()
    {
        RecurringJob.RemoveIfExists("jobwithcron");
    }

    [HttpGet("error")]
    public void Errorjob()
    {
        _backgroundJob.Enqueue<IDosomething>(x => x.ErrorMethod());
    }
    [HttpGet("error2")]
    public void Errorjob2()
    {
        _backgroundJob.Enqueue<IDosomething>(x => x.ErrorMethodWithLimit());
    }
}
