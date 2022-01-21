namespace Labuladong;

public class Code0188
{
    public static void Exection()
    {
        var prices = new int[] { 7, 1, 5, 3, 7, 4, 9 };
        var k = 4;
        int res = MaxProfit(prices, k);
        Console.WriteLine(res);
    }

    // buy k times, sell k times
    private static int MaxProfit(int[] prices, int k)
    {
        var n = prices.Length;
        // day, limit, 0 not have/1 have
        var dp = new int[n, k + 1, 2];
        for (int i = 0; i < n; i++)
        {
            dp[i, 0, 1] = int.MinValue;
            dp[i, 0, 0] = 0;
        }
        for (int i = 0; i <= k; i++)
        {
            dp[0, i, 1] = -prices[0];
            dp[0, i, 0] = 0;
        }
        for (int i = 1; i < n; i++)
        {
            for (int j = 1; j <= k; j++)
            {
                dp[i, j, 1] = Math.Max(dp[i - 1, j, 1], dp[i - 1, j - 1, 0] - prices[i]);
                dp[i, j, 0] = Math.Max(dp[i - 1, j, 0], dp[i - 1, j, 1] + prices[i]);
            }
        }
        return dp[n - 1, k, 0];
    }
}