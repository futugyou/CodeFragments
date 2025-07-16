
using System.ComponentModel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Google;

namespace AspnetcoreEx.KernelService.Skills;

[Experimental("SKEXP0011")]
public class WebSearch(IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
{
    private readonly IOptionsMonitor<SemanticKernelOptions> _options = optionsMonitor;

    [KernelFunction("simple_web_search")]
    [Description("Use the corresponding browser engine to search the web page and only return the simple text collection")]
    public async Task<List<string>> SimpleWebSearchAsync(string engine = "bing", string query = "", int count = 1)
    {
        // SK do not support IAsyncEnumerable<string> as function.
        // System.NotSupportedException: The type 'AspnetcoreEx.KernelService.Skills.WebSearch+<CommonWebSearchAsync>d__2' can only be serialized using async serialization methods. Path: $.
        SemanticKernelOptions options = _options.CurrentValue;
        ITextSearch textSearch = engine switch
        {
            "bing" => new BingTextSearch(apiKey: options.WebSearch.BingApiKey),
            "google" => new GoogleTextSearch(searchEngineId: options.WebSearch.GoogleSearchEngineId, apiKey: options.WebSearch.GoogleApiKey),
            _ => throw new Exception("No suitable search engine"),
        };
        var stringResults = await textSearch.SearchAsync(query, new() { Top = count, Skip = 0 });
        List<string> result = [];
        await foreach (var res in stringResults.Results)
        {
            result.Add(res);
        }
        return result;
    }

    [KernelFunction("common_web_search")]
    [Description("Use the corresponding browser engine to search the web page and return a collection containing name, link, and value.")]
    public async Task<List<TextSearchResult>> CommonWebSearchAsync(string engine = "bing", string query = "", int count = 1)
    {
        SemanticKernelOptions options = _options.CurrentValue;
        ITextSearch textSearch = engine switch
        {
            "bing" => new BingTextSearch(apiKey: options.WebSearch.BingApiKey),
            "google" => new GoogleTextSearch(searchEngineId: options.WebSearch.GoogleSearchEngineId, apiKey: options.WebSearch.GoogleApiKey),
            _ => throw new Exception("No suitable search engine"),
        };
        var stringResults = await textSearch.GetTextSearchResultsAsync(query, new() { Top = count, Skip = 0 });
        List<TextSearchResult> result = [];
        await foreach (var res in stringResults.Results)
        {
            result.Add(res);
        }
        return result;
    }

}
