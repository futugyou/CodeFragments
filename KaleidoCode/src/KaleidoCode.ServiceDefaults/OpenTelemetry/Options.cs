
namespace Microsoft.Extensions.DependencyInjection;

public class OpenTelemetryOptions
{
    public string ServiceName { get; set; } = "default-service";
    public ExportersSettings Exporters { get; set; } = new();
}

public class ExportersSettings
{
    public OtlpExporterSettings Otlp { get; set; } = new();
    public ConsoleExporterSettings Console { get; set; } = new();
}

public class OtlpExporterSettings
{
    public bool Enabled { get; set; } = false;
    public string? Endpoint { get; set; }
    public string? Protocol { get; set; }
    public string? Headers { get; set; }
}

public class ConsoleExporterSettings
{
    public bool Enabled { get; set; } = false;
}
