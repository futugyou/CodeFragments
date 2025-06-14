
using System.Globalization;
using CsvHelper;
using Path = System.IO.Path;

namespace AspnetcoreEx.KernelService.CompanyReports;

public class RunConfig
{
    public bool UseSerializedTables { get; set; } = false;
    public string ConfigSuffix { get; set; } = "";
    public bool ParentDocumentRetrieval { get; set; } = true;
    public bool LlmReranking { get; set; } = true;
    public int LlmRerankingSampleSize { get; set; } = 10;
    public int TopNRetrieval { get; set; } = 5;
    public bool ParallelRequests { get; set; } = true;
    public string ApiProvider { get; set; } = "OpenAI";
    public string LlmApiKey { get; set; } = "";
    public string AnsweringModel { get; set; } = "gpt-3.5-turbo";
    public bool FullContext { get; set; } = false;
    public string SubmissionFile { get; set; } = "";
    public string TeamEmail { get; set; } = "";
    public string SubmissionName { get; set; } = "";
    public string PipelineDetails { get; set; } = "";

}

public class PipelineConfig
{
    public string RootPath { get; }
    public string SubsetPath { get; }
    public string QuestionsFilePath { get; }
    public string PdfReportsDir { get; }
    public string ParsedReportsPath { get; }
    public string ParsedReportsDebugPath { get; }
    public string MergedReportsPath { get; }
    public string ReportsMarkdownPath { get; }
    public string DocumentsDir { get; }
    public string VectorDbDir { get; }
    public string Bm25DbPath { get; }
    public string AnswersFilePath { get; }

    public PipelineConfig(string rootPath, string subsetName, string questionsFileName, string pdfReportsDirName, bool useSerializedTables, string configSuffix)
    {
        RootPath = rootPath;
        SubsetPath = Path.Combine(rootPath, subsetName);
        QuestionsFilePath = Path.Combine(rootPath, questionsFileName);
        PdfReportsDir = Path.Combine(rootPath, pdfReportsDirName);
        ParsedReportsPath = Path.Combine(rootPath, $"parsed_reports{configSuffix}");
        ParsedReportsDebugPath = Path.Combine(rootPath, $"parsed_reports_debug{configSuffix}");
        MergedReportsPath = Path.Combine(rootPath, $"merged_reports{configSuffix}.json");
        ReportsMarkdownPath = Path.Combine(rootPath, $"reports_markdown{configSuffix}");
        DocumentsDir = Path.Combine(rootPath, $"documents{configSuffix}");
        VectorDbDir = Path.Combine(rootPath, $"vector_db{configSuffix}");
        Bm25DbPath = Path.Combine(rootPath, $"bm25_db{configSuffix}.pkl");
        AnswersFilePath = Path.Combine(rootPath, $"answers{configSuffix}.json");
    }
}

// TODO: need DI Extensions
public class Pipeline
{
    private readonly RunConfig runConfig;
    private readonly PipelineConfig paths;

    private readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Pipeline(string rootPath, string subsetName = "subset.csv", string questionsFileName = "questions.json", string pdfReportsDirName = "pdf_reports", RunConfig? runConfig = null)
    {
        this.runConfig = runConfig ?? new RunConfig();
        this.paths = InitializePaths(rootPath, subsetName, questionsFileName, pdfReportsDirName);
        ConvertJsonToCsvIfNeeded();
    }

    private PipelineConfig InitializePaths(string rootPath, string subsetName, string questionsFileName, string pdfReportsDirName)
    {
        return new PipelineConfig(
            rootPath,
            subsetName,
            questionsFileName,
            pdfReportsDirName,
            runConfig.UseSerializedTables,
            runConfig.ConfigSuffix
        );
    }

