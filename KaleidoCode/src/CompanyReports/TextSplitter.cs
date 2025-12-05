
using CompanyReports.PDFParser;
using CompanyReports.TokenCounter;

namespace CompanyReports;

public class TextSplitter
{
    private readonly ITokenCounter _tokenCounter = new SharpTokenCounter();

    public async Task SplitAllReportsAsync(string inputDir, string outputDir, string? serializedTableDir = null, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputDir);
        var files = Directory.GetFiles(inputDir, "*.json");

        foreach (var file in files)
        {
            string filename = System.IO.Path.GetFileName(file);
            string? serializedPath = serializedTableDir != null
                ? System.IO.Path.Combine(serializedTableDir, filename)
                : null;

            var content = JsonSerializer.Deserialize<ProcessedReport>(await File.ReadAllTextAsync(file, cancellationToken));
            if (content == null)
            {
                continue;
            }
            var updated = await SplitReportAsync(content, serializedPath, cancellationToken);
            File.WriteAllText(System.IO.Path.Combine(outputDir, filename), JsonSerializer.Serialize(updated));
        }

        Console.WriteLine($"Split {files.Length} files.");
    }



    #region private methods
    private async Task<ProcessedReport> SplitReportAsync(ProcessedReport fileContent, string? serializedTablesPath = null, CancellationToken cancellationToken = default)
    {
        List<ProcessedChunk> chunks = [];
        int chunkId = 0;

        var tablesByPage = new Dictionary<int, List<ProcessedChunk>>();
        if (!string.IsNullOrEmpty(serializedTablesPath) && File.Exists(serializedTablesPath))
        {
            var reportData = JsonSerializer.Deserialize<PdfReport>(await File.ReadAllTextAsync(serializedTablesPath, cancellationToken));
            tablesByPage = GetSerializedTablesByPage(reportData?.Tables ?? []);
        }

        foreach (var page in fileContent.Content.Pages)
        {
            int pageNumber = page.Page;
            var pageText = page.Text;
            var pageChunks = SplitPage(pageText, pageNumber);

            foreach (var chunk in pageChunks)
            {
                chunk.Id = chunkId++;
                chunk.Type = "content";
                chunks.Add(chunk);
            }

            if (tablesByPage.TryGetValue(pageNumber, out List<ProcessedChunk>? value))
            {
                foreach (var table in value)
                {
                    table.Id = chunkId++;
                    table.Type = "serialized_table";
                    chunks.Add(table);
                }
            }
        }

        fileContent.Content.Chunks = chunks;
        return fileContent;
    }

    private Dictionary<int, List<ProcessedChunk>> GetSerializedTablesByPage(List<ReportTable> tables)
    {
        var result = new Dictionary<int, List<ProcessedChunk>>();
        foreach (var table in tables)
        {
            if (string.IsNullOrEmpty(table.Serialized))
            {
                continue;
            }


            int page = table.Page;
            var infoBlocks = JsonSerializer.Deserialize<TableBlocksCollection>(table.Serialized)?.InformationBlocks ?? [];
            var text = string.Join("\n", infoBlocks.Select(b => b.InformationBlock));

            if (!result.ContainsKey(page))
                result[page] = [];

            result[page].Add(new ProcessedChunk
            {
                Page = page,
                Text = text,
                TableId = table.TableId,
                LengthTokens = _tokenCounter.Count(text)
            });
        }

        return result;
    }

    private IEnumerable<ProcessedChunk> SplitPage(string text, int page, int chunkSize = 300, int overlap = 50)
    {
        var splitTexts = RecursiveTokenTextSplitter.SharpTokenSplitText(text, chunkSize: chunkSize, chunkOverlap: overlap);
        foreach (var splitText in splitTexts)
        {
            yield return new ProcessedChunk
            {
                Page = page,
                Text = splitText,
                LengthTokens = _tokenCounter.Count(splitText),
                TableId = -1
            };
        }
    }

    #endregion

}