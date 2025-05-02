using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using WellKnownTypes = Google.Protobuf.WellKnownTypes;

namespace Actors;

public class Const
{
   public const string DaprSeparator = "||";
}


public class ActorHostedRequest
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }

    public string ActorKey() => $"{ActorType}{Const.DaprSeparator}{ActorID}";
}

public class DeleteStateRequest
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("key")]
    public string Key { get; set; }

    public string ActorKey() => $"{ActorType}{Const.DaprSeparator}{ActorID}";
}

public class GetStateRequest
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("key")]
    public string Key { get; set; }

    public string ActorKey() => $"{ActorType}{Const.DaprSeparator}{ActorID}";
}

public class GetBulkStateRequest
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("keys")]
    public string[] Keys { get; set; }

    public string ActorKey() => $"{ActorType}{Const.DaprSeparator}{ActorID}";
}

[JsonConverter(typeof(ReminderResponse.JsonConverter))]
public class ReminderResponse
{
    [JsonPropertyName("data")]
    public WellKnownTypes.Any Data { get; set; }
    [JsonPropertyName("dueTime")]
    public string DueTime { get; set; }
    [JsonPropertyName("period")]
    public string Period { get; set; }

    public sealed class JsonConverter : JsonConverter<ReminderResponse>
    {
        public override ReminderResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        { 
            throw new NotImplementedException("Deserialization not implemented.");
        }

        public override void Write(Utf8JsonWriter writer, ReminderResponse value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (!string.IsNullOrEmpty(value.DueTime))
                writer.WriteString("dueTime", value.DueTime);

            if (!string.IsNullOrEmpty(value.Period))
                writer.WriteString("period", value.Period);

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
                    var unpacked = TryUnpackKnownType(value.Data);
                    if (unpacked != null)
                    {
                        var json = JsonFormatter.Default.Format(unpacked);
                        using var doc = JsonDocument.Parse(json);
                        doc.RootElement.WriteTo(writer);
                    }
                    else
                    {
                        writer.WriteNullValue();  
                    }
                }
            }

            writer.WriteEndObject();
        }

        private IMessage TryUnpackKnownType(WellKnownTypes.Any any)
        {
            // TODO:  
            // if (any.TypeUrl.EndsWith("sametype"))
            //     return any.Unpack<Object>();

            return null!;
        }
    }
}
