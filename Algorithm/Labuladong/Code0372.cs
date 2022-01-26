namespace Labuladong;

public class Code0372
{
    public static void Exection()
    {
        var a = 10;
        var b = new int[] { 1, 4, 2, 1 };
        int result = SuperPow(a, b.ToList());
        Console.WriteLine(result);
    }

    private static int SuperPow(int a, List<int> b)
    {
        if (!b.Any())
        {
            return 1;
        }
        var last = b.Last();
        b.RemoveAt(b.Count - 1);
        var part1 = CustomPow(a, last);
        var part2 = CustomPow(SuperPow(a, b), 10);
        return (part1 * part2) % 1337;
    }

    private static int CustomPow(int a, int k)
    {
        a = a % 1337;
        var res = 1;
        for (int i = 0; i < k; i++)
        {
            res = res * a;
            res = res % 1337;
        }
        return res;
    }
}