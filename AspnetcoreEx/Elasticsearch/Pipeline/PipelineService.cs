using Nest;

namespace AspnetcoreEx.Elasticsearch;

public class PipelineService
{
    public PipelineService(ILogger<PipelineService> log, ElasticClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly ElasticClient client;
    private readonly ILogger<PipelineService> log;

    public void CreatePipeline()
    {
        client.Ingest.PutPipeline("person-pipeline", p => p
            .Processors(ps => ps
                .Uppercase<Person>(s => s
                    .Field(t => t.LastName) // uppercase the lastname
                )
                .Script(s => s
                    .Lang("painless") // use a painless script to populate the new field
                    .Source("ctx.initials = ctx.firstName.substring(0,1) + ctx.lastName.substring(0,1)")
                )
                .GeoIp<Person>(s => s // use ingest-geoip plugin to enrich the GeoIp object from the supplied IP Address
                    .Field(i => i.IpAddress)
                    .TargetField(i => i.GeoIp)
                )
            )
        );
    }

    public void InsertDataWithPipline()
    {
        var person = new Person
        {
            Id = 1,
            FirstName = "Martijn",
            LastName = "Laarman",
            IpAddress = "139.130.4.5"
        };
        // index the document using the created pipeline
        var indexResponse = client.Index(person, p => p.Index("people").Pipeline("person-pipeline"));
    }
}