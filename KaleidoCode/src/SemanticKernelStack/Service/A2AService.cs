
using A2A;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace SemanticKernelStack.Services;

public class A2AService
{
    private readonly Kernel _kernel;
    private readonly SemanticKernelOptions _options;
    public A2AService(Kernel kernel, IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        _kernel = kernel;
        _options = optionsMonitor.CurrentValue;
    }

    public async IAsyncEnumerable<string> Concurrent(string text)
    {
        var cardResolver = new A2ACardResolver(new Uri(_options.A2AEndpoint));
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
    }
}