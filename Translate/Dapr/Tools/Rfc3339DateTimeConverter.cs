

using System.Globalization;

namespace Tools;

public class Rfc3339DateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (DateTimeOffset.TryParse(reader.GetString(), null, DateTimeStyles.RoundtripKind, out var dto))
        {
            return dto.UtcDateTime;
        }
        throw new JsonException("Invalid RFC3339 datetime format");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utcValue = value.ToUniversalTime();
        var text = utcValue.ToString(Format, CultureInfo.InvariantCulture);
        writer.WriteStringValue(text);
    }
}