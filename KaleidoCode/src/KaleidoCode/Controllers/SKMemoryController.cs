
using KernelMemoryStack;
using Microsoft.KernelMemory;
namespace KaleidoCode.Controllers;

[Experimental("SKEXP0011")]
[Route("api/sk/memory")]
[ApiController]
public class SKMemoryController : ControllerBase
{
    private readonly IKernelMemory _kernelMemory;
    private readonly KernelMemoryOptions _options;

    public SKMemoryController(IKernelMemory kernelMemory, IOptionsMonitor<KernelMemoryOptions> optionsMonitor)
    {
        _kernelMemory = kernelMemory;
        _options = optionsMonitor.CurrentValue;
    }

    [Route("memclientweb")]
    [HttpPost]
    public async Task<string> ClientImportWeb(string url, string documentId, string question)
    {
        var memory = new MemoryWebClient(endpoint: _options.KernelMemory.Endpoint, apiKey: _options.KernelMemory.ApiKey);
        await memory.ImportWebPageAsync(url);

        while (!await memory.IsDocumentReadyAsync(documentId))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

        var answer = await memory.AskAsync(question);

        return answer.Result;
    }

    [Route("memweb")]
    [HttpPost]
    public async Task<string> ImportWeb(string url, string documentId, string question)
    {
        await _kernelMemory.ImportWebPageAsync(url);

        while (!await _kernelMemory.IsDocumentReadyAsync(documentId))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
        }

        var answer = await _kernelMemory.AskAsync(question);

        return answer.Result;
    }

}
