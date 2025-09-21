
using System.Globalization;
using CsvHelper;
using Path = System.IO.Path;

namespace KaleidoCode.KernelService.CompanyReports;

public class Pipeline
{
    private readonly IOptionsMonitor<CompanyReportlOptions> runConfig;
    private readonly IPDFParser pdfParser;
    private readonly TableSerializer tableSerializer;
    private readonly IIngestor bm25Ingestor;
    private readonly IIngestor vectorDBIngestor;
    private readonly QuestionsProcessor questionsProcessor;
    private readonly ILogger<Pipeline> logger;

    private readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Pipeline(
        ILogger<Pipeline> logger,
        IOptionsMonitor<CompanyReportlOptions> runConfig,
        [FromKeyedServices("docling")] IPDFParser pdfParser,
        TableSerializer tableSerializer,
        [FromKeyedServices("BM25")] IIngestor bm25Ingestor,
        [FromKeyedServices("VectorDB")] IIngestor vectorDBIngestor,
        QuestionsProcessor questionsProcessor)
    {
        this.logger = logger;
        this.runConfig = runConfig;
        this.pdfParser = pdfParser;
        this.tableSerializer = tableSerializer;
        this.bm25Ingestor = bm25Ingestor;
        this.vectorDBIngestor = vectorDBIngestor;
        this.questionsProcessor = questionsProcessor;
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
                logger.LogError("Error converting JSON to CSV: {Message}", e.Message);
            }
        }
    }

    public async Task ParsePdfReportsSequential(CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        await pdfParser.ParseAndExportAsync(config.PdfReportsDir, cancellation);
        logger.LogInformation("PDF reports parsed and saved to {ParsedReportsPath}", config.ParsedReportsPath);
    }

    public async Task ParsePdfReportsParallel(int chunkSize = 2, int maxWorkers = 10, CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        await pdfParser.ParseAndExportParallelAsync(config.PdfReportsDir, maxWorkers, chunkSize, cancellation);
        logger.LogInformation("PDF reports parsed and saved to {ParsedReportsPath}", config.ParsedReportsPath);
    }

    public async Task SerializeTables(int maxWorkers = 10, CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        await tableSerializer.ProcessDirectoryParallelAsync(config.ParsedReportsPath, maxWorkers, cancellation);
    }

    public async Task<List<ProcessedReport>> MergeReports(CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        var ptp = new PageTextPreparation(config.UseSerializedTables, config.SerializedTablesInsteadOfMarkdown);
        var result = await ptp.ProcessReportsAsync(reportsDir: config.ParsedReportsPath, [], outputDir: config.MergedReportsPath, cancellation);
        logger.LogInformation("Reports saved to {MergedReportsPath}", config.MergedReportsPath);
        return result;
    }

    public async Task ExportReportsToMarkdown(List<ProcessedReport> processedReports, CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        var ptp = new PageTextPreparation(config.UseSerializedTables, config.SerializedTablesInsteadOfMarkdown);
        await ptp.ExportToMarkdownAsync(processedReports, config.ReportsMarkdownPath, cancellation);
        logger.LogInformation("Reports saved to {ReportsMarkdownPath}", config.ReportsMarkdownPath);
    }

    public async Task ExportReportsToMarkdown(CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        var ptp = new PageTextPreparation(config.UseSerializedTables);
        await ptp.ExportToMarkdownAsync(config.ParsedReportsPath, config.ReportsMarkdownPath, cancellation);
        logger.LogInformation("Reports saved to {ReportsMarkdownPath}", config.ReportsMarkdownPath);
    }

    public async Task ChunkReports(bool includeSerializedTables = false, CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        var textSplitter = new TextSplitter();
        var serializedTablesDir = includeSerializedTables ? config.ParsedReportsPath : null;
        await textSplitter.SplitAllReportsAsync(config.MergedReportsPath, config.DocumentsDir, serializedTablesDir, cancellation);
        logger.LogInformation("Chunked reports saved to {DocumentsDir}", config.DocumentsDir);
    }

    public async Task CreateVectorDbs(CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        await vectorDBIngestor.ProcessReportsAsync(config.DocumentsDir, config.VectorDbDir, cancellation);
        logger.LogInformation("Vector databases created in {VectorDbDir}", config.VectorDbDir);
    }

    public async Task CreateBm25Db(CancellationToken cancellation = default)
    {
        var config = runConfig.CurrentValue;
        await bm25Ingestor.ProcessReportsAsync(config.DocumentsDir, config.Bm25DbPath, cancellation);
        logger.LogInformation("BM25 database created at {Bm25DbPath}", config.Bm25DbPath);
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
        logger.LogInformation("Starting reports processing pipeline...");
        logger.LogInformation("Step 1: Merging reports...");
        var mergeDatas = await MergeReports(cancellation);
        logger.LogInformation("Step 2: Exporting reports to markdown...");
        await ExportReportsToMarkdown(mergeDatas, cancellation);
        logger.LogInformation("Step 3: Chunking reports...");
        await ChunkReports(config.UseSerializedTables, cancellation);
        logger.LogInformation("Step 4: Creating vector databases...");
        await CreateVectorDbs(cancellation);
        logger.LogInformation("Reports processing pipeline completed successfully!");
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
        var outputPath = GetNextAvailableFilename(config.AnswersFilePath);
        await questionsProcessor.ProcessAllQuestionsAsync(
            outputPath,
            config.SubmissionFile,
            config.TeamEmail,
            config.SubmissionName,
            config.PipelineDetails,
            cancellation
        );
        logger.LogInformation("Answers saved to {outputPath}", outputPath);
    }
}
