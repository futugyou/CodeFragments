
using System.ComponentModel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web.Google;

namespace AspnetcoreEx.KernelService.Skills;

[Experimental("SKEXP0011")]
public class WebSearch(IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
{
    private readonly IOptionsMonitor<SemanticKernelOptions> _options = optionsMonitor;

    [KernelFunction("common_web_search")]
    [Description("Use the corresponding browser engine to search the web page and only return the text collection")]
    public async IAsyncEnumerable<string> CommonWebSearchAsync(string engine = "bing", string query = "", int count = 10)
    {
        SemanticKernelOptions options = _options.CurrentValue;
        ITextSearch textSearch = engine switch
        {
            "bing" => new BingTextSearch(apiKey: options.WebSearch.BingApiKey),
            "google" => new GoogleTextSearch(searchEngineId: options.WebSearch.GoogleSearchEngineId, apiKey: options.WebSearch.BingApiKey),
            _ => throw new Exception("No suitable search engine"),
        };
        var stringResults = await textSearch.SearchAsync(query, new() { Top = count, Skip = 0 });
        await foreach (var result in stringResults.Results)
        {
            yield return result;
        }
    }
}