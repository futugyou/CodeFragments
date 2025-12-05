
namespace CompanyReports;

public interface IAPIProcessor
{
    string Provider { get; }
    string DefaultModel { get; }
    ResponseData ResponseData { get; }
    Task<RephrasedQuestions> SendMessageAsync(string model = "gpt-4o-2024-08-06", float temperature = 0.5f, long? seed = null, string systemContent = "You are a helpful assistant.", string humanContent = "Hello!", bool isStructured = false, object? responseFormat = null, Dictionary<string, object>? kwargs = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// List of rephrased questions
/// </summary>
public class RephrasedQuestions
{
    [JsonPropertyName("questions")]
    [Description("List of rephrased questions for each company")]
    public List<RephrasedQuestion> Questions { get; set; } = [];
    [JsonPropertyName("references")]
    public List<ReferenceKey> References { get; set; } = [];
    [JsonPropertyName("relevant_pages")]
    public List<int> RelevantPages { get; set; } = [];
    [JsonPropertyName("error")]
    public string Error { get; set; }
    [JsonPropertyName("value")]
    public string Value { get; set; }
    [JsonPropertyName("answer")]
    public string Answer { get; set; }

    [JsonPropertyName("step_by_step_analysis")]
    public string StepByStepAnalysis { get; set; }

    [JsonPropertyName("reasoning_summary")]
    public string ReasoningSummary { get; set; }

    [JsonPropertyName("final_answer")]
    public string FinalAnswer { get; set; }
}

public class ResponseData
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("input_tokens")]
    public long InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public long OutputTokens { get; set; }
}

public class ReferenceKey
{
    [JsonPropertyName("pdf_sha1")]
    public string PdfSha1 { get; set; }
    [JsonPropertyName("page_index")]
    public int PageIndex { get; set; }

    public ReferenceKey(string pdfSha1, int pageIndex)
    {
        PdfSha1 = pdfSha1 ?? "";
        PageIndex = pageIndex;
    }

    public override string ToString()
    {
        // Use JSON-style escaping to ensure that there will be no conflicts due to characters such as colons and commas
        return $"sha1={Escape(PdfSha1)};page={PageIndex}";
    }

    private static string Escape(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace(";", "\\;")
            .Replace("=", "\\=");
    }
}
