
namespace AspnetcoreEx.KernelService.CompanyReports;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AspnetcoreEx.KernelService.Tools;

public class BM25Ingestor : IIngestor
{
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

            BM25 bm25Index = new BM25Okapi(chunks);

            var sha1Name = reportData.GetProperty("metainfo").GetProperty("sha1_name").GetString();
            var outputFile = Path.Combine(outputDir, $"{sha1Name}.bm25.json");
            var json = JsonSerializer.Serialize(bm25Index);
            await File.WriteAllTextAsync(outputFile, json);
        }

        Console.WriteLine($"Processed {allReportPaths.Length} reports");
    }
}