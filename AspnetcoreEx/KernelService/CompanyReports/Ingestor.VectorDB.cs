
namespace AspnetcoreEx.KernelService.CompanyReports;

using Microsoft.Extensions.AI;
using OpenAI;
using Path = System.IO.Path;

public class VectorDBIngestor : IIngestor
{
    private readonly OpenAIClient _client;

    public VectorDBIngestor(string openAiApiKey)
    {
        _client = new OpenAIClient(openAiApiKey);
    }

    public async Task<List<float[]>> GetEmbeddingsAsync(List<string> texts, string model = "text-embedding-3-large")
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

            var sha1Name = reportData.Metainfo.Sha1Name;
            var outputFile = Path.Combine(outputDir, $"{sha1Name}.vectors.json");
            var json = JsonSerializer.Serialize(embeddings);
            await File.WriteAllTextAsync(outputFile, json, cancellationToken);
        }

        Console.WriteLine($"Processed {allReportPaths.Length} reports");
    }
}

