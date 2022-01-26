namespace Labuladong;

public class Code0877
{
    public static void Exection()
    {
        var piles = new int[] { 2, 5, 8, 3 };
        int result = StoneGame(piles);
        Console.WriteLine(result);
    }

    private static int StoneGame(int[] piles)
    {
        var n = piles.Length;
        var dp = new int[n, n, 2];
        for (int i = 0; i < n; i++)
        {
            dp[i, i, 0] = piles[i];
        }
        for (int i = n - 2; i >= 0; i--)
        {
            for (int j = i + 1; j < n; j++)
            {
                var left = piles[i] + dp[i + 1, j, 1];
                var right = piles[j] + dp[i, j - 1, 1];

                if (left > right)
                {
                    dp[i, j, 0] = left;
                    dp[i, j, 1] = dp[i + 1, j, 0];
                }
                else
                {
                    dp[i, j, 0] = right;
                    dp[i, j, 1] = dp[i, j - 1, 0];
                }
            }
        }
        return dp[0, n - 1, 0] - dp[0, n - 1, 1];
    }
}