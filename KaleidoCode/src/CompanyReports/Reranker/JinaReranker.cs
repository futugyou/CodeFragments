
namespace CompanyReports.Reranker;

public class JinaReranker
{
    private readonly string _url = "https://api.jina.ai/v1/rerank";
    private readonly string _apiKey;

    public JinaReranker()
    {
        _apiKey = Environment.GetEnvironmentVariable("JINA_API_KEY") ?? "";
    }

    public async Task<JinaRerankResult?> RerankAsync(string query, List<string> documents, int topN = 10)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var data = new
        {
            model = "jina-reranker-v2-base-multilingual",
            query,
            top_n = topN,
            documents
        };

        var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(_url, content);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JinaRerankResult>(responseString);
    }
}

public class JinaRerankResult
{
    [JsonPropertyName("model")]
    public string Model { get; set; }
    [JsonPropertyName("usage")]
    public JinaUsage Usage { get; set; }
    [JsonPropertyName("results")]
    public List<JinaResult> Results { get; set; }
}

public class JinaUsage
{
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

}

public class JinaResult
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
    [JsonPropertyName("relevance_score")]
    public double RelevanceScore { get; set; }

}