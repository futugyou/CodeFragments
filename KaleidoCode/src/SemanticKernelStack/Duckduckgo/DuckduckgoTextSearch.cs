
using  SemanticKernelStack.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Data;

namespace SemanticKernelStack.Duckduckgo;

public class DuckduckgoTextSearch : ITextSearch
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly Uri? _uri = null;
    private const string DefaultUri = "https://html.duckduckgo.com/html";
    public DuckduckgoTextSearch(DuckTextSearchOptions? options = null)
    {
        _uri = options?.Endpoint ?? new Uri(DefaultUri);
        _logger = options?.LoggerFactory?.CreateLogger(typeof(DuckduckgoTextSearch)) ?? NullLogger.Instance;
        _httpClient = options?.HttpClient ?? HttpClientProvider.GetHttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "User-Agent");
        _httpClient.DefaultRequestHeaders.Add("Referer", "https://html.duckduckgo.com/");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
        _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
    }

    public async Task<KernelSearchResults<object>> GetSearchResultsAsync(string query, TextSearchOptions? searchOptions = null, CancellationToken cancellationToken = default)
    {
        searchOptions ??= new TextSearchOptions();
        List<DuckSearchResult> searchResponse = await ExecuteSearchAsync(query, searchOptions, cancellationToken).ConfigureAwait(false);
        long totalCount = searchResponse.Count;
        return new KernelSearchResults<object>(GetResultsAsWebPageAsync(searchResponse), totalCount);
    }

    public async Task<KernelSearchResults<TextSearchResult>> GetTextSearchResultsAsync(string query, TextSearchOptions? searchOptions = null, CancellationToken cancellationToken = default)
    {
        searchOptions ??= new TextSearchOptions();
        List<DuckSearchResult> searchResponse = await ExecuteSearchAsync(query, searchOptions, cancellationToken).ConfigureAwait(false);
        long totalCount = searchResponse.Count;
        return new KernelSearchResults<TextSearchResult>(GetResultsAsTextSearchResultAsync(searchResponse), totalCount);
    }

    public async Task<KernelSearchResults<string>> SearchAsync(string query, TextSearchOptions? searchOptions = null, CancellationToken cancellationToken = default)
    {
        searchOptions ??= new TextSearchOptions();
        List<DuckSearchResult> searchResponse = await ExecuteSearchAsync(query, searchOptions, cancellationToken).ConfigureAwait(false);
        long totalCount = searchResponse.Count;
        return new KernelSearchResults<string>(GetResultsAsStringAsync(searchResponse), totalCount);
    }

    private async Task<List<DuckSearchResult>> ExecuteSearchAsync(string query, TextSearchOptions searchOptions, CancellationToken cancellationToken)
    {
        var form = new List<KeyValuePair<string, string>> { new("q", query), new("b", ""), new("kl", ""), new("df", "") };
        if (searchOptions.Skip > 0)
        {
            form.Add(new KeyValuePair<string, string>("s", searchOptions.Skip.ToString()));
        }
        while (true)
        {
            var content = new FormUrlEncodedContent(form);
            var response = await _httpClient.PostAsync(_uri, content, cancellationToken);
            var resultString = await response.Content.ReadAsStringAsync(cancellationToken);
            var results = DuckHTMLTool.GenerationDuckSearchResult(resultString, form);

            if (form.Count == 0)
            {
                return results;
            }
        }
    }

    private static async IAsyncEnumerable<string> GetResultsAsStringAsync(List<DuckSearchResult> searchResponse)
    {

        foreach (var webPage in searchResponse)
        {
            yield return webPage.Snippet;
            await Task.Yield();
        }
    }

    private static async IAsyncEnumerable<TextSearchResult> GetResultsAsTextSearchResultAsync(List<DuckSearchResult> searchResponse)
    {
        foreach (var webPage in searchResponse)
        {
            yield return new TextSearchResult(webPage.Title)
            {
                Value = webPage.Snippet,
                Link = webPage.Url,
            };
            await Task.Yield();
        }
    }

    private static async IAsyncEnumerable<object> GetResultsAsWebPageAsync(List<DuckSearchResult> searchResponse)
    {
        foreach (var webPage in searchResponse)
        {
            yield return webPage;
            await Task.Yield();
        }
    }
}
