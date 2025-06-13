
namespace AspnetcoreEx.KernelService.CompanyReports;

using OpenAI;
using FaissMask;
using System.Numerics.Tensors;
using Path = System.IO.Path;

public class VectorRetriever
{
    private readonly string _vectorDbDir;
    private readonly string _documentsDir;
    private readonly List<ReportDb> _allDbs;
    private readonly OpenAIClient _llm;

    public VectorRetriever(string vectorDbDir, string documentsDir)
    {
        _vectorDbDir = vectorDbDir;
        _documentsDir = documentsDir;
        _allDbs = LoadDbs();
        _llm = SetUpLlm();
    }

    private static OpenAIClient SetUpLlm()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        return new OpenAIClient(apiKey);
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
                Console.WriteLine($"No matching vector DB found for document {documentPath}");
                continue;
            }
            try
            {
                var document = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(documentPath));
                if (document == null || !document.ContainsKey("metainfo") || !document.ContainsKey("content"))
                {
                    Console.WriteLine($"Skipping {documentPath}: does not match the expected schema.");
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
                Console.WriteLine($"Error loading {documentPath}: {ex.Message}");
            }
        }
        return allDbs;
    }

    public static async Task<double> GetStringsCosineSimilarity(string str1, string str2, CancellationToken cancellationToken = default)
    {
        var llm = SetUpLlm();
        var embeddings = await llm.GetEmbeddingClient("text-embedding-3-large").GenerateEmbeddingsAsync([str1, str2], cancellationToken: cancellationToken);
        return CosineSimilarity(embeddings.Value[0].ToFloats().Span, embeddings.Value[1].ToFloats().Span);
    }

    public async Task<List<Dictionary<string, object>>> RetrieveByCompanyName(
        string companyName, string query, int topN = 3, bool returnParentPages = false, CancellationToken cancellationToken = default)
    {
        var targetReport = _allDbs.FirstOrDefault(r =>
        {
            var metainfo = ((JsonElement)r.Document["metainfo"]).Deserialize<Dictionary<string, object>>();
            return metainfo != null && metainfo.ContainsKey("company_name") && (string)metainfo["company_name"] == companyName;
        }) ?? throw new Exception($"No report found with '{companyName}' company name.");

        var document = targetReport.Document;
        var content = ((JsonElement)document["content"]).Deserialize<Dictionary<string, object>>() ?? throw new Exception("Document content is missing or malformed.");
        var chunks = ((JsonElement)content["chunks"]).Deserialize<List<Dictionary<string, object>>>() ?? throw new Exception("Document chunks are missing or malformed.");
        var pages = ((JsonElement)content["pages"]).Deserialize<List<Dictionary<string, object>>>() ?? throw new Exception("Document pages are missing or malformed.");

        int actualTopN = Math.Min(topN, chunks.Count);
        var embedding = await _llm.GetEmbeddingClient("text-embedding-3-large").GenerateEmbeddingsAsync([query,], cancellationToken: cancellationToken);

        var results = targetReport.VectorDb.Search(embedding.Value[0].ToFloats().ToArray(), actualTopN);

        var retrievalResults = new List<Dictionary<string, object>>();
        var seenPages = new HashSet<int>();

        foreach (var result in results)
        {
            var distance = Math.Round(result.Distance, 4);
            // TODO: long to int conversion have risk
            var chunk = chunks[(int)result.Label];
            var pageNum = Convert.ToInt32(chunk["page"]);
            var parentPage = pages.First(p => Convert.ToInt32(p["page"]) == pageNum);
            if (returnParentPages)
            {
                if (seenPages.Add(pageNum))
                {
                    retrievalResults.Add(new Dictionary<string, object>
                    {
                        { "distance", distance },
                        { "page", parentPage["page"] },
                        { "text", parentPage["text"] }
                    });
                }
            }
            else
            {
                retrievalResults.Add(new Dictionary<string, object>
                {
                    { "distance", distance },
                    { "page", chunk["page"] },
                    { "text", chunk["text"] }
                });
            }
        }
        return retrievalResults;
    }

    public List<Dictionary<string, object>> RetrieveAll(string companyName)
    {
        var targetReport = _allDbs.FirstOrDefault(r =>
        {
            var metainfo = ((JsonElement)r.Document["metainfo"]).Deserialize<Dictionary<string, object>>();
            return metainfo != null && metainfo.ContainsKey("company_name") && (string)metainfo["company_name"] == companyName;
        }) ?? throw new Exception($"No report found with '{companyName}' company name.");
        var document = targetReport.Document;
        var content = ((JsonElement)document["content"]).Deserialize<Dictionary<string, object>>() ?? throw new Exception("Document content is missing or malformed.");
        var pages = ((JsonElement)content["pages"]).Deserialize<List<Dictionary<string, object>>>() ?? throw new Exception("Document pages are missing or malformed.");

        return [.. pages.OrderBy(p => Convert.ToInt32(p["page"]))
            .Select(p => new Dictionary<string, object>
            {
                { "distance", 0.5 },
                { "page", p["page"] },
                { "text", p["text"] }
            })];
    }

    // internal class
    private class ReportDb
    {
        public string Name { get; set; }
        public IndexFlat VectorDb { get; set; }
        public Dictionary<string, object> Document { get; set; }
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