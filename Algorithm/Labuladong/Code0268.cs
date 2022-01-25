namespace Labuladong;

public class Code0268
{
    public static void Exection()
    {
        var nums = new int[] { 9, 8, 6, 5, 4, 3, 2, 1, 0 };
        var result = LostNum(nums);
        Console.WriteLine(result);
    }

    private static int LostNum(int[] nums)
    {
        var n = nums.Length;
        var res = 0;
        res = res ^ n;
        for (int i = 0; i < n; i++)
        {
            res = res ^ (i ^ nums[i]);
        }
        return res;
    }
}