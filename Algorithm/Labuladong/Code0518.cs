namespace Labuladong;

public class Code0518
{
    public static void Exection()
    {
        var amount = 5;
        var coins = new int[] { 1, 2, 5 };
        int result = Change(coins, amount);
        Console.WriteLine(result);
    }

    private static int Change(int[] coins, int amount)
    {
        var n = coins.Length;
        var dp = new int[n + 1, amount + 1];

        for (int i = 0; i < n + 1; i++)
        {
            dp[i, 0] = 1;
        }
        for (int i = 1; i < n + 1; i++)
        {
            for (int j = 1; j < amount + 1; j++)
            {
                if (j >= coins[i - 1])
                {
                    dp[i, j] = dp[i - 1, j] + dp[i, j - coins[i - 1]];
                }
                else
                {
                    dp[i, j] = dp[i - 1, j];
                }
            }
        }
        return dp[n, amount];
    }
}