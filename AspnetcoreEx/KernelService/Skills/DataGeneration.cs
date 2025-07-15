
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using AspnetcoreEx.KernelService.Tools;

namespace AspnetcoreEx.KernelService.Skills;

public class DataGenerationPlugin
{
    public static readonly JsonSerializerOptions DefaultJsonOptions = JsonSerializerOptions.Default;
    readonly JsonSchemaExporterOptions exporterOptions = new()
    {
        TransformSchemaNode = (context, schema) =>
        {
            // Determine if a type or property and extract the relevant attribute provider.
            ICustomAttributeProvider? attributeProvider = context.PropertyInfo is not null
                ? context.PropertyInfo.AttributeProvider
                : context.TypeInfo.Type;

            // Look up any description attributes.
            DescriptionAttribute? descriptionAttr = attributeProvider?
                .GetCustomAttributes(inherit: true)
                .Select(attr => attr as DescriptionAttribute)
                .FirstOrDefault(attr => attr is not null);

            // Apply description attribute to the generated schema.
            if (descriptionAttr != null)
            {
                if (schema is not JsonObject jObj)
                {
                    // Handle the case where the schema is a Boolean.
                    JsonValueKind valueKind = schema.GetValueKind();
                    schema = jObj = [];
                    if (valueKind is JsonValueKind.False)
                    {
                        jObj.Add("not", true);
                    }
                }

                jObj.Insert(0, "description", descriptionAttr.Description);
            }

            return schema;
        }
    };

    [KernelFunction("generate_data_by_class_type")]
    public string GenerateDataByClassType(string typeName, int count)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var type = assembly.GetTypes()
           .FirstOrDefault(t => t.Name == typeName);

        // var type = AppDomain.CurrentDomain.GetAssemblies()
        // .Select(a => a.GetType(typeName, throwOnError: false))
        // .FirstOrDefault(t => t != null);

        if (type == null)
        {
            return $"Type {typeName} not found";
        }

        // Maybe https://github.com/RicoSuter/NJsonSchema is better?
        var schema = DefaultJsonOptions.GetJsonSchemaAsNode(type, exporterOptions).ToString();
        return BuildPrompt(schema, count);
    }

    [KernelFunction("generate_data_by_json_schema")]
    public string GenerateDataByJsonSchema(string jsonSchema, int count)
    {
        return BuildPrompt(jsonSchema, count);
    }

    [KernelFunction("generate_data_by_class_definition")]
    [Description("Generate JSON data based on a C# class definition string")]
    public string GenerateDataByClassDefinition(string classDefinition, int count = 5)
    {
        var type = TypeFromStringCompiler.GenerateTypeFromClassBody(classDefinition);
        if (type == null)
        {
            return "Invalid class definition provided.";
        }
        var schema = DefaultJsonOptions.GetJsonSchemaAsNode(type, exporterOptions).ToString();
        return BuildPrompt(schema, count);
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