using System.Runtime.ExceptionServices;
using Nest;

namespace AspnetcoreEx.Elasticsearch;

public class EsService
{
    public EsService(ILogger<EsService> log, ElasticClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly ElasticClient client;
    private readonly ILogger<EsService> log;

    // create index mapping
    public void Mapping()
    {
        client.Indices.Create("order", c => c.Map<OrderInfo>(m => m.AutoMap()));
        client.Indices.Create("order_propert_visitor", c => c.Map<OrderInfo>(m => m.AutoMap(new DisableDocValuesPropertyVisitor())));
        client.Indices.Create("company", c => c
            .Map<Company>(m => m
                .Properties(ps => ps
                    .Text(s => s
                        .Name(n => n.Name)
                    )
                    .Object<Employee>(o => o
                        .Name(n => n.Employees)
                        .Properties(eps => eps
                            .Text(s => s
                                .Name(e => e.FirstName)
                            )
                            .Text(s => s
                                .Name(e => e.LastName)
                            )
                            .Number(n => n
                                .Name(e => e.Salary)
                                .Type(NumberType.Integer)
                            )
                        )
                    )
                )
            )
        );
        client.Indices.Create("company1", c => c
            .Map<Company>(m => m
                .AutoMap()
                .Properties(ps => ps
                    .Nested<Employee>(n => n
                        .Name(nn => nn.Employees)
                    )
                )
            )
        );

        client.Indices.Create("company2", c => c
            .Map<CompanyWithAttributes>(m => m
                .AutoMap()
                .Properties(ps => ps
                    .Nested<EmployeeWithAttributes>(n => n
                        .Name(nn => nn.Employees)
                        .AutoMap()
                        .Properties(pps => pps
                            .Text(s => s
                                .Name(e => e.FirstName)
                                .Fields(fs => fs
                                    .Keyword(ss => ss
                                        .Name("firstNameRaw")
                                    )
                                    .TokenCount(t => t
                                        .Name("length")
                                        .Analyzer("standard")
                                    )
                                )
                            )
                            .Number(nu => nu
                                .Name(e => e.Salary)
                                .Type(NumberType.Double)
                                .IgnoreMalformed(false)
                            )
                            .Date(d => d
                                .Name(e => e.Birthday)
                                .Format("MM-dd-yy")
                            )
                        )
                    )
                )
            )
        );

        client.Indices.Create("people", c => c
            .Map<Person>(p => p
                .AutoMap() // automatically create the mapping from the type
                .Properties(props => props
                    .Keyword(t => t.Name("initials")) // create an additional field to store the initials
                    .Ip(t => t.Name(dv => dv.IpAddress)) // map field as IP Address type
                    .Object<GeoIp>(t => t.Name(dv => dv.GeoIp)) // map GeoIp as object
                )
            )
        );

        client.Ingest.PutPipeline("person-pipeline", p => p
            .Processors(ps => ps
                .Uppercase<Person>(s => s
                    .Field(t => t.LastName) // uppercase the lastname
                )
                .Script(s => s
                    .Lang("painless") // use a painless script to populate the new field
                    .Source("ctx.initials = ctx.firstName.substring(0,1) + ctx.lastName.substring(0,1)")
                )
                .GeoIp<Person>(s => s // use ingest-geoip plugin to enrich the GeoIp object from the supplied IP Address
                    .Field(i => i.IpAddress)
                    .TargetField(i => i.GeoIp)
                )
            )
        );
    }

    public void Insert()
    {
        var order = new OrderInfo
        {
            Id = Guid.NewGuid().ToString(),
            Name = "tom",
            CreateTime = DateTime.Now,
            Status = "chart",
            GoodsName = "phone",
            Price = 10,
        };
        var response = client.Index(order, i => i.Index("order"));
        // var response = client.Index(new IndexRequest<OrderInfo>(order, "order"));
        response = client.IndexDocument(order);// it wil use default index. in this case is "demo"
    }

    public void InsertMany()
    {
        var orders = new List<OrderInfo>{
            new OrderInfo{
                Id = Guid.NewGuid().ToString(),
                Name = "tom",
                CreateTime = DateTime.Now,
                Status = "chart",
                GoodsName = "phone",
                Price = 12,
            }
        };
        // 1. IndexMany
        var response = client.IndexMany(orders);
        if (response.Errors)
        {
            foreach (var itemWithError in response.ItemsWithErrors)
            {
                Console.WriteLine($"Failed to index document {itemWithError.Id}: {itemWithError.Error}");
            }
        }

        // 2. Bulk
        var responseBulk = client.Bulk(b => b.Index("order").IndexMany(orders));

        // 3. BulkAll
        var bulkAllObservable1 = client.BulkAll(orders, b => b
            .Index("order")
            .BackOffTime("30s") // how long to wait between retries
            .BackOffRetries(2) // how many retries are attempted if a failure occurs
            .RefreshOnCompleted()
            .MaxDegreeOfParallelism(Environment.ProcessorCount)
            .Size(1000) // items per bulk request
        )
        // perform the indexing and wait up to 15 minutes, 
        // whilst the BulkAll calls are asynchronous this is a blocking operation
        .Wait(TimeSpan.FromMinutes(15), next =>
        {
            Console.WriteLine($"next.Page: {next.Page}");
            // do something e.g. write number of pages to console
        });

        // 4. BulkAll with more option
        var bulkAllObservable2 = client.BulkAll(orders, b => b
        .BufferToBulk((descriptor, buffer) => // Customise each bulk operation before it is dispatched
        {
            foreach (var order in buffer)
            {
                descriptor.Index<OrderInfo>(bi => bi
                    .Index(order.Price % 2 == 0 ? "even-index" : "odd-index") // Index each document into either even-index or odd-index
                    .Document(order)
                );
            }
        })
        .RetryDocumentPredicate((bulkResponseItem, order) => // Decide if a document should be retried in the event of a failure
        {
            return bulkResponseItem.Error.Index == "even-index" && order.Name == "Martijn";
        })
        .DroppedDocumentCallback((bulkResponseItem, order) => // If a document cannot be indexed this delegate is called
        {
            Console.WriteLine($"Unable to index: {bulkResponseItem} {order}");
        }));

        var waitHandle = new ManualResetEvent(false);
        ExceptionDispatchInfo? exceptionDispatchInfo = null;

        var observer = new BulkAllObserver(
            onNext: response =>
            {
                // do something e.g. write number of pages to console
            },
            onError: exception =>
            {
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
                waitHandle.Set();
            },
            onCompleted: () => waitHandle.Set());

        // Subscribe to the observable, which will initiate the bulk indexing process
        bulkAllObservable2.Subscribe(observer);

        // Block the current thread until a signal is received
        waitHandle.WaitOne();

        // If an exception was captured during the bulk indexing process, throw it
        exceptionDispatchInfo?.Throw();
    }

    public void Pipeline()
    {
        var person = new Person
        {
            Id = 1,
            FirstName = "Martijn",
            LastName = "Laarman",
            IpAddress = "139.130.4.5"
        };
        // index the document using the created pipeline
        var indexResponse = client.Index(person, p => p.Index("people").Pipeline("person-pipeline"));
    }

    public void GetAll()
    {
        var query = new MatchAllQuery();
        var response = client.Search<OrderInfo>(s => s.Index("order").Query(p => query));
        var list = response.Documents.ToList();
        log.LogInformation("list count " + list.Count);
    }

    public void GetPage()
    {
        var query = new MatchAllQuery();
        var response = client.Search<OrderInfo>(p => p.Index("order").Query(_ => query).Skip(1).Take(2));//TODO: From Size?
        var list = response.Documents.ToList();
        log.LogInformation("list count " + list.Count);
    }

    public void ScrollGet()
    {
        var query = new MatchAllQuery();
        var response = client.Search<OrderInfo>(p => p.Index("order").Query(_ => query).Size(2).Scroll("10s"));
        var list = response.Documents.ToList();
        log.LogInformation("list count " + list.Count);
        var scrollid = response.ScrollId;
        response = client.Scroll<OrderInfo>("10s", scrollid);
        list = response.Documents.ToList();
        log.LogInformation("list count " + list.Count);
    }

    public void Search()
    {
        var response = client.Search<OrderInfo>(p => p.Index("order").Query(
            p => (p.Term(o => o.Name, "tom") || p.Term(o => o.Name, "tony")) && p.Term(o => o.GoodsName, "phone")
            ));
        var list = response.Documents.ToList();
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
        list = response.Documents.ToList();
        log.LogInformation("list count " + list.Count);
    }

    public void Aggs()
    {
        var response = client.Search<object>(s => s
            .Index("order")
            .Size(0)
            .Aggregations(a => a
                .Average("price", avg => avg.Field("price"))
                .Max("maxprice", m => m.Field("price"))
            )
        );
        var list = response.Aggregations;
        log.LogInformation("list count " + list.Count);

        response = client.Search<object>(s => s
            .Index("order")
            .Size(0)
            .Aggregations(a => a
                .Terms("goodsgroup", group => group.Field("goodsName"))
            )
        );
        list = response.Aggregations;
        log.LogInformation("list count " + list.Count);
    }
}