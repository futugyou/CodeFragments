
using System.Globalization;
using System.Text.Json.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace KaleidoCode.KernelService.CompanyReports;

public interface IPDFParser
{
    Task ParseAndExportAsync(string doclingDirPath, CancellationToken cancellationToken = default);
    Task ParseAndExportParallelAsync(string doclingDirPath, int optimalWorkers = 10, int? chunkSize = null, CancellationToken cancellationToken = default);
    static Dictionary<string, CsvMetadata> ParseCsvMetadata(string csvPath)
    {
        var lookup = new Dictionary<string, CsvMetadata>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim,
        };

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, config);

        foreach (var record in csv.GetRecords<CsvMetadata>())
        {
            if (!string.IsNullOrWhiteSpace(record.Sha1))
            {
                lookup[record.Sha1] = record;
            }
        }

        return lookup;
    }
}

public class ReportGroup
{
    [JsonPropertyName("group_id")]
    public int GroupId { get; set; }
    [JsonPropertyName("group_name")]
    public string GroupName { get; set; }
    [JsonPropertyName("group_label")]
    public string GroupLabel { get; set; }
    [JsonPropertyName("ref")]
    public string Ref { get; set; }
}

public class ReportContentItem
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("text_id")]
    public int TextId { get; set; }
    [JsonPropertyName("orig")]
    public string Orig { get; set; }
    [JsonPropertyName("enumerated")]
    public bool Enumerated { get; set; }
    [JsonPropertyName("marker")]
    public string Marker { get; set; }
    [JsonPropertyName("group_id")]
    public int GroupId { get; set; }
    [JsonPropertyName("group_name")]
    public string GroupName { get; set; }
    [JsonPropertyName("group_label")]
    public string GroupLabel { get; set; }
    [JsonPropertyName("table_id")]
    public int TableId { get; set; }
    [JsonPropertyName("picture_id")]
    public int PictureId { get; set; }
}

public class ReportContent
{
    [JsonPropertyName("page")]
    public int Page { get; set; }
    [JsonPropertyName("content")]
    public List<ReportContentItem> Content { get; internal set; }
    [JsonPropertyName("page_dimensions")]
    public ReportBbox PageDimensions { get; internal set; }
}

public class ReportTable
{
    [JsonPropertyName("table_id")]
    public int TableId { get; set; }
    [JsonPropertyName("page")]
    public int Page { get; set; }
    [JsonPropertyName("bbox")]
    public ReportBbox Bbox { get; set; }
    [JsonPropertyName("rows")]
    public int Rows { get; set; }
    [JsonPropertyName("cols")]
    public int Cols { get; set; }
    [JsonPropertyName("markdown")]
    public string Markdown { get; set; }
    [JsonPropertyName("html")]
    public string Html { get; set; }
    [JsonPropertyName("json")]
    public string Json { get; set; }
    [JsonPropertyName("serialized")]
    public string Serialized { get; set; }
}

public class ReportBbox
{
    [JsonPropertyName("l")]
    public double L { get; set; }  // Left
    [JsonPropertyName("t")]
    public double T { get; set; }  // Top
    [JsonPropertyName("r")]
    public double R { get; set; }  // Right
    [JsonPropertyName("b")]
    public double B { get; set; }  // Bottom

    public static ReportBbox Union(ReportBbox a, ReportBbox b)
    {
        return new ReportBbox
        {
            L = Math.Min(a.L, b.L),
            T = Math.Max(a.T, b.T),
            R = Math.Max(a.R, b.R),
            B = Math.Min(a.B, b.B)
        };
    }

    public static implicit operator ReportBbox(DoclingBbox bbox)
    {
        return new ReportBbox
        {
            L = bbox.L,
            T = bbox.T,
            R = bbox.R,
            B = bbox.B
        };
    }
}

public class ReportPicture
{
    [JsonPropertyName("picture_id")]
    public int PictureId { get; set; }
    [JsonPropertyName("page")]
    public int Page { get; set; }
    [JsonPropertyName("bbox")]
    public ReportBbox Bbox { get; set; }
    [JsonPropertyName("children")]
    public List<ReportContentItem> Children { get; set; }
}

public class PdfReport
{
    [JsonPropertyName("metainfo")]
    public Metainfo Metainfo { get; set; }
    [JsonPropertyName("content")]
    public List<ReportContent> Content { get; set; }
    [JsonPropertyName("tables")]
    public List<ReportTable> Tables { get; set; }
    [JsonPropertyName("pictures")]
    public List<ReportPicture> Pictures { get; set; }
}

public class Metainfo
{
    [JsonPropertyName("sha1_name")]
    public string Sha1Name { get; set; }
    [JsonPropertyName("pages_amount")]
    public int PagesAmount { get; set; }
    [JsonPropertyName("text_blocks_amount")]
    public int TextBlocksAmount { get; set; }
    [JsonPropertyName("tables_amount")]
    public int TablesAmount { get; set; }
    [JsonPropertyName("pictures_amount")]
    public int PicturesAmount { get; set; }
    [JsonPropertyName("equations_amount")]
    public int EquationsAmount { get; set; }
    [JsonPropertyName("footnotes_amount")]
    public int FootnotesAmount { get; set; }
    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; }
}

public class CsvMetadata
{
    [Name("sha1")]
    public string Sha1 { get; set; }

    [Name("company_name")]
    public string CompanyName { get; set; }

    [Name("name")]
    public string Name { get; set; }

    public string GetCompanyName()
    {
        return (CompanyName ?? Name ?? string.Empty).Trim('"');
    }
}