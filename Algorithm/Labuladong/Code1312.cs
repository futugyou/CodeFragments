namespace Labuladong;
public class Code1312
{
    public static void Exection()
    {
        Console.WriteLine(MinInsert(""));
        Console.WriteLine(MinInsert("race"));
        Console.WriteLine(MinInsert("google"));
    }

    ///  0~n-1
    ///  i from n-2 to 0(include 0)
    ///  j from i+1 to n
    ///  0 Q Q Q Q
    ///  Q 0 Q Q Q
    ///  Q Q 0 Q Q
    ///  Q Q Q 0 Q
    ///  Q Q Q Q 0
    public static int MinInsert(string s)
    {
        var n = s.Length;
        if (n == 0 || n == 1)
        {
            return 0;
        }
        var dp = new int[n, n];
        for (int i = n - 2; i >= 0; i--)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (s[i] == s[j])
                {
                    dp[i, j] = dp[i + 1, j - 1];
                }
                else
                {
                    dp[i, j] = Math.Min(dp[i, j - 1], dp[i + 1, j]) + 1;
                }
            }
        }
        return dp[0, n - 1];
    }
}