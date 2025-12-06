
using SemanticKernelStack.Services;

namespace KaleidoCode.Controllers;

[Route("api/sk/process")]
[ApiController]
public class SKProcessController : ControllerBase
{
    private readonly ProcessService _service;

    public SKProcessController(ProcessService service)
    {
        _service = service;
    }

    // https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/GettingStartedWithProcesses/Step01
    [Route("sample")]
    [HttpPost]
    public IAsyncEnumerable<string> Sample()
    {
        return _service.Sample();
    }
}