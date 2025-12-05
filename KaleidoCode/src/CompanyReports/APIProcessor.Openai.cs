
namespace CompanyReports;

public class OpenaiProcessor : IAPIProcessor
{
    public OpenaiProcessor([FromKeyedServices("report")] OpenAIClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client), "OpenAIClient cannot be null.");
    }

    private readonly OpenAIClient _client;
    private ResponseData _responseData;

    public string Provider => "openai";
    public string DefaultModel => "gpt-4o-2024-08-06";
    public ResponseData ResponseData => _responseData;

    public async Task<RephrasedQuestions> SendMessageAsync(string model = "gpt-4o-2024-08-06", float temperature = 0.5f, long? seed = null, string systemContent = "You are a helpful assistant.", string humanContent = "Hello!", bool isStructured = false, object? responseFormat = null, Dictionary<string, object>? kwargs = null, CancellationToken cancellationToken = default)
    {
        var client = _client.GetChatClient(model ?? DefaultModel).AsIChatClient();
        var history = new ChatMessage[]
        {
            new(ChatRole.System, systemContent),
            new(ChatRole.User, humanContent)
        };
        var options = new ChatOptions
        {
            Temperature = temperature,
            Seed = seed,
            ResponseFormat = ChatResponseFormat.Json,
        };
        var content = await client.GetResponseAsync(history, options, cancellationToken: cancellationToken);

        _responseData = new ResponseData
        {

            Model = model ?? DefaultModel,
            InputTokens = content?.Usage?.InputTokenCount ?? 0,
            OutputTokens = content?.Usage?.OutputTokenCount ?? 0
        };

        var data = content?.Text ?? "{}";
        return JsonSerializer.Deserialize<RephrasedQuestions>(data) ?? new();
    }
}
