
namespace Labuladong;

public class Code0096
{
    public static void Exection()
    {
        TotleCount(3);
    }

    private static int TotleCount(int n)
    {
        if (n < 1)
        {
            return 1;
        }
        memo = new int[n + 1, n + 1];
        return Count(1, n);
    }
    private static int[,] memo;
    private static int Count(int low, int high)
    {
        if (low > high)
        {
            return 1;
        }
        if (memo[low, high] != 0)
        {
            return memo[low, high];
        }
        var res = 0;
        for (int i = low; i <= high; i++)
        {
            var leftcount = Count(low, i - 1);
            var rightcount = Count(i + 1, high);
            res += leftcount * rightcount;
        }
        memo[low, high] = res;
        return res;
    }
}