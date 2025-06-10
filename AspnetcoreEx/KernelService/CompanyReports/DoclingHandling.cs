

using Path = System.IO.Path;

namespace AspnetcoreEx.KernelService.CompanyReports;

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

    public Dictionary<string, object> AssembleReport(DoclingRoot doclingData, Dictionary<string, Metadata> metadataLookup)
    {
        _metadataLookup = metadataLookup;
        var assembledReport = new Dictionary<string, object>
        {
            ["metainfo"] = AssembleMetainfo(doclingData),
            ["content"] = AssembleContent(doclingData),
        };

        return assembledReport;
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

    #endregion
}

public class ReportGroup
{
    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public string GroupLabel { get; set; }
    public string Ref { get; set; }
}

public class ReportContentItem
{
    public string Text { get; set; }
    public string Type { get; set; }
    public int TextId { get; set; }
    public string Orig { get; set; }
    public bool Enumerated { get; set; }
    public string Marker { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public string GroupLabel { get; set; }
    public int TableId { get; set; }
    public int PictureId { get; set; }
}

public class ReportContent
{
    public int Page { get; set; }
    public List<ReportContentItem> Content { get; internal set; }
    public DoclingBbox PageDimensions { get; internal set; }
}
