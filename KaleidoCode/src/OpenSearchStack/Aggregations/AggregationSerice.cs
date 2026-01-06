
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

    public async Task<AggregateDictionary> AverageMax()
    {
        var response = await client.SearchAsync<object>(s => s
            .Index("order")
            .Size(0)
            .Aggregations(a => a
                .Average("price", avg => avg.Field("price"))
                .Max("maxprice", m => m.Field("price"))
            )
        );
        
        return response.Aggregations;
    }

    public async Task<AggregateDictionary> Terms()
    {
        var response = await client.SearchAsync<object>(s => s
            .Index("order")
            .Size(0)
            .Aggregations(a => a
                .Terms("goodsgroup", group => group.Field("goodsName"))
            )
        );
        return response.Aggregations;
    }
}