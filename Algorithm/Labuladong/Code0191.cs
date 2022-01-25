namespace Labuladong;

public class Code0191
{
    public static void Exection()
    {
        var result = CountOne(10);
        Console.WriteLine(result);
    }

    private static int CountOne(int n)
    {
        var res = 0;
        while (n != 0)
        {
            n = n & (n - 1);
            res++;
        }
        return res;
    }
}