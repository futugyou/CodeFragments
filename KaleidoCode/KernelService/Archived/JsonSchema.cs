using System.Text.Json.Serialization;

namespace KaleidoCode.KernelService.Archived;

public class JsonSchema
{
    /// <summary>
    /// The type of the schema, should be "object".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    /// <summary>
    /// Map of property names to property definitions.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, JsonSchemaProperty>? Properties { get; set; }

    /// <summary>
    /// List of required property names.
    /// </summary>
    [JsonPropertyName("required")]
    public List<string>? Required { get; set; }
}

/// <summary>
/// Represents a property in a JSON schema.
/// <see href="https://github.com/modelcontextprotocol/specification/blob/main/schema/2024-11-05/schema.json">See the schema for details</see>
/// </summary>
public class JsonSchemaProperty
{
    /// <summary>
    /// The type of the property. Should be a JSON Schema type and is required.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// A human-readable description of the property.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; } = string.Empty;
}