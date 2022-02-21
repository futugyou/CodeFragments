using System.Runtime.ExceptionServices;
using Nest;

namespace AspnetcoreEx.Elasticsearch;

public class BaseElasticService
{
    public BaseElasticService(
        ILogger<BaseElasticService> log,
        ElasticClient client,
        IndexService indexService,
        ReindexService reindexService,
        InsertService insertService,
        PipelineService pipelineService
    )
    {
        this.log = log;
        this.client = client;
        this.indexService = indexService;
        this.reindexService = reindexService;
        this.insertService = insertService;
        this.pipelineService = pipelineService;
    }
    private readonly ElasticClient client;
    private readonly IndexService indexService;
    private readonly ReindexService reindexService;
    private readonly InsertService insertService;
    private readonly PipelineService pipelineService;
    private readonly ILogger<BaseElasticService> log;


    public void GetAll()
    {
        var query = new MatchAllQuery();
        var response = client.Search<OrderInfo>(s => s.Index("order").Query(p => query));
        var list = response.Documents.ToList();
        log.LogInformation("list count " + list.Count);
    }

    internal void InsertMany()
    {
        insertService.InsertManyData();
        insertService.InsertManyWithBulk();
    }

    internal void Pipeline()
    {
        pipelineService.CreatePipeline();
        pipelineService.InsertDataWithPipline();
    }

    internal void Reindex()
    {
        reindexService.CreateReindex();
    }

    internal void Insert()
    {
        insertService.InsertData();
    }

    internal void Mapping()
    {
        indexService.CreteElasticIndex();
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