
using Path = System.IO.Path;

namespace AspnetcoreEx.KernelService.CompanyReports;

public class TableSerializer
{
    private readonly ILogger<TableSerializer> logger;
    private readonly JsonSerializerOptions DefaultJsonSerializerOptions = new() { WriteIndented = true };
    private readonly AsyncOpenaiProcessor openaiProcessor;
    public TableSerializer(ILogger<TableSerializer> logger, AsyncOpenaiProcessor openaiProcessor)
    {
        this.logger = logger;
        this.openaiProcessor = openaiProcessor;
    }

    public async Task ProcessDirectoryParallelAsync(string inputDir, int maxDegreeOfParallelism = 5, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting parallel table serialization...");

        var jsonFiles = Directory.GetFiles(inputDir, "*.json");
        if (jsonFiles.Length == 0)
        {
            logger.LogWarning("No JSON files found in {inputDir}", inputDir);
            return;
        }

        await Parallel.ForEachAsync(jsonFiles, new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        }, async (jsonFile, ct) =>
        {
            await ProcessFileAsync(jsonFile, cancellationToken);
        });

        logger.LogInformation("Table serialization completed!");
    }

    public async Task ProcessFileAsync(string jsonPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonText = await File.ReadAllTextAsync(jsonPath, cancellationToken);
            var jsonReport = JsonSerializer.Deserialize<PdfReport>(jsonText) ?? throw new ArgumentNullException(nameof(jsonPath), "json text cannot be deserialize.");

            int threadId = Environment.CurrentManagedThreadId;
            string requestsFilepath = $"./temp/async_llm_requests_{threadId}.jsonl";
            string resultsFilepath = $"./temp/async_llm_results_{threadId}.jsonl";

            PdfReport updatedReport;
            try
            {
                updatedReport = await SerializeTablesAsync(
                    jsonReport,
                    requestsFilepath,
                    resultsFilepath,
                    cancellationToken
                );
            }
            finally
            {
                try { File.Delete(requestsFilepath); } catch (FileNotFoundException) { }
                try { File.Delete(resultsFilepath); } catch (FileNotFoundException) { }
            }

            var updatedJson = JsonSerializer.Serialize(updatedReport, DefaultJsonSerializerOptions);
            await File.WriteAllTextAsync(jsonPath, updatedJson, cancellationToken);
        }
        catch (JsonException e)
        {
            logger.LogError("JSON Error in {path}: {message}", Path.GetFileName(jsonPath), e.Message);
            throw;
        }
        catch (Exception e)
        {
            logger.LogError("Error processing {path}: {message}", Path.GetFileName(jsonPath), e.Message);
            throw;
        }
    }

    /// <summary>
    /// Processes all tables in the given JSON report asynchronously by sending each table (with its context)
    /// to a large language model (such as OpenAI GPT) for intelligent serialization (e.g., summarization, restructuring, or extraction).
    /// The results are then written back to the original JSON report structure.
    /// 
    /// Typical use cases:
    /// - Automating the batch processing of tables extracted from documents (PDF, Word, web pages, etc.).
    /// - Leveraging LLMs to intelligently summarize, standardize, or structure table content.
    /// - Scenarios requiring unified, intelligent processing of large numbers of tables, such as financial reports, contracts, or data analysis.
    /// </summary>
    public async Task<PdfReport> SerializeTablesAsync(PdfReport jsonReport, string requestsFilepath, string resultsFilepath, CancellationToken cancellationToken = default)
    {
        var queries = new List<string>();
        var tableIndices = new List<string>();
        foreach (var table in jsonReport.Tables)
        {
            var tableIndex = table.TableId.ToString();
            tableIndices.Add(tableIndex);

            var (contextBefore, contextAfter) = GetTableContext(jsonReport, tableIndex);

            var tableInfo = FindTableById(jsonReport.Tables, tableIndex);
            var tableContent = tableInfo?.Html ?? "";

            // Construct the query
            var query = "";
            if (!string.IsNullOrEmpty(contextBefore))
                query += $"Here is additional text before the table that might be relevant (or not):\n\"\"\"{contextBefore}\"\"\"\n\n";

            if (!string.IsNullOrEmpty(tableContent))
                query += $"Here is a table in HTML format:\n\"\"\"{tableContent}\"\"\"";

            if (!string.IsNullOrEmpty(contextAfter))
                query += $"\n\nHere is additional text after the table that might be relevant (or not):\n\"\"\"{contextAfter}\"\"\"";

            queries.Add(query);
        }

        // Call OpenAI 
        var results = await openaiProcessor.ProcessStructuredOutputsRequestsAsync(
            model: "gpt-4o-mini-2024-07-18",
            temperature: 0,
            systemContent: TableSerialization.SystemPrompt,
            queries: queries,
            responseFormat: typeof(TableBlocksCollection),
            preserveRequests: false,
            preserveResults: false,
            requestsFilepath: requestsFilepath,
            saveFilepath: resultsFilepath,
            cancellationToken: cancellationToken
        );

        for (int idx = 0; idx < tableIndices.Count; idx++)
        {
            var tableIndex = tableIndices[idx];
            var result = results[idx];
            var tableInfo = FindTableById(jsonReport.Tables, tableIndex);

            if (tableInfo != null)
            {
                tableInfo.Serialized = result.Answer;
            }
        }

        return jsonReport;
    }

    private static ReportTable? FindTableById(List<ReportTable> tables, string tableId)
    {
        return tables.FirstOrDefault(p => p.TableId.ToString() == tableId);
    }

    // | page | type     | conent |
    // | -- | --------- | -- |
    // | 1  | paragraph | A  |
    // | 1  | paragraph | B  |
    // | 1  | table     | table1 |
    // | 1  | paragraph | C  |
    // | 1  | table     | table2 |
    // | 1  | paragraph | D  |
    // | 1  | paragraph | E  |
    // If you search for table_id = table1, this code will return:
    // context_before: "A\nB"
    // context_after: "C"
    private static (string ContextBefore, string ContextAfter) GetTableContext(PdfReport jsonReport, string targetTableIndex)
    {
        var tableInfo = FindTableById(jsonReport.Tables, targetTableIndex);
        if (tableInfo == null)
        {
            return ("", "");
        }

        int pageNum = tableInfo.Page;

        var page = jsonReport.Content.FirstOrDefault(p => p.Page == pageNum);
        var pageContent = page?.Content ?? [];

        if (page == null || pageContent.Count == 0)
        {
            return ("", "");
        }

        int currentTablePosition = -1;
        for (int i = 0; i < pageContent.Count; i++)
        {
            var block = pageContent[i];
            if (block.Type == "table" && block.TableId.ToString() == targetTableIndex)
            {
                currentTablePosition = i;
                break;
            }
        }

        int previousTablePosition = -1;
        for (int i = currentTablePosition - 1; i >= 0; i--)
        {
            if (pageContent[i].Type == "table")
            {
                previousTablePosition = i;
                break;
            }
        }

        int nextTablePosition = -1;
        for (int i = currentTablePosition + 1; i < pageContent.Count; i++)
        {
            if (pageContent[i].Type == "table")
            {
                nextTablePosition = i;
                break;
            }
        }

        // Get the text block above the current table
        int startPosition = previousTablePosition != -1 ? previousTablePosition + 1 : 0;
        var contextBeforeBlocks = pageContent
            .Skip(startPosition)
            .Take(currentTablePosition - startPosition)
            .Where(b => !string.IsNullOrEmpty(b.Text))
            .Select(b => b.Text);

        // Get the text block below the current table
        List<ReportContentItem> contextAfterBlocks = [];
        if (nextTablePosition == -1)
        {
            contextAfterBlocks = [.. pageContent
                .Skip(currentTablePosition + 1)
                .Take(3)];
        }
        else
        {
            int blocksBetween = nextTablePosition - (currentTablePosition + 1);
            if (blocksBetween > 3)
            {
                contextAfterBlocks = [.. pageContent
                    .Skip(currentTablePosition + 1)
                    .Take(3)];
            }
            else if (blocksBetween > 1)
            {
                contextAfterBlocks = [.. pageContent
                    .Skip(currentTablePosition + 1)
                    .Take(blocksBetween)];
            }
        }

        var contextAfter = contextAfterBlocks
            .Where(b => !string.IsNullOrEmpty(b.Text))
            .Select(b => b.Text);

        return (
            string.Join("\n", contextBeforeBlocks),
            string.Join("\n", contextAfter)
        );
    }

}
