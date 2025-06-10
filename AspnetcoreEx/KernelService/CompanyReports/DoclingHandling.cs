
namespace AspnetcoreEx.KernelService.CompanyReports;

public class DoclingHandling
{

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
}