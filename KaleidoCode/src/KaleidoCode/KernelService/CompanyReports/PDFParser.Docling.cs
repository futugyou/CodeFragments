
using Microsoft.Extensions.Logging.Abstractions;
using Path = System.IO.Path;

namespace KaleidoCode.KernelService.CompanyReports;

/// <summary>
/// Used to process document structured data and generate JSON reports containing metainfo, content, tables, and pictures. Its main functions include:
///
/// Assemble report: Generate a structured report containing metainfo, content, tables, and pictures based on the input document data.
/// Assemble metainfo: Count the number of pages, text blocks, tables, pictures, etc. of the document, and merge external metadata.
/// Assemble content: Group the document body content by page, support expanding group references, and process content blocks such as text, tables, and pictures.
/// Assemble tables: Convert table objects to markdown, html, json, and extract table page numbers, positions, number of rows and columns, and other information.
/// Assemble pictures: Extract the page number, position, and text blocks contained in the picture.
/// </summary>
public class DoclingPDFParser : IPDFParser
{
    private readonly ILogger<DoclingPDFParser> _logger;
    private readonly string _outputDir;
    private readonly Dictionary<string, CsvMetadata> _metadataLookup;
    private readonly JsonSerializerOptions DefaultJsonSerializerOptions = new() { WriteIndented = true };

    public DoclingPDFParser(ILogger<DoclingPDFParser>? logger, string outputDir, string? csvMetadataPath = null)
    {
        _logger = logger ?? NullLogger<DoclingPDFParser>.Instance;
        if (!outputDir.EndsWith('/'))
        {
            outputDir += "/";
        }
        _outputDir = outputDir;
        _metadataLookup = csvMetadataPath != null ? IPDFParser.ParseCsvMetadata(csvMetadataPath) : [];
    }

    public async Task ParseAndExportAsync(string doclingDirPath, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;
        Directory.CreateDirectory(_outputDir);
        var inputDocPaths = Directory.GetFiles(doclingDirPath, "*.json");
        var convResults = await ReadDoclingConvertedDataAsync(inputDocPaths, cancellationToken);
        foreach (var convResult in convResults)
        {
            var pdfReport = AssembleReport(convResult);
            await WritePdfReportDataAsync(pdfReport, cancellationToken);
            _logger.LogInformation("Processed report for {Filename} with SHA1: {Sha1Name}", convResult.Origin.Filename, pdfReport.Metainfo.Sha1Name);
        }

        var elapsed = DateTime.Now - startTime;
        _logger.LogInformation("Parallel processing completed in {TotalSeconds} seconds.", elapsed.TotalSeconds);
    }

    public async Task ParseAndExportParallelAsync(string doclingDirPath, int optimalWorkers = 10, int? chunkSize = null, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;
        Directory.CreateDirectory(_outputDir);
        var inputDocPaths = Directory.GetFiles(doclingDirPath, "*.json");
        int totalDocs = inputDocPaths.Length;
        if (chunkSize == null || chunkSize <= 0)
        {
            chunkSize = Math.Max(1, totalDocs / optimalWorkers);
        }

        var chunks = inputDocPaths
            .Select((path, idx) => new { path, idx })
            .GroupBy(x => x.idx / chunkSize.Value)
            .Select(g => g.Select(x => x.path).ToList())
            .ToList();

        var successCount = 0;
        var failureCount = 0;

        // Parallel.ForEachAsync started in NET6.0. The purpose of this is to keep two ways of writing.
#if NET9_0_OR_GREATER
        await Parallel.ForEachAsync(chunks, new ParallelOptions
        {
            MaxDegreeOfParallelism = optimalWorkers,
            CancellationToken = cancellationToken
        }, async (chunk, ct) =>
        {
            try
            {
                var convResults = await ReadDoclingConvertedDataAsync(chunk, ct);
                foreach (var convResult in convResults)
                {
                    var pdfReport = AssembleReport(convResult);
                    await WritePdfReportDataAsync(pdfReport, ct);
                }

                Interlocked.Add(ref successCount, convResults.Count);
            }
            catch (Exception ex)
            {
                Interlocked.Add(ref failureCount, chunk.Count);
                _logger.LogError(ex, "Error processing chunk");
            }
        });
#else
        var tasks = chunks.Select(async chunk =>
        {
            try
            {
                var convResults = await ReadDoclingConvertedData(chunk, cancellationToken);
                foreach (var convResult in convResults)
                {
                    var pdfReport = AssembleReport(convResult);
                    await WritePdfReportDataAsync(pdfReport, cancellationToken);
                }

                Interlocked.Add(ref successCount, convResults.Count);
            }
            catch (Exception ex)
            {
                Interlocked.Add(ref failureCount, chunk.Count);
                _logger.LogError(ex, "Error processing chunk");
            }
        });

        var throttler = new SemaphoreSlim(optimalWorkers);
        var throttledTasks = tasks.Select(async task =>
        {
            await throttler.WaitAsync(cancellationToken);
            try
            {
                await task;
            }
            finally
            {
                throttler.Release();
            }
        });

        await Task.WhenAll(throttledTasks);
#endif

        var elapsed = DateTime.Now - startTime;
        if (failureCount > 0)
        {
            _logger.LogError("Failed converting {FailureCount} out of {TotalDocs} documents.", failureCount, totalDocs);
            throw new Exception($"Failed converting {failureCount} out of {totalDocs} documents.");
        }

        _logger.LogInformation("Parallel processing completed in {TotalSeconds} seconds. Successfully converted {SuccessCount}/{TotalDocs} documents.", elapsed.TotalSeconds, successCount, totalDocs);
    }

