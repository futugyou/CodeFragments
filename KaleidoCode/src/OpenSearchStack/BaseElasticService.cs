
namespace OpenSearchStack;

public class BaseElasticService
{
    public BaseElasticService(
        ILogger<BaseElasticService> log,
        PipelineService pipelineService,
        SearchService searchService,
        AggregationSerice aggregationSerice
    )
    {
        this.log = log;
        this.pipelineService = pipelineService;
        this.searchService = searchService;
        this.aggregationSerice = aggregationSerice;
    }    
    private readonly PipelineService pipelineService;
    private readonly SearchService searchService;
    private readonly AggregationSerice aggregationSerice;
    private readonly ILogger<BaseElasticService> log;

    public async Task Pipeline()
    {
        await pipelineService.CreatePipeline();
        await pipelineService.InsertDataWithPipline();
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