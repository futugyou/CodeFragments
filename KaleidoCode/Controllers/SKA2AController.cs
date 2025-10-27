
using A2A;
using KaleidoCode.KernelService;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace KaleidoCode.Controllers;

[Experimental("SKEXP0011")]
[Route("api/sk/a2a")]
[ApiController]
public class SKA2AController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly SemanticKernelOptions _options;

    public SKA2AController(Kernel kernel, IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        _kernel = kernel;
        _options = optionsMonitor.CurrentValue;
    }

    [Route("shop")]
    [HttpPost]
    public async IAsyncEnumerable<string> Concurrent(string text = "Give the fruit with a unit price greater than 10 yuan!")
    {
        var cardResolver = new A2ACardResolver(new Uri("http://localhost:5000"));
        var agentCard = await cardResolver.GetAgentCardAsync();
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        yield return JsonSerializer.Serialize(agentCard, options);

        var client = new A2AClient(new Uri(agentCard.Url));
        var a2aResponse = await client.SendMessageAsync(new MessageSendParams
        {
            Message = new AgentMessage
            {
                Role = MessageRole.User,
                Parts = [new TextPart { Text = text }]
            }
        });

        if (a2aResponse is AgentTask)
        {
            var message = a2aResponse as AgentTask;
            yield return JsonSerializer.Serialize(message, options);
        }

        yield return "ok!";
    }

}
