using Google.Protobuf;
using WellKnownTypes = Google.Protobuf.WellKnownTypes;

namespace Actors.Models;

// [JsonConverter(typeof(JsonConverter))]
public class TimerResponse
{
    [JsonPropertyName("data")]
    public WellKnownTypes.Any Data { get; set; }
    [JsonPropertyName("callback")]
    public string Callback { get; set; }
    [JsonPropertyName("dueTime")]
    public string DueTime { get; set; }
    [JsonPropertyName("period")]
    public string Period { get; set; }

    public sealed class JsonConverter : JsonConverter<TimerResponse>
    {
        public override TimerResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException("Deserialization not implemented.");
        }

        public override void Write(Utf8JsonWriter writer, TimerResponse value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (!string.IsNullOrEmpty(value.DueTime))
                writer.WriteString("dueTime", value.DueTime);

            if (!string.IsNullOrEmpty(value.Period))
                writer.WriteString("period", value.Period);

            if (!string.IsNullOrEmpty(value.Callback))
                writer.WriteString("callback", value.Callback);

            if (value.Data != null)
            {
                writer.WritePropertyName("data");

                if (value.Data.TypeUrl.EndsWith("BytesValue"))
                {
                    var bytesValue = value.Data.Unpack<WellKnownTypes.BytesValue>();
                    writer.WriteBase64StringValue(bytesValue.Value.ToByteArray());
                }
                else
                {
                    var json = JsonFormatter.Default.Format(value.Data);

                    using var doc = JsonDocument.Parse(json);
                    doc.RootElement.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }
    }
}
