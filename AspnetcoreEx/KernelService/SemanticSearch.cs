using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using System.Linq.Expressions;

namespace AspnetcoreEx.KernelService;

public class SemanticSearch(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, VectorStore vectorStore)
{
    public async Task<IReadOnlyList<SemanticSearchRecord>> SearchAsync(string text, string? filenameFilter, int maxResults)
    {
        var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(text);
        var vectorCollection = vectorStore.GetCollection<string, SemanticSearchRecord>("data-test-ingested");
        Expression<Func<SemanticSearchRecord, bool>>? filter = filenameFilter is { Length: > 0 }
            ? record => record.FileName == filenameFilter
            : null;
        var results = new List<SemanticSearchRecord>();
        await foreach (var item in vectorCollection.SearchAsync(queryEmbedding, maxResults, new VectorSearchOptions<SemanticSearchRecord>
        {
            Filter = filter,
        }))
        {
            results.Add(item.Record);
        }

        return results;
    }
}

public class SemanticSearchRecord
{
    [VectorStoreKey]
    public required string Key { get; set; }

    [VectorStoreData]
    public required string FileName { get; set; }

    [VectorStoreData]
    public int PageNumber { get; set; }

    [VectorStoreData]
    public required string Text { get; set; }

    [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity)] // 1536 is the default vector size for the OpenAI text-embedding-3-small model
    public ReadOnlyMemory<float> Vector { get; set; }
}
