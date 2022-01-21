namespace Labuladong;

public class Code0309
{
    public static void Exection()
    {
        var prices = new int[] { 7, 1, 5, 3, 7, 4, 8 };
        int res = MaxProfit(prices);
        Console.WriteLine(res);
    }

    // buy infinity, sell infinity, but sell have one day for CD.
    private static int MaxProfit(int[] prices)
    {
        var n = prices.Length;
        var dp = new int[n, 2];
        dp[0, 1] = -prices[0];
        dp[0, 0] = 0;

        dp[1, 1] = Math.Max(dp[0, 1], -prices[1]);
        dp[1, 0] = Math.Max(dp[0, 0], dp[0, 1] + prices[1]);

        for (int i = 2; i < n; i++)
        {
            dp[i, 1] = Math.Max(dp[i - 1, 1], dp[i - 2, 0] - prices[i]);
            dp[i, 0] = Math.Max(dp[i - 1, 0], dp[i - 1, 1] + prices[i]);
        }
        return dp[n - 1, 0];
    }
}