using System.Runtime.ExceptionServices;
using Nest;

namespace KaleidoCode.Elasticsearch;
public class ReindexService
{
    public ReindexService(ILogger<ReindexService> log, ElasticClient client)
    {
        this.log = log;
        this.client = client;
    }
    private readonly ElasticClient client;
    private readonly ILogger<ReindexService> log;

    public void CreateReindex()
    {
        // 1. base reindex(use reindexOnServer)
        var reindexResponse = client.ReindexOnServer(r => r
            .Source(s => s
                .Index("source_index")
            )
            .Destination(d => d
                .Index("destination_index")
            )
            .WaitForCompletion()
        );

        // 2. use redinx
        // Number of slices to split each scroll into
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

        // 3. using Reindex create index
        // Get the settings for the source index
        var getIndexResponse = client.Indices.Get("source_index");
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
        var reindexObserver2 = client.Reindex<Person>(r => r
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
        var waitHandle = new ManualResetEvent(false);
        ExceptionDispatchInfo? exceptionDispatchInfo = null;
        var observer = new ReindexObserver(
            onNext: response =>
            {
                // do something e.g. write number of pages to console
            },
            onError: exception =>
            {
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
                waitHandle.Set();
            },
            onCompleted: () => waitHandle.Set());
        // Subscribe to the observable, which will initiate the reindex process
        reindexObservable.Subscribe(observer);
        // Block the current thread until a signal is received
        waitHandle.WaitOne();
        // If an exception was captured during the reindex process, throw it
        exceptionDispatchInfo?.Throw();
    }

}