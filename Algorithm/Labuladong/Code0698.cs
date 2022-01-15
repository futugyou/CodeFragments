namespace Labuladong;

public class Code0698
{
    public static void Exection()
    {
        var nums = new int[] { 4, 3, 2, 3, 5, 2, 1 };
        var k = 4;
        var result = Subset(nums, k);
        Console.WriteLine(result);
    }
    private static bool Subset(int[] nums, int k)
    {
        var sum = 0;
        foreach (var n in nums)
        {
            sum += n;
        }
        if (sum % k != 0)
        {
            return false;
        }
        var target = sum / k;
        var used = new bool[nums.Length];
        return BackTrack(nums, 0, target, used, k, 0);
    }

    private static bool BackTrack(int[] nums, int bucket, int target, bool[] used, int count, int start)
    {
        if (count == 0)
        {
            return true;
        }
        if (bucket == target)
        {
            return BackTrack(nums, 0, target, used, count - 1, 0);
        }
        for (int i = start; i < nums.Length; i++)
        {
            if (used[i])
            {
                continue;
            }
            if (nums[i] + bucket > target)
            {
                continue;
            }
            used[i] = true;
            bucket += nums[i];
            if (BackTrack(nums, bucket, target, used, count, i + 1))
            {
                return true;
            }
            used[i] = false;
            bucket -= nums[i];
        }
        return false;
    }
}