namespace DailyCodingProblem;

/// <summary>
/// Write a program that computes the length of the longest common subsequence of three given strings. 
/// For example, given "epidemiologist", "refrigeration", and "supercalifragilisticexpialodocious", 
/// it should return 5, since the longest common subsequence is "eieio".
/// </summary>
public class D01042
{
    public static void Exection()
    {
        ExecTwo();
        ExecThree();
    }

    public static void ExecTwo()
    {
        var str1 = "epidemiologist";
        var str2 = "refrigeration";
        var m = str1.Length;
        var n = str2.Length;
        var dp = new int[m + 1, n + 1];
        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                if (str1[i - 1] == str2[j - 1])
                {
                    dp[i, j] = dp[i - 1, j - 1] + 1;
                }
                else
                {
                    dp[i, j] = Math.Max(dp[i, j - 1], dp[i - 1, j]);
                }
            }
        }
        Console.WriteLine(dp[m, n]);
    }

    public static void ExecThree()
    {
        var str1 = "epidemiologist";
        var str2 = "refrigeration";
        var str3 = "supercalifragilisticexpialodocious";
        var m = str1.Length;
        var n = str2.Length;
        var l = str3.Length;
        var dp = new int[m + 1, n + 1, l + 1];
        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                for (int k = 1; k <= l; k++)
                {
                    if (str1[i - 1] == str2[j - 1] && str2[j - 1] == str3[k - 1])
                    {
                        dp[i, j, k] = dp[i - 1, j - 1, k - 1] + 1;
                    }
                    else
                    {
                        dp[i, j, k] = Math.Max(dp[i, j, k - 1], Math.Max(dp[i, j - 1, k], dp[i - 1, j, k]));
                    }
                }
            }
        }
        Console.WriteLine(dp[m, n, l]);
    }
}