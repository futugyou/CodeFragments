
using System.Globalization;
using CsvHelper;
using Path = System.IO.Path;

namespace AspnetcoreEx.KernelService.CompanyReports;

public class Pipeline
{
    private readonly IOptionsMonitor<CompanyReportlOptions> runConfig;
    private readonly IPDFParser pdfParser;
    private readonly TableSerializer tableSerializer;
    private readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Pipeline(IOptionsMonitor<CompanyReportlOptions> runConfig, IPDFParser pdfParser, TableSerializer tableSerializer)
    {
        this.runConfig = runConfig;
        this.pdfParser = pdfParser;
        this.tableSerializer = tableSerializer;
        ConvertJsonToCsvIfNeeded();
    }

    private void ConvertJsonToCsvIfNeeded()
    {
        var config = runConfig.CurrentValue;
        var jsonPath = Path.Combine(config.RootPath, "subset.json");
        var csvPath = Path.Combine(config.RootPath, "subset.csv");

        if (File.Exists(jsonPath) && !File.Exists(csvPath))
        {
            try
            {
                string json = File.ReadAllText(jsonPath);
                var data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json, DefaultJsonSerializerOptions);

                // write CSV
                using var writer = new StreamWriter(csvPath);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                if (data != null && data.Count > 0)
                {
                    // write header
                    foreach (var key in data[0].Keys)
                        csv.WriteField(key);
                    csv.NextRecord();

                    // write every row
                    foreach (var row in data)
                    {
                        foreach (var key in row.Keys)
                            csv.WriteField(row[key]);
                        csv.NextRecord();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error converting JSON to CSV: {e.Message}");
            }
        }
    }

    public async Task ParsePdfReportsSequential(CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        await pdfParser.ParseAndExportAsync(config.PdfReportsDir, cancellation);
        Console.WriteLine($"PDF reports parsed and saved to {config.ParsedReportsPath}");
    }

    public async Task ParsePdfReportsParallel(int chunkSize = 2, int maxWorkers = 10, CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        await pdfParser.ParseAndExportParallelAsync(config.PdfReportsDir, maxWorkers, chunkSize, cancellation);
        Console.WriteLine($"PDF reports parsed and saved to {config.ParsedReportsPath}");
    }

    public async Task SerializeTables(int maxWorkers = 10, CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        await tableSerializer.ProcessDirectoryParallelAsync(config.ParsedReportsPath, maxWorkers, cancellation);
    }

    public async Task<List<ProcessedReport>> MergeReports(CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        var ptp = new PageTextPreparation(config.UseSerializedTables);
        var result = await ptp.ProcessReportsAsync(reportsDir: config.ParsedReportsPath, [], outputDir: config.MergedReportsPath, cancellation);
        Console.WriteLine($"Reports saved to {config.MergedReportsPath}");
        return result;
    }

    public async Task ExportReportsToMarkdown(List<ProcessedReport> processedReports, CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        var ptp = new PageTextPreparation(config.UseSerializedTables);
        await ptp.ExportToMarkdownAsync(processedReports, config.ReportsMarkdownPath, cancellation);
        Console.WriteLine($"Reports saved to {config.ReportsMarkdownPath}");
    }

    public async Task ExportReportsToMarkdown(CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        var ptp = new PageTextPreparation(config.UseSerializedTables);
        await ptp.ExportToMarkdownAsync(config.ParsedReportsPath, config.ReportsMarkdownPath, cancellation);
        Console.WriteLine($"Reports saved to {config.ReportsMarkdownPath}");
    }

    public async Task ChunkReports(bool includeSerializedTables = false, CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        var textSplitter = new TextSplitter();
        var serializedTablesDir = includeSerializedTables ? config.ParsedReportsPath : null;
        await textSplitter.SplitAllReportsAsync(config.MergedReportsPath, config.DocumentsDir, serializedTablesDir, cancellation);
        Console.WriteLine($"Chunked reports saved to {config.DocumentsDir}");
    }

    public async Task CreateVectorDbs(CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        var vdbIngestor = new VectorDBIngestor(config.LlmApiKey);
        await vdbIngestor.ProcessReportsAsync(config.DocumentsDir, config.VectorDbDir, cancellation);
        Console.WriteLine($"Vector databases created in {config.VectorDbDir}");
    }

    public async Task CreateBm25Db(CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        var bm25Ingestor = new BM25Ingestor();
        await bm25Ingestor.ProcessReportsAsync(config.DocumentsDir, config.Bm25DbPath, cancellation);
        Console.WriteLine($"BM25 database created at {config.Bm25DbPath}");
    }

    public async Task ParsePdfReports(bool parallel = true, int chunkSize = 2, int maxWorkers = 10, CancellationToken cancellation = default)
    {
        if (parallel)
            await ParsePdfReportsParallel(chunkSize, maxWorkers, cancellation);
        else
            await ParsePdfReportsSequential(cancellation);
    }

    public async Task ProcessParsedReports(CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        Console.WriteLine("Starting reports processing pipeline...");
        Console.WriteLine("Step 1: Merging reports...");
        var mergeDatas = await MergeReports(cancellation);
        Console.WriteLine("Step 2: Exporting reports to markdown...");
        await ExportReportsToMarkdown(mergeDatas, cancellation);
        Console.WriteLine("Step 3: Chunking reports...");
        await ChunkReports(config.UseSerializedTables, cancellation);
        Console.WriteLine("Step 4: Creating vector databases...");
        await CreateVectorDbs(cancellation);
        Console.WriteLine("Reports processing pipeline completed successfully!");
    }

    private static string GetNextAvailableFilename(string basePath)
    {
        if (!File.Exists(basePath))
            return basePath;

        var dir = Path.GetDirectoryName(basePath);
        var stem = Path.GetFileNameWithoutExtension(basePath);
        var ext = Path.GetExtension(basePath);
        int counter = 1;
        while (true)
        {
            var newPath = Path.Combine(dir ?? "", $"{stem}_{counter:00}{ext}");
            if (!File.Exists(newPath))
                return newPath;
            counter++;
        }
    }

    public async Task ProcessQuestions(CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        var processor = new QuestionsProcessor(
            config.VectorDbDir,
            config.DocumentsDir,
            config.QuestionsFilePath,
            true,
            config.SubsetPath,
            config.ParentDocumentRetrieval,
            config.LlmReranking,
            config.LlmRerankingSampleSize,
            config.TopNRetrieval,
            config.ParallelRequests,
            config.ApiProvider,
            config.AnsweringModel,
            config.FullContext
        );
        var outputPath = GetNextAvailableFilename(config.AnswersFilePath);
        await processor.ProcessAllQuestionsAsync(
            outputPath,
            config.SubmissionFile,
            config.TeamEmail,
            config.SubmissionName,
            config.PipelineDetails,
            cancellation
        );
        Console.WriteLine($"Answers saved to {outputPath}");
    }
}
