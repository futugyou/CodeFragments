
using SemanticKernelStack.Services;

namespace KaleidoCode.Controllers;

[Route("api/sk/plugins")]
[ApiController]
public class SKPluginsController : ControllerBase
{
    private readonly PluginsService _service;

    public SKPluginsController(PluginsService service)
    {
        _service = service;
    }


    [Route("google")]
    [HttpPost]
    public async Task<string> GoogleSearchAsPlugin(string input, bool isFunctionCall = false)
    {
        return await _service.GoogleSearchAsPlugin(input, isFunctionCall);
    }

    [Route("duck")]
    [HttpPost]
    public async Task<IAsyncEnumerable<string>> Dock(string input)
    {
        return await _service.Dock(input);
    }

    [Route("search")]
    [HttpPost]
    public async Task<string[]> WebSearch(string input)
    {
        return await _service.WebSearch(input);
    }

    [Route("infr-project-platforms-count")]
    [HttpPost]
    public async Task<string[]> InfrProjectPlatformsCount()
    {
        return await _service.InfrProjectPlatformsCount();
    }

    [Route("data-generator")]
    [HttpPost]
    public async Task<string[]> DataGenerator(string input)
    {
        return await _service.DataGenerator(input);
    }

    [Route("light-controller")]
    [HttpPost]
    public async Task<string[]> LightController()
    {
        return await _service.LightController();
    }

    [Route("email-sender")]
    [HttpPost]
    public async Task<string[]> EmailSender()
    {
        return await _service.EmailSender();
    }

    [Route("math-executor")]
    [HttpPost]
    public async Task<string[]> MathExecutor()
    {
        return await _service.MathExecutor();
    }

    [Route("word-reader")]
    [HttpPost]
    public async Task<string> WordReader(string filePath)
    {
        return await _service.WordReader(filePath);
    }

    [Route("file")]
    [HttpPost]
    public async Task<string[]> CallFilePlugin()
    {
        return await _service.CallFilePlugin();
    }

    [Route("summary")]
    [HttpPost]
    public async Task<string[]> Summary()
    {
        return await _service.Summary();
    }
}
