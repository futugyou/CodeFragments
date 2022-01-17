namespace Labuladong;

public class Code0053
{
    public static void Exection()
    {
        var nums = new int[] { -2, 1, -3, 4, -1, 2, 1, -5, 4 };
        int result = MaxSubArray(nums);
        Console.WriteLine(result);
    }

    public static int MaxSubArray(int[] nums)
    {
        int result = int.MinValue;
        var n = nums.Length;
        var dp = new int[n];
        dp[0] = nums[0];
        for (int i = 1; i < n; i++)
        {
            dp[i] = Math.Max(nums[i], dp[i - 1] + nums[i]);
        }
        for (int i = 0; i < n; i++)
        {
            result = Math.Max(result, dp[i]);
        }
        return result;
    }
}