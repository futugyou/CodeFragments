using Nest;

namespace KaleidoCode.Elasticsearch;

public class BaseElasticService
{
    public BaseElasticService(
        ILogger<BaseElasticService> log,
        ElasticClient client,
        IndexService indexService,
        ReindexService reindexService,
        InsertService insertService,
        PipelineService pipelineService,
        SearchService searchService,
        AggregationSerice aggregationSerice
    )
    {
        this.log = log;
        this.client = client;
        this.indexService = indexService;
        this.reindexService = reindexService;
        this.insertService = insertService;
        this.pipelineService = pipelineService;
        this.searchService = searchService;
        this.aggregationSerice = aggregationSerice;
    }
    private readonly ElasticClient client;
    private readonly IndexService indexService;
    private readonly ReindexService reindexService;
    private readonly InsertService insertService;
    private readonly PipelineService pipelineService;
    private readonly SearchService searchService;
    private readonly AggregationSerice aggregationSerice;
    private readonly ILogger<BaseElasticService> log;

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
        insertService.UpdateData();
    }

    internal void Mapping()
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