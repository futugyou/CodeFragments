using Elasticsearch.Net;
using Nest;

namespace KaleidoCode.Elasticsearch;

public class TestAnalyzerService
{
    public TestAnalyzerService(ILogger<TestAnalyzerService> log, ElasticClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly ElasticClient client;
    private readonly ILogger<TestAnalyzerService> log;

    public void TestAnalyzer()
    {
        var analyzeResponse = client.Indices.Analyze(a => a
            .Tokenizer("standard")
            .Filter("lowercase", "stop")
            .Text("F# is THE SUPERIOR language :)")
        );
        foreach (var analyzeToken in analyzeResponse.Tokens)
        {
            Console.WriteLine($"{analyzeToken.Token}");
        }
        client.Indices.Close("analysis-index");

        client.Indices.UpdateSettings("analysis-index", i => i
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

        client.Indices.Open("analysis-index");
        client.Cluster.Health("analysis-index", h => h
             .WaitForStatus(WaitForStatus.Green)
             .Timeout(TimeSpan.FromSeconds(5))
        );

        analyzeResponse = client.Indices.Analyze(a => a
            .Index("analysis-index")
            .Analyzer("my_analyzer")
            .Text("F# is THE SUPERIOR language :)")
        );
        foreach (var analyzeToken in analyzeResponse.Tokens)
        {
            Console.WriteLine($"{analyzeToken.Token}");
        }
    }

}