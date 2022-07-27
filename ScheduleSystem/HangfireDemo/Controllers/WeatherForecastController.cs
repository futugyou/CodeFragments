using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace HangfireDemo.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IBackgroundJobClient _backgroundJob;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IBackgroundJobClient  backgroundJob)
    {
        _logger = logger;
        _backgroundJob = backgroundJob;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        // fire and forgot
        _backgroundJob.Enqueue(() => Console.WriteLine("Hello world from Hangfire!"));
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
