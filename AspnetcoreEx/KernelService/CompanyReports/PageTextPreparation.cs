

using System.Text.RegularExpressions;

namespace AspnetcoreEx.KernelService.CompanyReports;

using System.Text.Json.Serialization;
using Path = System.IO.Path;

/// <summary>
/// Clean and format structured reports (JSON files), combine their contents (such as tables, lists, paragraphs, headers, etc.) into Markdown text according to rules,
///     and support exporting to Markdown files.
/// 
/// Its core functions include:
///     Read report JSON files, traverse each page of content, and format by block type (such as tables, lists, headers, paragraphs, etc.).
///     Support flexible switching between table serialization description and Markdown format.
///     Clean and correct text content.
///     Support batch processing of report files in a directory and output them as new JSON or Markdown files.
/// </summary>
public partial class PageTextPreparation(bool useSerializedTables = false, bool serializedTablesInsteadOfMarkdown = false)
{
    public bool UseSerializedTables { get; } = useSerializedTables;
    public bool SerializedTablesInsteadOfMarkdown { get; } = serializedTablesInsteadOfMarkdown;
    private readonly JsonSerializerOptions DefaultJsonSerializerOptions = new() { WriteIndented = true };
    private PdfReport _reportData;
    private static readonly Dictionary<string, string> CommandMapping = new()
    {
        ["zero"] = "0",
        ["one"] = "1",
        ["two"] = "2",
        ["three"] = "3",
        ["four"] = "4",
        ["five"] = "5",
        ["six"] = "6",
        ["seven"] = "7",
        ["eight"] = "8",
        ["nine"] = "9",
        ["period"] = ".",
        ["comma"] = ",",
        ["colon"] = ":",
        ["hyphen"] = "-",
        ["percent"] = "%",
        ["dollar"] = "$",
        ["space"] = " ",
        ["plus"] = "+",
        ["minus"] = "-",
        ["slash"] = "/",
        ["asterisk"] = "*",
        ["lparen"] = "(",
        ["rparen"] = ")",
        ["parenright"] = ")",
        ["parenleft"] = "(",
        ["wedge.1_E"] = ""
    };
    private static readonly Regex _glyphRegex = GlyphRegex();
    private static readonly Regex _atoZRegex = AtoZRegex();

    public async Task<List<ProcessedReport>> ProcessReportsAsync(string reportsDir, List<string> reportsPaths, string outputDir = "", CancellationToken cancellationToken = default)
    {
        var allReports = new List<ProcessedReport>();
        if (!string.IsNullOrEmpty(reportsDir))
        {
            reportsPaths = [.. Directory.GetFiles(reportsDir, "*.json")];
        }

        foreach (var reportPath in reportsPaths)
        {
            var reportData = JsonSerializer.Deserialize<PdfReport>(await File.ReadAllTextAsync(reportPath, cancellationToken));
            if (reportData == null)
            {
                continue;
            }
            var fullReportText = ProcessReport(reportData);
            var report = new ProcessedReport
            {
                Metainfo = reportData.Metainfo,
                Content = fullReportText
            };
            allReports.Add(report);

            if (!string.IsNullOrEmpty(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                File.WriteAllText(Path.Combine(outputDir, Path.GetFileName(reportPath)),
                    JsonSerializer.Serialize(report, DefaultJsonSerializerOptions));
            }
        }
        return allReports;
    }

    public ProcessedReportContent ProcessReport(PdfReport reportData)
    {
        _reportData = reportData;
        var processedPages = new List<ProcessedPageData>();
        int totalCorrections = 0;
        var correctionsList = new List<string>();

        foreach (var pageContent in reportData.Content)
        {
            int pageNumber = pageContent.Page;
            string pageText = PreparePageText(pageNumber);
            (string cleanedText, int correctionsCount, List<string> corrections) = CleanText(pageText);
            totalCorrections += correctionsCount;
            correctionsList.AddRange(corrections);
            processedPages.Add(new ProcessedPageData
            {
                Page = pageNumber,
                Text = cleanedText
            });
        }

        if (totalCorrections > 0)
        {
            Console.WriteLine($"Fixed {totalCorrections} occurrences in the file {reportData.Metainfo.Sha1Name}");
            Console.WriteLine(string.Join("\n", correctionsList.Take(30)));
        }

        return new ProcessedReportContent
        {
            Chunks = null!, // Placeholder for future use, if needed
            Pages = processedPages
        };
    }

    public string PreparePageText(int pageNumber)
    {
        var pageData = GetPageData(pageNumber);
        if (pageData == null || pageData.Content == null)
            return string.Empty;

        var filteredBlocks = FilterBlocks(pageData.Content);
        var finalBlocks = ApplyFormattingRules(filteredBlocks);

        if (finalBlocks.Count > 0)
        {
            finalBlocks[0] = finalBlocks[0].TrimStart();
            finalBlocks[^1] = finalBlocks.Last().TrimEnd();
        }

        return string.Join("\n", finalBlocks);
    }

    public async Task ExportToMarkdownAsync(string reportsDir, string outputDir, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputDir);
        foreach (var reportPath in Directory.GetFiles(reportsDir, "*.json"))
        {
            var reportData = JsonSerializer.Deserialize<PdfReport>(await File.ReadAllTextAsync(reportPath, cancellationToken));
            if (reportData == null)
            {
                continue;
            }

            var processedReport = ProcessReport(reportData);
            var sb = new StringBuilder();
            foreach (var page in processedReport.Pages)
            {
                sb.AppendLine($"\n\n---\n\n# Page {page.Page}\n\n");
                sb.AppendLine(page.Text);
            }

            var reportName = reportData.Metainfo.Sha1Name;
            await File.WriteAllTextAsync(Path.Combine(outputDir, $"{reportName}.md"), sb.ToString(), cancellationToken);
        }
    }

