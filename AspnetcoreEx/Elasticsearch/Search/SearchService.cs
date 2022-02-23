
using Nest;

namespace AspnetcoreEx.Elasticsearch;
public class SearchService
{
    public SearchService(ILogger<SearchService> log, ElasticClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly ElasticClient client;
    private readonly ILogger<SearchService> log;

    public void MatchAll()
    {
        // 1
        var searchResponse = client.Search<Person>(s => s
            .Query(q => q
                .MatchAll()
            )
        );
        // 2
        searchResponse = client.Search<Person>(s => s
            .MatchAll()
        );
        //3 
        var searchRequest = new SearchRequest<Person>
        {
            Query = new MatchAllQuery()
        };

        searchResponse = client.Search<Person>(searchRequest);
    }

    public void StructuredSearch()
    {
        var searchResponse = client.Search<Person>(s => s
           .Query(q => q
               .DateRange(r => r
                   .Field(f => f.BrithDay)
                   .GreaterThanOrEquals(new DateTime(2017, 01, 01))
                   .LessThan(new DateTime(2018, 01, 01))
               )
           )
        );

        searchResponse = client.Search<Person>(s => s
            .Query(q => q
                .Bool(b => b
                    .Filter(bf => bf
                        .DateRange(r => r
                            .Field(f => f.BrithDay)
                            .GreaterThanOrEquals(new DateTime(2017, 01, 01))
                            .LessThan(new DateTime(2018, 01, 01))
                        )
                    )
                )
            )
        );
    }
    public void UnstructuredSearch()
    {
        // full text queries 
        var searchResponse = client.Search<Person>(s => s
            .Query(q => q
                .Match(m => m
                    .Field(f => f.FirstName)
                    .Query("Russ")
                )
            )
        );
    }

    public void CombiningSearch()
    {
        var searchResponse = client.Search<Person>(s => s
           .Query(q => q
               .Bool(b => b
                   // running in a query context, have score 
                   .Must(mu => mu
                       .Match(m => m
                           .Field(f => f.FirstName)
                           .Query("Russ")
                       ), mu => mu
                       .Match(m => m
                           .Field(f => f.LastName)
                           .Query("Cam")
                       )
                   )
                   // running in a filter context, no score
                   .Filter(fi => fi
                       .DateRange(r => r
                           .Field(f => f.BrithDay)
                           .GreaterThanOrEquals(new DateTime(2017, 01, 01))
                           .LessThan(new DateTime(2018, 01, 01))
                       )
                   )
               )
           )
        );

        searchResponse = client.Search<Person>(s => s
            .Query(q => q
                .Match(m => m
                    .Field(f => f.FirstName)
                    .Query("Russ")
                ) && q
                .Match(m => m
                    .Field(f => f.LastName)
                    .Query("Cam")
                ) && +q
                .DateRange(r => r
                    .Field(f => f.BrithDay)
                    .GreaterThanOrEquals(new DateTime(2017, 01, 01))
                    .LessThan(new DateTime(2018, 01, 01))
                )
            )
        );
    }

}