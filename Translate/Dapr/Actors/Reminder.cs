using Google.Protobuf;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using WellKnownTypes = Google.Protobuf.WellKnownTypes;

namespace Actors;

[JsonConverter(typeof(JsonConverter))]
[BsonSerializer(typeof(BsonConverter))]
public class Reminder
{
    [JsonPropertyName("actorId")]
    [BsonElement("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    [BsonElement("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("name")]
    [BsonElement("name")]
    public string Name { get; set; }
    [JsonPropertyName("data")]
    [BsonElement("data")]
    public WellKnownTypes.Any Data { get; set; }
    [JsonPropertyName("period")]
    [BsonElement("period")]
    public ReminderPeriod Period { get; set; }
    [JsonPropertyName("registeredTime")]
    [BsonElement("registeredTime")]
    public DateTime RegisteredTime { get; set; }
    [JsonPropertyName("dueTime")]
    [BsonElement("dueTime")]
    public string DueTime { get; set; }
    [JsonPropertyName("expirationTime")]
    [BsonElement("expirationTime")]
    public DateTime ExpirationTime { get; set; }
    [JsonPropertyName("callback")]
    [BsonElement("callback")]
    public string Callback { get; set; }
    [JsonIgnore]
    [BsonIgnore]
    public bool IsTimer { get; set; }
    [JsonIgnore]
    [BsonIgnore]
    public bool IsRemote { get; set; }
    [JsonIgnore]
    [BsonIgnore]
    public bool SkipLock { get; set; }

    public string ActorKey() => $"{ActorType}{Const.DaprSeparator}{ActorID}";
    public string Key() => $"{ActorType}{Const.DaprSeparator}{ActorID}{Const.DaprSeparator}{Name}";

    public (DateTime, bool) NextTick()
    {
        var active = DateTime.MinValue == ExpirationTime || RegisteredTime.CompareTo(ExpirationTime) < 0;
        return (RegisteredTime, active);
    }

    public bool HasRepeats() => Period.HasRepeats();

    public int RepeatsLeft() => Period.Repeats;

    public bool TickExecuted()
    {
        if (Period.Repeats > 0)
        {
            Period.Repeats--;
        }

        if (!HasRepeats())
        {
            return true;
        }

        RegisteredTime = Period.GetFollowing(RegisteredTime);

        return false;
    }

    public DateTime ScheduledTime() => RegisteredTime;

    override public string ToString()
    {
        var hasData = Data != null;
        var dueTime = "null";
        if (DateTime.MinValue != RegisteredTime)
        {
            dueTime = "'" + RegisteredTime.ToString("yyyy-MM-ddTHH:mm:ssZ") + "'";
        }

        var expirationTime = "null";
        if (DateTime.MinValue != ExpirationTime)
        {
            expirationTime = "'" + ExpirationTime.ToString("yyyy-MM-ddTHH:mm:ssZ") + "'";
        }

        var period = Period.ToString();

        if (period == "")
        {
            period = "null";
        }
        else
        {
            period = "'" + period + "'";

        }

        return $"name='{Name}' hasData={hasData} period={period} dueTime={dueTime} expirationTime={expirationTime}";
    }

    public bool RequiresUpdating(Reminder newReminder)
    {
        if (ActorID != newReminder.ActorID || ActorType != newReminder.ActorType || Name != newReminder.Name)
        {
            return false;
        }

        return DueTime != newReminder.DueTime || Period != newReminder.Period ||
        newReminder.ExpirationTime != DateTime.MinValue || (ExpirationTime != DateTime.MinValue && newReminder.ExpirationTime == DateTime.MinValue) ||
       (Data != null && !Data.Equals(newReminder.Data));
    }

    public class BsonConverter : ClassSerializerBase<Reminder>
    {
        override public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Reminder value)
        {
            var json = JsonSerializer.Serialize(value);
            var document = BsonDocument.Parse(json);
            BsonDocumentSerializer.Instance.Serialize(context, document);
        }

        public override Reminder Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var document = BsonDocumentSerializer.Instance.Deserialize(context);
            var json = document.ToJson();
            return JsonSerializer.Deserialize<Reminder>(json) ?? new Reminder();
        }
    }

    public sealed class JsonConverter : JsonConverter<Reminder>
    {
        public override Reminder? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            var reminder = new Reminder
            {
                Period = new ReminderPeriod(),
            };

            // read expirationTime（string -> DateTime）
            if (root.TryGetProperty("expirationTime", out var expProp) && expProp.ValueKind == JsonValueKind.String)
            {
                if (DateTime.TryParse(expProp.GetString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var exp))
                {
                    reminder.ExpirationTime = exp.ToUniversalTime().AddTicks(-(exp.Ticks % TimeSpan.TicksPerSecond));
                }
            }

            // read registeredTime（string -> DateTime）
            if (root.TryGetProperty("registeredTime", out var regProp) && regProp.ValueKind == JsonValueKind.String)
            {
                if (DateTime.TryParse(regProp.GetString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var reg))
                {
                    reminder.RegisteredTime = reg.ToUniversalTime().AddTicks(-(reg.Ticks % TimeSpan.TicksPerSecond));
                }
            }

            if (root.TryGetProperty("period", out var periodProp) && periodProp.ValueKind == JsonValueKind.String)
            {
                var periodPropString = periodProp.GetString();
                if (!string.IsNullOrEmpty(periodPropString))
                {
                    reminder.Period = new ReminderPeriod(periodPropString);
                }
            }

            if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind != JsonValueKind.Null)
            {
                var rawData = dataProp.GetRawText();
                reminder.Data = WellKnownTypes.Any.Pack(new WellKnownTypes.BytesValue { Value = ByteString.CopyFromUtf8(rawData) });
            }

            var defaultReminder = JsonSerializer.Deserialize<ReminderJson>(root.GetRawText(), options);
            if (defaultReminder != null)
            {
                reminder.ActorID = defaultReminder.ActorID;
                reminder.ActorType = defaultReminder.ActorType;
                reminder.Name = defaultReminder.Name;
                reminder.DueTime = defaultReminder.DueTime;
                reminder.Callback = defaultReminder.Callback;
            }

            return reminder;
        }

        public override void Write(Utf8JsonWriter writer, Reminder value, JsonSerializerOptions options)
        {
            var json = JsonSerializer.Serialize(value, options);
            using var doc = JsonDocument.Parse(json);
            writer.WriteStartObject();

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                switch (property.Name)
                {
                    case "registeredTime":
                        if (value.RegisteredTime != DateTime.MinValue)
                            writer.WriteString("registeredTime", value.RegisteredTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                        break;

                    case "expirationTime":
                        if (value.ExpirationTime != DateTime.MinValue)
                            writer.WriteString("expirationTime", value.ExpirationTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                        break;

                    case "period":
                        if (value.Period != null)
                            writer.WriteString("period", value.Period.ToString());
                        break;

                    case "data":
                        writer.WritePropertyName("data");

                        if (value.Data.TypeUrl.EndsWith("BytesValue"))
                        {
                            var bytesValue = value.Data.Unpack<WellKnownTypes.BytesValue>();
                            writer.WriteBase64StringValue(bytesValue.Value.ToByteArray());
                        }
                        else
                        {
                            var jsonn = JsonFormatter.Default.Format(value.Data);
                            using var docc = JsonDocument.Parse(jsonn);
                            docc.RootElement.WriteTo(writer);
                        }
                        break;

                    default:
                        property.WriteTo(writer);
                        break;
                }
            }

            writer.WriteEndObject();
        }
    }

    private class ReminderJson
    {
        [JsonPropertyName("actorId")]
        [BsonElement("actorId")]
        public string ActorID { get; set; }
        [JsonPropertyName("actorType")]
        [BsonElement("actorType")]
        public string ActorType { get; set; }
        [JsonPropertyName("name")]
        [BsonElement("name")]
        public string Name { get; set; }
        [JsonPropertyName("dueTime")]
        [BsonElement("dueTime")]
        public string DueTime { get; set; }
        [JsonPropertyName("callback")]
        [BsonElement("callback")]
        public string Callback { get; set; }
    }
}