
using System.Globalization;
using CsvHelper;
using UglyToad.PdfPig;
using Path = System.IO.Path;

namespace AspnetcoreEx.KernelService.CompanyReports;

public class PDFParser
{
    private readonly ILogger<PDFParser> _logger;
    private readonly string _outputDir;
    private readonly Dictionary<string, Metadata> _metadataLookup;
    private readonly JsonSerializerOptions DefaultJsonSerializerOptions = new() { WriteIndented = true };

    public PDFParser(ILogger<PDFParser> logger, string outputDir, string? csvMetadataPath = null)
    {
        _logger = logger;
        _outputDir = outputDir;
        _metadataLookup = csvMetadataPath != null ? ParseCsvMetadata(csvMetadataPath) : [];
    }

    private static Dictionary<string, Metadata> ParseCsvMetadata(string csvPath)
    {
        var lookup = new Dictionary<string, Metadata>();
        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        foreach (var record in csv.GetRecords<Metadata>())
        {
            lookup[record.Sha1] = record;
        }
        return lookup;
    }

    public void ParseAndExport(List<string> inputDocPaths)
    {
        var startTime = DateTime.Now;
        Directory.CreateDirectory(_outputDir);

        var convResults = ConvertDocuments(inputDocPaths);
        var (successCount, failureCount) = ProcessDocuments(convResults);

        var elapsed = DateTime.Now - startTime;
        if (failureCount > 0)
        {
            _logger.LogError("Failed converting {failureCount} out of {Count} documents.", failureCount, inputDocPaths.Count);
            throw new Exception($"Failed converting {failureCount} out of {inputDocPaths.Count} documents.");
        }
        _logger.LogInformation("Completed in {TotalSeconds} seconds. Successfully converted {successCount}/{Count} documents.", elapsed.TotalSeconds, successCount, inputDocPaths.Count);
    }

    public void ParseAndExportParallel(List<string> inputDocPaths, int optimalWorkers = 10)
    {
        var startTime = DateTime.Now;
        Directory.CreateDirectory(_outputDir);

        int totalDocs = inputDocPaths.Count;
        int chunkSize = Math.Max(1, totalDocs / optimalWorkers);
        var chunks = inputDocPaths
            .Select((path, idx) => new { path, idx })
            .GroupBy(x => x.idx / chunkSize)
            .Select(g => g.Select(x => x.path).ToList())
            .ToList();

        var successCount = 0;
        var failureCount = 0;
        var lockObj = new object();

        Parallel.ForEach(chunks, new ParallelOptions { MaxDegreeOfParallelism = optimalWorkers }, chunk =>
        {
            try
            {
                var convResults = ConvertDocuments(chunk);
                var (succ, fail) = ProcessDocuments(convResults);
                lock (lockObj)
                {
                    successCount += succ;
                    failureCount += fail;
                }
                _logger.LogInformation("Processed {Count} PDFs in parallel.", chunk.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing chunk: {Message}", ex.Message);
                lock (lockObj)
                {
                    failureCount += chunk.Count;
                }
            }
        });

        var elapsed = DateTime.Now - startTime;
        if (failureCount > 0)
        {
            _logger.LogError("Failed converting {FailureCount} out of {TotalDocs} documents.", failureCount, totalDocs);
            throw new Exception($"Failed converting {failureCount} out of {totalDocs} documents.");
        }
        _logger.LogInformation("Parallel processing completed in {TotalSeconds} seconds. Successfully converted {successCount}/{totalDocs} documents.", elapsed.TotalSeconds, successCount, totalDocs);
    }

    public IEnumerable<ConversionResult> ConvertDocuments(List<string> inputDocPaths)
    {
        foreach (var path in inputDocPaths)
        {
            ConversionResult result;
            try
            {
                using var pdf = PdfDocument.Open(path);
                var text = string.Join("\n", pdf.GetPages().Select(p => p.Text));
                result = new ConversionResult
                {
                    Status = ConversionStatus.Success,
                    Input = new ConversionInput { File = new FileInfo(path) },
                    Document = new ParsedDocument { Content = text }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to convert {path}: {Message}", path, ex.Message);
                result = new ConversionResult
                {
                    Status = ConversionStatus.Failure,
                    Input = new ConversionInput { File = new FileInfo(path) },
                    Document = null
                };
            }
            yield return result;
        }
    }

    public (int successCount, int failureCount) ProcessDocuments(IEnumerable<ConversionResult> convResults)
    {
        int successCount = 0, failureCount = 0;
        foreach (var convRes in convResults)
        {
            if (convRes.Status == ConversionStatus.Success)
            {
                successCount++;
                // TODO: JsonReportProcessor
                var data = convRes.Document.ExportToDict();
                var normalizedData = NormalizePageSequence(data);
                var processedReport = AssembleReport(convRes, normalizedData);
                var docFilename = Path.GetFileNameWithoutExtension(convRes.Input.File.Name);
                var outPath = Path.Combine(_outputDir, $"{docFilename}.json");
                File.WriteAllText(outPath, JsonSerializer.Serialize(processedReport, DefaultJsonSerializerOptions));
            }
            else
            {
                failureCount++;
                _logger.LogInformation("Document {FullName} failed to convert.", convRes.Input.File.FullName);
            }
        }
        _logger.LogInformation($"Processed {successCount + failureCount} docs, of which {failureCount} failed");
        return (successCount, failureCount);
    }

    public Dictionary<string, object> NormalizePageSequence(Dictionary<string, object> data)
    {
        // TODO:
        if (!data.TryGetValue("content", out object? value)) return data;
        if (value is not List<Dictionary<string, object>> content) return data;

        var existingPages = content.Select(p => Convert.ToInt32(p["page"])).ToHashSet();
        int maxPage = existingPages.Max();
        var newContent = new List<Dictionary<string, object>>();
        for (int pageNum = 1; pageNum <= maxPage; pageNum++)
        {
            var page = content.FirstOrDefault(p => Convert.ToInt32(p["page"]) == pageNum);
            if (page == null)
            {
                page = new Dictionary<string, object>
                {
                    ["page"] = pageNum,
                    ["content"] = new List<object>(),
                    ["page_dimensions"] = new Dictionary<string, object>()
                };
            }
            newContent.Add(page);
        }
        data["content"] = newContent;
        return data;
    }

    public Dictionary<string, object> AssembleReport(ConversionResult convRes, Dictionary<string, object> normalizedData)
    {
        // TODO: JsonReportProcessor
        return normalizedData;
    }
}

public class Metadata
{
    public string Sha1 { get; set; }
    public string CompanyName { get; set; }
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
}

public class ParsedDocument
{
    public string Content { get; set; }
    public Dictionary<string, object> ExportToDict()
    {
        // TODO: document.export_to_dict
        return new Dictionary<string, object>
        {
            { "content", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        { "page", 1 },
                        { "content", new List<object> { Content } },
                        { "page_dimensions", new Dictionary<string, object>() }
                    }
                }
            }
        };
    }
}
