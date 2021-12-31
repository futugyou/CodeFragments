namespace Labuladong;

public class Code0146
{
    public static void Exection()
    {
        var c = new LRUCache(3);
        c.Put(1, 1);
        c.Put(2, 2);
        c.Put(3, 3);
        c.Put(4, 4);
        c.Put(5, 5);
        Console.WriteLine(c.Get(3));
        Console.WriteLine(c.Get(2));
    }
}