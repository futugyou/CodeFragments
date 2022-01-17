namespace Labuladong;

public class Code0045
{
    public static void Exection()
    {
        var nums = new int[] { 2, 3, 1, 1, 4 };
        int result = Jump(nums);
        Console.WriteLine(result);
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