
namespace AspnetcoreEx.KernelService.CompanyReports;

public interface IAPIProcessor
{
    string Provider { get; }
    string DefaultModel { get; }
    ResponseData ResponseData { get; }
    Task<RephrasedQuestions> SendMessageAsync(string model = "gpt-4o-2024-08-06", float temperature = 0.5f, long? seed = null, string systemContent = "You are a helpful assistant.", string humanContent = "Hello!", bool isStructured = false, object responseFormat = null, Dictionary<string, object> kwargs = null, CancellationToken cancellationToken = default);
}

public class ResponseData
{
    public string Model { get; set; }
    public long InputTokens { get; set; }
    public long OutputTokens { get; set; }
}