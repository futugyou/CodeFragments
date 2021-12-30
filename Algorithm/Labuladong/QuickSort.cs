namespace Labuladong;

public static class QuickSortExtensions
{
    public static void QuickSort(this int[] nums)
    {
        Sort(nums, 0, nums.Length - 1);
    }

    private static void Sort(int[] nums, int left, int right)
    {
        if (left >= right)
        {
            return;
        }
        var p = Partition(nums, left, right);
        Sort(nums, left, p - 1);
        Sort(nums, p + 1, right);
    }

    private static int Partition(int[] nums, int left, int right)
    {
        if (left == right)
        {
            return left;
        }
        var i = left;
        var j = right + 1;
        var p = nums[left];
        while (true)
        {
            while (nums[++i] < p)
            {
                if (i == right)
                {
                    break;
                }
            }
            while (nums[--j] > p)
            {
                if (j == left)
                {
                    break;
                }
            }
            if (i >= j)
            {
                break;
            }
            var t = nums[i];
            nums[i] = nums[j];
            nums[j] = t;
        }
        nums[left] = nums[j];
        nums[j] = p;
        return j;
    }
}