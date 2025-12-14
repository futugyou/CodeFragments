
using SemanticKernelStack.Services;

namespace KaleidoCode.Controllers;

[Route("api/sk/declarative")]
[ApiController]
public class SKDeclarativeController : ControllerBase
{
    private readonly DeclarativeService _service;

    public SKDeclarativeController(DeclarativeService service)
    {
        _service = service;
    }

    [Route("chat")]
    [HttpPost]
    public IAsyncEnumerable<string> Chat(string query = "Cats and Dogs")
    {
        return _service.Chat(query);
    }

    [Route("func")]
    [HttpPost]
    public IAsyncEnumerable<string> Function(string query = "Can you tell me the status of all the lights?")
    {
        return _service.Function(query);
    }

    [Route("template")]
    [HttpPost]
    public IAsyncEnumerable<string> Template(string topic = "Dogs", string length = "3")
    {
        return _service.Template(topic, length);
    }
}
