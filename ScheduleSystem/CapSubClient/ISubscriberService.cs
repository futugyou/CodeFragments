using DotNetCore.CAP;
using Newtonsoft.Json;

namespace CapSubClient;

public interface ISubscriberService
{
    void CheckReceivedMessage(string foodstring);
}

public class SubscriberService : ISubscriberService, ICapSubscribe
{
    [CapSubscribe("make.food")]
    public void CheckReceivedMessage(string foodstring)
    {
        Food food = JsonConvert.DeserializeObject<Food>(foodstring);
        Console.WriteLine(food?.Name);
        Console.WriteLine(food?.Price);
        Console.WriteLine(food?.Size);
    }
}

public class Food
{
    public string Name { get; set; }
    public double Price { get; set; }
    public double Size { get; set; }
}