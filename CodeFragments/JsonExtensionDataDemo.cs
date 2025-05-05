using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CodeFragments;

public class JsonExtensionDataDemo
{
    public static void Exection()
    {
        var jsonString = JsonSerializer.Serialize(new
        {
            Name = "Ming",
            Age = 10,
            Title = "SDE",
            City = "Shanghai"
        });
        Console.WriteLine(jsonString);

        var p1 = JsonSerializer.Deserialize<Person1>(jsonString);
        ArgumentNullException.ThrowIfNull(p1, nameof(p1));
        Console.WriteLine(JsonSerializer.Serialize(p1));
        Console.WriteLine(JsonSerializer.Serialize(p1.Extensions));

        var p2 = JsonSerializer.Deserialize<Person2>(jsonString);
        ArgumentNullException.ThrowIfNull(p2, nameof(p2));
        Console.WriteLine(JsonSerializer.Serialize(p2));
        Console.WriteLine(JsonSerializer.Serialize(p2.Extensions));

        var p3 = JsonSerializer.Deserialize<Person3>(jsonString);
        ArgumentNullException.ThrowIfNull(p3, nameof(p3));
        Console.WriteLine(JsonSerializer.Serialize(p3));
        Console.WriteLine(JsonSerializer.Serialize(p3.Extensions));

        Point1 obj = JsonSerializer.Deserialize<Point1>(@"{""X"":1,""Y"":2}");
        Console.WriteLine(obj.X); // 40 ,Would be 1 if property were set directly after object construction.
        Console.WriteLine(obj.Y); // 60 ,Would be 2 if property were set directly after object construction.

        Point2 point = JsonSerializer.Deserialize<Point2>(@"{""X"":1,""Y"":2,""X"":4}");
        Console.WriteLine(point.X); // 4 Note, the value isn't 1.
        Console.WriteLine(point.Y); // 2

        string json = @"{
    ""FirstName"":""Jet"",
    ""Id"":""270bb22b-4816-4bd9-9acd-8ec5b1a896d3"",
    ""EmailAddress"":""jetdoe@outlook.com"",
    ""Id"":""0b3aa420-2e98-47f7-8a49-fea233b89416"",
    ""LastName"":""Doe"",
    ""Id"":""63cf821d-fd47-4782-8345-576d9228a534""
    }";

        Person4 person4 = JsonSerializer.Deserialize<Person4>(json)!;
        Console.WriteLine(person4.FirstName); // Jet
        Console.WriteLine(person4.LastName); // Doe
        Console.WriteLine(person4.Id); // 63cf821d-fd47-4782-8345-576d9228a534 (note that the LAST matching JSON property "won")
        Console.WriteLine(person4.ExtensionData["EmailAddress"].GetString()); // jetdoe@outlook.com
        Console.WriteLine(person4.ExtensionData.ContainsKey("Id")); // False
    }
}

public record Person(string Name, int Age);

public record Person1(string Name, int Age) : Person(Name, Age)
{
    [JsonExtensionData]
    public Dictionary<string, object?> Extensions { get; set; } = new();
}

public record Person2(string Name, int Age) : Person(Name, Age)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Extensions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public record Person3(string Name, int Age) : Person(Name, Age)
{
    [JsonExtensionData]
    public JsonObject? Extensions { get; set; }
}

public struct Point1
{
    public int X { get; set; }
    public int Y { get; set; }

    [JsonConstructor]
    public Point1(int x, int y)
    {
        X = 40;
        Y = 60;
    }
}

public struct Point2
{
    public int X { get; set; }
    public int Y { get; set; }
    public Point2(int x, int y)
    {
        X = 40;
        Y = 60;
    }
}

public class Person4
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public Guid Id { get; }

    [JsonExtensionData] // It must implement 'IDictionary<string, JsonElement>' or 'IDictionary<string, object>', or be 'JsonObject'
    public Dictionary<string, JsonElement> ExtensionData { get; set; }

    public Person4(Guid id) => Id = id;
}
