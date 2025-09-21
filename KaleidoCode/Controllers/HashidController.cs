using System.Text.Json.Serialization;
using KaleidoCode.Extensions;

namespace KaleidoCode.Controllers;

[ApiController]
[Route("[controller]")]
public class HashidController : ControllerBase
{
    public class UserDto
    {
        [JsonConverter(typeof(HashIdJsonConverter))]
        public int Id { get; set; }

        public string Name { get; set; }
    }


    [HttpGet]
    public IEnumerable<UserDto> Get()
    {
        return new[] { new UserDto { Id = 12345, Name = "test" } };
    }

    [HttpGet("{id}")]
    public UserDto Get([ModelBinder(typeof(HashIdModelBinder))] int id)
    {
        return new UserDto { Id = id, Name = "testid: " + id };
    }
}
