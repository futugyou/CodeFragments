using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf;
using WellKnownTypes = Google.Protobuf.WellKnownTypes;

namespace Actors;

public class Const
{
    public const string DaprSeparator = "||";
    public static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromMinutes(60);
    public static readonly TimeSpan DefaultOngoingCallTimeout = TimeSpan.FromSeconds(60);
    public const int DefaultReentrancyStackLimit = 32;
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

[JsonConverter(typeof(JsonConverter))]
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

public class SaveStateRequest
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("keys")]
    public string[] Keys { get; set; }
    [JsonPropertyName("value")]
    public WellKnownTypes.Any Value { get; set; }
}

public class StateResponse
{
    [JsonPropertyName("data")]
    public byte[] Data { get; set; }
    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; }
}

public class BulkStateResponse
{
    [JsonPropertyName("data")]
    public Dictionary<string, byte[]> Data { get; set; }
}

[JsonConverter(typeof(JsonConverter))]
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
