
namespace AspnetcoreEx.KernelService.CompanyReports;


public class HybridRetriever
{
    private readonly VectorRetriever _vectorRetriever;
    private readonly LLMReranker _reranker;

    public HybridRetriever(string vectorDbDir, string documentsDir)
    {
        _vectorRetriever = new VectorRetriever(vectorDbDir, documentsDir);
        _reranker = new LLMReranker();
    }

    /// <summary>
    /// Retrieve and rerank documents using hybrid approach.
    /// </summary>
    /// <param name="companyName">Name of the company to search documents for</param>
    /// <param name="query">Search query</param>
    /// <param name="llmRerankingSampleSize">Number of initial results to retrieve from vector DB</param>
    /// <param name="documentsBatchSize">Number of documents to analyze in one LLM prompt</param>
    /// <param name="topN">Number of final results to return after reranking</param>
    /// <param name="llmWeight">Weight given to LLM scores (0-1)</param>
    /// <param name="returnParentPages">Whether to return full pages instead of chunks</param>
    /// <returns>List of reranked document dictionaries with scores</returns>
    public async Task<List<Dictionary<string, object>>> RetrieveByCompanyName(
        string companyName,
        string query,
        int llmRerankingSampleSize = 28,
        int documentsBatchSize = 2,
        int topN = 6,
        double llmWeight = 0.7,
        bool returnParentPages = false)
    {
        // Get initial results from vector retriever
        var vectorResults = await _vectorRetriever.RetrieveByCompanyName(
            companyName: companyName,
            query: query,
            topN: llmRerankingSampleSize,
            returnParentPages: returnParentPages
        );

        // Rerank results using LLM
        var rerankedResults = await _reranker.RerankDocumentsAsync(
            query: query,
            documents: vectorResults,
            documentsBatchSize: documentsBatchSize,
            llmWeight: llmWeight
        );

        return rerankedResults.GetRange(0, Math.Min(topN, rerankedResults.Count));
    }
}
