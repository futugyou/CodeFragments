using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using System.Linq.Expressions;

namespace AspnetcoreEx.KernelService;

public class SemanticSearch(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, IVectorStore vectorStore)
{
    public async Task<IReadOnlyList<SemanticSearchRecord>> SearchAsync(string text, string? filenameFilter, int maxResults)
    {
        var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(text);
        var vectorCollection = vectorStore.GetCollection<string, SemanticSearchRecord>("data-test-ingested");
        Expression<Func<SemanticSearchRecord, bool>>? filter = filenameFilter is { Length: > 0 }
            ? record => record.FileName == filenameFilter
            : null;
        var results = new List<SemanticSearchRecord>();
        await foreach (var item in vectorCollection.SearchEmbeddingAsync(queryEmbedding, maxResults, new VectorSearchOptions<SemanticSearchRecord>
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
    [VectorStoreRecordKey]
    public required string Key { get; set; }

    [VectorStoreRecordData]
    public required string FileName { get; set; }

    [VectorStoreRecordData]
    public int PageNumber { get; set; }

    [VectorStoreRecordData]
    public required string Text { get; set; }

    [VectorStoreRecordVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity)] // 1536 is the default vector size for the OpenAI text-embedding-3-small model
    public ReadOnlyMemory<float> Vector { get; set; }
}
