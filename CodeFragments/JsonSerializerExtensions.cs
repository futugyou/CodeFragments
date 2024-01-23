using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeFragments
{
    public class JsonSerializerExtensions
    {
        public static JsonSerializerOptions CreateJsonSetting()
        {
            var json_setting = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
            json_setting.Converters.Add(new CustomJsonStringEnumConverter());
            //json_setting.Converters.Add(new JsonStringEnumConverter());
            json_setting.Converters.Add(new Int32NullConverter());
            json_setting.Converters.Add(new BoolNullConverter());
            json_setting.Converters.Add(new LongNullConverter());
            return json_setting;
        }
    }

    public class CustomJsonStringEnumConverter : JsonConverterFactory
    {
        private readonly JsonNamingPolicy? namingPolicy;
        private readonly bool allowIntegerValues;
        private readonly JsonStringEnumConverter baseConverter;

        public CustomJsonStringEnumConverter() : this(null, true) { }

        public CustomJsonStringEnumConverter(JsonNamingPolicy? namingPolicy = null, bool allowIntegerValues = true)
        {
            this.namingPolicy = namingPolicy;
            this.allowIntegerValues = allowIntegerValues;
            this.baseConverter = new JsonStringEnumConverter(namingPolicy, allowIntegerValues);
        }

        public override bool CanConvert(Type typeToConvert) => baseConverter.CanConvert(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var query = from field in typeToConvert.GetFields(BindingFlags.Public | BindingFlags.Static)
                        let attr = field.GetCustomAttribute<EnumMemberAttribute>()
                        where attr != null
                        select (field.Name, attr.Value);
            var dictionary = query.ToDictionary(p => p.Item1!, p => p.Item2!);
            if (dictionary.Count > 0 && namingPolicy != null)
            {
                return new JsonStringEnumConverter(new DictionaryLookupNamingPolicy(dictionary, namingPolicy), allowIntegerValues).CreateConverter(typeToConvert, options);
            }
            else
            {
                return baseConverter.CreateConverter(typeToConvert, options);
            }
        }
    }

    public class JsonNamingPolicyDecorator : JsonNamingPolicy
    {
        readonly JsonNamingPolicy underlyingNamingPolicy;

        public JsonNamingPolicyDecorator(JsonNamingPolicy underlyingNamingPolicy) => this.underlyingNamingPolicy = underlyingNamingPolicy;

        public override string ConvertName(string name) => underlyingNamingPolicy == null ? name : underlyingNamingPolicy.ConvertName(name);
    }

    public class DictionaryLookupNamingPolicy : JsonNamingPolicyDecorator
    {
        readonly Dictionary<string, string> dictionary;

        public DictionaryLookupNamingPolicy(Dictionary<string, string> dictionary, JsonNamingPolicy underlyingNamingPolicy) : base(underlyingNamingPolicy) => this.dictionary = dictionary ?? throw new ArgumentNullException();

        public override string ConvertName(string name) => dictionary.TryGetValue(name, out var value) ? value : base.ConvertName(name);
    }
    public class Int32NullConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return 0;
            }

            return reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
    public class BoolNullConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return false;
            }

            return reader.GetBoolean();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
    public class LongNullConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return 0;
            }

            return reader.GetInt64();
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}