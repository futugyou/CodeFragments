
namespace OpenSearchStack.Aggregations;

public class AggregationSerice
{
    public AggregationSerice(ILogger<AggregationSerice> log, OpenSearchClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly OpenSearchClient client;
    private readonly ILogger<AggregationSerice> log;

    public async Task FluentDsl()
    {
        var response = await client.SearchAsync<object>(s => s
            .Index("order")
            .Size(0)
            .Aggregations(a => a
                .Average("price", avg => avg.Field("price"))
                .Max("maxprice", m => m.Field("price"))
            )
        );
        var list = response.Aggregations;
        log.LogInformation("list count " + list.Count);

        response = await client.SearchAsync<object>(s => s
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