    public async Task<List<DoclingRoot>> ReadDoclingConvertedDataAsync(IEnumerable<string> inputDocPaths, CancellationToken cancellationToken = default)
    {
        var lists = new List<DoclingRoot>();
        foreach (var reportPath in inputDocPaths)
        {
            var d = await File.ReadAllTextAsync(reportPath, cancellationToken);
            if (string.IsNullOrWhiteSpace(d))
            {
                continue;
            }
            var reportData = JsonSerializer.Deserialize<DoclingRoot>(d, DefaultJsonSerializerOptions);
            if (reportData == null)
            {
                continue;
            }
            lists.Add(reportData);
        }
        return lists;
    }

    public PdfReport AssembleReport(DoclingRoot doclingData)
    {
        return new PdfReport
        {
            Metainfo = AssembleMetainfo(doclingData),
            Content = AssembleContent(doclingData),
            Tables = AssembleTables(doclingData),
            Pictures = AssemblePictures(doclingData)
        };
    }

    public async Task WritePdfReportDataAsync(PdfReport data, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data, DefaultJsonSerializerOptions);
        var filePath = $"{_outputDir}{data.Metainfo.Sha1Name}.json";
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
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
            metainfo.CompanyName = csvMeta.GetCompanyName();
        }
        return metainfo;
    }

    private static List<ReportContent> AssembleContent(DoclingRoot data)
    {
        var pages = new Dictionary<int, ReportContent>();
        var expandedBodyChildren = ExpandGroups(data.Body.Children, data.Groups);

        foreach (var refItem in expandedBodyChildren)
        {
            var (ref_type, ref_num, ok) = GetPageTypeAndNum(refItem.Ref);
            if (!ok)
            {
                continue;
            }

            if (data.Texts.Count <= ref_num)
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
            var (ref_type, groupId, ok) = GetPageTypeAndNum(item.Ref);
            if (!ok)
            {
                expandedChildren.Add(new ReportGroup { Ref = item.Ref });
                continue;
            }

            if (ref_type == "groups" && groups.Count > groupId)
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
            var (ref_type, groupId, ok) = GetPageTypeAndNum(item.Ref);
            if (!ok || ref_type != "texts")
            {
                continue;
            }

            var contentItem = ProcessTextReference(groupId, data);
            childrenList.Add(contentItem);
        }
        return childrenList;
    }

    private static (string Sha1Name, int CompanyName, bool ok) GetPageTypeAndNum(string refString)
    {
        var parts = refString.Split('/');
        if (parts.Length < 2)
        {
            return (string.Empty, -1, false);
        }
        string ref_type = parts[^2];
        string ref_num = parts[^1];
        if (!int.TryParse(ref_num, out int groupId))
        {
            return (string.Empty, -1, false);
        }
        return (ref_type, groupId, true);
    }

    #endregion
}
