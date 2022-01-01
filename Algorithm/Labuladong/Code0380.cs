namespace Labuladong;

public class Code0380
{
    public static void Exection()
    {
        var c = new RandomizedSet();
        c.Insert(1);
        c.Insert(2);
        c.Insert(3);
        c.Insert(4);
        c.Insert(5);
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(c.GetRandom());
        }
    }
}