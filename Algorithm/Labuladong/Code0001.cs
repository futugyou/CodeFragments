namespace Labuladong;
public class Code0001
{
    public static void Exection()
    {
        var nums = new int[] { 3, 5, 6, 1 };
        var target = 9;
        nums = nums.OrderBy(a => a).ToArray();
        int left = 0;
        int right = nums.Length - 1;
        while (left < right)
        {
            var s = nums[left] + nums[right];
            if (s == target)
            {
                Console.WriteLine($"{nums[left]},{nums[right]}");
                break;
            }
            else if (s > target)
            {
                right--;
            }
            else
            {
                left++;
            }
        }
        Console.WriteLine("end");
    }
}