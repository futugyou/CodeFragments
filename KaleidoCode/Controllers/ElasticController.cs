using KaleidoCode.Elasticsearch;

namespace KaleidoCode.Controllers;

[ApiController]
[Route("api/elastic")]
public class ElasticController : ControllerBase
{
    private readonly BaseElasticService esService;

    public ElasticController(BaseElasticService esService)
    {
        this.esService = esService;
    }

    [Route("mapping")]
    [HttpPost]
    public void Mapping()
    {
        esService.Mapping();
    }

    [Route("insert")]
    [HttpPost]
    public void Insert()
    {
        esService.Mapping();
    }

    [Route("insert_many")]
    [HttpPost]
    public void InsertMany()
    {
        esService.Mapping();
    }

    [Route("get_all")]
    [HttpPost]
    public void GetAll()
    {
        esService.GetAll();
    }

    [Route("get_page")]
    [HttpPost]
    public void GetPage()
    {
        esService.GetPage();
    }

    [Route("scroll_get")]
    [HttpPost]
    public void ScrollGet()
    {
        esService.ScrollGet();
    }

    [Route("search")]
    [HttpPost]
    public void Search()
    {
        esService.Search();
    }

    [Route("aggregations")]
    [HttpPost]
    public void Aggregations()
    {
        esService.Aggs();
    }

    [Route("pipeline")]
    [HttpPost]
    public void Pipeline()
    {
        esService.Pipeline();
    }

    [Route("reindex")]
    [HttpPost]
    public void Reindex()
    { 
        esService.Reindex();
    }
}
