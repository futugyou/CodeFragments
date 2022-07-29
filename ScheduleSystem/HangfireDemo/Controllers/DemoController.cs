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
    private readonly IRecurringJobManager _recurringJobManager;

    public DemoController(ILogger<DemoController> logger, IBackgroundJobClient backgroundJob, IRecurringJobManager recurringJobManager)
    {
        _logger = logger;
        _backgroundJob = backgroundJob;
        _recurringJobManager = recurringJobManager;
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
        
        // this will not run, because not have queue named 'static_method'
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
        var jobname = BackgroundJob.Enqueue<IDosomething>(x => x.HaveReturnValue());
        //var jobname = _backgroundJob.Enqueue<IDosomething>(x => x.HaveReturnValue());
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
        //RecurringJob.AddOrUpdate<IDosomething>("thisiscronjob", x => x.DisplayTime(), Cron.Minutely);
        _recurringJobManager.AddOrUpdate<IDosomething>("thisiscronjob", x => x.DisplayTime(), Cron.Minutely);
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

    [HttpGet("long")]
    public void LongRunning()
    {
        _recurringJobManager.AddOrUpdate<IDosomething>("LongRunning", x => x.LongRunningJob(), "0/10 * * * * ? ");
    }
}
