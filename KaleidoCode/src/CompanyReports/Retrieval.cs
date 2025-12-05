
namespace CompanyReports;

public class RetrievalResult
{
    public double Distance { get; set; }
    public int Page { get; set; }
    public string Text { get; set; }
    public double RelevanceScore { get; set; }
    public double CombinedScore { get; set; }
    public string Reasoning { get; set; }
}


public interface IRetrieval
{
    Task<List<RetrievalResult>> RetrieveByCompanyNameAsync(
        string companyName,
        string query,
        int topN = 6,
        bool returnParentPages = false,
        int llmRerankingSampleSize = 28,
        int documentsBatchSize = 2,
        double llmWeight = 0.7,
        CancellationToken cancellationToken = default);
    Task<List<RetrievalResult>> RetrieveAllAsync(string companyName, CancellationToken cancellationToken = default);
}