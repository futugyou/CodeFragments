
using System.Threading.Tasks;

namespace OpenSearchStack.Analyzer;

public class TestAnalyzerService
{
    public TestAnalyzerService(ILogger<TestAnalyzerService> log, OpenSearchClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly OpenSearchClient client;
    private readonly ILogger<TestAnalyzerService> log;

    public async IAsyncEnumerable<string> TestAnalyzer()
    {
        var analyzeResponse = await client.Indices.AnalyzeAsync(a => a
            .Tokenizer("standard")
            .Filter("lowercase", "stop")
            .Text("F# is THE SUPERIOR language :)")
        );
        foreach (var analyzeToken in analyzeResponse.Tokens)
        {
            yield return $"{analyzeToken.Token}";
        }
        
        await client.Indices.CloseAsync("analysis-index");

        var updateIndexSettingsResponse = await client.Indices.UpdateSettingsAsync("analysis-index", i => i
              .IndexSettings(s => s
                  .Analysis(a => a
                      .CharFilters(cf => cf
                          .Mapping("my_char_filter", m => m
                              .Mappings("F# => FSharp")
                          )
                      )
                      .TokenFilters(tf => tf
                          .Synonym("my_synonym", sf => sf
                              .Synonyms("superior, great")

                          )
                      )
                      .Analyzers(an => an
                          .Custom("my_analyzer", ca => ca
                              .Tokenizer("standard")
                              .CharFilters("my_char_filter")
                              .Filters("lowercase", "stop", "my_synonym")
                          )
                      )

                  )
              )
          );

        yield return updateIndexSettingsResponse.Acknowledged.ToString();

        var openIndexResponse = await client.Indices.OpenAsync("analysis-index");
        yield return openIndexResponse.Acknowledged.ToString();

        var clusterHealthResponse = await client.Cluster.HealthAsync("analysis-index", h => h
                .WaitForStatus(WaitForStatus.Green)
                .Timeout(TimeSpan.FromSeconds(5))
           );
        yield return clusterHealthResponse.Status.ToString();

        analyzeResponse = await client.Indices.AnalyzeAsync(a => a
            .Index("analysis-index")
            .Analyzer("my_analyzer")
            .Text("F# is THE SUPERIOR language :)")
        );
        foreach (var analyzeToken in analyzeResponse.Tokens)
        {
            yield return $"{analyzeToken.Token}";
        }
    }

}