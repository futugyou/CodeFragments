using Microsoft.Extensions.Logging.Abstractions;
using Tabula;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Path = System.IO.Path;

namespace AspnetcoreEx.KernelService.CompanyReports;

/// <summary>
/// It is best to let Python handle it. 
/// PdfPig+Tabula cannot achieve the capabilities of Docling.
/// </summary>
public class PdfPigParser : IPDFParser
{
    private readonly ILogger<PdfPigParser> _logger;
    private readonly string _outputDir;
    private readonly Dictionary<string, Metadata> _metadataLookup;
    private readonly string? _debugDataPath;
    private readonly JsonSerializerOptions DefaultJsonSerializerOptions = new() { WriteIndented = true };

    public PdfPigParser(ILogger<PdfPigParser>? logger, string outputDir, string? csvMetadataPath = null, string? debugDataPath = null)
    {
        _logger = logger ?? NullLogger<PdfPigParser>.Instance;
        _outputDir = outputDir;
        _metadataLookup = csvMetadataPath != null ? IPDFParser.ParseCsvMetadata(csvMetadataPath) : [];
        _debugDataPath = debugDataPath;
    }

    public Task ParseAndExportAsync(string doclingDirPath, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;
        Directory.CreateDirectory(_outputDir);
        var inputDocPaths = Directory.GetFiles(doclingDirPath, "*.json")?.ToList() ?? [];

        var convResults = ConvertDocuments(inputDocPaths);
        var (successCount, failureCount) = ProcessDocuments(convResults);

        var elapsed = DateTime.Now - startTime;
        if (failureCount > 0)
        {
            _logger.LogError("Failed converting {failureCount} out of {Count} documents.", failureCount, inputDocPaths.Count);
            throw new Exception($"Failed converting {failureCount} out of {inputDocPaths.Count} documents.");
        }
        _logger.LogInformation("Completed in {TotalSeconds} seconds. Successfully converted {successCount}/{Count} documents.", elapsed.TotalSeconds, successCount, inputDocPaths.Count);
        return Task.CompletedTask;
    }

    public Task ParseAndExportParallelAsync(string doclingDirPath, int optimalWorkers = 10, int? chunkSize = null, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;
        Directory.CreateDirectory(_outputDir);
        var inputDocPaths = Directory.GetFiles(doclingDirPath, "*.json")?.ToList() ?? [];
        int totalDocs = inputDocPaths.Count;
        if (chunkSize == null || chunkSize <= 0)
        {
            chunkSize = Math.Max(1, totalDocs / optimalWorkers);
        }

        var chunks = inputDocPaths
            .Select((path, idx) => new { path, idx })
            .GroupBy(x => x.idx / chunkSize)
            .Select(g => g.Select(x => x.path).ToList())
            .ToList();

        var successCount = 0;
        var failureCount = 0;

        Parallel.ForEach(
            chunks,
            new ParallelOptions { MaxDegreeOfParallelism = optimalWorkers },
            () => (succ: 0, fail: 0), // init
            (chunk, state, localCounts) =>
            {
                try
                {
                    var convResults = ConvertDocuments(chunk);
                    var (succ, fail) = ProcessDocuments(convResults);
                    _logger.LogInformation("Processed {Count} PDFs in parallel.", chunk.Count);
                    return (localCounts.succ + succ, localCounts.fail + fail);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error processing chunk: {Message}", ex.Message);
                    return (localCounts.succ, localCounts.fail + chunk.Count);
                }
            },
            localCounts =>
            {
                Interlocked.Add(ref successCount, localCounts.succ);
                Interlocked.Add(ref failureCount, localCounts.fail);
            });

        var elapsed = DateTime.Now - startTime;
        if (failureCount > 0)
        {
            _logger.LogError("Failed converting {FailureCount} out of {TotalDocs} documents.", failureCount, totalDocs);
            throw new Exception($"Failed converting {failureCount} out of {totalDocs} documents.");
        }
        _logger.LogInformation("Parallel processing completed in {TotalSeconds} seconds. Successfully converted {successCount}/{totalDocs} documents.", elapsed.TotalSeconds, successCount, totalDocs);
        return Task.CompletedTask;
    }

