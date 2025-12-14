
using SemanticKernelStack.Services;

namespace KaleidoCode.Controllers;

[Route("api/sk/a2a")]
[ApiController]
public class SKA2AController : ControllerBase
{
    private readonly A2AService _service;
    public SKA2AController(A2AService service)
    {
        _service = service;
    }

    [Route("shop")]
    [HttpPost]
    public IAsyncEnumerable<string> Concurrent(string text = "Give the fruit with a unit price greater than 10 yuan!")
    {
        return _service.Concurrent(text);
    }

}
