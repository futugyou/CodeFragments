namespace Labuladong;

public class Code0072
{
    public static void Exection()
    {
        var word1 = "intention";
        var word2 = "execution";
        int res = MinDistance(word1, word2);
        Console.WriteLine(res);
    }

    private static int MinDistance(string word1, string word2)
    {
        var m = word1.Length;
        var n = word2.Length;
        var dp = new int[m + 1, n + 1];
        for (int i = 0; i < m; i++)
        {
            dp[i, 0] = i;
        }
        for (int i = 0; i < n; i++)
        {
            dp[0, i] = i;
        }
        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                if (word1[i - 1] == word2[j - 1])
                {
                    dp[i, j] = dp[i - 1, j - 1];
                }
                else
                {
                    dp[i, j] = 1 + Math.Min(dp[i, j - 1], Math.Min(dp[i - 1, j], dp[i - 1, j - 1]));
                }
            }
        }
        return dp[m, n];
    }
}