    #region Private Methods
    private ReportContent? GetPageData(int pageNumber)
    {
        return _reportData.Content.FirstOrDefault(p => p.Page == pageNumber);
    }

    private static List<ReportContentItem> FilterBlocks(List<ReportContentItem> blocks)
    {
        var ignoredTypes = new HashSet<string> { "pagefooter", "picture" };
        return [.. blocks.Where(b => b != null && !string.IsNullOrWhiteSpace(b.Text) && !ignoredTypes.Contains(b.Type))];
    }

    private static (string, int, List<string>) CleanText(string text)
    {
        var corrections = new List<string>();

        string recognizedCommands = string.Join("|", CommandMapping.Keys);
        string slashCommandPattern = @$"/({recognizedCommands})(\.pl\.tnum|\.tnum\.pl|\.pl|\.tnum|\.case|\.sups)";

        int occurrencesAmount = 0;

        // Count matches
        occurrencesAmount += Regex.Matches(text, slashCommandPattern).Count;
        occurrencesAmount += _glyphRegex.Matches(text).Count;
        occurrencesAmount += _atoZRegex.Matches(text).Count;

        // Replace slash commands
        text = Regex.Replace(text, slashCommandPattern, match =>
        {
            string baseCommand = match.Groups[1].Value;
            if (CommandMapping.TryGetValue(baseCommand, out var replacement))
            {
                corrections.Add($"{match.Value} -> {replacement}");
                return replacement;
            }
            return match.Value;
        });

        // Replace glyph<...>
        text = _glyphRegex.Replace(text, match =>
        {
            corrections.Add($"{match.Value} -> ");
            return "";
        });

        // Replace /X.cap
        text = _atoZRegex.Replace(text, match =>
        {
            string replacement = match.Groups[1].Value;
            corrections.Add($"{match.Value} -> {replacement}");
            return replacement;
        });

        return (text, occurrencesAmount, corrections);
    }

    private static bool BlockEndsWithColon(ReportContentItem block)
    {
        return block.Text != null && block.Text.Trim().EndsWith(':');
    }

