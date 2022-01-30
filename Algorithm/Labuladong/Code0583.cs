namespace Labuladong;
public class Code0583
{
    public static void Exection()
    {
        var str1 = "sea";
        var str2 = "eat";
        var result = CountChange(str1, str2);
        Console.WriteLine(result);
    }

    private static int CountChange(string str1, string str2)
    {
        var m = str1.Length;
        var n = str2.Length;
        var dp = new int[m + 1, n + 1];
        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                if (str1[i - 1] == str2[j - 1])
                {
                    dp[i, j] = 1 + dp[i - 1, j - 1];
                }
                else
                {
                    dp[i, j] = Math.Max(dp[i, j - 1], dp[i - 1, j]);
                }
            }
        }
        var lcs = dp[m, n];
        return m + n - 2 * lcs;
    }
}