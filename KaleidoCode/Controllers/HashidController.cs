
using KaleidoCode.RefitClient;
using System.Text.Json.Serialization;
using KaleidoCode.Extensions;

namespace KaleidoCode.Controllers;

[ApiController]
[Route("api")]
public class HomeController : ControllerBase
{
    private readonly IGitHubApi _gitHubApi;

    public HomeController(IGitHubApi gitHubApi)
    {
        _gitHubApi = gitHubApi;
    }

    [HttpGet("hashid")]
    public IEnumerable<UserDto> Get()
    {
        return [new UserDto { Id = 12345, Name = "test" }];
    }

    [HttpGet("hashid/{id}")]
    public UserDto Get([ModelBinder(typeof(HashIdModelBinder))] int id)
    {
        return new UserDto { Id = id, Name = "testid: " + id };
    }

    [HttpGet("bar/{x}/{y}/{z}")]
    public ValueTask<Result> Bar(string x, int y, double z) => ValueTask.FromResult(new Result(x, y, z));

    [HttpGet("/refit")]
    public async Task<dynamic> Refit()
    {
        return await _gitHubApi.GetUser("mojombo");
    }
}

public record Result(string X, int Y, double Z);

public class UserDto
{
    [JsonConverter(typeof(HashIdJsonConverter))]
    public int Id { get; set; }

    public string Name { get; set; }
}