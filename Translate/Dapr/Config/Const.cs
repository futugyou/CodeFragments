

namespace Config;

public static class Const
{
    // HostAddress is the address of the instance.
    public const string HostAddress = "HOST_ADDRESS";
    //DaprGRPCPort is the dapr api grpc port.
    public const string DaprGRPCPort = "DAPR_GRPC_PORT";
    //DaprHTTPPort is the dapr api http port.
    public const string DaprHTTPPort = "DAPR_HTTP_PORT";
    //DaprMetricsPort is the dapr metrics port.
    public const string DaprMetricsPort = "DAPR_METRICS_PORT";
    //DaprProfilePort is the dapr performance profiling port.
    public const string DaprProfilePort = "DAPR_PROFILE_PORT";
    //DaprPort is the dapr internal grpc port (sidecar to sidecar).
    public const string DaprPort = "DAPR_PORT";
    //AppPort is the port of the application, http/grpc depending on mode.
    public const string AppPort = "APP_PORT";
    //AppID is the ID of the application.
    public const string AppID = "APP_ID";
    //OpenTelemetry target URL for OTLP exporter
    public const string OtlpExporterEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";
    //OpenTelemetry target URL for OTLP exporter for traces
    public const string OtlpExporterTracesEndpoint = "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT";
    //OpenTelemetry disables client transport security
    public const string OtlpExporterInsecure = "OTEL_EXPORTER_OTLP_INSECURE";
    //OpenTelemetry transport protocol (grpc, http/protobuf, http/json)
    public const string OtlpExporterProtocol = "OTEL_EXPORTER_OTLP_PROTOCOL";
    //OpenTelemetry transport protocol for traces (grpc, http/protobuf, http/json)
    public const string OtlpExporterTracesProtocol = "OTEL_EXPORTER_OTLP_TRACES_PROTOCOL";
    //OpenTelemetry headers to add to the request
    public const string OtlpExporterHeaders = "OTEL_EXPORTER_OTLP_HEADERS";
    //OpenTelemetry headers to add to the traces request
    public const string OtlpExporterTracesHeaders = "OTEL_EXPORTER_OTLP_TRACES_HEADERS";
    //OpenTelemetry timeout for the request
    public const string OtlpExporterTimeout = "OTEL_EXPORTER_OTLP_TIMEOUT";
    //OpenTelemetry timeout for the traces request
    public const string OtlpExporterTracesTimeout = "OTEL_EXPORTER_OTLP_TRACES_TIMEOUT";
    public static readonly TimeSpan OperatorCallTimeout = TimeSpan.FromSeconds(5);
    public const int OperatorMaxRetries = 100;
    public const string AllowAccess = "allow";
    public const string DenyAccess = "deny";
    public const string DefaultTrustDomain = "public";
    public const string DefaultNamespace = "default";
    public const string ActionPolicyApp = "app";
    public const string ActionPolicyGlobal = "global";
    public const int defaultMaxWorkflowConcurrentInvocations = 1000;
    public const int defaultMaxActivityConcurrentInvocations = 1000;
}