namespace Labuladong;

public class Code0416
{
    public static void Exection()
    {
        var nums = new int[] { 1, 5, 11, 5 };
        bool res = CanSplit(nums);
        Console.WriteLine(res);
    }

    private static bool CanSplit(int[] nums)
    {
        var sum = nums.Sum();
        if (sum % 2 == 1)
        {
            return false;
        }
        var n = nums.Length;
        sum = sum / 2;
        var dp = new bool[n + 1, sum + 1];
        for (int i = 0; i < n + 1; i++)
        {
            dp[i, 0] = true;
        }
        for (int i = 1; i < n + 1; i++)
        {
            for (int j = 1; j < sum + 1; j++)
            {
                if (j < nums[i - 1])
                {
                    dp[i, j] = dp[i - 1, j];
                }
                else
                {
                    dp[i, j] = dp[i - 1, j] || dp[i - 1, j - nums[i - 1]];
                }
            }
        }
        return dp[n, sum];
    }
}