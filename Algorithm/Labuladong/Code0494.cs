namespace Labuladong;

public class Code0494
{
    public static void Exection()
    {
        var nums = new int[] { 1, 1, 1, 1, 1 };
        var target = 3;
        BackTrack(nums, 0, target);
        Console.WriteLine(result);
        result = Dp(nums, 0, target);
        Console.WriteLine(result);
        result = Dp2(nums, target);
        Console.WriteLine(result);

    }

    private static int Dp2(int[] nums, int target)
    {
        var sum = 0;
        foreach (var n in nums)
        {
            sum += n;
        }
        if (target > sum || (sum + target) % 2 == 1)
        {
            return 0;
        }
        return SubSets(nums, (sum + target) / 2);
    }

    private static int SubSets(int[] nums, int target)
    {
        var n = nums.Length;
        var dp = new int[n + 1][];
        for (int i = 0; i <= n; i++)
        {
            dp[i] = new int[target + 1];
            dp[i][0] = 1;
        }
        for (int i = 1; i <= n; i++)
        {
            for (int j = 0; j <= target; j++)
            {
                if (j >= nums[i - 1])
                {
                    dp[i][j] = dp[i - 1][j] + dp[i - 1][j - nums[i - 1]];
                }
                else
                {
                    dp[i][j] = dp[i - 1][j];
                }
            }
        }
        return dp[n][target];
    }

    private static Dictionary<string, int> dic = new();
    private static int Dp(int[] nums, int index, int target)
    {
        if (nums.Length == index)
        {
            if (target == 0)
            {
                return 1;
            }
            return 0;
        }

        var key = index + "," + target;
        if (dic.ContainsKey(key))
        {
            return dic[key];
        }
        var t = Dp(nums, index + 1, target - nums[index]) + Dp(nums, index + 1, target + nums[index]);
        dic[key] = t;
        return t;
    }

    private static void BackTrack(int[] nums, int index, int target)
    {
        if (nums.Length == index)
        {
            if (target == 0)
            {
                result += 1;
            }
            return;
        }

        target += nums[index];
        BackTrack(nums, index + 1, target);
        target -= nums[index];


        target -= nums[index];
        BackTrack(nums, index + 1, target);
        target += nums[index];
    }
    private static int result = 0;
}