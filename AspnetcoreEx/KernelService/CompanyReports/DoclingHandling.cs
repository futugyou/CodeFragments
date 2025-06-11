using System.Text.Json.Serialization;
using Path = System.IO.Path;

namespace AspnetcoreEx.KernelService.CompanyReports;

/// <summary>
/// Used to process document structured data and generate JSON reports containing metainfo, content, tables, and pictures. Its main functions include:
///
/// Assemble report: Generate a structured report containing metainfo, content, tables, and pictures based on the input document data.
/// Assemble metainfo: Count the number of pages, text blocks, tables, pictures, etc. of the document, and merge external metadata.
/// Assemble content: Group the document body content by page, support expanding group references, and process content blocks such as text, tables, and pictures.
/// Assemble tables: Convert table objects to markdown, html, json, and extract table page numbers, positions, number of rows and columns, and other information.
/// Assemble pictures: Extract the page number, position, and text blocks contained in the picture.
/// </summary>
public class DoclingHandling
{
    private Dictionary<string, Metadata> _metadataLookup;
    public async Task<List<DoclingRoot>> ReadDoclingConvertedData(string doclingDirPath)
    {
        var lists = new List<DoclingRoot>();
        foreach (var reportPath in Directory.GetFiles(doclingDirPath, "*.json"))
        {
            var d = await File.ReadAllTextAsync(reportPath);
            if (string.IsNullOrWhiteSpace(d))
            {
                continue;
            }
            var reportData = JsonSerializer.Deserialize<DoclingRoot>(d);
            if (reportData == null)
            {
                continue;
            }
            lists.Add(reportData);
        }
        return lists;
    }

    public PdfReport AssembleReport(DoclingRoot doclingData, Dictionary<string, Metadata> metadataLookup)
    {
        _metadataLookup = metadataLookup;
        return new PdfReport
        {
            Metainfo = AssembleMetainfo(doclingData),
            Content = AssembleContent(doclingData),
            Tables = AssembleTables(doclingData),
            Pictures = AssemblePictures(doclingData),
        };
    }


    #region private
    private Metainfo AssembleMetainfo(DoclingRoot data)
    {
        var metainfo = new Metainfo();
        var sha1Name = Path.GetFileNameWithoutExtension(data.Origin.Filename);
        metainfo.Sha1Name = sha1Name;
        metainfo.PagesAmount = data.Pages?.Count ?? 0;
        metainfo.TextBlocksAmount = data.Texts?.Count ?? 0;
        metainfo.TablesAmount = data.Tables?.Count ?? 0;
        metainfo.PicturesAmount = data.Pictures?.Count ?? 0;
        metainfo.EquationsAmount = data.Equations?.Count ?? 0;
        metainfo.FootnotesAmount = data.Texts?.Count(t => t.Label == "footnote") ?? 0;

        if (_metadataLookup?.TryGetValue(sha1Name, out var csvMeta) == true)
        {
            metainfo.CompanyName = csvMeta.CompanyName;
        }
        return metainfo;
    }

    private static List<ReportContent> AssembleContent(DoclingRoot data)
    {
        var pages = new Dictionary<int, ReportContent>();
        var expandedBodyChildren = ExpandGroups(data.Body.Children, data.Groups);

        foreach (var refItem in expandedBodyChildren)
        {
            var parts = refItem.Ref.Split('/');
            if (parts.Length < 2)
            {
                continue;
            }

            string ref_type = parts[^2];
            string ref_num_string = parts[^1];
            if (!int.TryParse(ref_num_string, out int ref_num) || data.Texts.Count <= ref_num)
            {
                continue;
            }
            switch (ref_type)
            {
                case "texts":
                    var textItem = data.Texts[ref_num];
                    var contentItem = ProcessTextReference(ref_num, data);
                    if (refItem.GroupId > 0)
                    {
                        contentItem.GroupId = refItem.GroupId;
                        contentItem.GroupName = refItem.GroupName;
                        contentItem.GroupLabel = refItem.GroupLabel;
                    }
                    {
                        contentItem.GroupId = refItem.GroupId;
                        contentItem.GroupName = refItem.GroupName;
                        contentItem.GroupLabel = refItem.GroupLabel;
                    }
                    if (textItem.Prov != null && textItem.Prov.Count > 0)
                    {
                        var pageNum = textItem.Prov[0].PageNo;
                        if (!pages.TryGetValue(pageNum, out ReportContent? value))
                        {
                            value = new ReportContent
                            {
                                Page = pageNum,
                                Content = [],
                                PageDimensions = textItem.Prov[0].Bbox
                            };
                            pages[pageNum] = value;
                        }

                        value.Content.Add(contentItem);
                    }
                    break;
                case "tables":
                    var tableItem = data.Tables[ref_num];
                    if (tableItem.Prov != null && tableItem.Prov.Count > 0)
                    {
                        var pageNum = tableItem.Prov[0].PageNo;
                        if (!pages.TryGetValue(pageNum, out ReportContent? value))
                        {
                            value = new ReportContent
                            {
                                Page = pageNum,
                                Content = [],
                                PageDimensions = tableItem.Prov[0].Bbox
                            };
                            pages[pageNum] = value;
                        }

                        value.Content.Add(new ReportContentItem
                        {
                            Type = "table",
                            TableId = ref_num
                        });
                    }
                    break;
                case "pictures":
                    var pictureItem = data.Pictures[ref_num];
                    if (pictureItem.Prov != null && pictureItem.Prov.Count > 0)
                    {
                        var pageNum = pictureItem.Prov[0].PageNo;
                        if (!pages.TryGetValue(pageNum, out ReportContent? value))
                        {
                            value = new ReportContent
                            {
                                Page = pageNum,
                                Content = [],
                                PageDimensions = pictureItem.Prov[0].Bbox
                            };
                            pages[pageNum] = value;
                        }

                        value.Content.Add(new ReportContentItem
                        {
                            Type = "picture",
                            PictureId = ref_num
                        });
                    }
                    break;
            }
        }

        return pages.OrderBy(p => p.Key).Select(p => p.Value).ToList();
    }

