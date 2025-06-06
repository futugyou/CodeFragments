
namespace AspnetcoreEx.KernelService.CompanyReports;

public class JsonReportProcessor
{
    private readonly string? _debugDataPath;
    private readonly Dictionary<string, Metadata> _metadataLookup;
    public JsonReportProcessor(string? debugDataPath, Dictionary<string, Metadata> metadataLookup)
    {
        _debugDataPath = debugDataPath;
        _metadataLookup = metadataLookup ?? [];
    }

    public Dictionary<string, object> AssembleReport(ConversionResult convResult, Dictionary<string, object> normalizedData)
    {
        var data = normalizedData ?? convResult.Document.ExportToDict();

        var assembledReport = new Dictionary<string, object>
        {
            ["metainfo"] = AssembleMetainfo(data),
            ["content"] = AssembleContent(data),
            ["tables"] = AssembleTables(convResult.Document.Tables, data),
            ["pictures"] = AssemblePictures(data)
        };

        DebugData(data);
        return assembledReport;
    }

    private object AssembleMetainfo(Dictionary<string, object> data)
    {
        // TODO: Implement actual metainfo assembling logic
        return new { Placeholder = "Metainfo" };
    }

    private object AssembleContent(Dictionary<string, object> data)
    {
        // TODO: Implement actual content assembling logic
        return new { Placeholder = "Content" };
    }

    private object AssembleTables(List<Tabula.Table> tables, Dictionary<string, object> data)
    {
        // TODO: Implement actual table assembling logic
        return new { Placeholder = "Tables", TableCount = tables?.Count ?? 0 };
    }

    private object AssemblePictures(Dictionary<string, object> data)
    {
        // TODO: Implement actual picture assembling logic
        return new { Placeholder = "Pictures" };
    }

    private void DebugData(Dictionary<string, object> data)
    {
        if (_debugDataPath != null)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_debugDataPath, json);
        }
    }
}