
namespace OpenSearchStack.Index;

public class ReindexService
{
    public ReindexService(ILogger<ReindexService> log, OpenSearchClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly OpenSearchClient client;
    private readonly ILogger<ReindexService> log;

    public async Task<ReindexOnServerResponse> ReindexOnServer()
    {
        // 1. base reindex(use reindexOnServer)
        return await client.ReindexOnServerAsync(r => r
            .Source(s => s
                .Index("source_index")
            )
            .Destination(d => d
                .Index("destination_index")
            )
            .WaitForCompletion()
        );
    }


    public async Task Reindex()
    {
        var slices = Environment.ProcessorCount;
        var reindexObserver = client.Reindex<Person>(r => r
            .ScrollAll("5s", slices, s => s // How to fetch documents to be reindexed
                .Search(ss => ss
                    .Index("source_index")
                )
            )
            .BulkAll(b => b // How to index fetched documents
                .Index("destination_index")
            )
        )
        // Wait up to 15 minutes for the reindex process to complete
        .Wait(TimeSpan.FromMinutes(15), response =>
        {
            // do something with each bulk response e.g. accumulate number of indexed documents
        });

    }

    public async Task CreateIndexWithReindex()
    {
        // 3. using Reindex create index
        // Get the settings for the source index
        var getIndexResponse = await client.Indices.GetAsync("source_index");
        var indexSettings = getIndexResponse.Indices["source_index"];
        // Get the mapping for the lastName property
        var lastNameProperty = indexSettings.Mappings.Properties["lastName"];
        // If the lastName property is a text datatype, add a keyword multi-field
        if (lastNameProperty is TextProperty textProperty)
        {
            if (textProperty.Fields == null)
                textProperty.Fields = new Properties();
            textProperty.Fields.Add("keyword", new KeywordProperty());
        }

        client.Reindex<Person>(r => r
            .CreateIndex(c => c
                // Use the index settings to create the destination index
                .InitializeUsing(indexSettings)
            )
            .ScrollAll("5s", Environment.ProcessorCount, s => s
                .Search(ss => ss
                    .Index("source_index")
                )
            )
            .BulkAll(b => b
                .Index("destination_index")
            )
        )
        .Wait(TimeSpan.FromMinutes(15), response =>
        {
            // do something with each bulk response e.g. accumulate number of indexed documents
        });

    }

    public async Task MappingReindex()
    {
        // 4. use ReindexObserver directly.
        var reindexObservable = client.Reindex<Person, Person>(
            // a function to define how source documents are mapped to destination documents
            person => person,
            r => r
            .ScrollAll("5s", Environment.ProcessorCount, s => s
                .Search(ss => ss
                    .Index("source_index")
                )
            )
            .BulkAll(b => b
                .Index("destination_index")
            )
        );
    }

}