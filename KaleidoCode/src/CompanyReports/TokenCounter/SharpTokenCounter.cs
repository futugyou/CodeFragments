
namespace CompanyReports.TokenCounter;

public class SharpTokenCounter : ITokenCounter
{
    private readonly ConcurrentDictionary<string, SharpToken.GptEncoding> _encodingCache = new();

    public SharpTokenCounter()
    {
        _encodingCache["cl100k_base"] = SharpToken.GptEncoding.GetEncoding("cl100k_base");
    }

    public int Count(string text, string encodingName = "cl100k_base")
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var encoding = _encodingCache.GetOrAdd(encodingName, SharpToken.GptEncoding.GetEncoding);
        return encoding.Encode(text).Count;
    }
}
