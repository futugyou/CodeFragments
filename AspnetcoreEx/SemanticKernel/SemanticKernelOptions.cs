namespace AspnetcoreEx.SemanticKernel;

public class SemanticKernelOptions
{
    public string Key { get; set; }
    public string GoogleApikey { get; set; }
    public string GoogleEngine { get; set; }
    public string GithubToken { get; set; }
    public string JiraAddress { get; set; }
    public string JiraEmailAddress { get; set; }
    public string JiraApiKey { get; set; }
    public string QdrantHost { get; set; }
    public string QdrantKey { get; set; }
    public int QdrantPort { get; set; }
    public int QdrantVectorSize { get; set; }
}
