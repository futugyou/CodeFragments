
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

    public void InsertMany()
    {
        insertService.InsertManyData();
        insertService.InsertManyWithBulk();
    }

    public void Pipeline()
    {
        pipelineService.CreatePipeline();
        pipelineService.InsertDataWithPipline();
    }

    public void Reindex()
    {
        reindexService.CreateReindex();
    }

    public async Task<IndexResponse> Insert()
    {
        return await insertService.InsertData();
    }

    public void Mapping()
    {
        indexService.CreteElasticIndex();
    }

    public void Aggs()
    {
        aggregationSerice.FluentDsl();
    }

    public void GetAll()
    {
        searchService.MatchAll();
    }

    public void GetPage()
    {
        searchService.PageSearch();
    }

    public void ScrollGet()
    {
        searchService.ScrollingSearch();
    }

    public void Search()
    {
        searchService.PageSearch();
    }
}