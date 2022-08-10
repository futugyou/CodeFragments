using Hangfire;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HangfireCapDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly ICapJob _capJob;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, ICapJob capJob)
    {
        _logger = logger;
        _capJob = capJob;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        var o = new RecurringJobOptions();

        var jobItem = new CapJobItem();
        jobItem.JobName = "thisiscapjob";
        jobItem.CapEventName = "make.food";
        jobItem.RetryTimes = 3;
        jobItem.Cron = "0/15 * * * * *";
        //jobItem.Data = new { Name = "cake", Price = 2.3 };
        jobItem.Data = new { Name = "cake", Price = 2.3 };
        jobItem.RecurringJobIdentifier = "thisiscapjob";

        RecurringJob.AddOrUpdate(jobItem.RecurringJobIdentifier, () => _capJob.Excute(jobItem, null, null, false, null), jobItem.Cron, o);
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
