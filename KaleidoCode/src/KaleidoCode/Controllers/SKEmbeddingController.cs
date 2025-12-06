
using SemanticKernelStack.Services;

namespace KaleidoCode.Controllers;

[Route("api/sk/embedding")]
[ApiController]
public class SKEmbeddingController : ControllerBase
{
    private readonly EmbeddingService _service;
    public SKEmbeddingController(EmbeddingService service)
    {
        _service = service;
    }

    [Route("search")]
    [HttpGet]
    public IAsyncEnumerable<string> EmbeddingSearch(string input)
    {
        return _service.EmbeddingSearch(input);
    }

    [Route("create")]
    [HttpPost]
    public IAsyncEnumerable<string> EmbeddingCreate()
    {
        return _service.EmbeddingCreate();
    }
}
