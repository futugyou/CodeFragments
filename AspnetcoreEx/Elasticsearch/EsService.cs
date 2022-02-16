using Nest;

namespace AspnetcoreEx.Elasticsearch;

public class EsService
{
    private static ElasticClient client = new ElasticClient(new Uri("http://localhost:9200"));

    // create index mapping
    public void Mapping()
    {
        client.Indices.Create("order", c => c.Map<OrderInfo>(m => m.AutoMap()));
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
        };
        var response = client.Index(order, i => i.Index("order"));
    }
}