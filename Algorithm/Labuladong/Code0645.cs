namespace Labuladong;
public class Code0645
{
    public static void Exection()
    {
        var nums = new int[] { 1, 2, 3, 3, 5, 6, 7 };
        var res = FindMiss(nums);
        Console.WriteLine(res);
    }

    private static (int, int) FindMiss(int[] nums)
    {
        var dup = -1;
        var miss = -1;
        for (int i = 0; i < nums.Length; i++)
        {
            var index = Math.Abs(nums[i]) - 1;
            if (nums[index] < 0)
            {
                dup = Math.Abs(nums[i]);
            }
            else
            {
                nums[index] = -nums[index];
            }
        }
        for (int i = 0; i < nums.Length; i++)
        {
            if (nums[i] > 0)
            {
                miss = i + 1;
                break;
            }
        }
        return (dup, miss);
    }
}