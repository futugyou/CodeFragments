namespace Labuladong;
public class Code0213
{
    public static void Exection()
    {
        var nums = new int[] { 3, 0, 1, 2, 1, 4, 1, 2 };
        var n = nums.Length;
        if (n == 1)
        {
            Console.WriteLine(nums[0]);
        }
        else
        {
            var count = Math.Max(Rob(nums, 0, n - 2), Rob(nums, 1, n - 1));
            Console.WriteLine(count);
        }
    }

    public static int Rob(int[] nums, int start, int end)
    {
        var n = nums.Length;
        int dp_i_1 = 0;
        int dp_i_2 = 0;
        int dp_i = 0;
        for (int i = end; i >= start; i--)
        {
            dp_i = Math.Max(dp_i_1, nums[i] + dp_i_2);
            dp_i_2 = dp_i_1;
            dp_i_1 = dp_i;
        }
        return dp_i;
    }
}