    private void ConvertJsonToCsvIfNeeded()
    {
        var jsonPath = Path.Combine(paths.RootPath, "subset.json");
        var csvPath = Path.Combine(paths.RootPath, "subset.csv");

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

    public async Task ParsePdfReportsSequential()
    {
        var pdfParser = new DoclingPDFParser(null, paths.ParsedReportsPath, paths.SubsetPath);
        await pdfParser.ParseAndExportAsync(paths.PdfReportsDir);
        Console.WriteLine($"PDF reports parsed and saved to {paths.ParsedReportsPath}");
    }

    public async Task ParsePdfReportsParallel(int chunkSize = 2, int maxWorkers = 10)
    {
        var pdfParser = new DoclingPDFParser(null, paths.ParsedReportsPath, paths.SubsetPath);
        await pdfParser.ParseAndExportParallelAsync(paths.PdfReportsDir, maxWorkers, chunkSize);
        Console.WriteLine($"PDF reports parsed and saved to {paths.ParsedReportsPath}");
    }

    public void SerializeTables(int maxWorkers = 10)
    {
        // var serializer = new TableSerializer();
        // serializer.ProcessDirectoryParallel(paths.ParsedReportsPath, maxWorkers);
    }

    public async Task<List<ProcessedReport>> MergeReports()
    {
        var ptp = new PageTextPreparation(runConfig.UseSerializedTables);
        var result = await ptp.ProcessReportsAsync(reportsDir: paths.ParsedReportsPath, [], outputDir: paths.MergedReportsPath);
        Console.WriteLine($"Reports saved to {paths.MergedReportsPath}");
        return result;
    }

    public async Task ExportReportsToMarkdown(List<ProcessedReport> processedReports)
    {
        var ptp = new PageTextPreparation(runConfig.UseSerializedTables);
        await ptp.ExportToMarkdownAsync(processedReports, paths.ReportsMarkdownPath);
        Console.WriteLine($"Reports saved to {paths.ReportsMarkdownPath}");
    }

    public async Task ExportReportsToMarkdown()
    {
        var ptp = new PageTextPreparation(runConfig.UseSerializedTables);
        await ptp.ExportToMarkdownAsync(paths.ParsedReportsPath, paths.ReportsMarkdownPath);
        Console.WriteLine($"Reports saved to {paths.ReportsMarkdownPath}");
    }

    public async Task ChunkReports(bool includeSerializedTables = false)
    {
        var textSplitter = new TextSplitter();
        var serializedTablesDir = includeSerializedTables ? paths.ParsedReportsPath : null;
        await textSplitter.SplitAllReportsAsync(paths.MergedReportsPath, paths.DocumentsDir, serializedTablesDir);
        Console.WriteLine($"Chunked reports saved to {paths.DocumentsDir}");
    }

    public async Task CreateVectorDbs()
    {
        var vdbIngestor = new VectorDBIngestor(runConfig.LlmApiKey);
        await vdbIngestor.ProcessReportsAsync(paths.DocumentsDir, paths.VectorDbDir);
        Console.WriteLine($"Vector databases created in {paths.VectorDbDir}");
    }

    public async Task CreateBm25Db()
    {
        var bm25Ingestor = new BM25Ingestor();
        await bm25Ingestor.ProcessReportsAsync(paths.DocumentsDir, paths.Bm25DbPath);
        Console.WriteLine($"BM25 database created at {paths.Bm25DbPath}");
    }

    public async Task ParsePdfReports(bool parallel = true, int chunkSize = 2, int maxWorkers = 10)
    {
        if (parallel)
            await ParsePdfReportsParallel(chunkSize, maxWorkers);
        else
            await ParsePdfReportsSequential();
    }

    public async Task ProcessParsedReports()
    {
        Console.WriteLine("Starting reports processing pipeline...");
        Console.WriteLine("Step 1: Merging reports...");
        var mergeDatas = await MergeReports();
        Console.WriteLine("Step 2: Exporting reports to markdown...");
        await ExportReportsToMarkdown(mergeDatas);
        Console.WriteLine("Step 3: Chunking reports...");
        await ChunkReports();
        Console.WriteLine("Step 4: Creating vector databases...");
        await CreateVectorDbs();
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

    public void ProcessQuestions()
    {
        var processor = new QuestionsProcessor(
            paths.VectorDbDir,
            paths.DocumentsDir,
            paths.QuestionsFilePath,
            true,
            paths.SubsetPath,
            runConfig.ParentDocumentRetrieval,
            runConfig.LlmReranking,
            runConfig.LlmRerankingSampleSize,
            runConfig.TopNRetrieval,
            runConfig.ParallelRequests,
            runConfig.ApiProvider,
            runConfig.AnsweringModel,
            runConfig.FullContext
        );
        var outputPath = GetNextAvailableFilename(paths.AnswersFilePath);
        processor.ProcessAllQuestions(
            outputPath,
            runConfig.SubmissionFile,
            runConfig.TeamEmail,
            runConfig.SubmissionName,
            runConfig.PipelineDetails
        );
        Console.WriteLine($"Answers saved to {outputPath}");
    }
}
