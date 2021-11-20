namespace Labuladong;
public class Code0034
{
    public static void Exection()
    {
        var nums = new int[] { 5, 7, 7, 8, 8, 10 };
        var target = 8;
        var leftbound = LeftBound(nums, target);
        var rightbound = RightBound(nums, target);
        Console.WriteLine($"[{leftbound},{rightbound}]");
    }

    public static int LeftBound(int[] nums, int target)
    {
        var left = 0;
        var right = nums.Length - 1;
        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            if (nums[mid] == target)
            {
                right = mid - 1;
            }
            else if (nums[mid] > target)
            {
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }
        if (left >= nums.Length || nums[left] != target)
        {
            return -1;
        }
        return left;
    }

    public static int RightBound(int[] nums, int target)
    {
        var left = 0;
        var right = nums.Length - 1;
        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            if (nums[mid] == target)
            {
                left = mid + 1;
            }
            else if (nums[mid] > target)
            {
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }
        if (right < 0 || nums[right] != target)
        {
            return -1;
        }
        return right;
    }
}