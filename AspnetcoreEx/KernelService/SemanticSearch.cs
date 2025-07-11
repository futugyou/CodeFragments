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

    public static IEnumerable<SemanticSearchRecord> CreateDemoDatas()
    {
        yield return new SemanticSearchRecord
        {
            Key = "1",
            FileName = "1.txt",
            PageNumber = 1,
            Text = "Application Programming Interface. A set of rules and specifications that allow software components to communicate and exchange data."
        };

        yield return new SemanticSearchRecord
        {
            Key = "2",
            FileName = "2.md",
            PageNumber = 2,
            Text = "Connectors allow you to integrate with various services provide AI capabilities, including LLM, AudioToText, TextToAudio, Embedding generation, etc."
        };

        yield return new SemanticSearchRecord
        {
            Key = "3",
            FileName = "3.pdf",
            PageNumber = 3,
            Text = "Retrieval Augmented Generation - a term that refers to the process of retrieving additional data to provide as context to an LLM to use when generating a response (completion) to a user’s question (prompt)."
        };
    }
}
