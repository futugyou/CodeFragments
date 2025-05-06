
using System.Collections.Generic;
using k8s;
using YamlDotNet.Serialization;

namespace Config;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Feature
{
    [JsonStringEnumMemberName("ActorStateTTL")]
    ActorStateTTL,
    [JsonStringEnumMemberName("HotReload")]
    HotReload,
    [JsonStringEnumMemberName("SchedulerReminders")]
    SchedulerReminders,
}

public class TypeMeta
{
    [JsonPropertyName("metadata")]
    [YamlMember(Alias = "metadata", ApplyNamingConventions = false)]
    public string ApiVersion { get; set; }

    [JsonPropertyName("kind")]
    [YamlMember(Alias = "kind", ApplyNamingConventions = false)]
    public string Kind { get; set; }
}

public class Configuration
{
    [JsonPropertyName("typeMeta")]
    [YamlMember(Alias = "typeMeta", ApplyNamingConventions = false)]
    public TypeMeta TypeMeta { get; set; }

    [JsonPropertyName("metadata")]
    [YamlMember(Alias = "metadata", ApplyNamingConventions = false)]
    public k8s.Models.V1ObjectMeta V1ObjectMeta { get; set; }

    [JsonPropertyName("spec")]
    [YamlMember(Alias = "spec", ApplyNamingConventions = false)]
    public ConfigurationSpec Spec { get; set; }

    [JsonIgnore]
    [YamlIgnore]
    public HashSet<Feature> FeaturesEnabled { get; set; }
}

public class ConfigurationSpec
{
    [JsonPropertyName("httpPipeline")]
    [YamlMember(Alias = "httpPipeline", ApplyNamingConventions = false)]
    public PipelineSpec HTTPPipelineSpec { get; set; }
    [JsonPropertyName("appHttpPipeline")]
    [YamlMember(Alias = "appHttpPipeline", ApplyNamingConventions = false)]
    public PipelineSpec AppHTTPPipelineSpec { get; set; }

    [JsonPropertyName("tracing")]
    [YamlMember(Alias = "tracing", ApplyNamingConventions = false)]
    public TracingSpec TracingSpec { get; set; }

    [JsonPropertyName("mtls")]
    [YamlMember(Alias = "mtls", ApplyNamingConventions = false)]
    public MTLSSpec MTLSSpec { get; set; }

    [JsonPropertyName("metric")]
    [YamlMember(Alias = "metric", ApplyNamingConventions = false)]
    public MetricSpec MetricSpec { get; set; }

    [JsonPropertyName("metrics")]
    [YamlMember(Alias = "metrics", ApplyNamingConventions = false)]
    public MetricSpec MetricsSpec { get; set; }

    [JsonPropertyName("secrets")]
    [YamlMember(Alias = "secrets", ApplyNamingConventions = false)]
    public SecretsSpec Secrets { get; set; }

    [JsonPropertyName("accessControl")]
    [YamlMember(Alias = "accessControl", ApplyNamingConventions = false)]
    public AccessControlSpec AccessControlSpec { get; set; }
}

public class AccessControlSpec
{
    [JsonPropertyName("defaultAction")]
    [YamlMember(Alias = "defaultAction", ApplyNamingConventions = false)]
    public string DefaultAction { get; set; }

    [JsonPropertyName("trustDomain")]
    [YamlMember(Alias = "trustDomain", ApplyNamingConventions = false)]
    public string TrustDomain { get; set; }

    [JsonPropertyName("policies")]
    [YamlMember(Alias = "policies", ApplyNamingConventions = false)]
    public List<AppPolicySpec> AppPolicies { get; set; }
}

public class AppPolicySpec
{
    [JsonPropertyName("appId")]
    [YamlMember(Alias = "appId", ApplyNamingConventions = false)]
    public string AppName { get; set; }

    [JsonPropertyName("defaultAction")]
    [YamlMember(Alias = "defaultAction", ApplyNamingConventions = false)]
    public string DefaultAction { get; set; }

    [JsonPropertyName("trustDomain")]
    [YamlMember(Alias = "trustDomain", ApplyNamingConventions = false)]
    public string TrustDomain { get; set; }

    [JsonPropertyName("namespace")]
    [YamlMember(Alias = "namespace", ApplyNamingConventions = false)]
    public string Namespace { get; set; }

    [JsonPropertyName("operations")]
    [YamlMember(Alias = "operations", ApplyNamingConventions = false)]
    public List<AppOperation> AppOperationActions { get; set; }
}

public class AppOperation
{
    [JsonPropertyName("name")]
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string Operation { get; set; }

    [JsonPropertyName("httpVerb")]
    [YamlMember(Alias = "httpVerb", ApplyNamingConventions = false)]
    public List<string> HTTPVerb { get; set; }

