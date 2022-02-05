using Microsoft.AspNetCore.Mvc;
using GrpcClient;

namespace AspClient.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly Greeter.GreeterClient _client;

    public WeatherForecastController(ILogger<WeatherForecastController> logger,
    Greeter.GreeterClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<string> Get()
    {
        var request = new HelloRequest { Name = "AspClient" };
        var call = await _client.SayHelloAsync(request);
        return call.Message;
    }
}
