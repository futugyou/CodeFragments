

using System.Text.RegularExpressions;

namespace AspnetcoreEx.KernelService.CompanyReports;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

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
public class PageTextPreparation(bool useSerializedTables = false, bool serializedTablesInsteadOfMarkdown = false)
{
    public bool UseSerializedTables { get; } = useSerializedTables;
    public bool SerializedTablesInsteadOfMarkdown { get; } = serializedTablesInsteadOfMarkdown;

    private ReportData _reportData;

    public List<ProcessedReport> ProcessReports(string reportsDir, List<string> reportsPaths, string outputDir = "")
    {
        var allReports = new List<ProcessedReport>();
        if (!string.IsNullOrEmpty(reportsDir))
        {
            reportsPaths = Directory.GetFiles(reportsDir, "*.json").ToList();
        }

        foreach (var reportPath in reportsPaths)
        {
            var reportData = JsonSerializer.Deserialize<ReportData>(File.ReadAllText(reportPath));
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
                    JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        return allReports;
    }

    public ProcessedReportContent ProcessReport(ReportData reportData)
    {
        _reportData = reportData;
        var processedPages = new List<PageData>();
        int totalCorrections = 0;
        var correctionsList = new List<string>();

        foreach (var pageContent in reportData.Content)
        {
            int pageNumber = pageContent.Page;
            string pageText = PreparePageText(pageNumber);
            (string cleanedText, int correctionsCount, List<string> corrections) = CleanText(pageText);
            totalCorrections += correctionsCount;
            correctionsList.AddRange(corrections);
            processedPages.Add(new PageData
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
            Chunks = null,
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
            finalBlocks[finalBlocks.Count - 1] = finalBlocks.Last().TrimEnd();
        }

        return string.Join("\n", finalBlocks);
    }

    public void ExportToMarkdown(string reportsDir, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        foreach (var reportPath in Directory.GetFiles(reportsDir, "*.json"))
        {
            var reportData = JsonSerializer.Deserialize<ReportData>(File.ReadAllText(reportPath));
            var processedReport = ProcessReport(reportData);

            var sb = new StringBuilder();
            foreach (var page in processedReport.Pages)
            {
                sb.AppendLine($"\n\n---\n\n# Page {page.Page}\n\n");
                sb.AppendLine(page.Text);
            }

            var reportName = reportData.Metainfo.Sha1Name;
            File.WriteAllText(Path.Combine(outputDir, $"{reportName}.md"), sb.ToString());
        }
    }

    #region Private Methods
    private PageContent GetPageData(int pageNumber)
    {
        return _reportData.Content.FirstOrDefault(p => p.Page == pageNumber);
    }

    private static List<Block> FilterBlocks(List<Block> blocks)
    {
        return [.. blocks.Where(b =>
            b != null &&
            (!string.IsNullOrWhiteSpace(b.Text) || b.Type == BlockType.Table || b.Type == BlockType.ListItem)
        )];
    }

    private static (string, int, List<string>) CleanText(string text)
    {
        // Simple implementation: remove extra blank lines and count the number of corrections
        var corrections = new List<string>();
        int count = 0;
        string cleaned = text;
        // Replace multiple blank lines with a single blank line
        string pattern = @"(\n\s*){3,}";
        if (Regex.IsMatch(cleaned, pattern))
        {
            cleaned = Regex.Replace(cleaned, pattern, "\n\n");
            count++;
            corrections.Add("Extra blank lines have been corrected");
        }
        // Other cleaning rules can be expanded as needed
        return (cleaned, count, corrections);
    }

    private static bool BlockEndsWithColon(Block block)
    {
        return block.Text != null && block.Text.Trim().EndsWith(':');
    }

    private List<string> ApplyFormattingRules(List<Block> blocks)
    {
        var finalBlocks = new List<string>();
        bool pageHeaderInFirst3 = false;
        bool sectionHeaderInFirst3 = false;
        for (int j = 0; j < Math.Min(3, blocks.Count); j++)
        {
            if (blocks[j].Type == BlockType.PageHeader) pageHeaderInFirst3 = true;
            if (blocks[j].Type == BlockType.SectionHeader) sectionHeaderInFirst3 = true;
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
            if (blockType == BlockType.PageHeader)
            {
                var prefix = i < 3 ? "\n# " : "\n## ";
                finalBlocks.Add($"{prefix}{text}\n");
                i++;
                continue;
            }
            if (blockType == BlockType.SectionHeader)
            {
                firstSectionHeaderIndex++;
                string prefix = (firstSectionHeaderIndex == 1 && i < 3 && !pageHeaderInFirst3) ? "\n# " : "\n## ";
                finalBlocks.Add($"{prefix}{text}\n");
                i++;
                continue;
            }
            if (blockType == BlockType.Paragraph)
            {
                if (BlockEndsWithColon(block) && i + 1 < n)
                {
                    var nextBlockType = blocks[i + 1].Type;
                    if (nextBlockType != BlockType.Table && nextBlockType != BlockType.ListItem)
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
            if (blockType == BlockType.Table ||
                (BlockEndsWithColon(block) && i + 1 < n && blocks[i + 1].Type == BlockType.Table))
            {
                var groupBlocks = new List<Block>();
                Block headerForTable = null;
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
                // 处理后续的 text/footnote
                if (i < n && blocks[i].Type == BlockType.Text)
                {
                    if ((i + 1 < n) && blocks[i + 1].Type == BlockType.Footnote)
                    {
                        groupBlocks.Add(blocks[i]);
                        i++;
                    }
                }
                while (i < n && blocks[i].Type == BlockType.Footnote)
                {
                    groupBlocks.Add(blocks[i]);
                    i++;
                }
                var groupText = RenderTableGroup(groupBlocks);
                finalBlocks.Add(groupText);
                continue;
            }
            // Handle list groups
            if (blockType == BlockType.ListItem ||
                (BlockEndsWithColon(block) && i + 1 < n && blocks[i + 1].Type == BlockType.ListItem))
            {
                var groupBlocks = new List<Block>();
                if (BlockEndsWithColon(block) && i + 1 < n)
                {
                    groupBlocks.Add(block);
                    i++;
                }
                while (i < n && blocks[i].Type == BlockType.ListItem)
                {
                    groupBlocks.Add(blocks[i]);
                    i++;
                }
                if (i < n && blocks[i].Type == BlockType.Text)
                {
                    if ((i + 1 < n) && blocks[i + 1].Type == BlockType.Footnote)
                    {
                        groupBlocks.Add(blocks[i]);
                        i++;
                    }
                }
                while (i < n && blocks[i].Type == BlockType.Footnote)
                {
                    groupBlocks.Add(blocks[i]);
                    i++;
                }
                var groupText = RenderListGroup(groupBlocks);
                finalBlocks.Add(groupText);
                continue;
            }
            // Handle normal blocks
            if (blockType == BlockType.Text || blockType == BlockType.Caption || blockType == BlockType.Footnote ||
                blockType == BlockType.CheckboxSelected || blockType == BlockType.CheckboxUnselected || blockType == BlockType.Formula)
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

    private string RenderTableGroup(List<Block> groupBlocks)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        foreach (var blk in groupBlocks)
        {
            switch (blk.Type)
            {
                case BlockType.Text:
                case BlockType.Caption:
                case BlockType.SectionHeader:
                case BlockType.Paragraph:
                    sb.AppendLine(blk.Text);
                    break;
                case BlockType.Table:
                    if (blk.TableId == null) continue;
                    sb.AppendLine(GetTableById(blk.TableId.Value));
                    break;
                case BlockType.Footnote:
                    sb.AppendLine(blk.Text);
                    break;
                default:
                    throw new Exception($"Unexpected block type in table group: {blk.Type}");
            }
        }
        sb.AppendLine();
        return sb.ToString();
    }

    private static string RenderListGroup(List<Block> groupBlocks)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine();
        foreach (var blk in groupBlocks)
        {
            switch (blk.Type)
            {
                case BlockType.Text:
                case BlockType.Caption:
                case BlockType.SectionHeader:
                case BlockType.Paragraph:
                    sb.AppendLine(blk.Text);
                    break;
                case BlockType.ListItem:
                    sb.AppendLine("- " + (blk.Text?.Trim() ?? string.Empty));
                    break;
                case BlockType.Footnote:
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
        var table = _reportData.Tables?.FirstOrDefault(t => t.TableId == tableId);
        if (table == null)
            throw new Exception($"Table with ID={tableId} not found in report_data!");

        if (UseSerializedTables)
            return GetSerializedTableText(table, SerializedTablesInsteadOfMarkdown);

        return table.Markdown ?? string.Empty;
    }

    private static string GetSerializedTableText(Table table, bool serializedTablesInsteadOfMarkdown)
    {
        if (table.Serialized == null)
            return table.Markdown ?? string.Empty;

        var infoBlocks = table.Serialized.InformationBlocks ?? new List<InformationBlock>();
        var serializedText = string.Join("\n", infoBlocks.Select(b => b.InformationBlockText));
        if (serializedTablesInsteadOfMarkdown)
            return serializedText;
        else
            return $"{table.Markdown}\nDescription of the table entities:\n{serializedText}";
    }
    #endregion

}

public class ReportData
{
    public Metainfo Metainfo { get; set; }
    public List<PageContent> Content { get; set; }
    public List<Table> Tables { get; set; }
}

public class Metainfo
{
    public string Sha1Name { get; set; }
}

public class PageContent
{
    public int Page { get; set; }
    public List<Block> Content { get; set; }
}

public class Block
{
    public BlockType Type { get; set; }
    public string Text { get; set; }
    public int? TableId { get; set; }
}

public enum BlockType
{
    PageHeader,
    SectionHeader,
    Paragraph,
    Table,
    ListItem,
    Text,
    Caption,
    Footnote,
    CheckboxSelected,
    CheckboxUnselected,
    Formula
}

public class Table
{
    public int TableId { get; set; }
    public string Markdown { get; set; }
    public TableSerialized Serialized { get; set; }
}

public class TableSerialized
{
    public List<InformationBlock> InformationBlocks { get; set; }
}

public class InformationBlock
{
    public string InformationBlockText { get; set; }
}

public class ProcessedReport
{
    public Metainfo Metainfo { get; set; }
    public ProcessedReportContent Content { get; set; }
}

public class ProcessedReportContent
{
    public object Chunks { get; set; }
    public List<PageData> Pages { get; set; }
}

public class PageData
{
    public int Page { get; set; }
    public string Text { get; set; }
}