    private static List<ReportGroup> ExpandGroups(List<DoclingChild> bodyChildren, List<DoclingGroup> groups)
    {
        var expandedChildren = new List<ReportGroup>();
        foreach (var item in bodyChildren)
        {
            var parts = item.Ref.Split('/');
            if (parts.Length < 2)
            {
                expandedChildren.Add(new ReportGroup { Ref = item.Ref });
                continue;
            }
            string ref_type = parts[^2];
            string ref_num = parts[^1];
            if (ref_type == "groups" && int.TryParse(ref_num, out int groupId) && groups.Count > groupId)
            {
                var group = groups[groupId];
                foreach (var child in group.Children)
                {
                    expandedChildren.Add(new ReportGroup
                    {
                        Ref = child.Ref,
                        GroupId = groupId,
                        GroupName = group.Name,
                        GroupLabel = group.Label
                    });
                }
            }
            else
            {
                expandedChildren.Add(new ReportGroup { Ref = item.Ref });
            }
        }
        return expandedChildren;
    }

    private static ReportContentItem ProcessTextReference(int refNum, DoclingRoot data)
    {
        var textItem = data.Texts[refNum];
        var contentItem = new ReportContentItem
        {
            Text = textItem.Text,
            Type = textItem.Label,
            TextId = refNum,
            Enumerated = textItem.Enumerated,
            Marker = textItem.Marker,
        };
        if (textItem.Orig != null && textItem.Orig != textItem.Text)
            contentItem.Orig = textItem.Orig;
        return contentItem;
    }

    private static List<ReportTable> AssembleTables(DoclingRoot doclingData)
    {
        var assembledTables = new List<ReportTable>();
        foreach (var table in doclingData.Tables)
        {
            var tableData = doclingData.Tables.FirstOrDefault(t => t.SelfRef == table.SelfRef);
            if (tableData == null)
                continue;

            var tablePageNum = tableData.Prov[0].PageNo;
            var tableBbox = tableData.Prov[0].Bbox;
            var nrows = tableData.Data.NumRows;
            var ncols = tableData.Data.NumCols;
            var refNum = int.Parse(tableData.SelfRef.Split('/').Last());

            var tableObj = new ReportTable
            {
                TableId = refNum,
                Page = tablePageNum,
                Bbox = new ReportBbox { L = tableBbox.L, T = tableBbox.T, R = tableBbox.R, B = tableBbox.B },
                Rows = nrows,
                Cols = ncols,
                Markdown = MarkdownBuilder.ToTable(tableData.Data.TableGrid),
                Html = HtmlTagsExporter.ExportGridToHtml(tableData.Data.TableGrid),
                Json = JsonSerializer.Serialize(tableData),
            };
            assembledTables.Add(tableObj);
        }

        return assembledTables;
    }

    private static List<ReportPicture> AssemblePictures(DoclingRoot data)
    {
        var assembledPictures = new List<ReportPicture>();
        foreach (var picture in data.Pictures)
        {
            var childrenList = ProcessPictureBlock(picture, data);
            var refNum = int.Parse(picture.SelfRef.Split('/').Last());
            var picturePageNum = picture.Prov[0].PageNo;
            var pictureBbox = picture.Prov[0].Bbox;
            var pictureObj = new ReportPicture
            {
                PictureId = refNum,
                Page = picturePageNum,
                Bbox = new ReportBbox { L = pictureBbox.L, T = pictureBbox.T, R = pictureBbox.R, B = pictureBbox.B },
                Children = childrenList
            };
            assembledPictures.Add(pictureObj);
        }

        return assembledPictures;
    }

    private static List<ReportContentItem> ProcessPictureBlock(DoclingPicture picture, DoclingRoot data)
    {
        var childrenList = new List<ReportContentItem>();
        foreach (var item in picture.Children)
        {
            var parts = item.Ref.Split('/');
            if (parts.Length < 2)
            {
                continue;
            }
            string ref_type = parts[^2];
            string ref_num = parts[^1];
            if (ref_type != "texts" || !int.TryParse(ref_num, out int groupId))
            {
                continue;
            }

            var contentItem = ProcessTextReference(groupId, data);
            childrenList.Add(contentItem);
        }
        return childrenList;
    }

    #endregion
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
    public DoclingBbox PageDimensions { get; internal set; }
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