
using CompanyReports.BM25;
using Path = System.IO.Path;

namespace CompanyReports;

public class BM25Ingestor : IIngestor
{
    private readonly ILogger<BM25Ingestor> logger;
    public BM25Ingestor(ILogger<BM25Ingestor> logger)
    {
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

            var bm25Index = new BM25Okapi(chunks);

            var sha1Name = reportData.Metainfo.Sha1Name;
            var outputFile = Path.Combine(outputDir, $"{sha1Name}.bm25.json");
            var json = JsonSerializer.Serialize(bm25Index);
            await File.WriteAllTextAsync(outputFile, json, cancellationToken);
        }

        logger.LogInformation("Processed {Length} reports", allReportPaths.Length);
    }
}