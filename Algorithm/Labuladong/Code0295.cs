namespace Labuladong;

public class Code0295
{
    public static void Exection()
    {
        var f = new MedianFinder();
        f.AddNum(1);
        f.AddNum(2);
        f.AddNum(3);
        f.AddNum(4);
        var d = f.FindMedian();
        Console.WriteLine(d);
    }
}