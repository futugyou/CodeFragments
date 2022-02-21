using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using AspnetcoreEx.Extensions;
using AspnetcoreEx.Elasticsearch;

namespace AspnetcoreEx.Controllers;

[ApiController]
[Route("[controller]")]
public class ElasticController : ControllerBase
{
    private readonly BaseElasticService esService;

    public ElasticController(BaseElasticService esService)
    {
        this.esService = esService;
    }
    [HttpGet]
    public void Insert()
    {
        esService.Mapping();
        esService.Insert();
        esService.InsertMany();
        esService.GetAll();
        esService.GetPage();
        esService.ScrollGet();
        esService.Search();
        esService.Aggs();
        esService.Pipeline();
        esService.Reindex();
    }
}
