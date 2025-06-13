
namespace AspnetcoreEx.KernelService.CompanyReports;

using AspnetcoreEx.KernelService.Tools;
using Path = System.IO.Path;

public class BM25Retriever
{
    private readonly string bm25DbDir;
    private readonly string documentsDir;

    public BM25Retriever(string bm25DbDir, string documentsDir)
    {
        this.bm25DbDir = bm25DbDir;
        this.documentsDir = documentsDir;
    }

    public List<Dictionary<string, object>> RetrieveByCompanyName(string companyName, string query, int topN = 3, bool returnParentPages = false)
    {
        string documentPath = "";
        dynamic? document = null;

        foreach (var path in Directory.GetFiles(documentsDir, "*.json"))
        {
            var json = File.ReadAllText(path, Encoding.UTF8);
            var doc = JsonSerializer.Deserialize<dynamic>(json) ?? throw new Exception($"Failed to deserialize document at {path}.");
            if (doc.Metainfo.CompanyName == companyName)
            {
                documentPath = path;
                document = doc;
                break;
            }
        }

        if (string.IsNullOrEmpty(documentPath))
            throw new Exception($"No report found with '{companyName}' company name.");

        if (document == null)
            throw new Exception($"No report found with '{companyName}' company name.");

        string bm25Path = Path.Combine(bm25DbDir, document.Metainfo.Sha1Name + ".bm25.json");
        BM25Okapi bm25Index = JsonSerializer.Deserialize<BM25Okapi>(File.ReadAllText(bm25Path)) ?? throw new Exception($"Failed to deserialize BM25 index at {bm25Path}.");

        var chunks = document.Content.Chunks;
        var pages = document.Content.Pages;

        var tokenizedQuery = query.Split(' ');
        var scores = bm25Index.GetScores([.. tokenizedQuery]);

        int actualTopN = Math.Min(topN, scores.Length);
        var topIndices = scores
            .Select((score, idx) => new { score, idx })
            .OrderByDescending(x => x.score)
            .Take(actualTopN)
            .Select(x => x.idx)
            .ToList();

        var retrievalResults = new List<Dictionary<string, object>>();
        var seenPages = new HashSet<int>();

        foreach (var index in topIndices)
        {
            double score = Math.Round(scores[index], 4);
            var chunk = chunks[index];
            var parentPage = ((List<dynamic>)pages).First(p => (int)p.Page == (int)chunk.Page);

            if (returnParentPages)
            {
                if (!seenPages.Contains(parentPage.Page))
                {
                    seenPages.Add(parentPage.Page);
                    retrievalResults.Add(new Dictionary<string, object>
                    {
                        { "distance", score },
                        { "page", parentPage.Page },
                        { "text", parentPage.Text }
                    });
                }
            }
            else
            {
                retrievalResults.Add(new Dictionary<string, object>
                {
                    { "distance", score },
                    { "page", chunk.Page  },
                    { "text", chunk.Text }
                });
            }
        }

        return retrievalResults;
    }
}