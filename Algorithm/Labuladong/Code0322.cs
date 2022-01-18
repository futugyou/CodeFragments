namespace Labuladong;
public class Code0322
{
    public static void Exection()
    {
        var coins = new int[] { 1, 2, 5 };
        var amount = 11;
        int result = CoinChange(coins, amount);
        Console.WriteLine(result);
    }

    public static int CoinChange(int[] coins, int amount)
    {
        if (amount == 0)
        {
            return -1;
        }
        // dp[i] : when amount = i, mix coin count is dp[i].
        int[] dp = new int[amount + 1];
        Array.Fill(dp, amount + 1);
        dp[0] = 0;
        for (int i = 1; i <= amount; i++)
        {
            foreach (var coin in coins)
            {
                if (i >= coin)
                {
                    dp[i] = Math.Min(dp[i], dp[i - coin] + 1);
                }
            }
        }
        return dp[amount];
    }
}
