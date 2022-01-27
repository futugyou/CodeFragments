namespace Labuladong;
public class Code0509
{
    public static void Exection()
    {
        var n = Fib(4);
        Console.WriteLine(n);
    }
    public static int Fib(int n)
    {
        if (n == 0)
        {
            return 0;
        }
        if (n == 1)
        {
            return 1;
        }
        var pre = 0;
        var curr = 1;
        var next = 1;
        for (int i = 2; i <= n; i++)
        {
            next = pre + curr;
            pre = curr;
            curr = next;
        }
        return curr;
    }
}