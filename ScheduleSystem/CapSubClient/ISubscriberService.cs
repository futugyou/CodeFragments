using DotNetCore.CAP;

namespace CapSubClient;

public interface ISubscriberService
{
    void CheckReceivedMessage(Food food);
}

public class SubscriberService : ISubscriberService, ICapSubscribe
{
    [CapSubscribe("make.food")]
    public void CheckReceivedMessage(Food food)
    {
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