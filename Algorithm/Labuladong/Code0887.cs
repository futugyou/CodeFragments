namespace Labuladong;
public class Code0887
{
    public static void Exection()
    {
        var k = 3;
        var n = 14;
        var result = Egg(k, n);
        Console.WriteLine(result);
    }

    private static int Egg(int k, int n)
    {
        var dp = new int[k + 1, n + 1];
        for (int i = 0; i <= k; i++)
        {
            for (int j = 0; j <= n; j++)
            {
                if (i == 0 || j == 0)
                {
                    dp[i, j] = 0;
                }
                else if (i == 1)
                {
                    dp[i, j] = j;
                }
                else
                    dp[i, j] = int.MaxValue;
            }
        }
        for (int i = 2; i < k + 1; i++)
        {
            for (int j = 1; j < n + 1; j++)
            {
                for (int nn = 1; nn <= j; nn++)
                {
                    dp[i, j] = Math.Min(dp[i, j], 1 + Math.Max(dp[i - 1, nn - 1], dp[i, j - nn]));
                }
            }
        }
        return dp[k, n];
    }
}