
using System.ComponentModel;
using System.Text.Json.Schema;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspnetcoreEx.KernelService.Skills;

public class DataGenerationPlugin
{
    public static readonly JsonSerializerOptions DefaultJsonOptions = JsonSerializerOptions.Default;
    
    [KernelFunction("Generate data (by type name)")]
    public string GenerateFromType(string typeName, int count)
    {
        var type = AppDomain.CurrentDomain.GetAssemblies()
        .Select(a => a.GetType(typeName, throwOnError: false))
        .FirstOrDefault(t => t != null);

        if (type == null)
        {
            return $"Type {typeName} not found";
        }

        var schema = DefaultJsonOptions.GetJsonSchemaAsNode(type).ToString();
        return BuildPrompt(schema, count);
    }

    [KernelFunction("Generate data (by JSON Schema）")]
    public string GenerateFromSchema(string jsonSchema, int count)
    {
        return BuildPrompt(jsonSchema, count);
    }

    private static string BuildPrompt(string schemaJson, int count)
    {
        return $"""
                Please generate {count} valid JSON data according to the following JSON Schema and output it in the form of a JSON array:

                JSON Schema:
                {schemaJson}
                """;
    }
}