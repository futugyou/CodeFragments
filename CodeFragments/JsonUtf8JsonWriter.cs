using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeFragments;
public class JsonUtf8JsonWriter
{
    public static void Base()
    {
        var options = new JsonWriterOptions
        {
            Indented = true
        };

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, options);

        writer.WriteStartObject();
        writer.WriteString("date", DateTimeOffset.UtcNow);
        writer.WriteNumber("temp", 42);
        writer.WriteEndObject();
        writer.Flush();

        string json = Encoding.UTF8.GetString(stream.ToArray());
        // {
        //   "date": "2024-01-23T02:52:15.4188945+00:00",
        //   "temp": 42
        // }
        Console.WriteLine(json);
    }

    public static void WriteRawValue()
    {
        JsonWriterOptions writerOptions = new() { Indented = true, };

        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, writerOptions);

        writer.WriteStartObject();

        writer.WriteStartArray("defaultJsonFormatting");
        var propName = JsonEncodedText.Encode("value");

        foreach (double number in new double[] { 50.4, 51 })
        {
            writer.WriteStartObject();
            writer.WritePropertyName(propName);
            writer.WriteNumberValue(number);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        writer.WriteStartArray("customJsonFormatting");
        foreach (double result in new double[] { 50.4, 51 })
        {
            writer.WriteStartObject();
            writer.WritePropertyName(propName);
            writer.WriteRawValue(FormatNumberValue(result), skipInputValidation: true);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
        writer.Flush();

        string json = Encoding.UTF8.GetString(stream.ToArray());
        // {
        //   "defaultJsonFormatting": [
        //     {
        //       "value": 50.4
        //     },
        //     {
        //       "value": 51
        //     }
        //   ],
        //   "customJsonFormatting": [
        //     {
        //       "value": 50.4
        //     },
        //     {
        //       "value": 51.0
        //     }
        //   ]
        // }
        Console.WriteLine(json);
    }

    static string FormatNumberValue(double numberValue) => numberValue == Convert.ToInt32(numberValue) ? numberValue.ToString() + ".0" : numberValue.ToString();
}