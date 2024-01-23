
using System.Text;
using System.Text.Json;

namespace CodeFragments;
public class JsonUtf8JsonReader
{
    public static void Base()
    {
        var d = new
        {
            Date = DateTime.Now, // this TokenType is string
            TemperatureCelsius = 25,
            Summary = "Hot",
            Flag = true, // this TokenType is True
        };

        var arr = new[]{
            d,
            d,
        };

        byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(arr);
        var options = new JsonReaderOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };
        var reader = new Utf8JsonReader(jsonUtf8Bytes, options);

        while (reader.Read())
        {
            Console.Write(reader.TokenType);

            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                case JsonTokenType.String:
                    {
                        string? text = reader.GetString();
                        Console.Write(" ");
                        Console.Write(text);
                        break;
                    }

                case JsonTokenType.Number:
                    {
                        int intValue = reader.GetInt32();
                        Console.Write(" ");
                        Console.Write(intValue);
                        break;
                    }

                case JsonTokenType.True:
                    {
                        var intValue = reader.GetBoolean();
                        Console.Write(" ");
                        Console.Write(intValue);
                        break;
                    }

                case JsonTokenType.False:
                    {
                        var intValue = reader.GetBoolean();
                        Console.Write(" ");
                        Console.Write(intValue);
                        break;
                    }

                    // Other token types elided for brevity
            }
            Console.WriteLine();
        }
    }

    private static readonly byte[] s_nameUtf8 = Encoding.UTF8.GetBytes("name");
    private static ReadOnlySpan<byte> Utf8Bom => [0xEF, 0xBB, 0xBF];
    public static void Filter()
    {
        // ReadAllBytes if the file encoding is UTF-8:
        string fileName = "file/UniversitiesUtf8.json";
        ReadOnlySpan<byte> jsonReadOnlySpan = File.ReadAllBytes(fileName);

        // Read past the UTF-8 BOM bytes if a BOM exists.
        if (jsonReadOnlySpan.StartsWith(Utf8Bom))
        {
            jsonReadOnlySpan = jsonReadOnlySpan[Utf8Bom.Length..];
        }

        // Or read as UTF-16 and transcode to UTF-8 to convert to a ReadOnlySpan<byte>
        //string fileName = "Universities.json";
        //string jsonString = File.ReadAllText(fileName);
        //ReadOnlySpan<byte> jsonReadOnlySpan = Encoding.UTF8.GetBytes(jsonString);

        int count = 0;
        int total = 0;

        var reader = new Utf8JsonReader(jsonReadOnlySpan);

        while (reader.Read())
        {
            JsonTokenType tokenType = reader.TokenType;

            switch (tokenType)
            {
                case JsonTokenType.StartObject:
                    total++;
                    break;
                case JsonTokenType.PropertyName:
                    if (reader.ValueTextEquals(s_nameUtf8))
                    {
                        // Assume valid JSON, known schema
                        reader.Read();
                        if (reader.GetString()!.EndsWith("University"))
                        {
                            count++;
                        }
                    }
                    break;
            }
        }
        Console.WriteLine($"{count} out of {total} have names that end with 'University'");
    }
}