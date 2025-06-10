

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
    #endregion
}