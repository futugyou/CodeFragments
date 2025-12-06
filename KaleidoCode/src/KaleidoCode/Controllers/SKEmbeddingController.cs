
using SemanticKernelStack;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace KaleidoCode.Controllers;

[Experimental("SKEXP0011")]
[Route("api/sk/embedding")]
[ApiController]
public class SKEmbeddingController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly IKernelMemory _kernelMemory;
    private readonly SemanticKernelOptions _options;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly VectorStore _vectorStore;
    private readonly int _dimensions = 1536;
    public SKEmbeddingController(Kernel kernel, IKernelMemory kernelMemory, VectorStore vectorStore, IOptionsMonitor<SemanticKernelOptions> optionsMonitor)
    {
        _kernel = kernel;
        _kernelMemory = kernelMemory;
        _options = optionsMonitor.CurrentValue;
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        _vectorStore = vectorStore;
        _embeddingGenerator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
        if (_options.Embedding.Dimensions > 0)
        {
            _dimensions = _options.Embedding.Dimensions;
        }
    }

    [Route("search")]
    [HttpGet]
    public async IAsyncEnumerable<string> EmbeddingSearch(string input)
    {
        var embeddings = await _embeddingGenerator.GenerateAsync(input);
        var collection = _vectorStore.GetCollection<string, SemanticSearchRecord>(SemanticSearchRecord.GetCollectionName(), new VectorStoreCollectionDefinition
        {
            Properties =
             [
                 new VectorStoreVectorProperty("Vector", typeof(ReadOnlyMemory<float>), dimensions:_dimensions) { DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw },
            ]
        });
        var searchResult = collection.SearchAsync(embeddings, top: 2);
        await foreach (var record in searchResult)
        {
            yield return $"Found key: {record.Record.Key}, score: {record.Score}, text: {record.Record.Text}";
        }
    }

    [Route("create")]
    [HttpPost]
    public async IAsyncEnumerable<string> EmbeddingCreate()
    {
        // I use https://supabase.com/ pgsql, collection.UpsertAsync and collection.EnsureCollectionExistsAsync() are ok
        // BUT collection.GetAsync throws an exception, `Exception while reading from stream`
        // 
        // SQL:
        // create extension if not exists vector;
        // create table public.semantic_search (
        //   "Key" character varying not null,
        //   "FileName" character varying not null,
        //   "PageNumber" bigint null,
        //   "Text" character varying null,
        //   "Vector" vector(1536) null,  
        //   constraint semantic_search_pkey primary key ("Key")
        // );
        var collection = _vectorStore.GetCollection<string, SemanticSearchRecord>(SemanticSearchRecord.GetCollectionName(), new VectorStoreCollectionDefinition
        {
            Properties =
             [
                 new VectorStoreVectorProperty("Vector", typeof(ReadOnlyMemory<float>), dimensions:_dimensions) { DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw },
            ]
        });
        // Ensure the collection is created
        await collection.EnsureCollectionExistsAsync();
        var en = await collection.GetAsync("1");
        if (en is not null)
        {
            yield return "Collection already exists.";
            yield break;
        }

        var datas = SemanticSearchRecord.CreateDemoDatas().ToList();
        var embeddings = await _embeddingGenerator.GenerateAsync(datas.Select(d => d.Text));
        if (embeddings.Count != datas.Count)
        {
            yield return "Embedding count does not match data count.";
            yield break;
        }
        foreach (var (data, embedding) in datas.Zip(embeddings))
        {
            data.Vector = embedding.Vector;
        }

        await collection.UpsertAsync(datas);

        var upsertedRecords = collection.GetAsync(datas.Select(p => p.Key), new RecordRetrievalOptions { IncludeVectors = true });
        await foreach (var data in upsertedRecords)
        {
            yield return $"Key: {data.Key}, FileName: {data.FileName}, PageNumber: {data.PageNumber}, Text: {data.Text}, Vector: [{string.Join(", ", data.Vector.ToArray())}]";
        }
    }
}