    [JsonPropertyName("action")]
    [YamlMember(Alias = "action", ApplyNamingConventions = false)]
    public string Action { get; set; }

}

public class SecretsSpec
{
    [JsonPropertyName("scopes")]
    [YamlMember(Alias = "scopes", ApplyNamingConventions = false)]
    public List<SecretsScope> Scopes { get; set; }
}

public class SecretsScope
{
    [JsonPropertyName("defaultAccess")]
    [YamlMember(Alias = "defaultAccess", ApplyNamingConventions = false)]
    public string DefaultAccess { get; set; }

    [JsonPropertyName("storeName")]
    [YamlMember(Alias = "storeName", ApplyNamingConventions = false)]
    public string StoreName { get; set; }

    [JsonPropertyName("allowedSecrets")]
    [YamlMember(Alias = "allowedSecrets", ApplyNamingConventions = false)]
    public List<string> AllowedSecrets { get; set; }

    [JsonPropertyName("deniedSecrets")]
    [YamlMember(Alias = "deniedSecrets", ApplyNamingConventions = false)]
    public List<string> DeniedSecrets { get; set; }
}

public class MetricSpec
{
    [JsonPropertyName("enabled")]
    [YamlMember(Alias = "enabled", ApplyNamingConventions = false)]
    public bool Enabled { get; set; }
    [JsonPropertyName("recordErrorCodes")]
    [YamlMember(Alias = "recordErrorCodes", ApplyNamingConventions = false)]
    public bool RecordErrorCodes { get; set; }
    [JsonPropertyName("http")]
    [YamlMember(Alias = "http", ApplyNamingConventions = false)]
    public MetricHTTP HTTP { get; set; }
    [JsonPropertyName("latencyDistributionBuckets")]
    [YamlMember(Alias = "latencyDistributionBuckets", ApplyNamingConventions = false)]
    public List<int> LatencyDistributionBuckets { get; set; }
    [JsonPropertyName("rules")]
    [YamlMember(Alias = "rules", ApplyNamingConventions = false)]
    public List<MetricsRule> Rules { get; set; }

    public bool GetEnabled()
    {
        return Enabled;
    }

    public bool GetHTTPIncreasedCardinality()
    {
        if (HTTP == null)
        {
            return true;
        }
        return HTTP.IncreasedCardinality;
    }

    public double[] GetLatencyBuckets(ILogger log)
    {
        var defaultLatencyBuckets = new double[]
        {
            1, 2, 3, 4, 5, 6, 8, 10, 13, 16, 20, 25, 30, 40, 50,
            65, 80, 100, 130, 160, 200, 250, 300, 400, 500,
            650, 800, 1_000, 2_000, 5_000, 10_000, 20_000, 50_000, 100_000
        };

        try
        {
            var json = JsonSerializer.Serialize(this);
            log.LogInformation("metric spec: {Json}", json);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error serializing metric spec");
        }

        if (LatencyDistributionBuckets == null || LatencyDistributionBuckets.Count == 0)
        {
            log.LogInformation("Using default latency buckets: {Buckets}", defaultLatencyBuckets);
            return defaultLatencyBuckets;
        }

        var customBuckets = LatencyDistributionBuckets.Select(v => (double)v).ToArray();
        log.LogInformation("Using custom latency buckets: {Buckets}", customBuckets);
        return customBuckets;
    }

    public bool GetHTTPExcludeVerbs()
    {
        if (HTTP == null)
        {
            return false;
        }
        return HTTP.ExcludeVerbs;
    }

    public bool GetHTTPPathMatching()
    {
        if (HTTP == null)
        {
            return false;
        }
        return HTTP.PathMatching;
    }

    public bool GetRecordErrorCodes()
    {
        if (HTTP == null)
        {
            return false;
        }
        return HTTP.RecordErrorCodes;
    }
}

public class MetricsRule
{
    [JsonPropertyName("name")]
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string Name { get; set; }
    [JsonPropertyName("labels")]
    [YamlMember(Alias = "labels", ApplyNamingConventions = false)]
    public List<MetricLabel> Labels { get; set; }
}

public class MetricLabel
{
    [JsonPropertyName("name")]
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string Name { get; set; }
    [JsonPropertyName("regex")]
    [YamlMember(Alias = "regex", ApplyNamingConventions = false)]
    public Dictionary<string, string> Regex { get; set; }
}

public class MetricHTTP
{
    [JsonPropertyName("enabled")]
    [YamlMember(Alias = "enabled", ApplyNamingConventions = false)]
    public bool Enabled { get; set; }
    [JsonPropertyName("increasedCardinality")]
    [YamlMember(Alias = "increasedCardinality", ApplyNamingConventions = false)]
    public bool IncreasedCardinality { get; set; }
    [JsonPropertyName("excludeVerbs")]
    [YamlMember(Alias = "excludeVerbs", ApplyNamingConventions = false)]
    public bool ExcludeVerbs { get; set; }
    [JsonPropertyName("pathMatching")]
    [YamlMember(Alias = "pathMatching", ApplyNamingConventions = false)]
    public bool PathMatching { get; set; }
    [JsonPropertyName("recordErrorCodes")]
    [YamlMember(Alias = "recordErrorCodes", ApplyNamingConventions = false)]
    public bool RecordErrorCodes { get; set; }
}

