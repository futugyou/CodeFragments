namespace Labuladong;
public class Code1143
{
    public static void Exection()
    {
        var str1 = "abcde";
        var str2 = "ace";
        var result = Lcs(str1, str2);
        Console.WriteLine(result);
    }

    private static int Lcs(string str1, string str2)
    {
        var m = str1.Length;
        var n = str2.Length;
        var dp = new int[m + 1, n + 1];
        for (int i = 0; i < m + 1; i++)
        {
            for (int j = 0; j < n + 1; j++)
            {
                if (str1[i - 1] == str2[j - 1])
                {
                    dp[i, j] = 1 + dp[i - 1, j - 1];
                }
                else
                {
                    dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                }
            }
        }
        return dp[m, n];
    }
}
