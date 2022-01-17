namespace Labuladong;

public class Code0045
{
    public static void Exection()
    {
        var nums = new int[] { 2, 2, 1, 1, 4 };
        int result = Jump(nums);
        Console.WriteLine(result);
        result = Jump2(nums);
        Console.WriteLine(result);
    }

    private static int Jump2(int[] nums)
    {
        var dp = new int[nums.Length];
        Array.Fill(dp, nums.Length);
        dp[0] = 0;
        for (int i = 1; i < nums.Length; i++)
        {
            for (int j = 0; j < i; j++)
            {
                if (i <= j + nums[j])
                {
                    dp[i] = Math.Min(dp[i], dp[j] + 1);
                }
            }
        }
        Console.WriteLine(string.Join(",", dp));
        return dp[nums.Length - 1];
    }

    private static int Jump(int[] nums)
    {
        var step = 0;
        var fast = 0;
        var end = 0;
        for (int i = 0; i < nums.Length - 1; i++)
        {
            fast = Math.Max(fast, nums[i] + i);
            if (i == end)
            {
                step++;
                end = fast;
            }
        }
        return step;
    }
}