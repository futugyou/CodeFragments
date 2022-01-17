namespace Labuladong;

public class Code0055
{
    public static void Exection()
    {
        var nums = new int[] { 3, 2, 2, 0, 4 };
        bool result = Jump(nums);
        Console.WriteLine(result);
    }

    private static bool Jump(int[] nums)
    {
        var n = nums.Length;
        var fast = 0;
        for (int i = 0; i < nums.Length; i++)
        {
            fast = Math.Max(fast, i + nums[i]);
            if (fast <= i)
            {
                return false;
            }
        }
        return fast >= n - 1;
    }
}