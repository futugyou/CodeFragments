namespace Labuladong;

public class Code0292
{
    public static void Exection()
    {
        var n = 3;
        bool result = NimGame(n);
        Console.WriteLine(result);
    }

    private static bool NimGame(int n)
    {
        return n % 4 != 0;
    }
}