
namespace AspnetcoreEx.KernelService.CompanyReports;

using AspnetcoreEx.KernelService.Tools;
using Path = System.IO.Path;

public class BM25Retriever : IRetrieval
{
    private readonly string bm25DbDir;
    private readonly string documentsDir;

    public BM25Retriever(IOptionsMonitor<CompanyReportlOptions> optionsMonitor)
    {
        var config = optionsMonitor.CurrentValue;
        this.bm25DbDir = config.Bm25DbPath;
        this.documentsDir = config.DocumentsDir;
    }

    public Task<List<RetrievalResult>> RetrieveAllAsync(string companyName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<List<RetrievalResult>>([]);
    }

    public async Task<List<RetrievalResult>> RetrieveByCompanyNameAsync(
        string companyName,
        string query,
        int topN = 6,
        bool returnParentPages = false,
        int llmRerankingSampleSize = 28,
        int documentsBatchSize = 2,
        double llmWeight = 0.7,
        CancellationToken cancellationToken = default)
    {
        string documentPath = "";
        ProcessedReport? document = null;

        foreach (var path in Directory.GetFiles(documentsDir, "*.json"))
        {
            var json = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
            var doc = JsonSerializer.Deserialize<ProcessedReport>(json) ?? throw new Exception($"Failed to deserialize document at {path}.");
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

        List<RetrievalResult> retrievalResults = [];
        var seenPages = new HashSet<int>();

        foreach (var index in topIndices)
        {
            double score = Math.Round(scores[index], 4);
            var chunk = chunks[index];
            var parentPage = pages.First(p => p.Page == chunk.Page);

            if (returnParentPages)
            {
                if (!seenPages.Contains(parentPage.Page))
                {
                    seenPages.Add(parentPage.Page);
                    retrievalResults.Add(new RetrievalResult
                    {
                        Distance = score,
                        Page = parentPage.Page,
                        Text = parentPage.Text
                    });
                }
            }
            else
            {
                retrievalResults.Add(new RetrievalResult
                {
                    Distance = score,
                    Page = chunk.Page,
                    Text = chunk.Text
                });
            }
        }

        return retrievalResults;
    }
}