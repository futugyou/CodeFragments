namespace Labuladong;
public class Code0283
{
    public static void Exection()
    {
        var nums = new int[] { 0, 0, 1, 2, 2, 3, 3 };
        var val = 0;
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
        for (; left < nums.Length; left++)
        {
            nums[left] = 0;
        }
        Console.WriteLine(string.Join(",", nums));
    }
}