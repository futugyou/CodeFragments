using System.Runtime.ExceptionServices;
using Nest;

namespace AspnetcoreEx.Elasticsearch;
public class AggregationSerice
{
    public AggregationSerice(ILogger<AggregationSerice> log, ElasticClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly ElasticClient client;
    private readonly ILogger<AggregationSerice> log;

    public void FluentDsl()
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