namespace Labuladong;

public class Code0714
{
    public static void Exection()
    {
        var prices = new int[] { 7, 1, 5, 3, 7, 4, 9 };
        var fee = 2;
        int res = MaxProfit(prices, fee);
        Console.WriteLine(res);
    }

    // buy/sell infinity with fee
    private static int MaxProfit(int[] prices, int fee)
    {
        var n = prices.Length;
        var dp = new int[n, 2];
        dp[0, 1] = -prices[0];
        dp[0, 0] = 0;
        for (int i = 1; i < n; i++)
        {
            dp[i, 1] = Math.Max(dp[i - 1, 1], dp[i - 1, 0] - prices[i]);
            dp[i, 0] = Math.Max(dp[i - 1, 0], dp[i - 1, 1] + prices[i] - fee);
        }
        return dp[n - 1, 0];
    }
}