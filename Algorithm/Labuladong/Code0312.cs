namespace Labuladong;

public class Code0312
{
    public static void Exection()
    {
        var nums = new int[] { 3, 1, 5, 8 };
        int res = MaxCoins(nums);
        Console.WriteLine(res);
    }

    private static int MaxCoins(int[] nums)
    {
        var n = nums.Length;
        var newnums = new int[n + 2];
        newnums[0] = 1;
        newnums[n + 1] = 1;
        for (int i = 0; i < n; i++)
        {
            newnums[i + 1] = nums[i];
        }
        var dp = new int[n + 2, n + 2];

        for (int i = n + 1; i >= 0; i--)
        {
            for (int j = i + 1; j < n + 2; j++)
            {
                for (int k = i + 1; k < j; k++)
                {
                    dp[i, j] = Math.Max(dp[i, j], dp[i, k] + dp[k, j] + newnums[i] * newnums[k] * newnums[j]);
                }
            }
        }

        return dp[0, n + 1];
    }
}