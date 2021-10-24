namespace DailyCodingProblem;

/// <summary>
/// Find the minimum number of coins required to make n cents.
/// You can use standard American denominations, that is, 1¢, 5¢, 10¢, and 25¢.
/// For example, given n = 16, return 3 since we can make it with a 10¢, a 5¢, and a 1¢.
/// </summary>
public class D01029
{
    public static void Exection()
    {
        var n = 26;
        var coins = new int[] { 25, 10, 5, 1 };
        var result = 0;
        foreach (var item in coins)
        {
            if (n / item > 0)
            {
                result += n / item;
                n = n % item;
            }
        }
        Console.WriteLine(result);

        n = 26;
        var dp = new int[n + 1];
        for (int i = 0; i < n + 1; i++)
        {
            dp[i] = n + 1;
        }
        dp[0] = 0;
        for (int i = 1; i < n + 1; i++)
        {
            foreach (var item in coins)
            {
                if (i >= item)
                {
                    dp[i] = Math.Min(dp[i], dp[i - item] + 1);
                }
            }
        }
        Console.WriteLine(dp[n]);
    }
}