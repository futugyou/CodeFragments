
namespace AspnetcoreEx.KernelService.CompanyReports;

public class QuestionsProcessor
{
    private string vectorDbDir;
    private string documentsDir;
    private string questionsFilePath;
    private bool v;
    private string subsetPath;
    private bool parentDocumentRetrieval;
    private bool llmReranking;
    private int llmRerankingSampleSize;
    private int topNRetrieval;
    private bool parallelRequests;
    private string apiProvider;
    private string answeringModel;
    private bool fullContext;

    public QuestionsProcessor(string vectorDbDir, string documentsDir, string questionsFilePath, bool v, string subsetPath, bool parentDocumentRetrieval, bool llmReranking, int llmRerankingSampleSize, int topNRetrieval, bool parallelRequests, string apiProvider, string answeringModel, bool fullContext)
    {
        this.vectorDbDir = vectorDbDir;
        this.documentsDir = documentsDir;
        this.questionsFilePath = questionsFilePath;
        this.v = v;
        this.subsetPath = subsetPath;
        this.parentDocumentRetrieval = parentDocumentRetrieval;
        this.llmReranking = llmReranking;
        this.llmRerankingSampleSize = llmRerankingSampleSize;
        this.topNRetrieval = topNRetrieval;
        this.parallelRequests = parallelRequests;
        this.apiProvider = apiProvider;
        this.answeringModel = answeringModel;
        this.fullContext = fullContext;
    }

    public void ProcessAllQuestions(string outputPath, string submissionFile, string teamEmail, string submissionName, string pipelineDetails)
    {
        throw new NotImplementedException();
    }

    #region private methos
    private static List<Dictionary<string, string>> LoadQuestions(string? questionsFilePath)
    {
        if (string.IsNullOrEmpty(questionsFilePath))
            return [];

        var json = File.ReadAllText(questionsFilePath);
        return JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json) ?? [];
    }

    private static string FormatRetrievalResults(List<RetrievalResult> retrievalResults)
    {
        if (retrievalResults == null)
            return string.Empty;

        var contextParts = new List<string>();
        foreach (var result in retrievalResults)
        {
            var pageNumber = result.Page;
            var text = result.Text;
            contextParts.Add($"Text retrieved from page {pageNumber}: \n\"\"\"\n{text}\n\"\"\"");
        }

        return string.Join("\n\n---\n\n", contextParts);
    }
    #endregion
}
