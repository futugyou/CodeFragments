
using SemanticKernelStack.Services;

namespace KaleidoCode.Controllers;

[Route("api/sk/prompt")]
[ApiController]
public class SKPromptController : ControllerBase
{
    private readonly PromptService _service;
    public SKPromptController(PromptService service)
    {
        _service = service;
    }

    [Route("base")]
    [HttpGet]
    public async Task<string[]> PromptBase()
    {
        return await _service.PromptBase();
    }

    [Route("liquid")]
    [HttpGet]
    public async Task<string[]> Liquid()
    {
        return await _service.Liquid();
    }

    [Route("semantic-kernel")]
    [HttpGet]
    public async Task<string[]> SemanticKernelTemplate()
    {
        return await _service.SemanticKernelTemplate();
    }

    [Route("handlebars")]
    [HttpGet]
    public async Task<string[]> PromptHandlebars()
    {
        return await _service.PromptHandlebars();
    }

    [Route("yaml")]
    [HttpGet]
    public async Task<string[]> PromptYAML()
    {
        return await _service.PromptYAML();
    }
}
