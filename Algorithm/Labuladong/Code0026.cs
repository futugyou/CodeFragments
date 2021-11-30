namespace Labuladong;
public class Code0026
{
    public static void Exection()
    {
        var nums = new int[] { 0, 0, 1, 2, 2, 3, 3 };
        var left = 0;
        var right = 0;
        while (right < nums.Length)
        {
            if (nums[left] != nums[right])
            {
                left++;
                nums[left] = nums[right];
            }
            right++;
        }
        Console.WriteLine(left);
    }
}