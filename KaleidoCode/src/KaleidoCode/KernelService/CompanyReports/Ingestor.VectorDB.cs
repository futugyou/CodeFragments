
namespace KaleidoCode.KernelService.CompanyReports;

using Microsoft.Extensions.AI;
using FaissMask;
using OpenAI;
using Path = System.IO.Path;

public class VectorDBIngestor : IIngestor
{
    private readonly OpenAIClient _client;
    private readonly ILogger<VectorDBIngestor> logger;

    public VectorDBIngestor([FromKeyedServices("report")] OpenAIClient client, ILogger<VectorDBIngestor> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client), "OpenAIClient cannot be null.");
        this.logger = logger;
    }

    public async Task ProcessReportsAsync(string allReportsDir, string outputDir, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputDir);
        var allReportPaths = Directory.GetFiles(allReportsDir, "*.json");

        foreach (var reportPath in allReportPaths)
        {
            var reportData = JsonSerializer.Deserialize<ProcessedReport>(await File.ReadAllTextAsync(reportPath, cancellationToken));
            if (reportData == null)
            {
                continue;
            }

            var chunks = (reportData.Content?.Chunks ?? []).Select(chunk => chunk.Text ?? "").Where(p => !string.IsNullOrEmpty(p)).ToList();

            var embeddings = await GetEmbeddingsAsync(chunks);

            var index = CreateVectorDb(embeddings);

            var sha1Name = reportData.Metainfo.Sha1Name;

            // TODO: Wait for FaissMask update.
            // The FaissMask library currently does not have a Write method. The relevant PR has been created, but has not been merged yet.
            // Here we need to write `.faiss` instead of `.json`, so that we can read .faiss in [VectorRetriever]
            /// <see cref="VectorRetriever.LoadDbs"/> 
            var outputFile = Path.Combine(outputDir, $"{sha1Name}.vectors.json");
            var json = JsonSerializer.Serialize(embeddings);
            await File.WriteAllTextAsync(outputFile, json, cancellationToken);
        }

        logger.LogInformation("Processed {Length} reports", allReportPaths.Length);
    }

    #region private methods 

    private async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts, string model = "text-embedding-3-large")
    {
        var embeddings = new List<float[]>();
        foreach (var text in texts)
        {
            if (string.IsNullOrWhiteSpace(text)) continue;

            var embeddingGenerator = _client.GetEmbeddingClient(model).AsIEmbeddingGenerator();
            var response = await embeddingGenerator.GenerateAsync(text);
            embeddings.AddRange(response.Vector.ToArray());
        }
        return embeddings;
    }

    private IndexFlatL2 CreateVectorDb(List<float[]> embeddings)
    {
        if (embeddings.Count == 0)
            throw new InvalidOperationException("No embeddings to index.");

        int dimension = embeddings[0].Length;
        var index = new IndexFlatL2(dimension);

        // Flatten & normalize all vectors
        var flatNormalized = embeddings
            .Select(Normalize)
            .SelectMany(x => x)
            .ToArray();

        index.Add(flatNormalized);
        return index;
    }

    private float[] Normalize(float[] vector)
    {
        float norm = MathF.Sqrt(vector.Sum(v => v * v));
        if (norm == 0f) return vector;
        return vector.Select(v => v / norm).ToArray();
    }

    #endregion
}

