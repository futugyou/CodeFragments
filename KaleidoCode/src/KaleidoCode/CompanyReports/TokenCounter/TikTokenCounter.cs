
namespace CompanyReports.TokenCounter;

public class TikTokenCounter : ITokenCounter
{
    private readonly ConcurrentDictionary<string, Tiktoken.Encoder> _encodingCache = new();

    public TikTokenCounter()
    {
        _encodingCache["gpt-4o"] = ModelToEncoder.For("gpt-4o");
    }

    public int Count(string text, string encodingName = "gpt-4o")
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var encoding = _encodingCache.GetOrAdd(encodingName, ModelToEncoder.For);
        return encoding.CountTokens(text);
    }
}
