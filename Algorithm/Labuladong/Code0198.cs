namespace Labuladong;

public class Code0198
{
    public static void Exection()
    {
        var nums = new int[] { 1, 2, 3, 1 };
        int result = Rob(nums);
        Console.WriteLine(result);
    }

    private static int Rob(int[] nums)
    {
        var dp = new int[nums.Length];
        dp[0] = nums[0];
        dp[1] = Math.Max(nums[0], nums[1]);
        for (int i = 2; i < nums.Length; i++)
        {
            dp[i] = Math.Max(dp[i - 1], dp[i - 2] + nums[i]);
        }
        Console.WriteLine(string.Join(",", dp));
        return dp[nums.Length - 1];
    }
}
