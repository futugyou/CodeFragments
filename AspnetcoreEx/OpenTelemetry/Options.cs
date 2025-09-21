
namespace AspnetcoreEx.OpenTelemetry;

public class OpenTelemetryOptions
{
    public string ServiceName { get; set; } = "default-service";
    public ExportersSettings Exporters { get; set; } = new();
}

public class ExportersSettings
{
    public OtlpExporterSettings Otlp { get; set; } = new();
    public JaegerExporterSettings Jaeger { get; set; } = new();
    public ConsoleExporterSettings Console { get; set; } = new();
    public bool AllowAspNetCoreInstrumentation { get; set; } = false;
    public bool AllowRuntimeInstrumentation { get; set; } = false;
    public bool AllowHttpClientInstrumentation { get; set; } = false;
}

public class OtlpExporterSettings
{
    public bool Enabled { get; set; } = false;
    public string? Endpoint { get; set; }
    public string? Protocol { get; set; }
    public string? Headers { get; set; }
}

public class JaegerExporterSettings
{
    public bool Enabled { get; set; } = false;
    public string AgentHost { get; set; } = "localhost";
    public int AgentPort { get; set; } = 6831;
}

public class ConsoleExporterSettings
{
    public bool Enabled { get; set; } = false;
}
