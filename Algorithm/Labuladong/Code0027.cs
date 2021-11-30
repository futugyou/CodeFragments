namespace Labuladong;
public class Code0027
{
    public static void Exection()
    {
        var nums = new int[] { 0, 0, 1, 2, 2, 3, 3 };
        var val = 2;
        var left = 0;
        var right = 0;
        while (right < nums.Length)
        {
            if (val != nums[right])
            {
                nums[left] = nums[right];
                left++;
            }
            right++;
        }
        Console.WriteLine(left);
    }
}