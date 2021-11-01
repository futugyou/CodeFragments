using Microsoft.AspNetCore.Mvc;

namespace AspnetcoreEx.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IGitHubApi _gitHubApi;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration, IGitHubApi gitHubApi)
    {
        _logger = logger;
        _configuration = configuration;
        _gitHubApi = gitHubApi;
    }


    [HttpGet("/refit")]
    public async Task<dynamic> Refit()
    {
        return await _gitHubApi.GetUser("mojombo");
    }
    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        Console.WriteLine(_configuration["Client:Password"]);
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
