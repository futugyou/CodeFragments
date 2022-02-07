using Microsoft.AspNetCore.Mvc;
using GrpcClient;
using Grpc.Core;

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
    private readonly IHttpClientFactory _clientFactory;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        Greeter.GreeterClient client,
        IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _client = client;
        _clientFactory = clientFactory;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<string> Get()
    {
        var client = _clientFactory.CreateClient();
        client.BaseAddress = new Uri("http://localhost:50002");
        var tokenResponse = await client.GetAsync("generateJwtToken?name=authuser");
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadAsStringAsync();

        var headers = new Metadata();
        headers.Add("Authorization", $"Bearer {token}");

        var request = new HelloRequest { Name = "AspClient" };
        var call = await _client.SayHelloAsync(request, headers);
        //var call = await _client.SayHelloAsync(request, deadline: DateTime.UtcNow.AddSeconds(5));
        return call.Message;
    }
}