    private List<string> ApplyFormattingRules(List<ReportContentItem> blocks)
    {
        var finalBlocks = new List<string>();
        bool pageHeaderInFirst3 = false;
        for (int j = 0; j < Math.Min(3, blocks.Count); j++)
        {
            if (blocks[j].Type == "pageheader") pageHeaderInFirst3 = true;
        }

        int firstSectionHeaderIndex = 0;
        int i = 0;
        int n = blocks.Count;
        while (i < n)
        {
            var block = blocks[i];
            var blockType = block.Type;
            var text = block.Text?.Trim() ?? string.Empty;
            // Handle headers
            if (blockType == "pageheader")
            {
                var prefix = i < 3 ? "\n# " : "\n## ";
                finalBlocks.Add($"{prefix}{text}\n");
                i++;
                continue;
            }
            if (blockType == "sectionheader")
            {
                firstSectionHeaderIndex++;
                string prefix = (firstSectionHeaderIndex == 1 && i < 3 && !pageHeaderInFirst3) ? "\n# " : "\n## ";
                finalBlocks.Add($"{prefix}{text}\n");
                i++;
                continue;
            }
            if (blockType == "paragraph")
            {
                if (BlockEndsWithColon(block) && i + 1 < n)
                {
                    var nextBlockType = blocks[i + 1].Type;
                    if (nextBlockType != "table" && nextBlockType != "listitem")
                    {
                        finalBlocks.Add($"\n### {text}\n");
                        i++;
                        continue;
                    }
                }
                finalBlocks.Add($"\n### {text}\n");
                i++;
                continue;
            }
            // Handle table groups
            if (blockType == "table" ||
                (BlockEndsWithColon(block) && i + 1 < n && blocks[i + 1].Type == "table"))
            {
                var groupBlocks = new List<ReportContentItem>();
                ReportContentItem? headerForTable;
                if (BlockEndsWithColon(block) && i + 1 < n)
                {
                    headerForTable = block;
                    var tableBlock = blocks[i + 1];
                    i += 2;
                    if (headerForTable != null) groupBlocks.Add(headerForTable);
                    groupBlocks.Add(tableBlock);
                }
                else
                {
                    groupBlocks.Add(block);
                    i++;
                }
                // Process subsequent text/footnote
                if (i < n && blocks[i].Type == "text")
                {
                    if ((i + 1 < n) && blocks[i + 1].Type == "footnote")
                    {
                        groupBlocks.Add(blocks[i]);
                        i++;
                    }
                }
                while (i < n && blocks[i].Type == "footnote")
                {
                    groupBlocks.Add(blocks[i]);
                    i++;
                }
                var groupText = RenderTableGroup(groupBlocks);
                finalBlocks.Add(groupText);
                continue;
            }
            // Handle list groups
            if (blockType == "listtime" ||
                (BlockEndsWithColon(block) && i + 1 < n && blocks[i + 1].Type == "listtime"))
            {
                var groupBlocks = new List<ReportContentItem>();
                if (BlockEndsWithColon(block) && i + 1 < n)
                {
                    groupBlocks.Add(block);
                    i++;
                }
                while (i < n && blocks[i].Type == "listtime")
                {
                    groupBlocks.Add(blocks[i]);
                    i++;
                }
                if (i < n && blocks[i].Type == "text")
                {
                    if ((i + 1 < n) && blocks[i + 1].Type == "footnote")
                    {
                        groupBlocks.Add(blocks[i]);
                        i++;
                    }
                }
                while (i < n && blocks[i].Type == "footnote")
                {
                    groupBlocks.Add(blocks[i]);
                    i++;
                }
                var groupText = RenderListGroup(groupBlocks);
                finalBlocks.Add(groupText);
                continue;
            }
            // Handle normal blocks
            if (blockType == "text" || blockType == "caption" || blockType == "footnote" ||
                blockType == "checkboxselected" || blockType == "checkboxunselected" || blockType == "formula")
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    i++;
                    continue;
                }
                finalBlocks.Add($"{text}\n");
                i++;
                continue;
            }
            throw new Exception($"Unknown block type: {blockType}");
        }
        return finalBlocks;
    }

    private string RenderTableGroup(List<ReportContentItem> groupBlocks)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        foreach (var blk in groupBlocks)
        {
            switch (blk.Type)
            {
                case "text":
                case "caption":
                case "sectionheader":
                case "paragraph":
                    sb.AppendLine(blk.Text);
                    break;
                case "table":
                    if (blk.TableId <= 0) continue;
                    sb.AppendLine(GetTableById(blk.TableId));
                    break;
                case "footnote":
                    sb.AppendLine(blk.Text);
                    break;
                default:
                    throw new Exception($"Unexpected block type in table group: {blk.Type}");
            }
        }
        sb.AppendLine();
        return sb.ToString();
    }

    private static string RenderListGroup(List<ReportContentItem> groupBlocks)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        foreach (var blk in groupBlocks)
        {
            switch (blk.Type)
            {
                case "text":
                case "caption":
                case "sectionheader":
                case "paragraph":
                    sb.AppendLine(blk.Text);
                    break;
                case "listitem":
                    sb.AppendLine("- " + (blk.Text?.Trim() ?? string.Empty));
                    break;
                case "footnote":
                    sb.AppendLine(blk.Text);
                    break;
                default:
                    throw new Exception($"Unexpected block type in list group: {blk.Type}");
            }
        }
        sb.AppendLine();
        return sb.ToString();
    }

    private string GetTableById(int tableId)
    {
        var table = (_reportData.Tables?.FirstOrDefault(t => t.TableId == tableId)) ?? throw new Exception($"Table with ID={tableId} not found in report_data!");
        if (UseSerializedTables)
            return GetSerializedTableText(table, SerializedTablesInsteadOfMarkdown);

        return table.Markdown ?? string.Empty;
    }

    private static string GetSerializedTableText(ReportTable table, bool serializedTablesInsteadOfMarkdown)
    {
        if (table.Serialized == null)
            return table.Markdown ?? string.Empty;

        var infoBlocks = JsonSerializer.Deserialize<TableBlocksCollection>(table.Serialized)?.InformationBlocks ?? [];
        var serializedText = string.Join("\n", infoBlocks.Select(b => b.InformationBlock));
        if (serializedTablesInsteadOfMarkdown)
            return serializedText;
        else
            return $"{table.Markdown}\nDescription of the table entities:\n{serializedText}";
    }

    [GeneratedRegex(@"glyph<[^>]*>")]
    private static partial Regex GlyphRegex();
    [GeneratedRegex(@"/([A-Z])\.cap")]
    private static partial Regex AtoZRegex();
    #endregion

}

public class ProcessedReport
{
    [JsonPropertyName("metainfo")]
    public Metainfo Metainfo { get; set; }
    [JsonPropertyName("content")]
    public ProcessedReportContent Content { get; set; }
}

public class ProcessedReportContent
{
    [JsonPropertyName("chunks")]
    public List<ProcessedChunk> Chunks { get; set; }
    [JsonPropertyName("pages")]
    public List<ProcessedPageData> Pages { get; set; }
}

public class ProcessedPageData
{
    [JsonPropertyName("page")]
    public int Page { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class ProcessedChunk
{
    [JsonPropertyName("page")]
    public int Page { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("table_id")]
    public int TableId { get; set; }
    [JsonPropertyName("length_tokens")]
    public int LengthTokens { get; set; }
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
}