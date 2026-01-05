
namespace OpenSearchStack;

public class BaseElasticService
{
    public BaseElasticService(
        ILogger<BaseElasticService> log,
        IndexService indexService,
        ReindexService reindexService,
        InsertService insertService,
        PipelineService pipelineService,
        SearchService searchService,
        AggregationSerice aggregationSerice
    )
    {
        this.log = log;
        this.indexService = indexService;
        this.reindexService = reindexService;
        this.insertService = insertService;
        this.pipelineService = pipelineService;
        this.searchService = searchService;
        this.aggregationSerice = aggregationSerice;
    }
    private readonly IndexService indexService;
    private readonly ReindexService reindexService;
    private readonly InsertService insertService;
    private readonly PipelineService pipelineService;
    private readonly SearchService searchService;
    private readonly AggregationSerice aggregationSerice;
    private readonly ILogger<BaseElasticService> log;

    public async IAsyncEnumerable<string> InsertMany()
    {
        await foreach (var item in insertService.InsertManyData())
        {
            yield return item;
        }
        await foreach (var item in insertService.InsertManyWithBulk())
        {
            yield return item;
        }
    }

    public async Task Pipeline()
    {
        await pipelineService.CreatePipeline();
        await pipelineService.InsertDataWithPipline();
    }

    public Task Reindex()
    {
        return reindexService.CreateReindex();
    }

    public async Task<IndexResponse> Insert()
    {
        return await insertService.InsertData();
    }

    public Task Mapping()
    {
        return indexService.CreteElasticIndex();
    }

    public Task Aggs()
    {
        return aggregationSerice.FluentDsl();
    }

    public Task GetAll()
    {
        return searchService.MatchAll();
    }

    public Task GetPage()
    {
        return searchService.PageSearch();
    }

    public Task ScrollGet()
    {
        return searchService.ScrollingSearch();
    }

    public Task Search()
    {
        return searchService.PageSearch();
    }
}