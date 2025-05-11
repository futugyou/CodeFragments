using Google.Protobuf;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using WellKnownTypes = Google.Protobuf.WellKnownTypes;

namespace Actors.Models;

// TODO: update JsonConverter, beacause JsonContext is used
[JsonConverter(typeof(JsonConverter))]
public class CreateReminderRequest
{
    [JsonPropertyName("actorId")]
    public string ActorID { get; set; }
    [JsonPropertyName("actorType")]
    public string ActorType { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("data")]
    public WellKnownTypes.Any Data { get; set; }

    [JsonPropertyName("period")]
    public string Period { get; set; }
    [JsonPropertyName("dueTime")]
    public string DueTime { get; set; }
    [JsonPropertyName("ttl")]
    public string TTL { get; set; }
    [JsonIgnore]
    public bool IsOneShot { get; set; }

    public string ActorKey() => $"{ActorType}{Const.DaprSeparator}{ActorID}";
    public string Key() => $"{ActorType}{Const.DaprSeparator}{ActorID}{Const.DaprSeparator}{Name}";

    public Reminder NewReminder(DateTime time)
    {
        var reminder = new Reminder
        {
            ActorID = ActorID,
            ActorType = ActorType,
            Name = Name,
            Data = Data,
        };

        SetReminderTimes(reminder, DueTime, Period, TTL, time, "reminder");
        return reminder;
    }

    private static void SetReminderTimes(Reminder reminder, string dueTime, string period, string ttl, DateTime now, string logMsg)
    {
        // Due time and registered time
        reminder.RegisteredTime = now;

        reminder.DueTime = dueTime;

        if (dueTime != "")
        {
            try
            {
                reminder.RegisteredTime = Tools.TimeUtil.ParseTimeTruncateSeconds(dueTime, now);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        // Parse period and check correctness
        try
        {
            reminder.Period = new ReminderPeriod(period);
        }
        catch (System.Exception)
        {
            throw;
        }

        // Set expiration time if configured
        if (!string.IsNullOrEmpty(ttl))
        {
            try
            {
                reminder.ExpirationTime = Tools.TimeUtil.ParseTimeTruncateSeconds(ttl, reminder.RegisteredTime);
            }
            catch (System.Exception)
            {
                throw;
            }

            // check if already expired
            if (now.CompareTo(reminder.ExpirationTime) > 0 || reminder.RegisteredTime.CompareTo(reminder.ExpirationTime) > 0)
            {
                throw new Exception($"{logMsg} {reminder.Key()} has already expired: dueTime: {reminder.DueTime} TTL: {ttl}");

            }
        }
    }

    private class CreateReminderRequestJson
    {
        [JsonPropertyName("actorId")]
        public string ActorID { get; set; }
        [JsonPropertyName("actorType")]
        public string ActorType { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("period")]
        public string Period { get; set; }
        [JsonPropertyName("dueTime")]
        public string DueTime { get; set; }
        [JsonPropertyName("ttl")]
        public string TTL { get; set; }
    }
    public sealed class JsonConverter : JsonConverter<CreateReminderRequest>
    {
        public override CreateReminderRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            var request = new CreateReminderRequest();

            if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind != JsonValueKind.Null)
            {
                var rawData = dataProp.GetRawText();
                request.Data = WellKnownTypes.Any.Pack(new WellKnownTypes.BytesValue { Value = ByteString.CopyFromUtf8(rawData) });
            }

            var defaultReminder = JsonSerializer.Deserialize<CreateReminderRequestJson>(root.GetRawText(), options);
            if (defaultReminder != null)
            {
                request.ActorID = defaultReminder.ActorID;
                request.ActorType = defaultReminder.ActorType;
                request.Name = defaultReminder.Name;
                request.DueTime = defaultReminder.DueTime;
                request.TTL = defaultReminder.TTL;
                request.Period = defaultReminder.Period;
            }

            return request;
        }

        public override void Write(Utf8JsonWriter writer, CreateReminderRequest value, JsonSerializerOptions options)
        {
            var json = JsonSerializer.Serialize(value, options);
            using var doc = JsonDocument.Parse(json);
            writer.WriteStartObject();

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                switch (property.Name)
                {
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
}
