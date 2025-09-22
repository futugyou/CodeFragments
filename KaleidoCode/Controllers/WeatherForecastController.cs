
using KaleidoCode.RefitClient;

namespace KaleidoCode.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IGitHubApi _gitHubApi;
    private readonly TestOption _options;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration, IGitHubApi gitHubApi,
     IOptionsMonitor<TestOption> optionsMonitor)
    {
        _logger = logger;
        _configuration = configuration;
        _gitHubApi = gitHubApi;
        _options = optionsMonitor.CurrentValue;
    }


    [HttpGet("/refit")]
    public async Task<dynamic> Refit()
    {
        return await _gitHubApi.GetUser("mojombo");
    }
    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        if (_options != null)
        {
            foreach (var item in _options.Services)
            {
                Console.WriteLine("test option " + item);
            }
        }
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

public class TestOption
{
    public string[] Services { get; set; }
}