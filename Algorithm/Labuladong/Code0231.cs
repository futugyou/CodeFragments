namespace Labuladong;

public class Code0231
{
    public static void Exection()
    {
        var result = IsPowerTwo(16);
        Console.WriteLine(result);
    }

    private static bool IsPowerTwo(int v)
    {
        return (v & (v - 1)) == 0;
    }
}
