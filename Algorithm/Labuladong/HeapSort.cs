namespace Labuladong;

public static class HeapSortExtensions
{
    public static void HeapSort(this int[] nums)
    {
        for (int i = nums.Length / 2; i >= 0; i--)
        {
            HeapAdjust(nums, i, nums.Length);
        }
        for (int i = nums.Length - 1; i > 0; i--)
        {
            var t = nums[i];
            nums[i] = nums[0];
            nums[0] = t;
            HeapAdjust(nums, 0, i);
        }
    }

    private static void HeapAdjust(int[] nums, int index, int length)
    {
        var tmp = nums[index];
        var child = 2 * index + 1;
        while (child < length)
        {
            // if right children exsits and big than left children, choose right children.
            if (child + 1 < length && nums[child] < nums[child + 1])
            {
                child++;
            }
            if (tmp >= nums[child])
            {
                break;
            }
            nums[index] = nums[child];
            index = child;
            child = 2 * child + 1;
        }
        nums[index] = tmp;
    }
}