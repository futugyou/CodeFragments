

namespace KaleidoCode.KernelService.Duckduckgo;

public class DuckTextSearchOptions
{
    public Uri? Endpoint { get; init; } = null;
    public HttpClient? HttpClient { get; init; } = null;
    public ILoggerFactory? LoggerFactory { get; init; } = null;
}