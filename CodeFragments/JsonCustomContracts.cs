using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace CodeFragments;
// Custom attribute to annotate the property
// we want to be incremented.
[AttributeUsage(AttributeTargets.Property)]
class SerializationCountAttribute : Attribute
{
}

// Example type to serialize and deserialize.
class Product
{
    public string Name { get; set; } = "";
    [SerializationCount]
    public int RoundTrips { get; set; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class JsonIncludePrivateFieldsAttribute : Attribute { }

[JsonIncludePrivateFields]
public class Human
{
    private string _name;
    private int _age;

    public Human()
    {
        // This constructor should be used only by deserializers.
        _name = null!;
        _age = 0;
    }

    public static Human Create(string name, int age)
    {
        Human h = new()
        {
            _name = name,
            _age = age
        };

        return h;
    }

    [JsonIgnore]
    public string Name
    {
        get => _name;
        set => throw new NotSupportedException();
    }

    [JsonIgnore]
    public int Age
    {
        get => _age;
        set => throw new NotSupportedException();
    }
}

public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class JsonCustomContracts
{
    static void IncrementCounterModifier(JsonTypeInfo typeInfo)
    {
        foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties)
        {
            if (propertyInfo.PropertyType != typeof(int))
                continue;

            object[] serializationCountAttributes = propertyInfo.AttributeProvider?.GetCustomAttributes(typeof(SerializationCountAttribute), true) ?? [];
            SerializationCountAttribute? attribute = serializationCountAttributes.Length == 1 ? (SerializationCountAttribute)serializationCountAttributes[0] : null;

            if (attribute != null)
            {
                Action<object, object?>? setProperty = propertyInfo.Set;
                if (setProperty is not null)
                {
                    propertyInfo.Set = (obj, value) =>
                    {
                        if (value != null)
                        {
                            // Increment the value by 1.
                            value = (int)value + 1;
                        }

                        setProperty(obj, value);
                    };
                }
            }
        }
    }

    public static void Base()
    {
        var product = new Product
        {
            Name = "Aquafresh"
        };

        JsonSerializerOptions options = new()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { IncrementCounterModifier }
            }
        };

        // First serialization and deserialization.
        string serialized = JsonSerializer.Serialize(product, options);
        Console.WriteLine(serialized);
        // {"Name":"Aquafresh","RoundTrips":0}

        Product deserialized = JsonSerializer.Deserialize<Product>(serialized, options)!;
        Console.WriteLine($"{deserialized.RoundTrips}");
        // 1

        // Second serialization and deserialization.
        serialized = JsonSerializer.Serialize(deserialized, options);
        Console.WriteLine(serialized);
        // { "Name":"Aquafresh","RoundTrips":1}

        deserialized = JsonSerializer.Deserialize<Product>(serialized, options)!;
        Console.WriteLine($"{deserialized.RoundTrips}");
        // 2
    }

    static void AddPrivateFieldsModifier(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        if (!jsonTypeInfo.Type.IsDefined(typeof(JsonIncludePrivateFieldsAttribute), inherit: false))
            return;

        foreach (FieldInfo field in jsonTypeInfo.Type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            JsonPropertyInfo jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(field.FieldType, field.Name);
            jsonPropertyInfo.Get = field.GetValue;
            jsonPropertyInfo.Set = field.SetValue;

            jsonTypeInfo.Properties.Add(jsonPropertyInfo);
        }
    }

    public static void PrivateFields()
    {
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { AddPrivateFieldsModifier }
            }
        };

        var human = Human.Create("Julius", 37);
        string json = JsonSerializer.Serialize(human, options);
        Console.WriteLine(json);
        // {"_name":"Julius","_age":37}

        Human deserializedHuman = JsonSerializer.Deserialize<Human>(json, options)!;
        Console.WriteLine($"[Name={deserializedHuman.Name}; Age={deserializedHuman.Age}]");
        // [Name=Julius; Age=37]
    }

    static void SetNumberHandlingModifier(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Type == typeof(int))
        {
            jsonTypeInfo.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        }
    }

    public static void AllowIntString()
    {
        JsonSerializerOptions options = new()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { SetNumberHandlingModifier }
            }
        };

        // Triple-quote syntax is a C# 11 feature.
        Point point = JsonSerializer.Deserialize<Point>("""{"X":"12","Y":"3"}""", options)!;
        Console.WriteLine($"({point.X},{point.Y})");
        // (12,3)
    }
}