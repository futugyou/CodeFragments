
using System.Reflection;
using SemanticKernelStack.Services;

namespace KaleidoCode.Controllers;

[Route("api/sk/token")]
[ApiController]
public class SKTokenCounterController : ControllerBase
{
    private readonly TokenCounterService _service;
    private const string text = @"The rise of city-states and the division of the empire (185-307),
185: The mayoral system was established in the city of Muse, and border disputes began to emerge,
212: Tulipa (Two Rivers City) adopted a two-chamber parliamentary system and transformed into a regional power.";

    public SKTokenCounterController(TokenCounterService service)
    {
        _service = service;
    }

    [Route("no-token")]
    [HttpGet]
    public List<string> NoToken()
    {
        return _service.NoToken(text);
    }

    [Route("sharp")]
    [HttpGet]
    public List<string> Sharp()
    {
        return _service.Sharp(text);
    }

    [Route("ml-token")]
    [HttpGet]
    public List<string> MlToken()
    {
        Assembly assembly = typeof(SKTokenCounterController).Assembly;
        Stream vocabStream = assembly.GetManifestResourceStream("vocab.bpe")!;
        return _service.MlToken(text, vocabStream);
    }

    [Route("ml-roberta")]
    [HttpGet]
    public List<string> Roberta()
    {
        Assembly assembly = typeof(SKTokenCounterController).Assembly;
        Stream vocabularyStream = assembly.GetManifestResourceStream("encoder.json")!;
        Stream mergeStream = assembly.GetManifestResourceStream("vocab.bpe")!;
        Stream highestOccurrenceMappingStream = assembly.GetManifestResourceStream("dict.txt")!;
        return _service.Roberta(text, vocabularyStream, mergeStream, highestOccurrenceMappingStream);
    }
}
