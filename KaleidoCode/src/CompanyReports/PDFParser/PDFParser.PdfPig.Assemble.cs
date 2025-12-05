
using Path = System.IO.Path;
using CompanyReports.HtmlExporter;

namespace CompanyReports.PDFParser;

public class JsonReportProcessor
{
    private readonly string? _debugDataPath;
    private readonly Dictionary<string, CsvMetadata> _metadataLookup;
    public JsonReportProcessor(string? debugDataPath, Dictionary<string, CsvMetadata> metadataLookup)
    {
        _debugDataPath = debugDataPath;
        _metadataLookup = metadataLookup ?? [];
    }

    public PdfReport AssembleReport(ConversionResult data)
    {
        var pdfReport = new PdfReport
        {
            Metainfo = AssembleMetainfo(data),
            Content = AssembleContent(data),
            Tables = AssembleTables(data),
            Pictures = AssemblePictures(data)
        };

        DebugData(pdfReport);
        return pdfReport;
    }

    private Metainfo AssembleMetainfo(ConversionResult data)
    {
        var metainfo = new Metainfo();
        var sha1Name = Path.GetFileNameWithoutExtension(data.Input.File.Name);
        metainfo.Sha1Name = sha1Name;
        metainfo.PagesAmount = data.Document.Pages?.Count ?? 0;
        metainfo.TextBlocksAmount = data.Document.TextBlocks?.Count ?? 0;
        metainfo.TablesAmount = data.Document.Tables?.Count ?? 0;
        metainfo.PicturesAmount = data.Document.Pictures?.Count ?? 0;
        metainfo.EquationsAmount = 0;
        metainfo.FootnotesAmount = data.Document.HeaderFooters?.Count ?? 0;

        if (_metadataLookup?.TryGetValue(sha1Name, out var csvMeta) == true)
        {
            metainfo.CompanyName = csvMeta.GetCompanyName();
        }
        return metainfo;
    }

    private static List<ReportContent> AssembleContent(ConversionResult data)
    {
        List<ReportContent> result = [];
        foreach (var (page, word) in data.Document.Words)
        {
            var con = new ReportContent
            {
                Page = page.Number,
                Content = [new ReportContentItem
                {
                    Text = word.Text,
                    Type =   "text"
                }],
                PageDimensions = new()
                {
                    B = word.BoundingBox.Bottom,
                    L = word.BoundingBox.Left,
                    R = word.BoundingBox.Right,
                    T = word.BoundingBox.Top
                }
            };
            result.Add(con);
        }

        var tableid = 1;
        foreach (var (page, table) in data.Document.Tables)
        {
            var con = new ReportContent
            {
                Page = page.Number,
                Content = [new ReportContentItem
                {
                    TableId = tableid,
                    Type =   "table"
                }],
                PageDimensions = new()
                {
                    B = table.BoundingBox.Bottom,
                    L = table.BoundingBox.Left,
                    R = table.BoundingBox.Right,
                    T = table.BoundingBox.Top
                }
            };
            result.Add(con);
            tableid++;
        }

        var pictureId = 1;
        foreach (var (page, picture) in data.Document.Pictures)
        {
            var con = new ReportContent
            {
                Page = page.Number,
                Content = [new ReportContentItem
                {
                    PictureId = pictureId,
                    Type =   "picture"
                }],
                PageDimensions = new()
                {
                    B = picture.Bounds.Bottom,
                    L = picture.Bounds.Left,
                    R = picture.Bounds.Right,
                    T = picture.Bounds.Top
                }
            };
            result.Add(con);
            pictureId++;
        }
        return result;
    }

    private static List<ReportTable> AssembleTables(ConversionResult data)
    {
        List<ReportTable> result = [];
        var tableid = 1;
        foreach (var (page, table) in data.Document.Tables)
        {
            var con = new ReportTable
            {
                Page = page.Number,
                TableId = tableid,
                Bbox = new()
                {
                    B = table.BoundingBox.Bottom,
                    L = table.BoundingBox.Left,
                    R = table.BoundingBox.Right,
                    T = table.BoundingBox.Top
                },
                Rows = table.Rows.Count,
                Cols = table.Cells.Count,
                Markdown = MarkdownBuilder.ToTable(table.TableGrid()),
                Html = HtmlTagsExporter.ExportGridToHtml(table.TableGrid()),
                Json = JsonSerializer.Serialize(table),
            };
            result.Add(con);
            tableid++;
        }

        return result;
    }
    private static List<ReportPicture> AssemblePictures(ConversionResult data)
    {
        var assembledPictures = new List<ReportPicture>();
        var refNum = 1;
        foreach (var (page, picture) in data.Document.Pictures)
        {
            var pictureObj = new ReportPicture
            {
                PictureId = refNum,
                Page = page.Number,
                Bbox = new ReportBbox
                {
                    B = picture.Bounds.Bottom,
                    L = picture.Bounds.Left,
                    R = picture.Bounds.Right,
                    T = picture.Bounds.Top
                },
            };
            assembledPictures.Add(pictureObj);
            refNum++;
        }

        return assembledPictures;
    }

    private void DebugData(PdfReport data)
    {
        if (_debugDataPath != null)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_debugDataPath, json);
        }
    }

}

public static class TabulaTableExtensions
{
    public static List<List<string>> TableGrid(this Tabula.Table table) => [.. table.Rows.Select(row => row.Select(c => c.GetText()).ToList())];
}