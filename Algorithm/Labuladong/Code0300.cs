namespace Labuladong;

public class Code0300
{
    public static void Exection()
    {
        var nums = new int[] { 10, 9, 2, 5, 3, 7, 101, 18 };
        int result = MaxLenghtSubArray(nums);
        Console.WriteLine(result);
    }

    private static int MaxLenghtSubArray(int[] nums)
    {
        var dp = new int[nums.Length];
        Array.Fill(dp, 1);
        for (int i = 1; i < nums.Length; i++)
        {
            for (int j = 0; j < i; j++)
            {
                if (nums[i] > nums[j])
                {
                    dp[i] = Math.Max(dp[i], dp[j] + 1);
                }
            }
        }
        var res = 0;
        for (int i = 0; i < nums.Length; i++)
        {
            res = Math.Max(res, dp[i]);
        }
        return res;
    }
}