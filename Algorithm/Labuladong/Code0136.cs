namespace Labuladong;

public class Code0136
{
    public static void Exection()
    {
        var nums = new int[] { 10, 2, 3, 4, 5, 2, 3, 4, 5 };
        var result = SingleNumber(nums);
        Console.WriteLine(result);
    }

    public static int SingleNumber(int[] nums)
    {
        var res = 0;
        foreach (var n in nums)
        {
            res = res ^ n;
        }
        return res;
    }
}