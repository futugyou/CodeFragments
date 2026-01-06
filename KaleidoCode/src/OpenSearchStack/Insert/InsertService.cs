
using System.Threading.Tasks;

namespace OpenSearchStack.Insert;

public class InsertService
{
    public InsertService(ILogger<InsertService> log, OpenSearchClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly OpenSearchClient client;
    private readonly ILogger<InsertService> log;
    private static List<OrderInfo> orders = new List<OrderInfo>{
        new OrderInfo{
            Id = Guid.NewGuid().ToString(),
            Name = "tom",
            CreateTime = DateTime.Now,
            Status = "chart",
            GoodsName = "phone",
            Price = 12,
        }
    };

    public async Task<IndexResponse> InsertData()
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
        // var responseSignal = await client.IndexAsync(order, i => i.Index("order"));
        // var responseSignal = await client.IndexAsync(new IndexRequest<OrderInfo>(order, "order"));
        return await client.IndexDocumentAsync(order);// it wil use default index. in this case is "demo"
    }

    public async Task<OrderInfo> UpdateData()
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

        var result = await client.UpdateAsync<OrderInfo>(order.Id,
                u =>
                    u.Index("order")
                        .Doc(order)
                        .DocAsUpsert());

        // i do not want to change all method async
        return result.Get.Source;
    }

    public async IAsyncEnumerable<string> InsertManyData()
    {
        var response = await client.IndexManyAsync(orders);
        if (response.Errors)
        {
            foreach (var itemWithError in response.ItemsWithErrors)
            {
                yield return $"Failed to index document {itemWithError.Id}: {itemWithError.Error}";
            }
        }

        yield return "finish";
    }

    public async IAsyncEnumerable<string> Bulk()
    {
        // 1. Bulk
        var responseBulk = await client.BulkAsync(b => b.Index("order").IndexMany(orders));
        foreach (var item in responseBulk.Items)
        {
            yield return item.Id;
        }
    }

    public async IAsyncEnumerable<string> SimpleBulk()
    {
        List<string> ids = [];
        // 2. BulkAll
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
            foreach (var item in next.Items)
            {
                ids.Add(item.Id);
            }
        });

        foreach (var id in ids)
        {
            yield return id;
        }
    }

    public async IAsyncEnumerable<string> ComplexBulk()
    {
        List<string> ids = [];
        // 3. BulkAll with more option
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
            ids.Add(bulkResponseItem.Id);
            Console.WriteLine($"Unable to index: {bulkResponseItem} {order}");
        }));

        foreach (var id in ids)
        {
            yield return id;
        }
    }
}