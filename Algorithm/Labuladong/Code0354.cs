namespace Labuladong;
public class Code0354
{
    public static void Exection()
    {
        var nums = new List<int[]> { new int[] { 1, 8 }, new int[] { 2, 3 }, new int[] { 5, 4 }, new int[] { 5, 2 }, new int[] { 6, 7 }, new int[] { 6, 4 } };
        int result = MaxEnvelopes(nums);
        Console.WriteLine(result);
    }

    private static int MaxEnvelopes(List<int[]> nums)
    {
        int n = nums.Count;
        var sorted = nums.OrderBy(p => p[0]).ThenByDescending(p => p[0]).ToList();
        var height = new int[n];
        for (int i = 0; i < n; i++)
        {
            height[i] = sorted[i][1];
        }
        return LengthOfLIS2(height);
    }

    public static int LengthOfLIS2(int[] nums)
    {
        var n = nums.Length;
        var dp = new int[n];
        Array.Fill(dp, 1);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < i; j++)
            {
                if (nums[j] > nums[i])
                {
                    dp[i] = Math.Max(dp[i], dp[j] + 1);
                }
            }
        }
        var result = 0;
        for (int i = 0; i < n; i++)
        {
            result = Math.Max(result, dp[i]);
        }
        return result;
    }
    public static int LengthOfLIS(int[] nums)
    {
        var piles = 0;
        var n = nums.Length;
        var top = new int[n];
        for (int i = 0; i < n; i++)
        {
            int target = nums[i];
            var left = 0;
            var right = piles - 1;
            while (left <= right)
            {
                var mid = left + (right - left) / 2;
                if (target == top[mid])
                {
                    right = mid - 1;
                }
                else if (target > top[mid])
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }
            if (left >= piles)
            {
                piles++;
            }
            top[left] = target;
        }
        return piles;
    }
}