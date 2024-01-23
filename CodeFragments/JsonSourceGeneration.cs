
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeFragments;
public class JsonSourceGeneration
{
    public static void Base()
    {
        string jsonString =
        @"{
            ""Date"": ""2019-08-01T00:00:00"",
            ""TemperatureCelsius"": 25,
            ""Summary"": ""Hot""
          }
        ";
        WeatherForecast? weatherForecast;

        weatherForecast = JsonSerializer.Deserialize<WeatherForecast>(jsonString, SourceGenerationContext.Default.WeatherForecast);
        Console.WriteLine($"Date={weatherForecast?.Date}");

        weatherForecast = JsonSerializer.Deserialize(jsonString, typeof(WeatherForecast), SourceGenerationContext.Default)
            as WeatherForecast;
        Console.WriteLine($"Date={weatherForecast?.Date}");

        var sourceGenOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = SourceGenerationContext.Default
        };

        weatherForecast = JsonSerializer.Deserialize(jsonString, typeof(WeatherForecast), sourceGenOptions) as WeatherForecast;
        Console.WriteLine($"Date={weatherForecast?.Date}");

        jsonString = JsonSerializer.Serialize(weatherForecast!, SourceGenerationContext.Default.WeatherForecast);
        Console.WriteLine(jsonString);

        jsonString = JsonSerializer.Serialize(weatherForecast, typeof(WeatherForecast), SourceGenerationContext.Default);
        Console.WriteLine(jsonString);

        sourceGenOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = SourceGenerationContext.Default
        };

        jsonString = JsonSerializer.Serialize(weatherForecast, typeof(WeatherForecast), sourceGenOptions);
        Console.WriteLine(jsonString);
    }
}

public class WeatherForecast
{
    public DateTime Date { get; set; }
    public int TemperatureCelsius { get; set; }
    public string? Summary { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(WeatherForecast))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}