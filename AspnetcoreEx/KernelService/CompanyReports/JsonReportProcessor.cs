
namespace AspnetcoreEx.KernelService.CompanyReports;

public class JsonReportProcessor
{
    private readonly string? _debugDataPath;
    private readonly Dictionary<string, Metadata> _metadataLookup;
    public JsonReportProcessor(string? debugDataPath, Dictionary<string, Metadata> metadataLookup)
    {
        _debugDataPath = debugDataPath;
        _metadataLookup = metadataLookup;
    }

    public Dictionary<string, object> AssembleReport(ConversionResult convRes, Dictionary<string, object> normalizedData)
    {
        // TODO: JsonReportProcessor
        return normalizedData;
    }
}