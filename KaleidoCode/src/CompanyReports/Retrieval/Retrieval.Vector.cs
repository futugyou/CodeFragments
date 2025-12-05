
namespace CompanyReports.Retrieval;

using Path = System.IO.Path;

public class VectorRetriever : IRetrieval
{
    private readonly string _vectorDbDir;
    private readonly string _documentsDir;
    private readonly List<ReportDb> _allDbs;
    private readonly OpenAIClient _llm;
    private readonly ILogger<VectorRetriever> logger;
    public VectorRetriever([FromKeyedServices("report")] OpenAIClient client, IOptionsMonitor<CompanyReportlOptions> optionsMonitor, ILogger<VectorRetriever> logger)
    {
        var config = optionsMonitor.CurrentValue;
        _vectorDbDir = config.VectorDbDir;
        _documentsDir = config.DocumentsDir;
        _allDbs = LoadDbs();
        _llm = client;
        this.logger = logger;
    }

    private List<ReportDb> LoadDbs()
    {
        var allDbs = new List<ReportDb>();
        var allDocumentPaths = Directory.GetFiles(_documentsDir, "*.json");
        var vectorDbFiles = Directory.GetFiles(_vectorDbDir, "*.faiss")
            .ToDictionary(f => Path.GetFileNameWithoutExtension(f), f => f);

        foreach (var documentPath in allDocumentPaths)
        {
            var stem = Path.GetFileNameWithoutExtension(documentPath);
            if (!vectorDbFiles.ContainsKey(stem))
            {
                logger.LogInformation("No matching vector DB found for document {documentPath}", documentPath);
                continue;
            }
            try
            {
                var document = JsonSerializer.Deserialize<ProcessedReport>(File.ReadAllText(documentPath));
                if (document == null || document.Metainfo == null || document.Content == null)
                {
                    logger.LogInformation("Skipping {documentPath}: does not match the expected schema.", documentPath);
                    continue;
                }
                var vectorDb = IndexFlat.Read(vectorDbFiles[stem]);
                allDbs.Add(new ReportDb
                {
                    Name = stem,
                    VectorDb = vectorDb,
                    Document = document
                });
            }
            catch (Exception ex)
            {
                logger.LogError("Error loading {documentPath}: {Message}", documentPath, ex.Message);
            }
        }
        return allDbs;
    }

    public async Task<double> GetStringsCosineSimilarityAsync(string str1, string str2, CancellationToken cancellationToken = default)
    {
        var embeddings = await _llm.GetEmbeddingClient("text-embedding-3-large").GenerateEmbeddingsAsync([str1, str2], cancellationToken: cancellationToken);
        return CosineSimilarity(embeddings.Value[0].ToFloats().Span, embeddings.Value[1].ToFloats().Span);
    }

    public async Task<List<RetrievalResult>> RetrieveByCompanyNameAsync(
        string companyName,
        string query,
        int topN = 6,
        bool returnParentPages = false,
        int llmRerankingSampleSize = 28,
        int documentsBatchSize = 2,
        double llmWeight = 0.7,
        CancellationToken cancellationToken = default
    )
    {
        var targetReport = _allDbs.FirstOrDefault(r =>
        {
            var metainfo = r.Document.Metainfo;
            return metainfo != null && metainfo.CompanyName == companyName;
        }) ?? throw new Exception($"No report found with '{companyName}' company name.");

        var document = targetReport.Document ?? throw new Exception("Document can not be null.");
        var content = document.Content ?? throw new Exception("Document content is missing or malformed.");
        var pages = content.Pages ?? throw new Exception("Document pages are missing or malformed.");
        var chunks = content.Chunks ?? throw new Exception("Document chunks are missing or malformed.");

        int actualTopN = Math.Min(topN, chunks.Count);
        var embedding = await _llm.GetEmbeddingClient("text-embedding-3-large").GenerateEmbeddingsAsync([query,], cancellationToken: cancellationToken);

        var results = targetReport.VectorDb.Search(embedding.Value[0].ToFloats().ToArray(), actualTopN);

        List<RetrievalResult> retrievalResults = [];
        var seenPages = new HashSet<int>();

        foreach (var result in results)
        {
            var distance = Math.Round(result.Distance, 4);
            // TODO: long to int conversion have risk
            var chunk = chunks[(int)result.Label];
            var pageNum = Convert.ToInt32(chunk.Page);
            var parentPage = pages.First(p => p.Page == pageNum);
            if (returnParentPages)
            {
                if (seenPages.Add(pageNum))
                {
                    retrievalResults.Add(new RetrievalResult
                    {
                        Distance = distance,
                        Page = parentPage.Page,
                        Text = parentPage.Text
                    });
                }
            }
            else
            {
                retrievalResults.Add(new RetrievalResult
                {
                    Distance = distance,
                    Page = chunk.Page,
                    Text = chunk.Text
                });
            }
        }
        return retrievalResults;
    }

    public Task<List<RetrievalResult>> RetrieveAllAsync(string companyName, CancellationToken cancellationToken = default)
    {
        var targetReport = _allDbs.FirstOrDefault(r =>
        {
            var metainfo = r.Document.Metainfo;
            return metainfo != null && metainfo.CompanyName == companyName;
        }) ?? throw new Exception($"No report found with '{companyName}' company name.");
        var document = targetReport.Document ?? throw new Exception("Document can not be null.");
        var content = document.Content ?? throw new Exception("Document content is missing or malformed.");
        var pages = content.Pages ?? throw new Exception("Document pages are missing or malformed.");

        return Task.FromResult<List<RetrievalResult>>([.. pages.OrderBy(p =>  p.Page )
            .Select(p => new RetrievalResult
            {
              Distance = 0.5,
                    Page = p.Page,
                    Text = p.Text
            })]);
    }

    // internal class
    private class ReportDb
    {
        public string Name { get; set; }
        public IndexFlat VectorDb { get; set; }
        public ProcessedReport Document { get; set; }
    }

    #region private methods
    public static float CosineSimilarity(ReadOnlySpan<float> x, ReadOnlySpan<float> y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("Input vectors must have the same length.");

        // TensorPrimitives.CosineSimilarity started in NET8.0. The purpose of this is to keep two ways of writing.
#if NET9_0_OR_GREATER
        return TensorPrimitives.CosineSimilarity(x, y);
#else
                
        float dot = 0f;
        float normX = 0f;
        float normY = 0f;

        for (int i = 0; i < x.Length; i++)
        {
            float xi = x[i];
            float yi = y[i];
            dot += xi * yi;
            normX += xi * xi;
            normY += yi * yi;
        }

        return dot / MathF.Sqrt(normX * normY);
#endif
    }

    #endregion
}