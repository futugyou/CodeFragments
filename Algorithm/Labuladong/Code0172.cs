namespace Labuladong;

public class Code0172
{
    public static void Exection()
    {
        var result = CountZero(25);
        Console.WriteLine(result);
    }

    private static int CountZero(int n)
    {
        var res = 0;
        var t = 5;
        while (t <= n)
        {
            res += n / t;
            t = t * 5;
        }
        return res;
    }
}
