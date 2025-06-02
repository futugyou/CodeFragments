
namespace AspnetcoreEx.KernelService.CompanyReports;

public interface IAPIProcessor
{
    string Provider { get; }
    string DefaultModel { get; }
    dynamic ResponseData { get; }
    dynamic SendMessage(
        string model = null,
        double temperature = 0.5,
        int? seed = null,
        string systemContent = "You are a helpful assistant.",
        string humanContent = "Hello!",
        bool isStructured = false,
        object responseFormat = null,
        Dictionary<string, object> kwargs = null
    );
}
