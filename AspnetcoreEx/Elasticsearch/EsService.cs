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
        response = client.IndexDocument(order);// it wil use default index. in this case is "demo"
    }

    public void InsertMany()
    {
        var orers = new List<OrderInfo>{
            new OrderInfo{
                Id = Guid.NewGuid().ToString(),
                Name = "tom",
                CreateTime = DateTime.Now,
                Status = "chart",
                GoodsName = "phone",
                Price = 12,
            }
        };
        var response = client.Bulk(b => b.Index("order").IndexMany(orers));
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