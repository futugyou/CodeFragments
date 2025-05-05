using Google.Protobuf;
using WellKnownTypes = Google.Protobuf.WellKnownTypes;

namespace Tools;

public class TypeAnyConverter : JsonConverter<WellKnownTypes.Any>
{
    public override WellKnownTypes.Any? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var rawJson = doc.RootElement.GetRawText();

        return WellKnownTypes.Any.Pack(new WellKnownTypes.BytesValue
        {
            Value = ByteString.CopyFromUtf8(rawJson)
        });
    }

    public override void Write(Utf8JsonWriter writer, WellKnownTypes.Any value, JsonSerializerOptions options)
    {

        if (value.TypeUrl.EndsWith("BytesValue"))
        {
            var bytesValue = value.Unpack<WellKnownTypes.BytesValue>();
            writer.WriteBase64StringValue(bytesValue.Value.ToByteArray());
        }
        else
        {
            var type = ResolveTypeFromTypeUrl(value.TypeUrl);
            if (type != null)
            {
                var unpacked = UnpackDynamic(value, type);
                JsonSerializer.Serialize(writer, unpacked, type, options);
            }
            else
            {
                // fallback：直接使用 JsonFormatter
                var json = JsonFormatter.Default.Format(value);
                using var doc = JsonDocument.Parse(json);
                doc.RootElement.WriteTo(writer);
            }
        }
    }

    private static IMessage? UnpackDynamic(WellKnownTypes.Any any, Type targetType)
    {
        var method = typeof(WellKnownTypes.Any).GetMethod(nameof(WellKnownTypes.Any.Unpack))!;
        var generic = method.MakeGenericMethod(targetType);
        return generic.Invoke(any, null) as IMessage;
    }

    private static Type? ResolveTypeFromTypeUrl(string typeUrl)
    {
        return typeUrl switch
        {
            _ => null
        };
    }
}