
using KernelMemoryStack.Services;

namespace KaleidoCode.Controllers;

[Experimental("SKEXP0011")]
[Route("api/km/memory")]
[ApiController]
public class KernelMemoryController : ControllerBase
{
    private readonly WebImportService _service;

    public KernelMemoryController(WebImportService service)
    {
        _service = service;
    }

    [Route("memclientweb")]
    [HttpPost]
    public async Task<string> ClientImportWeb(string url, string documentId, string question)
    {
        return await _service.ClientImportWeb(url, documentId, question);
    }

    [Route("memweb")]
    [HttpPost]
    public async Task<string> ImportWeb(string url, string documentId, string question)
    {
        return await _service.ImportWeb(url, documentId, question);
    }

}