public class MTLSSpec
{
    [JsonPropertyName("increasedCardinality")]
    [YamlMember(Alias = "increasedCardinality", ApplyNamingConventions = false)]
    public bool IncreasedCardinality { get; set; }
    [JsonPropertyName("pathMatching")]
    [YamlMember(Alias = "pathMatching", ApplyNamingConventions = false)]
    public List<string> PathMatching { get; set; }
    [JsonPropertyName("excludeVerbs")]
    [YamlMember(Alias = "excludeVerbs", ApplyNamingConventions = false)]
    public bool ExcludeVerbs { get; set; }
}

public class ValidatorSpec
{
    [JsonPropertyName("name")]
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string Name { get; set; }

    [JsonPropertyName("options")]
    [YamlMember(Alias = "options", ApplyNamingConventions = false)]
    public object Options { get; set; }

    public Dictionary<string, string> OptionsMap()
    {
        if (Options == null)
        {
            return [];
        }
        if (Options is Dictionary<string, string> options)
        {
            return options;
        }

        // TODO: need JsonSerializerOptions
        var json = JsonSerializer.Serialize(Options);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }
}


public class TracingSpec
{

    [JsonPropertyName("samplingRate")]
    [YamlMember(Alias = "samplingRate", ApplyNamingConventions = false)]
    public string SamplingRate { get; set; }
    [JsonPropertyName("stdout")]
    [YamlMember(Alias = "stdout", ApplyNamingConventions = false)]
    public bool Stdout { get; set; }
    [JsonPropertyName("zipkin")]
    [YamlMember(Alias = "zipkin", ApplyNamingConventions = false)]
    public ZipkinSpec Zipkin { get; set; }
    [JsonPropertyName("otel")]
    [YamlMember(Alias = "otel", ApplyNamingConventions = false)]
    public OtelSpec Otel { get; set; }
}

public class ZipkinSpec
{
    [JsonPropertyName("endpointAddress")]
    [YamlMember(Alias = "endpointAddress", ApplyNamingConventions = false)]
    public string EndpointAddress { get; set; }
}

public class OtelSpec
{
    [JsonPropertyName("protocol")]
    [YamlMember(Alias = "protocol", ApplyNamingConventions = false)]
    public string Protocol { get; set; }

    [JsonPropertyName("endpointAddress")]
    [YamlMember(Alias = "endpointAddress", ApplyNamingConventions = false)]
    public string EndpointAddress { get; set; }

    [JsonPropertyName("isSecure")]
    [YamlMember(Alias = "isSecure", ApplyNamingConventions = false)]
    public bool IsSecure { get; set; }

    [JsonPropertyName("headers")]
    [YamlMember(Alias = "headers", ApplyNamingConventions = false)]
    public string Headers { get; set; }

    [JsonPropertyName("timeout")]
    [YamlMember(Alias = "timeout", ApplyNamingConventions = false)]
    public int Timeout { get; set; }

    public bool GetIsSecure()
    {
        return IsSecure;
    }
}

public class PipelineSpec
{
    [JsonPropertyName("handlers")]
    [YamlMember(Alias = "handlers", ApplyNamingConventions = false)]
    public List<HandlerSpec> Handlers { get; set; }
}

public class HandlerSpec
{
    [JsonPropertyName("name")]
    [YamlMember(Alias = "name", ApplyNamingConventions = false)]
    public string Name { get; set; }
    [JsonPropertyName("type")]
    [YamlMember(Alias = "type", ApplyNamingConventions = false)]
    public string Type { get; set; }
    [JsonPropertyName("version")]
    [YamlMember(Alias = "version", ApplyNamingConventions = false)]
    public string Version { get; set; }
    [JsonPropertyName("selector")]
    [YamlMember(Alias = "selector", ApplyNamingConventions = false)]
    public SelectorSpec SelectorSpec { get; set; }
}

public class SelectorSpec
{

    [JsonPropertyName("fields")]
    [YamlMember(Alias = "fields", ApplyNamingConventions = false)]
    public SelectorField[] Fields { get; set; }
}

public class SelectorField
{
    [JsonPropertyName("field")]
    [YamlMember(Alias = "field", ApplyNamingConventions = false)]
    public string Field { get; set; }
    [JsonPropertyName("value")]
    [YamlMember(Alias = "value", ApplyNamingConventions = false)]
    public string Value { get; set; }
}
