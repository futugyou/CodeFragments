namespace Labuladong;

public class Code0319
{
    public static void Exection()
    {
        var n = 100;
        int result = CloseLight(n);
        Console.WriteLine(result);
    }

    private static int CloseLight(int n)
    {
        return (int)Math.Sqrt(n);
    }
}