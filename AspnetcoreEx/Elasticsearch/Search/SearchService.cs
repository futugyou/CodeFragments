
using System.Linq.Expressions;
using System.Runtime.ExceptionServices;
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
        //4 
        var query = new MatchAllQuery();
        var response = client.Search<OrderInfo>(s => s.Index("order").Query(p => query));
        var list = response.Documents.ToList();
        log.LogInformation("list count " + list.Count);
    }

    public void PageSearch()
    {
        //1
        var query = new MatchAllQuery();
        var response = client.Search<OrderInfo>(p => p
            .Index("order")
            .Query(_ => query)
            .From(1)
            .Size(2)
        );
        var list = response.Documents.ToList();
        log.LogInformation("list count " + list.Count);

        //2
        response = client.Search<OrderInfo>(p => p
            .Index("order")
            .Query(p =>
                (p.Term(o => o.Name, "tom")
                || p.Term(o => o.Name, "tony")
                ) && p.Term(o => o.GoodsName, "phone")
            )
        );
        list = [.. response.Documents];
        log.LogInformation("list count " + list.Count);
        response = client.Search<OrderInfo>(p => p
        //.AllIndices()
            .From(0)
            .Size(10)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Name)
                    .Query("tom")
                )
           )
        );
        list = [.. response.Documents];
        log.LogInformation("list count " + list.Count);
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

    public void BinaryOperator()
    {
        // 1. OR Fluent API == should
        var firstOrSearchResponse = client.Search<Person>(s => s
            .Query(q => q
                .Term(p => p.LastName, "x") || q
                .Term(p => p.LastName, "y")
            )
        );
        // 2. OR Object Initializer syntax == should
        var secondOrSearchResponse = client.Search<Person>(new SearchRequest<Person>
        {
            Query = new TermQuery { Field = Infer.Field<Person>(p => p.LastName), Value = "x" } ||
            new TermQuery { Field = Infer.Field<Person>(p => p.LastName), Value = "y" }
        });

        // 3. AND Fluent API == must
        var firstAndSearchResponse = client.Search<Person>(s => s
            .Query(q => q
                .Term(p => p.LastName, "x") && q
                .Term(p => p.LastName, "y")
            )
        );
        // 4. AND Object Initializer syntax == must
        var fieldString = new Field("lastName");
        var fieldProperty = new Field(typeof(Person).GetProperty(nameof(Person.LastName)));
        var secondSearchResponse = client.Search<Person>(new SearchRequest<Person>
        {
            Query = new TermQuery { Field = fieldString, Value = "x" } &&
            new TermQuery { Field = fieldProperty, Value = "y" }
        });
    }

    public void UnaryOperator()
    {
        // 1. NOT Fluent API == must_not 
        var firstNotSearchResponse = client.Search<Person>(s => s
            .Query(q => !q
                .Term(p => p.LastName, "x")
            )
        );

        // 2. NOT Object Initializer syntax == must_not 
        Expression<Func<Person, object>> expression = p => p.LastName;
        var secondNotSearchResponse = client.Search<Person>(new SearchRequest<Person>
        {
            Query = !new TermQuery { Field = new Field(expression), Value = "x" }
        });

        // 3. + Fluent API == filter
        var firstEqSearchResponse = client.Search<Person>(s => s
            .Query(q => +q
                .Term(p => p.LastName, "x")
            )
        );

        // 4. + Object Initializer syntax == filter
        var secondEqSearchResponse = client.Search<Person>(new SearchRequest<Person>
        {
            Query = +new TermQuery { Field = Infer.Field<Person>(p => p.LastName), Value = "x" }
        });

    }

    public void StoredField()
    {
        var searchResponse = client.Search<Person>(s => s
            .StoredFields(sf => sf
                .Fields(
                    f => f.LastName,
                    f => f.BrithDay,
                    f => f.FirstName
                )
            )
            .Query(q => q
                .MatchAll()
            )
        );
        foreach (var fieldValues in searchResponse.Fields)
        {
            var document = new
            {
                LastName = fieldValues.ValueOf<Person, string>(p => p.LastName),
                BrithDay = fieldValues.Value<DateTime>(Infer.Field<Person>(p => p.BrithDay)),
                FirstName = fieldValues.ValueOf<Person, string>(p => p.FirstName)
            };
        }
    }

    public void SourceFilter()
    {
        var searchResponse = client.Search<Person>(s => s
            .Source(sf => sf
                // Include the following fields
                .Includes(i => i
                    .Fields(
                        f => f.FirstName,
                        f => f.BrithDay
                    )
                )
                // Exclude the following fields
                .Excludes(e => e
                    // Fields can be included or excluded through wildcard patterns
                    .Fields("num*")
                )
            )
            .Query(q => q
                .MatchAll()
            )
        );

        // exclude _source
        searchResponse = client.Search<Person>(s => s
           .Source(false)
           .Query(q => q
               .MatchAll()
           )
        );
    }

    public void ScrollingSearch()
    {
        // 1
        var query = new MatchAllQuery();
        var response = client.Search<OrderInfo>(p => p
            .Index("order")
            .Query(_ => query)
            .Size(2)
            .Scroll("10s")
        );
        var list = response.Documents.ToList();
        log.LogInformation("list count " + list.Count);
        var scrollid = response.ScrollId;
        response = client.Scroll<OrderInfo>("10s", scrollid);
        list = [.. response.Documents];
        log.LogInformation("list count " + list.Count);

        // 2
        var searchResponse = client.Search<Person>(s => s
            .Query(q => q
                .Term(f => f.FirstName, "Russ")
            )
            // Specify a scroll time for how long Elasticsearch should keep this scroll open on the server side. 
            // The time specified should be sufficient to process the response on the client side.
            .Scroll("10s")
        );
        // make subsequent requests to the scroll API to keep fetching documents, whilst documents are returned
        while (searchResponse.Documents.Count != 0)
        {
            searchResponse = client.Scroll<Person>("10s", searchResponse.ScrollId);
        }
    }

    public void ScrollAllObservableSearch()
    {
        int numberOfSlices = Environment.ProcessorCount;

        var scrollAllObservable = client.ScrollAll<Person>("10s", numberOfSlices, sc => sc
            .MaxDegreeOfParallelism(numberOfSlices)
            .Search(s => s
                .Query(q => q
                    .Term(f => f.FirstName, "Russ")
                )
            )
        );

        var waitHandle = new ManualResetEvent(false);
        ExceptionDispatchInfo? info = null;

        var scrollAllObserver = new ScrollAllObserver<Person>(
            onNext: response => ProcessResponse(response.SearchResponse),
            onError: e =>
            {
                info = ExceptionDispatchInfo.Capture(e);
                waitHandle.Set();
            },
            onCompleted: () => waitHandle.Set()
        );

        scrollAllObservable.Subscribe(scrollAllObserver);

        waitHandle.WaitOne();
        info?.Throw();
    }

    private void ProcessResponse(ISearchResponse<Person> searchResponse)
    {

    }

    public void Highlight()
    {
        var searchResponse = client.Search<Company>(s => s
            .IndicesBoost(b => b
                .Add("company_index_1", 2.0)
                .Add("company_index_2", 1.0)
            )
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Name.Suffix("company"))
                    .Query("Upton Sons Shield Rice Rowe Roberts")
                )
            )
            .Highlight(h => h
                .PreTags("<tag1>")
                .PostTags("</tag1>")
                .Encoder(HighlighterEncoder.Html)
                .HighlightQuery(q => q
                    .Match(m => m
                        .Field(f => f.Name.Suffix("company"))
                        .Query("Upton Sons Shield Rice Rowe Roberts")
                    )
                )
                .Fields(
                    fs => fs
                        .Field(p => p.Name.Suffix("company"))
                        .Type("plain")
                        .ForceSource()
                        .FragmentSize(150)
                        .Fragmenter(HighlighterFragmenter.Span)
                        .NumberOfFragments(3)
                        .NoMatchSize(150)
                )
            )
        );
    }

    public void ScriptFields()
    {
        var searchResponse = client.Search<Company>(s => s
           .ScriptFields(sf => sf
               .ScriptField("test1", sc => sc
                   .Source("doc['assets'].value * 2")
               )
               .ScriptField("test2", sc => sc
                   .Source("doc['assets'].value * params.factor")
                   .Params(p => p
                       .Add("factor", 2.0)
                   )
               )
           )
        );

        foreach (var fields in searchResponse.Fields)
        {
            Console.WriteLine(fields.Value<int>("test1"));
            Console.WriteLine(fields.Value<int>("test2"));
        }
    }

    public void Sorting()
    {
        var searchResponse = client.Search<Company>(s => s
            .Sort(ss => ss
                .Ascending(p => p.CreateAt)
                .Descending(p => p.Name)
                .Descending(SortSpecialField.Score)
                .Ascending(SortSpecialField.DocumentIndexOrder)
                .Field(f => f
                    .Field(p => p.Employees.First().Salary)
                    .Order(SortOrder.Descending)
                    .MissingLast()
                    .UnmappedType(FieldType.Date)
                    .Mode(SortMode.Average)
                    .Nested(n => n
                        .Path(p => p.Employees)
                        .Filter(q => q.MatchAll())
                    )
                )
                .Field(f => f
                    .Field(p => p.Assets)
                    .Order(SortOrder.Descending)
                    .Missing(-1)
                )
                .GeoDistance(g => g
                    .Field(p => p.LocationPoint)
                    .DistanceType(GeoDistanceType.Arc)
                    .Order(SortOrder.Ascending)
                    .Unit(DistanceUnit.Centimeters)
                    .Mode(SortMode.Min)
                    .Points(new GeoLocation(70, -70), new GeoLocation(-12, 12))
                )
                .GeoDistance(g => g
                    .Field(p => p.LocationPoint)
                    .Points(new GeoLocation(70, -70), new GeoLocation(-12, 12))
                )
                .Script(sc => sc
                    .Type("number")
                    .Ascending()
                    .Script(script => script
                        .Source("doc['assets'].value * params.factor")
                        .Params(p => p.Add("factor", 1.1))
                    )
                )
            )
        );
    }

    public void CombinedFields()
    {
        var searchResponse = client.Search<Person>(s => s
            .Query(q => q
                .CombinedFields(c => c
                    .Fields(f => f.Field(p => p.FirstName).Field("lastName"))
                    .Query("tom")
                    .Boost(1.1)
                    .Operator(Operator.Or)
                    .MinimumShouldMatch("2")
                    .ZeroTermsQuery(ZeroTermsQuery.All)
                    .Name("combined_fields")
                    .AutoGenerateSynonymsPhraseQuery(false)
                )
            )
        );
    }
}