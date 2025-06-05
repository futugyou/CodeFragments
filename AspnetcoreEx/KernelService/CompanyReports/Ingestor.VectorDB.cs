
namespace AspnetcoreEx.KernelService.CompanyReports;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OpenAI;

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

    public async Task ProcessReportsAsync(string allReportsDir, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var allReportPaths = Directory.GetFiles(allReportsDir, "*.json");

        foreach (var reportPath in allReportPaths)
        {
            using var f = File.OpenRead(reportPath);
            var reportData = await JsonSerializer.DeserializeAsync<JsonElement>(f);

            var chunks = reportData.GetProperty("content").GetProperty("chunks")
                .EnumerateArray().Select(chunk => chunk.GetProperty("text").GetString() ?? "").Where(p => !string.IsNullOrEmpty(p)).ToList();

            var embeddings = await GetEmbeddingsAsync(chunks);

            var sha1Name = reportData.GetProperty("metainfo").GetProperty("sha1_name").GetString();
            var outputFile = Path.Combine(outputDir, $"{sha1Name}.vectors.json");
            var json = JsonSerializer.Serialize(embeddings);
            await File.WriteAllTextAsync(outputFile, json);
        }

        Console.WriteLine($"Processed {allReportPaths.Length} reports");
    }
}

