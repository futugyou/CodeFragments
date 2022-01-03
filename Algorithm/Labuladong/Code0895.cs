namespace Labuladong;

public class Code0895
{
    public static void Exection()
    {
        var c = new FreqStack();
        c.Push(1);
        c.Push(2);
        c.Push(3);
        c.Push(1);
        c.Push(2);
        Console.WriteLine(c.Pop());
        Console.WriteLine(c.Pop());
    }
}