    public IEnumerable<ConversionResult> ConvertDocuments(List<string> inputDocPaths)
    {
        foreach (var path in inputDocPaths)
        {
            ConversionResult result;
            try
            {
                result = ConvertSingleDocumentInternal(path);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to convert {path}: {Message}", path, ex.Message);
                result = new ConversionResult
                {
                    Status = ConversionStatus.Failure,
                    Input = new ConversionInput { File = new FileInfo(path) },
                    ErrorMessage = ex.Message,
                };
            }


            yield return result;
        }
    }

    private static ConversionResult ConvertSingleDocumentInternal(string path)
    {
        using var pdf = PdfDocument.Open(path, new ParsingOptions() { ClipPaths = true });
        var result = new ConversionResult
        {
            Status = ConversionStatus.Success,
            Input = new ConversionInput { File = new FileInfo(path) },
            Document = new ParsedDocument(),
        };
        foreach (var page in pdf.GetPages())
        {
            var tables = GetPdfTables(page);
            var images = GetPdfPictures(page);
            var words = GetPdfWords(page);
            result.Document.Tables.AddRange(tables);
            result.Document.Words.AddRange(words);
            result.Document.Pictures.AddRange(images);
            result.Document.Pages.Add(page);
        }
        return result;
    }

    private static IEnumerable<(Page, IPdfImage)> GetPdfPictures(Page page)
    {
        return (page.GetImages() ?? []).Select(img => (page, img));
    }

    private static IEnumerable<(Page, Word)> GetPdfWords(Page page)
    {
        return (page.GetWords() ?? []).Select(word => (page, word));
    }

    private static IEnumerable<(Page, Tabula.Table)> GetPdfTables(Page page)
    {
        var tables = page.GetTablesLattice();
        if (tables.Any() && tables.First().Rows.Count > 0)
        {
            tables = page.GetTablesStream();
        }

        return (tables ?? []).Select(t => (page, t));
    }

    private (int successCount, int failureCount) ProcessDocuments(IEnumerable<ConversionResult> convResults)
    {
        int successCount = 0, failureCount = 0;
        foreach (var convRes in convResults)
        {
            if (convRes.Status == ConversionStatus.Success)
            {
                successCount++;
                var processor = new JsonReportProcessor(_debugDataPath, _metadataLookup);
                var processedReport = processor.AssembleReport(convRes);
                var docFilename = Path.GetFileNameWithoutExtension(convRes.Input.File.Name);
                if (!string.IsNullOrWhiteSpace(docFilename))
                {
                    var outPath = Path.Combine(_outputDir, $"{docFilename}.json");
                    File.WriteAllText(outPath, JsonSerializer.Serialize(processedReport, DefaultJsonSerializerOptions));
                }
            }
            else
            {
                failureCount++;
                _logger.LogInformation("Document {FullName} failed to convert.", convRes.Input.File.FullName);
            }
        }
        _logger.LogInformation("Processed {TotalCount} docs, of which {FailureCount} failed", successCount + failureCount, failureCount);
        return (successCount, failureCount);
    }
}

public enum ConversionStatus
{
    Success,
    Failure
}

public class ConversionInput
{
    public FileInfo File { get; set; }
}

public class ConversionResult
{
    public ConversionStatus Status { get; set; }
    public ConversionInput Input { get; set; }
    public ParsedDocument Document { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class ParsedDocument
{
    public List<Page> Pages { get; set; } = [];
    public List<(Page, Tabula.Table)> Tables { get; set; } = [];
    public List<(Page, IPdfImage)> Pictures { get; set; } = [];
    public List<(Page, Word)> Words { get; set; } = [];
}
