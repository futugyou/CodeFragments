using Nest;

namespace AspnetcoreEx.Elasticsearch;
public class AnalyzerService
{
    public AnalyzerService(ILogger<AnalyzerService> log, ElasticClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly ElasticClient client;
    private readonly ILogger<AnalyzerService> log;

    public void CreateBaseAnalyzer()
    {
        var createIndexResponse = client.Indices.Create("person-index-with-analyzer", c => c
            .Map<Person>(mm => mm
                .Properties(p => p
                    .Text(t => t
                        .Name(n => n.FirstName)
                        .Analyzer("whitespace")
                    )
                )
            )
        );
    }

    public void CreateCustomAnalyzer()
    {
        var createIndexResponse = client.Indices.Create("questions", c => c
            .Settings(s => s
                .Analysis(a => a
                    .CharFilters(cf => cf
                        // map both C# and c# to "CSharp" and "csharp", respectively 
                        // (so the # is not stripped by the tokenizer)
                        .Mapping("programming_language", mca => mca
                            .Mappings(new[]
                            {
                                "c# => csharp",
                                "C# => Csharp"
                            })
                        )
                    )
                    .Analyzers(an => an
                        .Custom("index_question", ca => ca
                            // strip HTML tags
                            .CharFilters("html_strip", "programming_language")
                            // tokenize using the standard tokenizer
                            .Tokenizer("standard")
                            // filter tokens with the standard token filter. 
                            // 1. lowercase tokens
                            // 2. remove stop word tokens
                            .Filters("lowercase", "stop")
                        )
                        .Custom("search_question", ca => ca
                            .CharFilters("programming_language")
                            .Tokenizer("standard")
                            .Filters("lowercase", "stop")
                        )
                    )
                )
            )
            .Map<Question>(mm => mm
                .AutoMap()
                .Properties(p => p
                    .Text(t => t
                        .Name(n => n.Body)
                        .Analyzer("index_question")
                        .SearchAnalyzer("search_question")
                    )
                )
            )
        );
    }
}