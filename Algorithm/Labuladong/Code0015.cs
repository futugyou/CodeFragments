namespace Labuladong;
public class Code0015
{
    public static void Exection()
    {
        var nums = new int[] { -1, 0, 1, 2, -1, -4 };
        Array.Sort(nums);
        List<List<int>> result = NSumTarget(nums, 3, 0, 0);
    }

    private static List<List<int>> NSumTarget(int[] nums, int n, int start, int target)
    {
        var ns = nums.Length;
        var result = new List<List<int>>();
        if (n < 2 || ns < n)
        {
            return result;
        }
        if (n == 2)
        {
            var low = start;
            var high = ns;
            while (low < high)
            {
                var sum = nums[low] + nums[high];
                var left = nums[low];
                var right = nums[high];
                if (sum < target)
                {
                    // [1,1,1,1,1,2,3,4,5] low 0 -> 5
                    while (low < high && nums[low] == left)
                    {
                        low++;
                    }
                }
                else if (sum > target)
                {
                    // [1,2,3,4,4,4,4,4] high 7 -> 2
                    while (low < high && nums[high] == right)
                    {
                        high--;
                    }
                }
                else
                {
                    result.Add(new List<int> { low, high });
                    while (low < high && nums[low] == left)
                    {
                        low++;
                    }
                    while (low < high && nums[high] == right)
                    {
                        high--;
                    }
                }
            }
        }
        else
        {
            for (int i = start; i < ns; i++)
            {
                var sub = NSumTarget(nums, n - 1, i + 1, target - nums[i]);
                foreach (var item in sub)
                {
                    item.Add(nums[i]);
                    result.Add(item);
                }
                // [1,1,1,2,3,4,5] i 0 -> 3
                while (i < ns - 1 && nums[i] == nums[i + 1])
                {
                    i++;
                }
            }

        }
        return result;
    }
}